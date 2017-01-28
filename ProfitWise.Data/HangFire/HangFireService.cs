using Hangfire;
using ProfitWise.Data.Processes;
using Push.Foundation.Utilities.Logging;

namespace ProfitWise.Data.HangFire
{
    public class HangFireService
    {
        private readonly IPushLogger _logger;

        public HangFireService(IPushLogger logger)
        {
            _logger = logger;
        }

        private string ShopRefreshProcessId(string userId)
        {
            return $"ShopRefreshProcess:{userId}";
        }

        private const string ExchangeRateRefreshId = "ExchangeRateRefresh";


        public string TriggerInitialShopRefresh(string userId)
        {
            _logger.Info($"Scheduling Initial Shop Refresh for User {userId}");
            return BackgroundJob.Enqueue<ShopRefreshProcess>(x => x.Execute(userId));
        }

        public string ScheduleRoutineShopRefresh(string userId)
        {
            var jobIdentifier = ShopRefreshProcessId(userId);
            _logger.Info($"Scheduling Routine Shop Refresh for User {userId} / Job Id {jobIdentifier}");

            RecurringJob.AddOrUpdate<ShopRefreshProcess>(
                jobIdentifier, x => x.Execute(userId), Cron.Minutely, queue: Queues.RoutineShopRefresh);

            return jobIdentifier;
        }

        public void ScheduleExchangeRateRefresh()
        {
            _logger.Info($"Scheduling ExchangeRateRefreshProcess");
            
            RecurringJob.AddOrUpdate<ExchangeRateRefreshProcess>(
                ExchangeRateRefreshId, x => x.Execute(), Cron.Minutely, queue: Queues.ExchangeRateRefresh);
        }

        public void KillRecurringJob(string jobId)
        {
            RecurringJob.RemoveIfExists(jobId);
        }

        public void KillBackgroundJob(string jobId)
        {
            BackgroundJob.Delete(jobId);
        }
    }
}
