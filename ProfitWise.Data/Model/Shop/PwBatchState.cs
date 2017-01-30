using System;

namespace ProfitWise.Data.Model.Shop
{
    public class PwBatchState
    {
        public long PwShopId { get; set; }
        public DateTime? ProductsLastUpdated { get; set; }
        public DateTime? OrderDatasetStart { get; set; }
        public DateTime? OrderDatasetEnd { get; set; }

        public string InitialRefreshJobId { get; set; }
        public string RoutineRefreshJobId { get; set; }


        public override string ToString()
        {
            return 
                $"Batch State dump for Shop {PwShopId}: ProductsLastUpdated = {ProductsLastUpdated}, " +
                $", OrderDatasetStart = {OrderDatasetStart}, OrderDatasetEnd = {OrderDatasetEnd} " +
                $", InitialRefreshJobId = {InitialRefreshJobId}, RoutineRefreshJobId = {RoutineRefreshJobId}";
        }
    }
}
