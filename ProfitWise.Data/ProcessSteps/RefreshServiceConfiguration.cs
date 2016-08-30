namespace ProfitWise.Data.ProcessSteps
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
