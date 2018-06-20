using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Web.Models
{
    public class UploadStatusModel
    {
        public bool IsProcessing { get; set; }

        public bool HasPreviousUpload => PreviousUploadResult != null;
        public UploadResultModel PreviousUploadResult { get; set; }

    }

    public static class UploadStatusExtensions
    {
        public static Upload MostRecentlyUpdated(this List<Upload> input)
        {
            return input
                .OrderByDescending(x => x.LastUpdated)
                .FirstOrDefault();
        }
        public static Upload LastToComplete(this List<Upload> input)
        {
            return input
                .Where(x => x.UploadStatus != UploadStatusCode.Processing)
                .OrderByDescending(x => x.LastUpdated)
                .FirstOrDefault();
        }
    }

    public class UploadResultModel
    {
        public int UploadStatus { get; set; }
        public string LastUpdatedAt { get; set; }
        public string FeedbackFileUrl { get; set; }
        public int RowsProcessed { get; set; }
        public int TotalNumberOfRows { get; set; }
    }
}
