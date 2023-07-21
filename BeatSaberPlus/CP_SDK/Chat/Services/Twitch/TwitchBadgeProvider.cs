using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch badge provider
    /// </summary>
    public class TwitchBadgeProvider : IChatResourceProvider<ChatResourceData>
    {
        /// <summary>
        /// HTTP Client
        /// </summary>
        private HttpClient m_HTTPClient = new HttpClient();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Resource cache
        /// </summary>
        public ConcurrentDictionary<string, ChatResourceData> Resources { get; } = new ConcurrentDictionary<string, ChatResourceData>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try request resources from the provider
        /// </summary>
        /// <param name="p_ChannelID">ID of the channel</param>
        /// <param name="p_ChannelName">Name of the channel</param>
        /// <param name="p_AccessToken">Access token for the API</param>
        /// <returns></returns>
        public async Task TryRequestResources(string p_ChannelID, string p_ChannelName, string p_AccessToken)
        {
            var l_WebClient = new Network.WebClient("https://api.twitch.tv/helix/", TimeSpan.FromSeconds(10), true);
            if (!l_WebClient.Headers.ContainsKey("Client-Id"))
                l_WebClient.Headers.Remove("Authorization");

            if (l_WebClient.Headers.ContainsKey("Client-Id"))
                l_WebClient.Headers.Remove("Authorization");

            l_WebClient.Headers.Add("client-id",     TwitchService.TWITCH_CLIENT_ID);
            l_WebClient.Headers.Add("Authorization", "Bearer " + p_AccessToken.Replace("oauth:", ""));

            bool l_IsGlobal = string.IsNullOrEmpty(p_ChannelID);
            try
            {
                ChatPlexSDK.Logger.Debug($"Requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : $" for channel {p_ChannelName}")}.");

                var l_URL = "chat/badges/global";
                if (!l_IsGlobal)
                    l_URL = $"chat/badges?broadcaster_id={p_ChannelID}";

                var l_Completion = new TaskCompletionSource<Network.WebResponse>();
                l_WebClient.GetAsync(l_URL, CancellationToken.None, (p_Response) =>
                {
                    l_Completion.SetResult(p_Response);
                });
                await l_Completion.Task;
                var l_Response = l_Completion.Task?.Result;

                if (l_Response == null || !l_Response.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}. {l_Response.ReasonPhrase}");
                    return;
                }

                JSONNode l_JSON = JSON.Parse(l_Response.BodyString);
                if (!l_JSON["data"].IsArray)
                {
                    ChatPlexSDK.Logger.Error("data was not an object.");
                    return;
                }

                var l_Data = l_JSON["data"].AsArray;

                int l_Count = 0;
                foreach (KeyValuePair<string, JSONNode> l_SetKVP in l_Data)
                {
                    var l_Set       = l_SetKVP.Value;
                    var l_SetID     = l_Set["set_id"].Value;
                    var l_Versions  = l_Set["versions"].AsArray;

                    foreach (KeyValuePair<string, JSONNode> l_VersionKVP in l_Versions)
                    {
                        var l_Version   = l_VersionKVP.Value;
                        var l_ID        = l_Version["id"].Value;
                        var l_Picture   = l_Version["image_url_2x"].Value;
                        var l_FinalName = $"{l_SetID}{l_ID}";

                        var l_InternalID = l_IsGlobal ? l_FinalName : $"{p_ChannelID}_{l_FinalName}";

                        Resources[l_InternalID] = new ChatResourceData()
                        {
                            Uri         = l_Picture,
                            Animation   = Animation.EAnimationType.NONE,
                            Category    = EChatResourceCategory.Badge,
                            Type        = l_IsGlobal ? "TwitchGlobalBadge" : "TwitchChannelBadge"
                        };

                        l_Count++;
                    }
                }

                ChatPlexSDK.Logger.Debug($"Success caching {l_Count} Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}.");
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An error occurred while requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}.");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get a resource
        /// </summary>
        /// <param name="p_Identifier">Resource ID</param>
        /// <param name="p_Category">Channel / Category</param>
        /// <param name="p_Data">Result data</param>
        /// <returns></returns>
        public bool TryGetResource(string p_Identifier, string p_Category, out ChatResourceData p_Data)
        {
            if (!string.IsNullOrEmpty(p_Category) && Resources.TryGetValue($"{p_Category}_{p_Identifier}", out p_Data))
                return true;

            if (Resources.TryGetValue(p_Identifier, out p_Data))
                return true;

            p_Data = null;
            return false;
        }
    }
}
