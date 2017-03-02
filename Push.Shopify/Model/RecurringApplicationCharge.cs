using System;

namespace Push.Shopify.Model
{
    public class RecurringApplicationCharge
    {
        public string id { get; set; }
        public string name { get; set; }
        public string api_client_id { get; set; }
        public decimal price { get; set; }
        public string status { get; set; }
        public string return_url { get; set; }
        public DateTime? billing_on { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        public string test { get; set; }
        public DateTime? activated_on { get; set; }
        public DateTime? trial_ends_on { get; set; }
        public DateTime? cancelled_on { get; set; }

        public int trial_days { get; set; }
        
        public string decorated_return_url { get; set; }
        public string confirmation_url { get; set; }
    }
}

