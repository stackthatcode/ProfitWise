﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push.Shopify.Model
{
    public class Order
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public IList<OrderLineItem> LineItems { get; set; }
    }
}
