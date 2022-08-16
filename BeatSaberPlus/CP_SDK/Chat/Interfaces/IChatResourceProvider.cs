using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Interfaces
{
    public interface IChatResourceProvider<T>
    {
        ConcurrentDictionary<string, T> Resources { get; }
        Task TryRequestResources(string category, string p_Token);
        bool TryGetResource(string identifier, string category, out T data);
    }
}
