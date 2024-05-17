using Microsoft.Extensions.Logging;
using System.IO;

namespace TranscriptsProcessor.Services
{
    public class FileValidator : IFileValidator
    {
        public FileValidator(ILogger<FileValidator> logger)
        {
            Logger = logger;
        }

        public bool ValidateFiles(string filePath)
        {
            return IsValidSize(filePath) && IsValidBasicMP3(filePath);
        }

        private bool IsValidSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            long sizeInBytes = fileInfo.Length;

            return (51200 < sizeInBytes && sizeInBytes < 3145728);
        }

        public bool IsValidBasicMP3(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Logger.LogError("File doesn't exists.");
                return false;
            }

            if (Path.GetExtension(filePath).ToLower() != ".mp3")
            {
                Logger.LogError("File doesn't exists.");
                return false;
            }

            return true;
        }

        //We can also add Mp3 headers check

        private readonly ILogger<FileValidator> Logger;
    }
}
