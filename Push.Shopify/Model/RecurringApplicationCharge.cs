using System;

namespace Push.Shopify.Model
{
    public class RecurringApplicationCharge
    {
        public static RecurringApplicationCharge FromDynamic(dynamic input)
        {
            return new RecurringApplicationCharge()
            {
                id = input.id,
                name = input.name,
                trial_days = input.trial_days,
                activated_on = input.activated_on,
                api_client_id = input.api_client_id,
                billing_on = input.billing_on,
                cancelled_on = input.cancelled_on,
                confirmation_url = input.confirmation_url,
                created_at = input.created_at,
                decorated_return_url = input.decorated_return_url,
                price = input.price,
                test = input.test,
                return_url = input.return_url,
                status = input.status,
                trial_ends_on = input.trial_ends_on,
                updated_at = input.updated_at,
            };
        }

        public long id { get; set; }
        public string name { get; set; }
        public string api_client_id { get; set; }
        public decimal price { get; set; }
        public string status { get; set; }

        public string return_url { get; set; }
        public DateTime? billing_on { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }

        public bool? test { get; set; }
        public DateTime? activated_on { get; set; }
        public DateTime? trial_ends_on { get; set; }
        public DateTime? cancelled_on { get; set; }

        public int trial_days { get; set; }
        
        public string decorated_return_url { get; set; }
        public string confirmation_url { get; set; }
    }
}

