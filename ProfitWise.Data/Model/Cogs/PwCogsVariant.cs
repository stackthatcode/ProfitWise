using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool IsPrimary { get; set; }

        public string Sku { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public int CogsTypeId { get; set; }
        public decimal? CogsMarginPercent { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public bool? CogsDetail { get; set; }

        public MoneyRange Price => new MoneyRange
        {
            CurrencyId = NormalizedCurrencyId,
            AmountHigh = HighPrice,
            AmountLow = LowPrice,
            IncludesAverages = false
        };

        public int NormalizedCurrencyId { get; set; }

        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public int? Inventory { get; set; }        

        // This needs to be manually populated, by leveraging the Currency Service
        public decimal? NormalizedCogsAmount { get; set; }
        	
        [JsonIgnore]
        public PwCogsProductSummary Parent { get; set; }

        [JsonIgnore]
        public IList<PwCogsDetail> CogsDetails { get; set; }

        [JsonIgnore]
        public PwCogsDetail MostRecentCogsDetail
        {
            get
            {
                // *** NOTE - no timezone translation
                return CogsDetails?.Where(x => x.CogsDate <= DateTime.Now)
                    .OrderByDescending(x => x.CogsDate)
                    .FirstOrDefault();
            }
        }

        public PwCogsDetail ActiveDetail
        {
            get
            {
                var defaultCogs = new PwCogsDetail
                {
                    CogsAmount = this.CogsAmount,
                    CogsCurrencyId = this.CogsCurrencyId,
                    CogsMarginPercent = this.CogsMarginPercent,
                    CogsTypeId = this.CogsTypeId,
                };

                if (!CogsDetail.HasValue || !CogsDetail.Value)
                {
                    return defaultCogs;
                }

                var mostRecentCogsDetail = MostRecentCogsDetail;

                if (MostRecentCogsDetail == null)
                {
                    return defaultCogs;
                }
                else
                {
                    return MostRecentCogsDetail;
                }
            }
        }

        public void PopulateCogsDetails(IList<PwCogsDetail> details)
        {
            CogsDetails = new List<PwCogsDetail>();
            foreach (var detail in details.Where(x => x.PwMasterVariantId == this.PwMasterVariantId))
            {
                this.CogsDetails.Add(detail);
            }
        }

        public void PopulateNormalizedCogsAmount(CurrencyService currencyService, int shopCurrencyId)
        {
            this.NormalizedCurrencyId = shopCurrencyId;

            if (CogsAmount != null && CogsCurrencyId != null)
            {
                NormalizedCogsAmount =
                    currencyService.Convert(
                        CogsAmount.Value, CogsCurrencyId.Value, shopCurrencyId, DateTime.Now);
            }
        }
    }
}

