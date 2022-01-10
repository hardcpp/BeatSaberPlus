using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models;
using BeatSaberPlus.SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
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
        /// Try request resources
        /// </summary>
        /// <param name="p_Category">Category / Channel</param>
        /// <returns></returns>
        public async Task<bool> TryRequestResources(string p_Category)
        {
            bool l_IsGlobal = string.IsNullOrEmpty(p_Category);

            try
            {
                Logger.Instance.Debug($"Requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");
                using (HttpRequestMessage l_Query = new HttpRequestMessage(HttpMethod.Get, l_IsGlobal ? "https://api.frankerfacez.com/v1/set/global" : $"https://api.frankerfacez.com/v1/room/{p_Category}"))
                {
                    var l_Response = await m_HTTPClient.SendAsync(l_Query);
                    if (!l_Response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Error($"Unsuccessful status code when requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                        return false;
                    }

                    JSONNode l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync());
                    if (!l_JSON["sets"].IsObject)
                    {
                        Logger.Instance.Error("sets was not an object");
                        return false;
                    }

                    int l_Count = 0;
                    foreach (JSONObject l_Object in l_IsGlobal ? l_JSON["sets"]["3"]["emoticons"].AsArray : l_JSON["sets"][l_JSON["room"]["set"].ToString()]["emoticons"].AsArray)
                    {
                        JSONObject l_URLs = l_Object["urls"].AsObject;

                        string l_URI = l_URLs[l_URLs.Count - 1].Value;
                        string l_ID  = l_IsGlobal ? l_Object["name"].Value : $"{p_Category}_{l_Object["name"].Value}";

                        Resources[l_ID] = new ChatResourceData()
                        {
                            Uri         = l_URI,
                            Animation   = Animation.AnimationType.NONE,
                            Type        = l_IsGlobal ? "FFZGlobalEmote" : "FFZChannelEmote"
                        };
                        l_Count++;
                    }

                    Logger.Instance.Debug($"Success caching {l_Count} FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                    return true;
                }
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"An error occurred while requesting FFZ {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                Logger.Instance.Error(l_Exception);
            }

            return false;
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
