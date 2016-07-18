namespace Push.Shopify.Model
{
    public class Event
    {
        public long Id { get; set; }
        public long SubjectId { get; set; }
        public string SubjectType { get; set; }
        public string Verb { get; set; }
    }


    public class EventTypes
    {
        public const string Product = "Product";
        public const string Order = "Order";
    }

    public class EventVerbs
    {
        public const string Destroy = "destroy";
    }
}
