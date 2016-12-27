using Castle.Core.Internal;
using ProfitWise.Data.Model.Reports;

namespace ProfitWise.Data.Model.Profit
{
    public class GroupingKeyAndName
    {

        public ReportGrouping GroupingId { get; set; }
        public string GroupingKey { get; set; }
        public string GroupingName { get; set; }
    }
}
