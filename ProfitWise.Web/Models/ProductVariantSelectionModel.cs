namespace ProfitWise.Web.Models
{
    public class ProductVariantSelectionModel
    {
        public const string ProductType = "Product";
        public const string VariantType = "Variant";

        public long Id { get; set; }
        public string SelectionType { get; set; }
    }
}
