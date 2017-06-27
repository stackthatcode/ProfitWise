using System.Collections.Generic;

namespace ProfitWise.Data.Model.Shop
{
    // Tranlsates between a "normalized" form and the denormalized table
    public class TourIdentifiers
    {
        public const int ShowPreferences = 1;
        public const int ShowProducts = 2;
        public const int ShowProductDetails = 3;
        public const int ShowProductConsolidationOne = 4;
        public const int ShowProductConsolidationTwo = 5;
        public const int ShowProfitabilityDashboard = 6;
        public const int ShowEditFilters = 7;
        public const int ShowProfitabilityDetail = 8;
        public const int ShowGoodsOnHand = 9;
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
                { TourIdentifiers.ShowProductConsolidationOne, tourState.ShowProductConsolidationOne },
                { TourIdentifiers.ShowProductConsolidationTwo, tourState.ShowProductConsolidationTwo },
                { TourIdentifiers.ShowProfitabilityDashboard, tourState.ShowProfitabilityDashboard },
                { TourIdentifiers.ShowEditFilters, tourState.ShowEditFilters },
                { TourIdentifiers.ShowProfitabilityDetail, tourState.ShowProfitabilityDetail },
                { TourIdentifiers.ShowGoodsOnHand, tourState.ShowGoodsOnHand },
            };

            return output;
        }
    }
}
