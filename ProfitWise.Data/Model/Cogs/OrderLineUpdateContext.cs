using System;
using System.Collections.Generic;
using System.Linq;

namespace ProfitWise.Data.Model.Cogs
{
    public class OrderLineUpdateContext
    {
        public static readonly DateTime MinimumCogsDate = new DateTime(2000, 1, 1);
        public static readonly DateTime MaximumCogsDate = new DateTime(2099, 12, 31);


        public long PwShopId { get; set; }

        public CogsDto Cogs { get; set; }

        public long? PwMasterVariantId { get; set; }
        public long? PwMasterProductId { get; set; }
        public long? PwPickListId { get; set; }

        public int CogsTypeId => Cogs.CogsTypeId;
        public int? CogsCurrencyId => Cogs.CogsCurrencyId;
        public decimal? CogsAmount => Cogs.CogsAmount;
        public decimal CogsPercentOfUnitPrice => Cogs.CogsPercentOfUnitPrice;

        public int DestinationCurrencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }



        public static OrderLineUpdateContext Make(CogsDto defaults, int destinationCurrency, long? pickListId)
        {
            return new OrderLineUpdateContext
            {
                Cogs = defaults,
                StartDate = MinimumCogsDate,
                EndDate = MaximumCogsDate,
                DestinationCurrencyId = destinationCurrency,
                PwPickListId = pickListId,
            };
        }

        public static IList<OrderLineUpdateContext> Make(
                    CogsDto defaults, IList<CogsDto> details, int destinationCurrency,
                    long? pwMasterProductId, long? pwMasterVariantId)
        {
            if (details != null && details.Count == 0)
            {
                return new List<OrderLineUpdateContext> {
                    new OrderLineUpdateContext
                    {
                        Cogs = defaults,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = MinimumCogsDate,
                        EndDate = MaximumCogsDate,
                        PwMasterProductId = pwMasterProductId,
                    }
                };
            }
            else
            {
                var output = new List<OrderLineUpdateContext>()
                {
                    new OrderLineUpdateContext
                    {
                        Cogs = defaults,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = MinimumCogsDate,
                        EndDate = details.FirstDetail().CogsDate.AddDays(-1),
                        PwMasterProductId = pwMasterProductId,
                    }
                };

                foreach (var detail in details)
                {
                    var nextDetail = details.NextDetail(detail);
                    output.Add(new OrderLineUpdateContext
                    {
                        Cogs = detail,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = detail.CogsDate,
                        EndDate = nextDetail?.CogsDate.AddDays(-1) ?? MaximumCogsDate,
                        PwMasterProductId = pwMasterProductId,
                    });
                }

                return output;
            }
        }
    }

    public static class OrderLineUpdateContextExtensions
    {
        public static OrderLineUpdateContext SelectContextByDate(
                this IList<OrderLineUpdateContext> contexts, DateTime orderDate)
        {
            return contexts.FirstOrDefault(x => x.StartDate <= orderDate && x.EndDate > orderDate);
        }
    }
}
