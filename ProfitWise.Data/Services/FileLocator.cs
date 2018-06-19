using System;
using System.Configuration;
using ProfitWise.Data.Model.Cogs;

namespace ProfitWise.Data.Services
{
    public class FileLocator
    {
        private readonly string _uploadDirectory;

        public FileLocator()
        {
            _uploadDirectory = ConfigurationManager.AppSettings["FileUploadDirectory"];
        }

        public Guid NewFileLockerId()
        {
            return Guid.NewGuid();
        }

        public string Directory(FileLocker fileLocker)
        {
            return System.IO.Path.Combine(
                        _uploadDirectory, fileLocker.FileLockerId.ToString());
        }

        public string Path(FileLocker fileLocker)
        {
            var targetDirectory = Directory(fileLocker);
            var targetPath = System.IO.Path.Combine(targetDirectory, fileLocker.FileName);
            return targetPath;
        }
    }
}
