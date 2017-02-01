using System;
using System.Configuration;
using Hangfire;
using ProfitWise.Data.HangFire;
using ProfitWise.Data.Repositories;
using Push.Foundation.Utilities.Helpers;
using Push.Foundation.Utilities.Logging;


namespace ProfitWise.Data.Processes
{
    public class SystemCleanupProcess
    {
        private readonly SystemRepository _systemRepository;
        private readonly IPushLogger _pushLogger;
        

        public SystemCleanupProcess(
                IPushLogger pushLogger, SystemRepository systemRepository)
        {
            _pushLogger = pushLogger;
            _systemRepository = systemRepository;
        }


        [Queue(ProfitWiseQueues.CleanupService)]
        public void Execute()
        {
            // Pick List clean-up...
            var expirationSeconds = 
                ConfigurationManager.AppSettings
                    .GetAndTryParseAsInt("PickListExpirationSeconds", 15 * 60);

            var pickListCutOffDate = DateTime.Now.AddSeconds(-expirationSeconds);
            _systemRepository.DeletePickListByDate(pickListCutOffDate);

            _pushLogger.Info("System Clean-up Service for Pick List - FIN");


        }
    }
}
