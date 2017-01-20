using System.Collections.Generic;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    public class CogsProductGridModel
    {
        public long MasterProductId { get; set; }
        public string Vendor { get; set; }
        public string ProductTitle { get; set; }
        public int InventoryCount { get; set; }
        public int VariantCount { get; set; }
        public MoneyRange Price { get; set; }
        public MoneyRange Cogs { get; set; }
        public int StockedDirectlyCount { get; set; }
        public int ExcludedCount { get; set; }
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
                    Cogs = new MoneyRange
                    {
                        CurrencyId = currencyId,
                        AmountLow = product.LowNormalizedCogs,
                        AmountHigh = product.HighNormalizedCogs,
                        IncludesAverages = product.HasCogsDetail,
                    },
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

