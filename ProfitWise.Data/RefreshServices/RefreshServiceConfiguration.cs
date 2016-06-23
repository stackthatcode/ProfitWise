namespace ProfitWise.Batch.RefreshServices
{
    public class RefreshServiceConfiguration
    {
        public RefreshServiceConfiguration()
        {
            MaxOrderRate = 50;
            MaxProduceRate = 100;
        }

        public int MaxOrderRate { get; set; }
        public int MaxProduceRate { get; set; }
    }
}
