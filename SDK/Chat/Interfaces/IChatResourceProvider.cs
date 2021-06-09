using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    internal interface IChatResourceProvider<T>
    {
        ConcurrentDictionary<string, T> Resources { get; }
        Task<bool> TryRequestResources(string category);
        bool TryGetResource(string identifier, string category, out T data);
    }
}
