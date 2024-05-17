using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TranscriptsProcessor.Services;
using TranscriptsProcessor.TranscriptionService;
using Xunit;

namespace TranscriptsProcessorTests
{
    public class TranscriptDataTest
    {
        [Fact]
        public async Task TranscriptData()
        {
            var path = ".\\TestData";
            var pendingFiles = new Dictionary<string, List<string>>();
            pendingFiles["user1"] = new List<string> { "UserData1a.mp3", "UserData1b.mp3", "UserData1c.mp3" };
            pendingFiles["user2"] = new List<string> { "UserData2a.mp3", "UserData2b.mp3", "UserData2c.mp3" };
            var fileContent = new byte[50];

            var mockLogger = new Mock<ILogger<Sender>>();
            var mockFileValidator = new Mock<IFileValidator>();
            var mockFileManager = new Mock<IFileManager>();
            var mockTranscriptService = new Mock<ITranscriptiService>();

            mockTranscriptService.Setup(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("Example text one")
                .Verifiable();

            mockFileManager.Setup(x => x.GetPendingFiles(It.IsAny<string>()))
                .Returns(pendingFiles)
                .Verifiable();

            mockFileManager.Setup(x => x.ReadAllBytes(It.IsAny<string>()))
               .Returns(fileContent)
               .Verifiable();

            mockFileManager.Setup(x => x.WriteToFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockFileValidator.Setup(x => x.ValidateFiles(It.IsAny<string>()))
               .Returns(true)
               .Verifiable();

            var sender = new Sender(mockLogger.Object, mockFileValidator.Object, mockFileManager.Object, mockTranscriptService.Object);
            var processor = new Processor(mockLogger.Object, mockFileManager.Object, sender);
            await processor.Run(path);

            //One call per a success mp3 file
            mockTranscriptService.Verify(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(6));
        }

        [Fact]
        public async Task TranscriptData_Retry()
        {
            var path = ".\\TestData";
            var pendingFiles = new Dictionary<string, List<string>>();
            pendingFiles["user1"] = new List<string> { "UserData1a.mp3", "UserData1b.mp3", "UserData1c.mp3" };
            pendingFiles["user2"] = new List<string> { "UserData2a.mp3", "UserData2b.mp3", "UserData2c.mp3" };
            var fileContent = new byte[50];

            var isFirstCall = true;

            var mockLogger = new Mock<ILogger<Sender>>();
            var mockFileValidator = new Mock<IFileValidator>();
            var mockFileManager = new Mock<IFileManager>();
            var mockTranscriptService = new Mock<ITranscriptiService>();

            mockTranscriptService.Setup(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()))
             .Returns(() =>
             {
                 if (isFirstCall)
                 {
                     isFirstCall = false;
                     throw new Exception("Generic error in the transcription.");
                 }

                 return "Example text two";
             });

            mockFileManager.Setup(x => x.GetPendingFiles(It.IsAny<string>()))
                .Returns(pendingFiles)
                .Verifiable();

            mockFileManager.Setup(x => x.ReadAllBytes(It.IsAny<string>()))
               .Returns(fileContent)
               .Verifiable();

            mockFileManager.Setup(x => x.WriteToFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockFileValidator.Setup(x => x.ValidateFiles(It.IsAny<string>()))
               .Returns(true)
               .Verifiable();

            var sender = new Sender(mockLogger.Object, mockFileValidator.Object, mockFileManager.Object, mockTranscriptService.Object);
            var processor = new Processor(mockLogger.Object, mockFileManager.Object, sender);
            await processor.Run(path);

            //One call per a success mp3 file, plus a retry call
            mockTranscriptService.Verify(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
        }

        [Fact]
        public async Task TranscriptData_Fails()
        {
            var path = ".\\TestData";
            var pendingFiles = new Dictionary<string, List<string>>();
            pendingFiles["user1"] = new List<string> { "UserData1a.mp3", "UserData1b.mp3", "UserData1c.mp3" };
            pendingFiles["user2"] = new List<string> { "UserData2a.mp3", "UserData2b.mp3", "UserData2c.mp3" };
            var fileContent = new byte[50];

            var mockLogger = new Mock<ILogger<Sender>>();
            var mockFileValidator = new Mock<IFileValidator>();
            var mockFileManager = new Mock<IFileManager>();
            var mockTranscriptService = new Mock<ITranscriptiService>();

            mockTranscriptService.Setup(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()))
                 .Throws(new Exception("Generic error in the transcription."));

            mockFileManager.Setup(x => x.GetPendingFiles(It.IsAny<string>()))
                .Returns(pendingFiles)
                .Verifiable();

            mockFileManager.Setup(x => x.ReadAllBytes(It.IsAny<string>()))
               .Returns(fileContent)
               .Verifiable();

            mockFileManager.Setup(x => x.WriteToFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockFileValidator.Setup(x => x.ValidateFiles(It.IsAny<string>()))
               .Returns(true)
               .Verifiable();

            var sender = new Sender(mockLogger.Object, mockFileValidator.Object, mockFileManager.Object, mockTranscriptService.Object);
            var processor = new Processor(mockLogger.Object, mockFileManager.Object, sender);
            await processor.Run(path);

            //One call per a success mp3 file, plus 2 retry calls per each one
            mockTranscriptService.Verify(x => x.Transcribe(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(18));
        }
    }
}
