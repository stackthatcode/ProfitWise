using System.Collections.Generic;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    // TODO - this is an entirely unnecessary structure - do away with it!
    public class CogsProductGridModel
    {
        public long MasterProductId { get; set; }
        public string Vendor { get; set; }
        public string ProductTitle { get; set; }
        public int InventoryCount { get; set; }
        public int VariantCount { get; set; }
        public MoneyRange Price { get; set; }

        public int NormalizedCurrencyId { get; set; }
        public bool HasNormalizedCogs { get; set; }
        public bool HasPercentages { get; set; }
        public decimal? HighNormalizedCogs { get; set; }
        public decimal? LowNormalizedCogs { get; set; }
        public decimal? HighPercentage { get; set; }
        public decimal? LowPercentage { get; set; }

        public int StockedDirectlyCount { get; set; }
        public int ExcludedCount { get; set; }
        public bool HasCogsDetail { get; set; }
    }

    public static class CogsGridModelExtension
    {
        public static IList<CogsProductGridModel> 
                    ToCogsGridModel(this IList<PwCogsProductSummary> products, int currencyId)
        {
            var output = new List<CogsProductGridModel>();
            foreach (var product in products)
            {
                output.Add(new CogsProductGridModel()
                {
                    MasterProductId = product.PwMasterProductId,
                    ProductTitle = product.Title,
                    Vendor = product.Vendor,
                    Price = new MoneyRange
                    {
                        CurrencyId = currencyId,
                        AmountLow = product.LowPrice,
                        AmountHigh = product.HighPrice,
                        IncludesAverages = false,
                    },
                    HasCogsDetail = product.HasCogsDetail,
                    NormalizedCurrencyId = product.NormalizedCurrencyId,
                    HasNormalizedCogs = product.HasNormalizedCogs,
                    HasPercentages = product.HasPercentages,
                    HighNormalizedCogs = product.HighNormalizedCogs,
                    LowNormalizedCogs = product.LowNormalizedCogs,
                    HighPercentage = product.HighPercentage,
                    LowPercentage = product.LowPercentage,

                    ExcludedCount = product.Excluded,
                    InventoryCount = product.TotalInventory,
                    StockedDirectlyCount = product.StockedDirectly,
                    VariantCount = product.NumberOfVariants
                });
            }
            return output;
        }
    }
}

