namespace ProfitWise.Web.Models
{
    public class SearchResultSelection
    {
        public long PickListId { get;set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int SortByColumn { get; set; }
        public bool SortByDirectionDown { get; set; }

    }
}