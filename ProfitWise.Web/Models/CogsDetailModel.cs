using System;
using System.Collections.Generic;
using ProfitWise.Data.Model.Catalog;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    public class CogsDetailModel
    {
        public long? MasterVariantId { get; set; }
        public DateTime DateDefault { get; set; }
        public PwCogsDetail Defaults { get; set; }
        public List<PwCogsDetail> Details { get; set; }
    }
}
