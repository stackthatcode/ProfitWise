using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.System;

namespace ProfitWise.Data.Model.Cogs
{
    public class CogsDateBlockContext
    {
        public static readonly DateTime MinimumCogsDate = new DateTime(2000, 1, 1);
        public static readonly DateTime MaximumCogsDate = new DateTime(2099, 12, 31);


        public CogsDateBlockContext()
        {
            StartDate = MinimumCogsDate;
            EndDate = MaximumCogsDate;
        }

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


        public CalcContext ToCalcContext()
        {
            return new CalcContext
            {
                FixedAmount = CogsTypeId == CogsType.FixedAmount && CogsAmount.HasValue ? CogsAmount.Value : 0,
                PercentMultiplier = CogsTypeId == CogsType.MarginPercentage ? CogsPercentOfUnitPrice : 0m,
                SourceCurrencyId = CogsCurrencyId ?? Currency.DefaultCurrencyId,
            };
        }

        public static CogsDateBlockContext Make(CogsDto defaults, int destinationCurrency, long? pickListId)
        {
            return new CogsDateBlockContext
            {
                Cogs = defaults,
                StartDate = MinimumCogsDate,
                EndDate = MaximumCogsDate,
                DestinationCurrencyId = destinationCurrency,
                PwPickListId = pickListId,
            };
        }

        public static IList<CogsDateBlockContext> Make(PwMasterVariant masterVariant, int destinationCurrency)
        {
            var defaults = new CogsDto()
            {
                CogsCurrencyId = masterVariant.CogsCurrencyId,
                CogsAmount = masterVariant.CogsAmount,
                CogsMarginPercent = masterVariant.CogsMarginPercent,
                CogsTypeId = masterVariant.CogsTypeId,
            };

            var details = masterVariant.CogsDetails.Select(x => x.ToCogsDto()).ToList();
            return Make(defaults, details, destinationCurrency, null, masterVariant.PwMasterVariantId);
        }

        public static IList<CogsDateBlockContext> Make(
                    CogsDto defaults, IList<CogsDto> details, int destinationCurrency, 
                    long? pwMasterProductId = null, long? pwMasterVariantId = null, long? pwPickListId = null)
        {
            if (details == null || details.Count == 0)
            {
                return new List<CogsDateBlockContext> {
                    new CogsDateBlockContext
                    {
                        Cogs = defaults,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = MinimumCogsDate,
                        EndDate = MaximumCogsDate,
                        PwMasterProductId = pwMasterProductId,
                        PwMasterVariantId = pwMasterVariantId,
                        PwPickListId = pwPickListId,
                    }
                };
            }
            else
            {
                var output = new List<CogsDateBlockContext>()
                {
                    new CogsDateBlockContext
                    {
                        Cogs = defaults,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = MinimumCogsDate,
                        EndDate = details.FirstDetail().CogsDate.AddDays(-1),
                        PwMasterProductId = pwMasterProductId,
                        PwMasterVariantId = pwMasterVariantId,
                        PwPickListId = pwPickListId,
                    }
                };

                foreach (var detail in details)
                {
                    var nextDetail = details.NextDetail(detail);
                    output.Add(new CogsDateBlockContext
                    {
                        Cogs = detail,
                        DestinationCurrencyId = destinationCurrency,
                        StartDate = detail.CogsDate,
                        EndDate = nextDetail?.CogsDate.AddDays(-1) ?? MaximumCogsDate,
                        PwMasterProductId = pwMasterProductId,
                        PwMasterVariantId = pwMasterVariantId,
                        PwPickListId = pwPickListId,
                    });
                }

                return output;
            }
        }
    }

    public static class OrderLineUpdateContextExtensions
    {
        public static CogsDateBlockContext SelectContextByDate(
                this IList<CogsDateBlockContext> contexts, DateTime orderDate)
        {
            return contexts.FirstOrDefault(x => x.StartDate <= orderDate && x.EndDate > orderDate);
        }
    }
}
