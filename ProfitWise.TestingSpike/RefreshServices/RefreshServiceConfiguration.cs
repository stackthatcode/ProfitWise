namespace ProfitWise.Batch.RefreshServices
{
    public class RefreshServiceConfiguration
    {
        public RefreshServiceConfiguration()
        {
            RefreshServiceMaxOrderRate = 50;
            RefreshServiceMaxProduceRate = 100;
        }

        public int RefreshServiceMaxOrderRate { get; set; }
        public int RefreshServiceMaxProduceRate { get; set; }
    }
}
