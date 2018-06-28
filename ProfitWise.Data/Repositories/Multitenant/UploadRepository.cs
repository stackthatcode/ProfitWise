using System;
using System.Collections.Generic;
using System.Linq;
using ProfitWise.Data.Database;
using ProfitWise.Data.Model.Cogs.UploadObjects;
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

        public long Insert(Upload upload)
        {
            upload.PwShopId = PwShopId;
            upload.DateCreated = DateTime.UtcNow;
            upload.LastUpdated = DateTime.UtcNow;
            var query =
                @"INSERT INTO uploads(@PwShopId) VALUES ( 
                    @PwShopId, @FileLockerId, @UploadFileName, @FeedbackFileName, @UploadStatus,
                    @DateCreated, @LastUpdated, @TotalNumberOfRows, @SuccessfulRows );
                SELECT CAST(SCOPE_IDENTITY() as int)";

           return _connectionWrapper.Query<long>(query, upload).Single();
        }

        public void UpdateStatus(long fileUploadId, int status)
        {
            var query = @"UPDATE uploads(@PwShopId) 
                        SET UploadStatus = @status,
                        LastUpdated = GETUTCDATE()
                        WHERE FileUploadId = @fileUploadId";

            _connectionWrapper.Execute(query, new { PwShopId, fileUploadId, status });
        }

        public void UpdateStatus(
                long fileUploadId, int status, int totalNumberOfRows, int successfulRows)
        {
            var query = @"UPDATE uploads(@PwShopId) 
                        SET UploadStatus = @status,
                        LastUpdated = GETUTCDATE(),
                        TotalNumberOfRows = @totalNumberOfRows,
                        SuccessfulRows = @successfulRows
                        WHERE FileUploadId = @fileUploadId";

            _connectionWrapper.Execute(query, new
            {
                PwShopId, fileUploadId, status, totalNumberOfRows, successfulRows,

            });
        }

        public void UpdateFeedbackFilename(long fileUploadId, string feedbackFilename)
        {
            var query = @"UPDATE uploads(@PwShopId) 
                        SET FeedbackFilename = @feedbackFilename,
                            LastUpdated = GETUTCDATE()
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
                @"SELECT * FROM uploads(@PwShopId) 
                WHERE LastUpdated > DATEADD(hour, -1, GETUTCDATE());";

            if (status.HasValue)
            {
                query += " AND UploadStatus = @status";
            }
            return _connectionWrapper
                    .Query<Upload>(query, new { PwShopId, maximumAgeHours, status })
                    .OrderByDescending(x => x.LastUpdated)
                    .ToList();
        }

        public Upload Retrieve(long fileUploadId)
        {
            var query = 
                @"SELECT * FROM uploads(@PwShopId) WHERE FileUploadId = @fileUploadId";

            return _connectionWrapper
                .Query<Upload>(query, new {PwShopId, fileUploadId})
                .FirstOrDefault();
        }
    }
}

