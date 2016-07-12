namespace ProfitWise.Batch.RefreshServices
{
    public class RefreshServiceConfiguration
    {
        public RefreshServiceConfiguration()
        {
            MaxOrderRate = 50;
            MaxProductRate = 100;
        }

        public int MaxOrderRate { get; set; }
        public int MaxProductRate { get; set; }
    }
}
