using Newtonsoft.Json;

namespace Push.Shopify.Model
{
    public class Shop
    {
        public Shop()
        {            
        }

        public Shop(string serializedJson)
        {
            dynamic parent = JsonConvert.DeserializeObject(serializedJson);
            this.Id = parent.shop.id;
            this.Currency = parent.shop.currency;
            this.TimeZone = parent.shop.timezone;
        }

        public long Id { get; set; }
        public string Currency { get; set; }
        public string TimeZone { get; set; }
    }
}
