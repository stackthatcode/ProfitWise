﻿using System.Collections.Generic;
using ProfitWise.Data.Model;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    public class BulkEditProductCogsModel
    {
        public long PwMasterProductId { get; set; }
        public string Title { get; set; }
        public IList<PwCogsVariantSummary> Variant { get; set; }
        public int CurrencyId { get; set; }
    }
}
