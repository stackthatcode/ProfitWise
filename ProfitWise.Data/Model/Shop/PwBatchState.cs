using System;

namespace ProfitWise.Data.Model.Shop
{
    public class PwBatchState
    {
        public long PwShopId { get; set; }
        public DateTime? ProductsLastUpdated { get; set; }
        public DateTime? OrderDatasetStart { get; set; }
        public DateTime? OrderDatasetEnd { get; set; }


        public override string ToString()
        {
            return $"Batch State dump for Shop {PwShopId}: ProductsLastUpdated = {ProductsLastUpdated} and OrderDatasetStart = {OrderDatasetStart} and OrderDatasetEnd = {OrderDatasetEnd}";
        }
    }
}
