namespace ProfitWise.Data.HangFire
{
    public class QueueChannel
    {
        public readonly string[] RapidResponse = { Queues.InitialShopRefresh };
        public readonly string[] Routine = { Queues.RoutineShopRefresh };
        public readonly string[] System = { Queues.ExchangeRateRefresh, Queues.CleanupService };
    }
}
