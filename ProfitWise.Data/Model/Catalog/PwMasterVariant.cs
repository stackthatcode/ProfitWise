using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwMasterVariant
    {
        public PwMasterVariant()
        {
            CogsTypeId = CogsType.FixedAmount;
            CogsDetails = new List<PwCogsDetail>();
            Variants = new List<PwVariant>();
        }

        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }
        public PwMasterProduct ParentMasterProduct { get; set; }

        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public int? CogsTypeId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public decimal? CogsMarginPercent { get; set; }

        public bool CogsDetail { get; set; }
        public IList<PwCogsDetail> CogsDetails { get; set; }

        public IList<PwVariant> Variants { get; set; }

        public PwVariant DeterminePrimaryVariant()
        {
            if (Variants.Count(x => x.IsPrimary) > 1 ||
                Variants.Count(x => x.IsPrimaryManual) > 1)
            {
                var msg = $"Inconsistent data - Master Variant {PwMasterVariantId} " +
                            $"has more than one Primary / PrimaryManual Variant";
                throw new Exception(msg);
            }

            var manualPrimaryVariant = Variants.FirstOrDefault(x => x.IsPrimaryManual);
            if (manualPrimaryVariant != null)
            {
                return manualPrimaryVariant;
            }

            var activeVariant = Variants.Where(x => x.IsActive).ToList();
            if (activeVariant.Count == 1)
            {
                return activeVariant.First();
            }

            return Variants.OrderByDescending(x => x.LastUpdated).First();
        }
    }
}

