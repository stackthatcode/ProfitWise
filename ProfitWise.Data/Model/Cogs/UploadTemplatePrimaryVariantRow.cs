﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfitWise.Data.Model.Cogs
{
    public class UploadTemplatePrimaryVariantRow
    {
        public long PwMasterVariantId { get; set; }
        public string ProductTitle { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set; }
        public decimal LowPrice { get; set; }
        public decimal HighPrice { get; set; }

        public decimal CurrentUnitPrice { get; set; }
        public decimal MarginPercent { get; set; }
        public decimal FixedAmount { get; set; }
        public string Abbreviation { get; set; }
    }
}
