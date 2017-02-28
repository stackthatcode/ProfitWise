namespace ProfitWise.Data.Model.Admin
{
    public class ProfitWiseUser
    {
        public string UserId { get; set; } 
        public string UserName { get; set; }
        public string Email { get; set; }
        public string TimeZone { get; set; }
        public string Domain { get; set; }
        public int CurrencyId { get; set; }
        public long PwShopId { get; set; }

        // Set downstream...
        public string CurrencyText { get; set; }
    }
}
