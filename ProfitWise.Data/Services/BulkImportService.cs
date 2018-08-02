using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Castle.Core.Internal;
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
        private readonly SystemRepository _systemRepository;

        public BulkImportService(
                    FileLocator fileLocator, 
                    MultitenantFactory factory, 
                    IPushLogger logger, 
                    ShopRepository shopRepository, 
                    CurrencyService currencyService, 
                    SystemRepository systemRepository)
        {
            _fileLocator = fileLocator;
            _factory = factory;
            _logger = logger;
            _shopRepository = shopRepository;
            _currencyService = currencyService;
            _systemRepository = systemRepository;
        }


        [AutomaticRetry(Attempts = 1)]
        [Queue(ProfitWiseQueues.BulkImportService)]
        public void Process(int pwShopId, long fileUploadId)
        {
            _logger.Info($"Processing Bulk Import for {pwShopId} - {fileUploadId}");
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var repository = _factory.MakeUploadRepository(shop);
            var context = new ImportContext(shop, fileUploadId);

            const int maximumNumberOfRows = 50000;

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

                        if (context.ReachedFailureLimit)
                        {
                            break;
                        }

                        if (index > maximumNumberOfRows)
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


        [AutomaticRetry(Attempts = 1)]
        [Queue(ProfitWiseQueues.BulkImportService)]
        public void CleanupOldFiles(int pwShopId)
        {
            _logger.Info($"Executing CleanupOldFilesfor {pwShopId}");
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var repository = _factory.MakeUploadRepository(shop);

            //const int maximumAgeDays = 7;
            var files = repository.RetrieveCompleted();

            foreach (var file in files.OrderByDescending(x => x.LastUpdated).Skip(4))
            {
                var locker = FileLocker.MakeFromUpload(file);
                var path = _fileLocator.Directory(locker);
                if (System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(path, true);

                repository.Delete(file.FileUploadId);
            }
        }

        public void CleanupOldFiles()
        {
            const int maximumFileAgeDays = 7;
            var files = _systemRepository.RetrieveOldUploads(maximumFileAgeDays);
            foreach (var file in files)
            {
                var locker = FileLocker.MakeFromUpload(file);
                var path = _fileLocator.Directory(locker);
                if (System.IO.Directory.Exists(path))
                    System.IO.Directory.Delete(path, true);

                _systemRepository.DeleteFileUpload(file.FileUploadId);
            }
        }

        public void ReportUploadSystemFault(int pwShopId, long fileUploadId)
        {
            try
            {
                var shop = _shopRepository.RetrieveByShopId(pwShopId);
                var repository = _factory.MakeUploadRepository(shop);
                repository.UpdateStatus(fileUploadId, UploadStatusCode.FailureSystemFault);
            }
            catch (Exception ex)
            {
                _logger.Fatal("Unable to update status for file upload");
                _logger.Error(ex);
            }
        }
        
        public void ReportUploadResults(ImportContext context)
        {
            var repository = _factory.MakeUploadRepository(context.PwShop);

            // Flag the upload according to import context
            var code =
                context.ReachedFailureLimit
                    ? UploadStatusCode.FailureTooManyErrors
                    : UploadStatusCode.Success;

            repository.UpdateStatus(context.FileUploadId, code, context.TotalRows, context.SuccessfulRowCount);

            // Generate feedback file
            var feedBackFile = new List<string>();
            feedBackFile.Add(
                $"Imported {context.SuccessfulRowCount} rows successfully; " + 
                $"{context.FailedRows.Count} rows failed");            
            feedBackFile.AddRange(context.FailedRows.Select(x => x.ToString()));

            // Save the upload
            var upload = repository.Retrieve(context.FileUploadId);
            var locker = FileLocker.MakeFromUpload(upload);
            locker.ProvisionFeedbackFileName();
            var path = _fileLocator.FeedbackFilePath(locker);
            System.IO.File.WriteAllText(path, feedBackFile.ToDelimited(Environment.NewLine));

            // Update feedback filename record
            repository.UpdateFeedbackFilename(context.FileUploadId, locker.FeedbackFileName);
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
                        x => x[UploadAnatomy.MarginPercent].IsEmptyOrNumber() 
                                && x[UploadAnatomy.FixedAmount].IsEmptyOrNumber(),
                            "Invalid data entry for Margin Percent or Fixed Amount - please enter a number value or leave the field blank",
                            instantFailure: true))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.MarginPercent].IsDecimal() || x[UploadAnatomy.FixedAmount].IsDecimal(), 
                            "Neither MarginPercent or FixedAmount are populated with a valid number", 
                            instantFailure:true))
                
                .Add(new Rule<List<string>>(
                        x => !(x[UploadAnatomy.MarginPercent].IsNonZeroDecimal() && x[UploadAnatomy.FixedAmount].IsNonZeroDecimal()),
                            "Both MarginPercent and FixedAmount are populated - please only enter a value for one or the other"))
                
                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.MarginPercent].IsNullOrEmpty() || x[UploadAnatomy.MarginPercent].IsWithinRange(-1.0m, 1.0m),
                            "MarginPercent is not a valid number between -1.00 and 1.00 (-100.00% and 100.00%)"))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.FixedAmount].IsNullOrEmpty() || x[UploadAnatomy.FixedAmount].IsWithinRange(0m, 999999999.99m),
                            "FixedAmount is not a valid number between 0 and 999,999,999.99"))

                .Add(new Rule<List<string>>(
                        x => x[UploadAnatomy.FixedAmount].IsNullOrEmpty() || 
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

                var validation = 
                    BuildValidator()
                        .Run(input.ToList());

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

            output.CogsCurrencyId = 
                _currencyService.AbbrToCurrencyId(importRow[UploadAnatomy.Abbreviation]);

            output.CogsMarginPercent = importRow[UploadAnatomy.MarginPercent].ToDecimal() * 100.00m;
            output.CogsAmount = importRow[UploadAnatomy.FixedAmount].ToDecimal();

            output.ApplyConstraints();

            output.CogsTypeId = 
                DetermineCogsType(
                    importRow[UploadAnatomy.FixedAmount],
                    importRow[UploadAnatomy.MarginPercent]);
            
            return output;
        }

        public int DetermineCogsType(string fixedAmountInput, string marginPercentageInput)
        {
            if (fixedAmountInput.IsNullOrEmpty())
                return CogsType.MarginPercentage;

            if (marginPercentageInput.IsNullOrEmpty())
                return CogsType.FixedAmount;

            if (fixedAmountInput.IsNonZeroDecimal() &&
                marginPercentageInput.IsZero())
                return CogsType.FixedAmount;

            return CogsType.MarginPercentage;
        }
    }
}

