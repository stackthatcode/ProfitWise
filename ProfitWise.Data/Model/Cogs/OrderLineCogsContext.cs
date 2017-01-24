using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.ShopifyImport;

namespace ProfitWise.Data.Model.Cogs
{

    // Largely a pass-thru context with some additional properties that will be used to for the SQL update
    public class OrderLineCogsContext
    {
        public PwCogsDetail Cogs { get; set; }

        public long PwShopId => Cogs.PwShopId;
        public long? PwMasterVariantId => Cogs.PwMasterVariantId;
        public long? PwMasterProductId { get; set; }
        public long? PwPickListId { get; set; }

        public int CogsTypeId => Cogs.CogsTypeId;
        public int? CogsCurrencyId => Cogs.CogsCurrencyId;
        public decimal? CogsAmount => Cogs.CogsAmount;
        public decimal CogsPercentOfUnitPrice => Cogs.CogsPercentOfUnitPrice;

        public int DestinationCurrencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public static class OrderLineUpdateContextExtensions
    {
        public static OrderLineCogsContext SelectContextByDate(this IList<OrderLineCogsContext> contexts, DateTime orderDate)
        {
            return contexts.FirstOrDefault(x => x.StartDate <= orderDate && x.EndDate > orderDate);
        }
    }
}
