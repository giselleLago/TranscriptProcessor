using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TranscriptsProcessor.Services
{
    public sealed class Processor
    {
        public Processor(ILogger logger,
                         IFileManager fileManager,
                         ISender sender)
        {
            Logger = logger;
            FileManager = fileManager;
            Sender = sender;
        }

        public async Task Run(string filePath)
        {
            Logger.LogInformation("Running Processor service.");
            var userDictionary = FileManager.GetPendingFiles(filePath);
            await Sender.SendFilesAsync(userDictionary);
        }

        private readonly ILogger Logger;
        private readonly IFileManager FileManager;
        private readonly ISender Sender;
    }
}
