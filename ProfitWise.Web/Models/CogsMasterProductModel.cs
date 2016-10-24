using System;
using System.Collections.Generic;
using ProfitWise.Data.Model;


namespace ProfitWise.Web.Models
{
    public class CogsMasterProductModel
    {
        public string Title { get; set; }
        public long MasterProductId { get; set; }
        public List<CogsMasterVariantModel> MasterVariants { get; set; }
    }

    public class CogsMasterVariantModel
    {
        public long MasterVariantId { get; set; }
        public string Title { get; set; }
        public string Sku { get; set; }
        public int Inventory { get; set; }

        public MoneyRange Price { get; set; }

        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }

        public bool StockedDirectly { get; set; }
        public bool Excluded { get; set; }

        public static CogsMasterVariantModel Build(PwCogsVariant variant, int shopCurrencyId)
        {
            return new CogsMasterVariantModel()
            {
                MasterVariantId = variant.PwMasterVariantId,
                Title = variant.Title,
                CogsAmount = variant.CogsAmount,
                CogsCurrencyId = variant.CogsCurrencyId ?? shopCurrencyId,
                Inventory = variant.Inventory ?? 0,

                Price = new MoneyRange()
                {
                    CurrencyId = shopCurrencyId,
                    AmountHigh = variant.HighPrice,
                    AmountLow = variant.LowPrice,
                    IncludesAverages = false,
                },
                Sku = variant.Sku,
                Excluded = variant.Exclude,
                StockedDirectly = variant.StockedDirectly,
            };
        }

    }
}
