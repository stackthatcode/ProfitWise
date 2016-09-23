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

        public decimal? HighNormalizedCogs
        {
            get
            {
                if (Variants.All(x => x.NormalizedCogsAmount == null))
                {
                    return null;
                }
                else
                {
                    return Variants
                            .Where(x => x.NormalizedCogsAmount != null)
                            .Max(x => x.NormalizedCogsAmount);
                }
            }
        }

        public decimal? LowNormalizedCogs
        {
            get
            {
                if (Variants.All(x => x.NormalizedCogsAmount == null))
                {
                    return null;
                }
                else
                {
                    return Variants
                            .Where(x => x.NormalizedCogsAmount != null)
                            .Min(x => x.NormalizedCogsAmount);
                }
            }
        }

        public bool HasCogsDetail
        {
            get { return Variants.Any(x => x.CogsDetail != null && x.CogsDetail.Value == true); }            
        }

        public bool IsCogsRange
        {
            get
            {
                if (HighNormalizedCogs == null || LowNormalizedCogs == null)
                    return false;

                return HighNormalizedCogs.Value != LowNormalizedCogs.Value;
            }
        }

        public bool HasCogs
        {
            get { return Variants.Any(x => x.NormalizedCogsAmount != null); }
        }


        public bool HasInventory
        {
            get { return Variants.Any(x => x.Inventory != null); }
        }

        public int TotalInventory
        {
            get
            {
                return Variants
                        .Where(x => x.Inventory.HasValue)
                        .Sum(x => x.Inventory.Value);
            }
        }


        public decimal? HighPrice
        {
            get
            {
                return Variants.Max(x => x.HighPrice);
            }
        }

        public decimal? LowPrice
        {
            get { return Variants.Min(x => x.LowPrice); }
        }

        public bool IsPriceRange => HighPrice != LowPrice;


        public int StockedDirectly
        {
            get
            {
                return Variants.Count(x => x.StockedDirectly);
            }
        }

        public int Excluded
        {
            get
            {
                return Variants.Count(x => x.Exclude);
            }
        }



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
