using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ProfitWise.Data.Services;
using Push.Foundation.Utilities.Helpers;

namespace ProfitWise.Data.Model.Cogs
{
    public class PwCogsVariantSummary
    {
        public long PwMasterProductId { get; set;  }
        public long PwMasterVariantId { get; set; }
        public string Title { get; set; }

        public string ProductTitle { get; set; }
        public string CombinedTitle => Title + " (" + ProductTitle + ")";
        
        public string Sku { get; set; }
        public string SkuCorrected => Sku.IsNullOrEmpty() ? "(No SKU)" : Sku;
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        // These come straight from Master Variant
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

        // TODO => these need to be populated by the database
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }

        public int? Inventory { get; set; }
        public string InventoryText =>
            Inventory.HasValue ? (Inventory.Value + " in-stock") : "Inactive - no inventory";

        // This is manually populated, by leveraging the Currency Service
        public decimal? NormalizedCogsAmount { get; set; }
        
        [JsonIgnore]
        public PwCogsProductSummary Parent { get; set; }

        [JsonIgnore]
        public IList<PwCogsDetail> CogsDetails { get; set; }

        public DateTime DateToday { get; set; }

        [JsonIgnore]
        public PwCogsDetail MostRecentCogsDetail
        {
            get
            {
                return CogsDetails?.Where(x => x.CogsDate <= DateToday)
                    .OrderByDescending(x => x.CogsDate)
                    .FirstOrDefault();
            }
        }

        public PwCogsDetail ActiveCostOfGoods
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

            if (ActiveCostOfGoods.CogsAmount.HasValue && ActiveCostOfGoods.CogsCurrencyId.HasValue)
            {
                NormalizedCogsAmount = currencyService.Convert(
                    ActiveCostOfGoods.CogsAmount.Value, ActiveCostOfGoods.CogsCurrencyId.Value, 
                    shopCurrencyId, DateTime.Now);
            }
        }
    }
}

