using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Services;


namespace ProfitWise.Data.Model.Cogs
{
    public class PwCogsProductSummary
    {
        public long PwMasterProductId { get; set; }
        public long PwProductId { get; set; }
        public string Title { get; set; }
        public string ProductTitle => Title;

        public string Vendor { get; set; }


        [JsonIgnore]
        public IList<PwCogsVariant> Variants { get; set; }

        [JsonIgnore]
        public IList<PwCogsVariant> PrimaryVariants => Variants.Where(x => x.IsPrimary).ToList();

        public int VariantCount => PrimaryVariants.Count();

        public bool HasNormalizedCogs
        {
            get { return Variants.Any(x => x.CogsTypeId == CogsType.FixedAmount); }
        }

        public bool HasPercentages
        {
            get { return Variants.Any(x => x.CogsTypeId == CogsType.MarginPercentage); }
        }

        public bool HasCogsDetail
        {
            get { return Variants.Any(x => x.CogsDetail != null && x.CogsDetail.Value == true); }
        }

        public bool HasInventory
        {
            get { return Variants.Any(x => x.Inventory != null); }
        }
        
        public int NormalizedCurrencyId { get; set; }

        public decimal? HighNormalizedCogs
        {
            get
            {
                return HasNormalizedCogs ?
                        Variants.Where(x => x.CogsTypeId == CogsType.FixedAmount)
                                .Max(x => x.NormalizedCogsAmount) : null;
            }
        }
        public decimal? LowNormalizedCogs
        {
            get
            {
                return HasNormalizedCogs ?
                        Variants.Where(x => x.CogsTypeId == CogsType.FixedAmount)
                                .Min(x => x.NormalizedCogsAmount) : null;
            }
        }

        public decimal? HighPercentage
        {
            get
            {
                return HasPercentages ?
                        Variants.Where(x => x.CogsTypeId == CogsType.MarginPercentage)
                            .Max(x => x.CogsMarginPercent) : null;
            }
        }
        public decimal? LowPercentage
        {
            get
            {
                return HasPercentages ?
                        Variants.Where(x => x.CogsTypeId == CogsType.MarginPercentage)
                            .Min(x => x.CogsMarginPercent) : null;
            }
        }

        public decimal? HighPrice
        {
            get { return Variants.Any() ? Variants.Max(x => x.HighPrice) : 0; }
        }
        public decimal? LowPrice
        {
            get { return Variants.Any() ? Variants.Min(x => x.LowPrice) : 0; }
        }

        public MoneyRange Price => new MoneyRange()
        {
            CurrencyId = NormalizedCurrencyId,
            AmountHigh = HighPrice,
            AmountLow = LowPrice,
            IncludesAverages = false,
        };

        public int TotalInventory
        {
            get
            {
                return Variants
                        .Where(x => x.Inventory.HasValue)
                        .Sum(x => x.Inventory.Value);
            }
        }
        
        public bool IsPriceRange => HighPrice != LowPrice;

        public int StockedDirectlyCount
        {
            get
            {
                return PrimaryVariants.Count(x => x.StockedDirectly);
            }
        }

        public int ExcludedCount
        {
            get
            {
                return PrimaryVariants.Count(x => x.Exclude);
            }
        }

        public PwCogsProductSummary()
        {
            Variants = new List<PwCogsVariant>();
        }

        public PwCogsProductSummary 
                    PopulateNormalizedCogsAmount(CurrencyService currencyService, int shopCurrencyId)
        {
            this.NormalizedCurrencyId = shopCurrencyId;

            if (currencyService.AllCurrencies().All(x => x.CurrencyId != shopCurrencyId))
            {
                throw new ArgumentException($"shopCurrencyId {shopCurrencyId} is not a valid currency");
            }

            foreach (var variant in this.Variants)
            {
                variant.PopulateNormalizedCogsAmount(currencyService, shopCurrencyId);
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
                this IList<PwCogsProductSummary> products, CurrencyService currencyService, int shopCurrencyId)
        {
            foreach (var product in products)
            {
                product.PopulateNormalizedCogsAmount(currencyService, shopCurrencyId);
            }
        }
    }
}
