using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TranscriptsProcessor.Services
{
    public class FileManager : IFileManager
    {
        public FileManager(ILogger<FileManager> logger)
        {
            Logger = logger;
        }

        public Dictionary<string, List<string>> GetPendingFiles(string path)
        {
            Logger.LogInformation("Getting pending MP3 files");
            var userDictionary = new Dictionary<string, List<string>>();

            var userData = Directory.GetDirectories(path);

            foreach (var filePath in userData)
            {
                var mp3Files = Directory.GetFiles(filePath, "*.mp3");
                var pendingFiles = mp3Files.Where(HasNoTxtFile).ToList();

                if (pendingFiles.Any())
                {
                    userDictionary[filePath] = pendingFiles;
                }
            }

            return userDictionary;
        }

        private bool HasNoTxtFile(string filePath)
        {
            var txtPath = filePath.Substring(0, filePath.Length - 3) + "txt";
            var fileInfo = new FileInfo(txtPath);
            return !fileInfo.Exists;
        }

        public byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes($"{filePath}");
        }

        public Task WriteToFileAsync(string filePath, string text)
        {
            return File.WriteAllTextAsync($"{filePath}", text);
        }

        private readonly ILogger<FileManager> Logger;
    }
}
