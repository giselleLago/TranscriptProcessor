using System.Collections.Generic;
using System.Threading.Tasks;

namespace TranscriptsProcessor.Services
{
    public interface ISender
    {
        Task SendFilesAsync(Dictionary<string, List<string>> userDictionary);
    }
}
