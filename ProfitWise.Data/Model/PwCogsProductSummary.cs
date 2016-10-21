using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Model
{
    public class PwCogsProductSummary
    {
        public long PwMasterProductId { get; set; }
        public long PwProductId { get; set; }
        public string Title { get; set; }
        public string Vendor { get; set; }


        [JsonIgnore]
        public IList<PwCogsVariant> Variants { get; set; }

        public int NumberOfVariants => Variants.Count();

        public int NormalizedCurrencyId { get; set; }

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

        public PwCogsProductSummary()
        {
            Variants = new List<PwCogsVariant>();
        }

        public PwCogsProductSummary PopulateNormalizedCogsAmount(CurrencyService currencyService, int targetCurrencyId)
        {
            this.NormalizedCurrencyId = targetCurrencyId;

            if (currencyService.AllCurrencies().All(x => x.CurrencyId != targetCurrencyId))
            {
                throw new ArgumentException($"targetCurrencyId {targetCurrencyId} is not a valid currency");
            }

            foreach (var variant in this.Variants)
            {
                variant.PopulateNormalizedCogsAmount(currencyService, targetCurrencyId);
            }
            return this;
        }
    }

    public static class CogsExtensions
    {
        public static void PopulateVariants(
                this IList<PwCogsProductSummary> products, IList<PwCogsVariant> variants)
        {
            foreach (var variant in variants)
            {
                var product = products.First(x => x.PwMasterProductId == variant.PwMasterProductId);
                product.Variants.Add(variant);
                variant.Parent = product;
            }
        }

        public static void PopulateNormalizedCogsAmount(
                this IList<PwCogsProductSummary> products, CurrencyService currencyService, int targetCurrencyId)
        {
            foreach (var product in products)
            {
                product.PopulateNormalizedCogsAmount(currencyService, targetCurrencyId);
            }
        }
    }
}
