using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// FFZ data provider
    /// </summary>
    public class FFZDataProvider : IChatResourceProvider<ChatResourceData>
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
            bool l_IsGlobal = string.IsNullOrEmpty(p_ChannelID);

            try
            {
                ChatPlexSDK.Logger.Debug($"Requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : $" for channel {p_ChannelName}")}.");
                using (HttpRequestMessage l_Query = new HttpRequestMessage(HttpMethod.Get, l_IsGlobal ? "https://api.frankerfacez.com/v1/set/global" : $"https://api.frankerfacez.com/v1/room/{p_ChannelName}"))
                {
                    var l_Response = await m_HTTPClient.SendAsync(l_Query).ConfigureAwait(false);
                    if (!l_Response.IsSuccessStatusCode)
                    {
                        ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}. {l_Response.ReasonPhrase}");
                        return;
                    }

                    JSONNode l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    if (!l_JSON["sets"].IsObject)
                    {
                        ChatPlexSDK.Logger.Error("sets was not an object");
                        return;
                    }

                    int l_Count = 0;
                    foreach (JSONObject l_Object in l_IsGlobal ? l_JSON["sets"]["3"]["emoticons"].AsArray : l_JSON["sets"][l_JSON["room"]["set"].ToString()]["emoticons"].AsArray)
                    {
                        JSONObject l_URLs = l_Object["urls"].AsObject;

                        string l_URI = l_URLs[l_URLs.Count - 1].Value;
                        string l_ID  = l_IsGlobal ? l_Object["name"].Value : $"{p_ChannelID}_{l_Object["name"].Value}";

                        if (l_URI.Length > 0 && l_URI[0] == '/')
                            l_URI = "https:" + l_URI;

                        Resources[l_ID] = new ChatResourceData()
                        {
                            Uri         = l_URI,
                            Animation   = Animation.EAnimationType.NONE,
                            Category    = EChatResourceCategory.Emote,
                            Type        = l_IsGlobal ? "FFZGlobalEmote" : "FFZChannelEmote"
                        };
                        l_Count++;
                    }

                    ChatPlexSDK.Logger.Debug($"Success caching {l_Count} FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}.");
                    return;
                }
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An error occurred while requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_ChannelName)}.");
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
