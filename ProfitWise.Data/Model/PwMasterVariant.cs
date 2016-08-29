﻿using System.Collections.Generic;

namespace ProfitWise.Data.Model
{
    public class PwMasterVariant
    {
        public long PwShopId { get; set; }
        public long PwMasterVariantId { get; set; }
        public long PwProductId { get; set; }
        public bool Exclude { get; set; }
        public bool StockedDirectly { get; set; }

        public IList<PwVariant> Variants { get; set; }
    }

}
