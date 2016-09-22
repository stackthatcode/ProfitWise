using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model
{
    public class PwCogsProductSearchResult
    {
        public long PwMasterProductId { get; set; }
        public long PwProductId { get; set; }
        public string Title { get; set; }
        public string Vendor { get; set; }

        public IList<PwCogsVariantSearchResult> Variants { get; set; }
        
        public PwCogsProductSearchResult()
        {
            Variants = new List<PwCogsVariantSearchResult>();
        }
    }

    public static class PwCogsProductSearchResultExtensions
    {
        public static void PopulateVariants(
            this IList<PwCogsProductSearchResult> input, IList<PwCogsVariantSearchResult> variants)
        {
            foreach (var variant in variants)
            {
                var product = input.First();
                product.Variants.Add(variant);
                variant.Parent = product;
            }
        }
    }
}
