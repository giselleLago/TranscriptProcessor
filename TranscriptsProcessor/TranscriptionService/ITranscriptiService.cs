namespace TranscriptsProcessor.TranscriptionService
{
    public interface ITranscriptiService
    {
        string Transcribe(string userId, string mp3File);
    }
}
