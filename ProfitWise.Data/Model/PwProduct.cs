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
        public decimal Price { get; set; }
        public int Inventory { get; set; }
        public string Tags { get; set; }
    }
}
