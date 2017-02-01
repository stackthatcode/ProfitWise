using Newtonsoft.Json;

namespace Push.Shopify.Model
{
    public class Shop
    {
        public long Id { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
        public string Domain { get; set; }
        
        public Shop(string serializedJson)
        {
            dynamic parent = JsonConvert.DeserializeObject(serializedJson);
            this.Id = parent.shop.id;
            this.Currency = parent.shop.currency;
            this.TimeZone = parent.shop.timezone;
            this.Domain = parent.shop.myshopify_domain;
        }

        public Shop()
        {
        }
    }
}
