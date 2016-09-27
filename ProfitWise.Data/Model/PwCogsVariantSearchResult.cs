using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Model
{
    public class PwCogsVariantSearchResult
    {
        public long PwMasterProductId { get; set;  }
        public long PwMasterVariantId { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public bool? CogsDetail { get; set; }

        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }
        public int? Inventory { get; set; }

        // This needs to be manually populated, thus leveraging the Currency Service
        public decimal? NormalizedCogsAmount { get; set; }

        	
        [JsonIgnore]
        public PwCogsProductSearchResult Parent { get; set; }


        public void PopulateNormalizedCogsAmount(
                        CurrencyService currencyService, int targetCurrencyId)
        {
            if (CogsAmount != null && CogsCurrencyId != null)
            {
                NormalizedCogsAmount =
                    currencyService.Convert(
                        CogsAmount.Value, CogsCurrencyId.Value, targetCurrencyId, DateTime.Now);
            }
        }
    }

    public static class SearchResultExtension
    {
        public static IList<PwCogsVariantSearchResult> PopulateNormalizedCogsAmount(
                    this IList<PwCogsVariantSearchResult> searchResults,
                    CurrencyService currencyService, int targetCurrencyId)
        {
            foreach (var result in searchResults)
            {
                result.PopulateNormalizedCogsAmount(currencyService, targetCurrencyId);
            }
            return searchResults;
        }
    }
}
