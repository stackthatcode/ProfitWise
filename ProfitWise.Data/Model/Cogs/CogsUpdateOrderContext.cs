using System;
using System.Collections.Generic;
using ProfitWise.Data.Model.Shop;

namespace ProfitWise.Data.Model.Cogs
{
    public class CogsUpdateOrderContext
    {
        public PwShop PwShop { get; set; }

        public int PwShopId => PwShop.PwShopId;
        public int DestinationCurrencyId => PwShop.CurrencyId;

        public long? PwMasterProductId { get; set; }
        public long? PwMasterVariantId { get; set; }
        public PwCogsDetail Cogs { get; set; }
       
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
