using System;

namespace ProfitWise.Data.Model.Shop
{
    public class PwPreferences
    {
        public long PwShopId { get; set; }
        public string Permission { get; set; }
        public DateTime StartingDateForOrders { get; set; }
        public string CostTrackingMethod { get; set; }
        public bool CurrencyConversionEnabled { get; set; }
        public bool ViewProfitsByChannelOnly { get; set;  }
        public bool RecognizeRevenueByOrderCreateDate { get; set; }
    }
}
