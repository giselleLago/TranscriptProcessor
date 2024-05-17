using System;

namespace TranscriptsProcessor.TranscriptionService
{
    public class TranscriptService : ITranscriptiService
    {
        public string Transcribe(string userId, string mp3File)
        {
            if (_random.NextDouble() < 0.05)
            {
                throw new Exception("Generic error in the transcription.");
            }

            int index = _random.Next(PredefinedTexts.Length);
            return PredefinedTexts[index];
        }

        private static readonly string[] PredefinedTexts = new string[]
       {
            "Example text one",
            "Example text two",
            "Example text three",
            "Example text four"
       };

        private readonly Random _random = new Random();
    }
}
