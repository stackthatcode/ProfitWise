﻿namespace ProfitWise.Data.Model.Reports
{
    public class MasterVariantOption
    {
        public long PwMasterProductId { get; set; }
        public string Vendor { get; set; }
        public string ProductTitle { get; set; }
        public long PwMasterVariantId { get; set; }
        public string VariantTitle { get; set; }
        public string Sku { get; set;  }


        public long Key => PwMasterVariantId;
        public string Title => VariantTitle;

    }
}
