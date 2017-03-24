using Newtonsoft.Json;

namespace Push.Shopify.Model
{
    public class Shop
    {
        public long Id { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string TimeZoneIana { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }


        private Shop(string serializedJson)
        {
            dynamic parent = JsonConvert.DeserializeObject(serializedJson);
            var shop = parent.shop;
            this.Id = shop.id;
            this.Currency = shop.currency;
            this.TimeZone = shop.timezone;
            this.TimeZoneIana = shop.iana_timezone;
            this.Domain = shop.myshopify_domain;
            this.Email = shop.email;
        }

        public static Shop MakeFromJson(string json)
        {
            return new Shop(json);
        }

        public Shop()
        {
        }
    }
}
