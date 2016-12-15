namespace ProfitWise.Data.Model
{
    public enum ProductSearchFilterType
    {
        ProductType = 1,
        ProductVendor = 2,
        TaggedWith = 3,
        MissingCogs = 4,
    }

    public class ProductSearchFilter
    {
        public string Value { get; set; }
        public ProductSearchFilterType Type { get; set; }
    }
}
