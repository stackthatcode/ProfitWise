namespace Push.Shopify.Model
{
    public class Transaction
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }

        public bool IsSuccess => Status == "success";
    }
}
