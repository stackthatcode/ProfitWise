using System;
using ProfitWise.Data.Model.Cogs.UploadObjects;

namespace ProfitWise.Data.Model.Cogs
{
    public class FileLocker
    {
        public Guid FileLockerId { get; set; }
        public string UploadFileName { get; set; }
        public string FeedbackFileName { get; set; }


        public FileLocker()
        {
            FileLockerId = Guid.NewGuid();
        }

        public void ProvisionFeedbackFileName()
        {
            FeedbackFileName = "feedback.txt";
        }

        public static FileLocker MakeNewForCogsUpload()
        {
            return new FileLocker()
            {
                UploadFileName = "upload.csv"
            };
        }

        public static FileLocker MakeFromUpload(Upload upload)
        {
            return new FileLocker()
            {
                FileLockerId = upload.FileLockerId,
                UploadFileName = upload.UploadFileName,
                FeedbackFileName = upload.FeedbackFileName,
            };
        }
    }
}
