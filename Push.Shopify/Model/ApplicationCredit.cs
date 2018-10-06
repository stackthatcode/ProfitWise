namespace Push.Shopify.Model
{
    public class ApplicationCredit
    {
        public long? id { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
        public string test { get; set; }
    }

    public class ApplicationCreditParent
    {
        public ApplicationCredit application_credit { get; set; }
    }
}
