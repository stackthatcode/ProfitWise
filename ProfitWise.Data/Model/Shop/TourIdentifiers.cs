using System.Collections.Generic;

namespace ProfitWise.Data.Model.Shop
{
    // Tranlsates between a "normalized" form and the denormalized table
    public class TourIdentifiers
    {
        public const int ShowPreferences = 1;
        public const int ShowProducts = 2;
        public const int ShowProductDetails = 3;
        public const int ShowProductConsolidation = 4;
        public const int ShowProfitabilityDashboard = 5;
        public const int ShowEditFilters = 6;
        public const int ShowProfitabilityDetail = 7;
        public const int ShowGoodsOnHand = 8;
    }

    public static class TourIdentifierExtensions
    {
        public static Dictionary<int, bool> ToDictionary(this PwTourState tourState)
        {
            var output = new Dictionary<int, bool>
            {
                { TourIdentifiers.ShowPreferences, tourState.ShowPreferences },
                { TourIdentifiers.ShowProducts, tourState.ShowProducts },
                { TourIdentifiers.ShowProductDetails, tourState.ShowProductDetails },
                { TourIdentifiers.ShowProductConsolidation, tourState.ShowProductConsolidation },
                { TourIdentifiers.ShowProfitabilityDashboard, tourState.ShowProfitabilityDashboard },
                { TourIdentifiers.ShowEditFilters, tourState.ShowEditFilters },
                { TourIdentifiers.ShowProfitabilityDetail, tourState.ShowProfitabilityDetail },
                { TourIdentifiers.ShowGoodsOnHand, tourState.ShowGoodsOnHand },
            };

            return output;
        }
    }
}
