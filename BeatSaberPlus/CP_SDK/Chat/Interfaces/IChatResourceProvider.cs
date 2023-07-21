using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Interfaces
{
    public interface IChatResourceProvider<T>
    {
        ConcurrentDictionary<string, T> Resources { get; }
        /// <summary>
        /// Try request resources from the provider
        /// </summary>
        /// <param name="p_ChannelID">ID of the channel</param>
        /// <param name="p_ChannelName">Name of the channel</param>
        /// <param name="p_AccessToken">Access token for the API</param>
        /// <returns></returns>
        Task TryRequestResources(string p_ChannelID, string p_ChannelName, string p_AccessToken);
        bool TryGetResource(string identifier, string category, out T data);
    }
}
