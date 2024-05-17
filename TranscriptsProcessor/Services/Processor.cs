using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TranscriptsProcessor.Services
{
    public class Processor
    {
        public Processor(ILogger<Processor> logger,
                         IFileManager pendingFile,
                         ISender senderService)
        {
            Logger = logger;
            PendingFiles = pendingFile;
            SenderService = senderService;
        }

        public async Task Run(string filePath)
        {
            Logger.LogInformation("Running Processor service.");
            FilePath = filePath;
            var userDictionary = PendingFiles.GetPendingFiles(FilePath);
            await SenderService.SendFilesAsync(userDictionary);
        }


        private string FilePath;
        private readonly ILogger<Processor> Logger;
        private readonly IFileManager PendingFiles;
        private readonly ISender SenderService;
    }
}
