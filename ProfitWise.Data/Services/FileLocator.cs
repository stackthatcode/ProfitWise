using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
            return Path.Combine(_uploadDirectory, fileLocker.FileLockerId.ToString());
        }

        public string UploadFilePath(FileLocker fileLocker)
        {
            var targetDirectory = Directory(fileLocker);
            var targetPath = Path.Combine(targetDirectory, fileLocker.UploadFileName);
            return targetPath;
        }
        
        public string FeedbackFilePath(FileLocker fileLocker)
        {
            var targetDirectory = Directory(fileLocker);
            var targetPath = Path.Combine(targetDirectory, fileLocker.FeedbackFileName);
            return targetPath;
        }
        
    }
}

