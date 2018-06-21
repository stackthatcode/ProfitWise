using System;

namespace ProfitWise.Data.Model.Cogs.UploadObjects
{
    public class Upload
    {
        public long FileUploadId { get; set; }
        public long PwShopId { get; set; }
        public Guid FileLockerId { get; set; }
        public string UploadFileName { get; set; }
        public string FeedbackFileName { get; set; }
        public int UploadStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdated { get; set; }
        public int TotalNumberOfRows { get; set; }
        public int RowsProcessed { get; set; }
    }
}
