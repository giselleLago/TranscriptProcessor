using Microsoft.Extensions.Logging;
using System.IO;

namespace TranscriptsProcessor.Services
{
    public class FileValidator : IFileValidator
    {
        public FileValidator(ILogger logger)
        {
            Logger = logger;
        }

        public bool ValidateFiles(string filePath)
        {
            return IsValidSize(filePath) && IsValidBasicMP3(filePath);// && ContainsMP3Headers(filePath);
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

        //ToCheck
        public bool ContainsMP3Headers(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var buffer = new byte[10];  // Read first 10 bytes from the file
            while (fs.Read(buffer, 0, buffer.Length) == buffer.Length)
            {
                if ((buffer[0] == 0xFF) && ((buffer[1] & 0xE0) == 0xE0))
                {
                    return true;  // Found an MP3 frame header
                }
            }
            return false;
        }

        private readonly ILogger Logger;
    }
}
