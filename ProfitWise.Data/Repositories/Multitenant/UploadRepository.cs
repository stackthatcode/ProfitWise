using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs;
using ProfitWise.Data.Model.Shop;


namespace ProfitWise.Data.Repositories.Multitenant
{
    public class UploadRepository
    {
        public PwShop PwShop { get; set; }
        public long PwShopId => PwShop.PwShopId;
        private readonly ConnectionWrapper _connectionWrapper;

        public UploadRepository(ConnectionWrapper connectionWrapper)
        {
            _connectionWrapper = connectionWrapper;
        }

        public void Insert(Upload upload)
        {
            upload.PwShopId = PwShopId;
            upload.DateCreated = DateTime.UtcNow;
            upload.LastUpdated = DateTime.UtcNow;
            var query =
                @"INSERT INTO uploads(@PwShopId) VALUES ( 
                    @PwShopId, @FileLockerId, @UploadFileName, @FeedbackFileName, @UploadStatus,
                    @DateCreated, @LastUpdated, @TotalNumberOfRows, @RowsProcessed );";
            _connectionWrapper.Execute(query, upload);
        }

        public void UpdateStatus(long fileUploadId, int status)
        {
            var query = @"UPDATE uploads(@PwShopId) 
                        SET UploadStatus = @status,
                        LastUpdated = GETUTCNOW()
                        WHERE FileUploadId = @fileUploadId";

            _connectionWrapper.Execute(query, new { PwShopId, fileUploadId, status });
        }
        public void UpdateFeedbackFilename(long fileUploadId, string feedbackFilename)
        {
            var query = @"UPDATE uploads(@PwShopId) 
                        SET FeedbackFilename = @feedbackFilename,
                        LastUpdated = GETUTCNOW()
                        WHERE FileUploadId = @fileUploadId";

            _connectionWrapper.Execute(query, new { PwShopId, fileUploadId, feedbackFilename });
        }

        public List<Upload> RetrieveByStatus(int status)
        {
            var query = @"SELECT * FROM uploads(@PwShopId) WHERE UploadStatus = @status";

            return _connectionWrapper
                .Query<Upload>(query, new {PwShopId, status})
                .ToList();
        }

        public List<Upload> RetrieveByAgeOfLastUpdate(int maximumAgeHours, int? status = null)
        {
            var query =
                @"SELECT * FROM uploads(@PwShopId) WHERE LastUpdated > DATEADD(hour, -1, GETUTCDATE());";

            if (status.HasValue)
            {
                query += " AND UploadStatus = @status";
            }
            return _connectionWrapper
                    .Query<Upload>(query, new { PwShopId, maximumAgeHours, status })
                    .OrderByDescending(x => x.LastUpdated)
                    .ToList();
        }
    }
}

