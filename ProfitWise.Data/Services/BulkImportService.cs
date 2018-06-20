using System;
using ProfitWise.Data.Factories;

namespace ProfitWise.Data.Services
{
    public class BulkImportService
    {
        private readonly FileLocator _fileLocator;
        private readonly MultitenantFactory _factory;


        public BulkImportService(FileLocator fileLocator, MultitenantFactory factory)
        {
            _fileLocator = fileLocator;
            _factory = factory;
        }

        public void Process(int pwShopId, Guid fileLocker)
        {
            // Load the file into memory
        }
    }
}
