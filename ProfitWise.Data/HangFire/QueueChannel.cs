namespace ProfitWise.Data.HangFire
{
    public class QueueChannel
    {
        public static readonly 
            string[] All =
                {
                    ProfitWiseQueues.InitialShopRefresh,
                    ProfitWiseQueues.RoutineShopRefresh,
                    ProfitWiseQueues.ExchangeRateRefresh,
                    ProfitWiseQueues.CleanupService,
                };
    }
}
