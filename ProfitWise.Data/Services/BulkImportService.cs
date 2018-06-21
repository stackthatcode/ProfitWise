using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualBasic;
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

        public BulkImportService(
                FileLocator fileLocator, 
                MultitenantFactory factory, 
                IPushLogger logger, 
                ShopRepository shopRepository)
        {
            _fileLocator = fileLocator;
            _factory = factory;
            _logger = logger;
            _shopRepository = shopRepository;
        }


        [AutomaticRetry(Attempts = 1)]
        [Queue(ProfitWiseQueues.BulkImportService)]
        public void Process(int pwShopId, long fileUploadId)
        {
            _logger.Info($"Processing Bulk Import for {pwShopId} - {fileUploadId}");
            var shop = _shopRepository.RetrieveByShopId(pwShopId);
            var repository = _factory.MakeUploadRepository(shop);

            try
            {
                var upload = repository.Retrieve(fileUploadId);
                var fileLocker = FileLocker.MakeFromUpload(upload);

                // Load the file into memory
                var path = _fileLocator.UploadFilePath(fileLocker);

                var lineNumber = 1;
                var parsingResults = new List<ValidationResult>();

                using (var parser = new TextFieldParser(path))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();
                        var results = ProcessRow(fields);
                        parsingResults.Add(results);
                        lineNumber++;
                    }
                }

                repository.UpdateStatus(fileUploadId, UploadStatusCode.Success);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                repository.UpdateStatus(fileUploadId, UploadStatusCode.Failure);
            }
        }

        public ValidationSequence<List<string>> ValidationFactory()
        {

            return new ValidationSequence<List<string>>()
                .Add(new Rule<List<string>>(
                    x => x[UploadAnatomy.PwMasterVariantId].IsInteger(),
                    "PwMasterVariantId is not a valid number"));
                       
        }

        public ValidationResult ProcessRow(string[] line)
        {            
            try
            {
                if (line.Length == 0)
                {
                    return ValidationResult.InstantFailure("Is empty");
                }
                if (line.Length < 10)
                {
                    return ValidationResult.InstantFailure(
                        "Is missing a column; cannot read row if any data is missing");
                }
                

                _logger.Info(line[0]);
                return new ValidationResult();
            }
            catch (Exception e)
            {
                return ValidationResult.InstantFailure("Encountered system fault");
            }
        }
    }
}

