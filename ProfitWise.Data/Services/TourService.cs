using ProfitWise.Data.Factories;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Services
{
    public class TourService
    {
        private readonly MultitenantFactory _multitenantFactory;
        
        public TourService(MultitenantFactory multitenantFactory)
        {
            _multitenantFactory = multitenantFactory;
        }

        public void ShowTour(PwShop shop, int identifier)
        {
            var repository = _multitenantFactory.MakeTourRepository(shop);
            var tour = repository.Retreive();
            bool currentValue = false;

            if (identifier == TourIdentifiers.ShowPreferences)
            {
                tour.ShowPreferences = false;
            }
            if (identifier == TourIdentifiers.ShowProducts)
            {
                tour.ShowProducts = false;
            }
            if (identifier == TourIdentifiers.ShowProductDetails)
            {
                tour.ShowProductDetails = false;
            }
            if (identifier == TourIdentifiers.ShowProductConsolidationOne)
            {
                tour.ShowProductConsolidationOne = false;
            }
            if (identifier == TourIdentifiers.ShowProductConsolidationTwo)
            {
                tour.ShowProductConsolidationTwo = false;
            }
            if (identifier == TourIdentifiers.ShowProfitabilityDashboard)
            {
                tour.ShowProfitabilityDashboard = false;
            }
            if (identifier == TourIdentifiers.ShowEditFilters)
            {
                tour.ShowEditFilters = false;
            }
            if (identifier == TourIdentifiers.ShowProfitabilityDetail)
            {
                tour.ShowProfitabilityDetail = false;
            }
            if (identifier == TourIdentifiers.ShowGoodsOnHand)
            {
                tour.ShowGoodsOnHand = false;
            }

            repository.Update(tour);
        }        
    }
}

