using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ProfitWise.Data.Model.Cogs;
using Push.Foundation.Utilities.General;

namespace ProfitWise.Data.Model.Catalog
{
    public class PwMasterVariant
    {
        public PwMasterVariant()
        {
            CogsTypeId = CogsType.FixedAmount;
            CogsDetail = false;
            CogsDetails = new List<PwCogsDetail>();
            Variants = new List<PwVariant>();
        }

        public long PwMasterVariantId { get; set; }
        public long PwShopId { get; set; }
        public long PwMasterProductId { get; set; }

        public PwMasterProduct ParentMasterProduct { get; set; }
        public IList<PwCogsDetail> CogsDetails { get; set; }
        public IList<PwVariant> Variants { get; set; }

        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public int CogsTypeId { get; set; }
        public int? CogsCurrencyId { get; set; }
        public decimal? CogsAmount { get; set; }
        public decimal? CogsMarginPercent { get; set; }

        public bool CogsDetail { get; set; }

        public PwMasterVariant Clone()
        {
            var output = new PwMasterVariant
            {
                PwShopId = this.PwShopId,
                PwMasterVariantId = this.PwMasterVariantId,
                PwMasterProductId = this.PwMasterProductId,
                Exclude = this.Exclude,
                StockedDirectly = this.StockedDirectly,
                CogsCurrencyId = this.CogsCurrencyId,
                CogsAmount = this.CogsAmount,
                CogsTypeId = this.CogsTypeId,
                CogsMarginPercent = this.CogsMarginPercent,
                CogsDetail = this.CogsDetail,
            };

            foreach (var detail in this.CogsDetails)
            {
                output.CogsDetails.Add(detail.Clone());
            }

            return output;
        }

        public PwVariant AutoPrimaryVariant()
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

        public void AssignVariant(PwVariant variant)
        {
            variant.ParentMasterVariant?.Variants?.Remove(variant);
            this.Variants.Add(variant);
            variant.PwMasterVariantId = this.PwMasterVariantId;
            variant.IsPrimaryManual = false;
            variant.IsPrimary = false;
            variant.ParentMasterVariant = this;
        }
    }

    public static class PwMasterVariantExtensions
    {
        public static void LoadCogsDetail(
                this IList<PwMasterVariant> masterVariants, IList<PwCogsDetail> details)
        {
            masterVariants
                .Where(x => x.CogsDetail)
                .ForEach(
                    masterVariant =>
                    {
                        masterVariant.CogsDetails
                            = details
                                    .Where(detail => detail.PwMasterVariantId == masterVariant.PwMasterVariantId)
                                    .ToList();
                    });
        }
    }
}

