using System;

namespace ProfitWise.Data.Model
{
    public class ProfitWiseBatchState
    {
        public long ShopId { get; set; }
        public DateTime? ProductsLastUpdated { get; set; }
        public DateTime? OrderDatasetStart { get; set; }
        public DateTime? OrderDatasetEnd { get; set; }
    }
}
