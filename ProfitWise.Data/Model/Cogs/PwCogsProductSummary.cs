using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;


namespace ProfitWise.Data.Model.Cogs
{
    public class PwCogsProductSummary
    {
        public long PwMasterProductId { get; set; }
        public long PwProductId { get; set; }
        public string Title { get; set; }
        public string ProductTitle => Title;

        public string Vendor { get; set; }
        public string VendorText => Vendor.IsNullOrEmptyAlt("(No Vendor)");

        [JsonIgnore]
        public IList<PwCogsVariantSummary> Variants { get; set; }

        public int VariantCount => Variants.Count();

        [JsonIgnore]
        public IList<PwCogsDetail> ActiveCostOfGoods => Variants.Select(x => x.ActiveCostOfGoods).ToList();

        [JsonIgnore]
        public IList<PwCogsVariantSummary> VariantsWithNormalizedCogs
                        => Variants.Where(x => x.NormalizedCogsAmount.HasValue).ToList();
        
        public bool HasNormalizedCogs => VariantsWithNormalizedCogs.Any();

        public bool HasPercentages => ActiveCostOfGoods.Any(x => x.CogsTypeId == CogsType.MarginPercentage);         

        public bool HasCogsDetail
        {
            get { return Variants.Any(x => x.CogsDetail != null && x.CogsDetail.Value); }
        }

        public bool HasInventory
        {
            get { return Variants.Any(x => x.Inventory != null); }
        }
        
        public int NormalizedCurrencyId { get; set; }

        // This is necessary as multiple currencies may have been used
        public decimal? HighNormalizedCogs => 
                HasNormalizedCogs ? VariantsWithNormalizedCogs.Max(x => x.NormalizedCogsAmount) : null;

        public decimal? LowNormalizedCogs => 
                HasNormalizedCogs ? VariantsWithNormalizedCogs.Min(x => x.NormalizedCogsAmount) : null;


        public decimal? HighPercentage
        {
            get
            {
                return HasPercentages ?
                        ActiveCostOfGoods.Where(x => x.CogsTypeId == CogsType.MarginPercentage)
                            .Max(x => x.CogsMarginPercent) : null;
            }
        }

        public decimal? LowPercentage
        {
            get
            {
                return HasPercentages ?
                        ActiveCostOfGoods.Where(x => x.CogsTypeId == CogsType.MarginPercentage)
                            .Min(x => x.CogsMarginPercent) : null;
            }
        }

        public decimal? HighPrice =>  Variants.Any() ? Variants.Max(x => x.HighPrice) : 0; 
        
        public decimal? LowPrice =>  Variants.Any() ? Variants.Min(x => x.LowPrice) : 0; 

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

        public int StockedDirectlyCount =>  Variants.Count(x => x.StockedDirectly);

        public int ExcludedCount =>  Variants.Count(x => x.Exclude);

        public PwCogsProductSummary()
        {
            Variants = new List<PwCogsVariantSummary>();
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
                this IList<PwCogsProductSummary> products, IList<PwCogsVariantSummary> variants)
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
