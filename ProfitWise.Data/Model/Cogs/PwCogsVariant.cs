using System;
using Newtonsoft.Json;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Model.Cogs
{
    public class PwCogsVariant
    {
        public long PwMasterProductId { get; set;  }
        public long PwMasterVariantId { get; set; }
        public string Title { get; set; }

        public string ProductTitle { get; set; }

        public string Sku { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public int CogsTypeId { get; set; }
        public decimal? CogsMarginPercent { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }

        public bool? CogsDetail { get; set; }

        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public int? Inventory { get; set; }


        // This needs to be manually populated, by leveraging the Currency Service
        public decimal? NormalizedCogsAmount { get; set; }

        	
        [JsonIgnore]
        public PwCogsProductSummary Parent { get; set; }


        public void PopulateNormalizedCogsAmount(CurrencyService currencyService, int targetCurrencyId)
        {
            if (CogsAmount != null && CogsCurrencyId != null)
            {
                NormalizedCogsAmount =
                    currencyService.Convert(
                        CogsAmount.Value, CogsCurrencyId.Value, targetCurrencyId, DateTime.Now);
            }
        }
    }
}

