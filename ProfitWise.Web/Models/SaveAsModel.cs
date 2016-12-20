using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Web.Models
{
    public class SaveAsModel
    {
        public PwReport Current { get; set; }
        public PwReport Original { get; set; }
    }
}