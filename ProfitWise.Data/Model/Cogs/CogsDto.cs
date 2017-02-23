using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Services;

namespace ProfitWise.Data.Model.Cogs
{
    // Stand-alone object exists for the transport of CoGS data entry to other object context(s)
    // ... and for computations
    public class CogsDto
    {
        public int CogsTypeId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public decimal? CogsMarginPercent { get; set; }
        public DateTime CogsDate { get; set; } // Don't like that this may not be nullable

        public decimal CogsPercentOfUnitPrice => 100m - CogsMarginPercent ?? 0m;

        public PwCogsDetail ToPwCogsDetail(long? pwMasterVariantId)
        {
            this.ApplyConstraints();

            var output = new PwCogsDetail
            {             
                PwMasterVariantId = pwMasterVariantId,
                CogsDate = this.CogsDate,
                CogsTypeId = this.CogsTypeId,
                CogsAmount = this.CogsAmount,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsMarginPercent = this.CogsMarginPercent,
            };

            return output;
        }

        public CogsDto ApplyConstraints()
        {
            CogsAmount = ConstrainAmount(CogsAmount);
            CogsMarginPercent = ConstrainMargin(CogsMarginPercent);

            if (CogsTypeId == CogsType.FixedAmount)
            {
                CogsMarginPercent = null;
            }
            if (CogsTypeId == CogsType.MarginPercentage)
            {
                CogsCurrencyId = null;
                CogsAmount = null;
            }
            return this;
        }

        private decimal? ConstrainMargin(decimal? cogsMarginPercent)
        {
            if (!cogsMarginPercent.HasValue)
            {
                return null;
            }
            if (cogsMarginPercent < 0m)
            {
                return 0m;
            }
            if (cogsMarginPercent > 100m)
            {
                return 100m;
            }
            return cogsMarginPercent;
        }

        private decimal? ConstrainAmount(decimal? cogsAmount)
        {
            if (!cogsAmount.HasValue)
            {
                return null;
            }
            if (cogsAmount < 0m)
            {
                return 0m;
            }
            if (cogsAmount > 999999999.99m)
            {
                return 999999999.99m;
            }
            return cogsAmount;
        }


        public CogsDto ValidateType()
        {
            if (CogsTypeId != CogsType.FixedAmount && CogsTypeId != CogsType.MarginPercentage)
            {
                throw new ArgumentException("Invalid CogsTypeId");
            }
            return this;
        }

        public void ValidateCurrency(CurrencyService service)
        {
            if (CogsTypeId != CogsType.FixedAmount) return;

            if (!CogsCurrencyId.HasValue || !service.CurrencyExists(CogsCurrencyId.Value))
            {
                throw new Exception($"Unable to locate Currency {CogsCurrencyId}");
            }
        }
    }

    public static class CogsDtoExtensions
    {
        public static CogsDto FirstDetail(this IList<CogsDto> input)
        {
            return input.OrderBy(x => x.CogsDate).FirstOrDefault();
        }

        public static CogsDto LastDetail(this IList<CogsDto> input)
        {
            return input.OrderByDescending(x => x.CogsDate).FirstOrDefault();
        }

        public static IEnumerable<CogsDto> DetailsAfter(this IList<CogsDto> input, DateTime date)
        {
            return input.Where(x => x.CogsDate > date);
        }

        public static CogsDto NextDetail(this IList<CogsDto> input, CogsDto detail)
        {
            return input.DetailsAfter(detail.CogsDate).OrderBy(x => x.CogsDate).FirstOrDefault();
        }
    }
}

