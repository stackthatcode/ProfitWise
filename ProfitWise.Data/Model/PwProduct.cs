namespace ProfitWise.Data.Model
{
    public class PwProduct
    {
        public long ShopId { get; set; }
        public long PwProductId { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set;  }
        public string Name { get; set; }
        public string Sku { get; set; }
    }
}
