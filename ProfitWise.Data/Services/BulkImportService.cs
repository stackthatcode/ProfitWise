using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Microsoft.VisualBasic.FileIO;
using ProfitWise.Data.Factories;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Cogs.UploadObjects;
using ProfitWise.Data.Repositories.System;
using Push.Foundation.Utilities.General;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.Services
{
    public class BulkImportService
    {
        private readonly FileLocator _fileLocator;
        private readonly MultitenantFactory _factory;
        private readonly IPushLogger _logger;
        private readonly ShopRepository _shopRepository;
        private readonly CurrencyService _currencyService;


        public BulkImportService(
                    FileLocator fileLocator, 
                    MultitenantFactory factory, 
                    IPushLogger logger, 
                    ShopRepository shopRepository, 
                    CurrencyService currencyService)
        {
            _fileLocator = fileLocator;
            _factory = factory;
            _logger = logger;
            _shopRepository = shopRepository;
            _currencyService = currencyService;
        }


        [AutomaticRetry(Attempts = 1)]
        [Queue(ProfitWiseQueues.BulkImportService)]
        public void Process(int pwShopId, long fileUploadId)
        {
            _logger.Info($"Processing Bulk Import for {pwShopId} - {fileUploadId}");
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var repository = _factory.MakeUploadRepository(shop);
            var context = new ImportContext(shop, fileUploadId);

            try
            {
                var upload = repository.Retrieve(fileUploadId);
                var fileLocker = FileLocker.MakeFromUpload(upload);

                // Load the file into memory
                var path = _fileLocator.UploadFilePath(fileLocker);
                
                using (var parser = new TextFieldParser(path))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Skip over the Column Headings...
                    var columnHeadings = parser.ReadFields();
                    var index = 1;
                    
                    while (!parser.EndOfData)
                    {
                        ProcessRow(index++, parser.ReadFields(), context);

                        if (context.ExceededFailureLimit)
                        {
                            break;
                        }
                    }
                }
                
                ReportUploadResults(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);

                ReportUploadSystemFault(pwShopId, fileUploadId);
            }
        }

        // TODO - pad this in error handling in case it fails
        public void ReportUploadSystemFault(int pwShopId, long fileUploadId)
        {
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var repository = _factory.MakeUploadRepository(shop);

            repository.UpdateStatus(fileUploadId, UploadStatusCode.Success);
        }
        
        public void ReportUploadResults(ImportContext context)
        {
            var repository = _factory.MakeUploadRepository(context.PwShop);

            // TODO - add failure or success metrics...?
            repository.UpdateStatus(context.FileUploadId, UploadStatusCode.Success);
        }



        public ValidationSequence<List<string>> BuildValidator()
        {

            return new ValidationSequence<List<string>>()
                .Add(new Rule<List<string>>(
                        x => x.Count >= 10, 
                            "Missing one or more column - cannot read row if any data is missing",
                            instantFailure: true))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.PwMasterVariantId].IsInteger(),
                            "PwMasterVariantId is not a valid number",
                            instantFailure: true))
                
                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.MarginPercent].IsDecimal() || x[UploadAnatomy.FixedAmount].IsDecimal(), 
                            "Neither MarginPercent or FixedAmount are populated with a valid number", 
                            instantFailure:true))
                
                .Add(new Rule<List<string>>(
                        x => !(x[UploadAnatomy.MarginPercent].IsNonZeroDecimal() && x[UploadAnatomy.FixedAmount].IsNonZeroDecimal()),
                            "Both MarginPercent and FixedAmount are populated - please only enter a value for or the other"))
                
                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.MarginPercent].IsZero() || x[UploadAnatomy.MarginPercent].IsWithinRange(-1.0m, 1.0m),
                            "MarginPercent is not a valid number between -1.00 and 1.00 (-100.00% and 100.00%)"))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.FixedAmount].IsZero() || x[UploadAnatomy.FixedAmount].IsWithinRange(0m, 999999999.99m),
                            "FixedAmount is not a valid number between 0 and 999,999,999.99"))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.FixedAmount].IsZero() || 
                            _currencyService.CurrencyExists(x[UploadAnatomy.Abbreviation]),
                            "Invalid currency for FixedAmount"));
        }

        public void ProcessRow(int index, string[] input, ImportContext context)
        {            
            try
            {
                if (input[UploadAnatomy.ProductTitle] == "SATAN CAUSES SYSTEM FAULTS")
                {
                    throw new Exception("Artificially triggered row-level fault");
                }

                var validation = BuildValidator().Run(input.ToList());

                if (validation.Success)
                {
                    var cogs = BuildCogsDto(input.ToList());
                    var service = _factory.MakeCogsService(context.PwShop);
                    var repository = _factory.MakeVariantRepository(context.PwShop);

                    var pwMasterVariantId = input[UploadAnatomy.PwMasterVariantId].ToLong();

                    if (!repository.MasterVariantExists(pwMasterVariantId))
                    {
                        context.ReportFailure(
                            index, 
                            ValidationResult.InstantFailure(
                                $"PwMasterVariantId {pwMasterVariantId} doesn't exist"));

                        return;
                    }

                    service.SaveCogsForMasterVariant(pwMasterVariantId, cogs, null);
                    context.ReportSuccess();

                    _logger.Debug($"Successfully updated PwMasterVariantId {pwMasterVariantId}");
                }
                else
                {
                    context.ReportFailure(index, validation);
                    _logger.Debug($"Failed validation for row number {index + 1}");
                }
            }
            catch (Exception exception)
            {
                context.ReportFailure(index, ValidationResult.InstantFailure("Encountered system fault"));

                if (_logger.IsDebugEnabled)
                {
                    _logger.Error($"Failed validation for row number {index + 1}");
                    _logger.Error(exception);
                }
            }
        }
        


        // This presumes valid data -- validation logic being strictly isolated to ValidationSequence stuff
        public CogsDto BuildCogsDto(List<string> importRow)
        {
            var output = new CogsDto();
            output.CogsCurrencyId = _currencyService.AbbrToCurrencyId(importRow[UploadAnatomy.Abbreviation]);
            output.CogsMarginPercent = importRow[UploadAnatomy.MarginPercent].ToDecimal() * 100.00m;
            output.CogsAmount = importRow[UploadAnatomy.FixedAmount].ToDecimal();
            output.ApplyConstraints();
            output.CogsTypeId =
                    !importRow[UploadAnatomy.FixedAmount].IsZero()
                        ? CogsType.FixedAmount
                        : CogsType.MarginPercentage;
            return output;
        }
    }
}

