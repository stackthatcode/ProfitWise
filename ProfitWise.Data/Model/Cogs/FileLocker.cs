using System;

namespace ProfitWise.Data.Model.Cogs
{
    public class FileLocker
    {
        public Guid FileLockerId { get; set; }
        public string FileName { get; set; }

        public FileLocker()
        {
            FileLockerId = Guid.NewGuid();
        }

        public static FileLocker MakeNewForCogsUpload()
        {
            return new FileLocker()
            {
                FileName = "upload.csv"
            };
        }
    }
}
