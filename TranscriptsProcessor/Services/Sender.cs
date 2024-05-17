using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TranscriptsProcessor.TranscriptionService;

namespace TranscriptsProcessor.Services
{
    public class Sender : ISender
    {
        public Sender(ILogger<Sender> logger,
                      IFileValidator validator,
                      IFileManager fileManager,
                      ITranscriptiService transcriptionService)
        {
            Logger = logger;
            Validator = validator;
            FileManager = fileManager;
            TranscriptionService = transcriptionService;
        }

        public async Task SendFilesAsync(Dictionary<string, List<string>> userDictionary)
        {
            foreach (var userPath in userDictionary.Keys)
            {
                var splitFilePaths = SplitFilePaths(userDictionary[userPath].ToList());
                foreach (var splitFilePath in splitFilePaths)
                {
                    await SendUserFilesAsync(splitFilePath, userPath);
                }
            }

            if (ErrorFiles.Any())
            {
                Logger.LogWarning($"Fail to transcript the following documents: {ErrorFiles}");
            }
        }

        private Task SendUserFilesAsync(List<string> splitFilePath, string userPath)
        {
            foreach (var filePath in splitFilePath)
            {
                try
                {
                    if (Validator.ValidateFiles($"{userPath}\\{filePath}"))
                    {
                        var fileContent = FileManager.ReadAllBytes(filePath);
                        var retryAttempt = 2;
                        SendToTranscript(userPath, retryAttempt, filePath, fileContent);
                    }
                }
                catch (IOException e)
                {
                    Logger.LogWarning($"An IO exception was caught: {e.Message}");
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"An exception was caught: {e.Message}");
                }
            }

            return Task.CompletedTask;
        }

        private void SendToTranscript(string userPath, int retryAttempt, string fileName, byte[] fileContents)
        {
            try
            {
                Logger.LogInformation("Sending MP3 files to FTS INVOX to transcript");

                var text = TranscriptionService.Transcribe(userPath, fileName);

                var filePath = fileName.Substring(0, fileName.Length - 3) + "txt";

                Logger.LogInformation("Adding TXT file with the transcription");
                FileManager.WriteToFileAsync(filePath, text);
            }
            catch (Exception)
            {
                if (retryAttempt > 0)
                {
                    //Here we can use a RetryPolicy installing "Polly" NuGet Package and define the basic or the exponential backoff
                    Logger.LogInformation($"Resend to transcript file {fileName}");
                    retryAttempt--;
                    SendToTranscript(userPath, retryAttempt, fileName, fileContents);
                }
                else
                {
                    ErrorFiles.Add(fileName);
                }
            }
        }

        private List<List<string>> SplitFilePaths(IList<string> files)
        {
            var blocks = files
             .Select((value, index) => new { Value = value, Index = index })
             .GroupBy(item => item.Index / 3)
             .Select(group => group.Select(item => item.Value).ToList())
             .ToList();

            return blocks;
        }

        private readonly ConcurrentBag<string> ErrorFiles = new ConcurrentBag<string>();
        private readonly ILogger<Sender> Logger;
        private readonly IFileValidator Validator;
        private readonly IFileManager FileManager;
        private readonly ITranscriptiService TranscriptionService;
    }
}
