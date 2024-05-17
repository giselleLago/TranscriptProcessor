using System.Collections.Generic;
using System.Threading.Tasks;

namespace TranscriptsProcessor.Services
{
    public interface IFileManager
    {
        Dictionary<string, List<string>> GetPendingFiles(string path);

        byte[] ReadAllBytes(string filePath);

        Task WriteToFileAsync(string filePath, string text);
    }
}
