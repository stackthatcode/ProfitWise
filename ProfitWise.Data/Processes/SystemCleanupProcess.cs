using System;
using System.Configuration;
using System.IO;
using Hangfire;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.ProcessSteps;
using ProfitWise.Data.Repositories.System;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Processes
{
    public class SystemCleanupProcess
    {
        private readonly SystemRepository _systemRepository;
        private readonly BulkImportService _bulkImportService;
        private readonly IPushLogger _pushLogger;
        private readonly object _lock = new object();


        public SystemCleanupProcess(
                IPushLogger pushLogger, 
                SystemRepository systemRepository,
                BulkImportService bulkImportService)
        {
            _pushLogger = pushLogger;
            _systemRepository = systemRepository;
            _bulkImportService = bulkImportService;
        }

        [Queue(ProfitWiseQueues.CleanupService)]
        public void Execute()
        {
            RunWithTryCatch(() => MonitorForGdprInvocations());
            RunWithTryCatch(() => CleanupReportData());
            RunWithTryCatch(() => _bulkImportService.CleanupOldFiles());
            RunWithTryCatch(() => CleanupChildlessObjects());
        }

        private void RunWithTryCatch(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _pushLogger.Error(ex);
            }
        }


        private void CleanupChildlessObjects()
        {
            // Deletes any childless Master Products or Master Variants
            _pushLogger.Info($"Deleting Childless Master Products and Master Variants");

            // Exclusive lock this section to prevent SQL from deadlocking on 
            // object-level locks from the DELETE's esp. masterproducts
            lock (_lock)
            {
                _systemRepository.DeleteChildlessMasterVariants();
                _systemRepository.DeleteChildlessMasterProducts();
            }

            _pushLogger.Info("System Clean-up Service for Childless Master Variants and Master Products - FIN");
        }

        private void CleanupReportData()
        {
            // Pick List clean-up...
            var expirationSeconds =
                ConfigurationManager.AppSettings
                    .GetAndTryParseAsInt("PickListExpirationSeconds", 15 * 60);

            var pickListCutOffDate = DateTime.UtcNow.AddSeconds(-expirationSeconds);
            _systemRepository.DeletePickListByDate(pickListCutOffDate);

            _pushLogger.Info("System Clean-up Service for Pick List - FIN");

            // Report Data clean-up, too
            var reportDataExpirationSeconds =
                ConfigurationManager.AppSettings
                    .GetAndTryParseAsInt("ReportDataExpirationSeconds", 15 * 60);

            var reportDataCutOffDate = DateTime.UtcNow.AddSeconds(-reportDataExpirationSeconds);

            _systemRepository.CleanupReportData(reportDataCutOffDate);

            _pushLogger.Info("System Clean-up Service for Report Data - FIN");
        }

        private void MonitorForGdprInvocations()
        {
            var count = _systemRepository.RetrieveNumberOfUnhandledGdprInvocations();
            if (count > 0)
            {
                _pushLogger.Fatal("WARNING - unhandled GDPR invocations");
            }
        }
    }
}
