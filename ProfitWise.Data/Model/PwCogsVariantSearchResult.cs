namespace ProfitWise.Data.Model
{
    public class PwCogsVariantSearchResult
    {
        public long PwMasterProductId { get; set;  }
        public long PwMasterVariantId { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public bool? CogsDetail { get; set; }

        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public int? Inventory { get; set; }

        // This needs to be manually populated, thus leveraging the Currency Service
        public decimal? NormalizedCogsAmount { get; set; }

        public PwCogsProductSearchResult Parent { get; set; }
    }
}
