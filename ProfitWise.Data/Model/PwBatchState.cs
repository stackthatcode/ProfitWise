using System;

namespace ProfitWise.Data.Model
{
    public class PwBatchState
    {
        public long ShopId { get; set; }
        public DateTime? ProductsLastUpdated { get; set; }
        public DateTime? OrderDatasetStart { get; set; }
        public DateTime? OrderDatasetEnd { get; set; }

        public override string ToString()
        {
            return $"Batch State dump for Shop {ShopId}: ProductsLastUpdated = {ProductsLastUpdated} and OrderDatasetStart = {OrderDatasetStart} and OrderDatasetEnd = {OrderDatasetEnd}";
        }
    }
}
