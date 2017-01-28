namespace ProfitWise.Data.HangFire
{
    public class QueueChannel
    {
        public static readonly string[] Routine = { Queues.RoutineShopRefresh };
        public static readonly string[] System = { Queues.ExchangeRateRefresh, Queues.CleanupService };
    }
}
