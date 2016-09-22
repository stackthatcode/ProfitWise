namespace ProfitWise.Data.Model
{
    public class PwCogsVariantSearchResult
    {
        public long PwMasterProductId { get; set;  }
        public long PwMasterVariantId { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }
        public int CogsCurrencyId { get; set; }
        public decimal CogsAmount { get; set; }
        public bool CogsDetail { get; set; }

        public PwCogsProductSearchResult Parent { get; set; }
    }
}
