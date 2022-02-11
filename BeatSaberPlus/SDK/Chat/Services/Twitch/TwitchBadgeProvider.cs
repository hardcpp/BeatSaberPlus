using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models;
using BeatSaberPlus.SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
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
        /// Try request resources
        /// </summary>
        /// <param name="p_Category">Category / Channel</param>
        /// <returns></returns>
        public async Task TryRequestResources(string p_Category)
        {
            bool l_IsGlobal = string.IsNullOrEmpty(p_Category);
            try
            {
                Logger.Instance.Debug($"Requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");
                using (HttpRequestMessage l_Query = new HttpRequestMessage(HttpMethod.Get, l_IsGlobal ? $"https://badges.twitch.tv/v1/badges/global/display" : $"https://badges.twitch.tv/v1/badges/channels/{p_Category}/display")) //channel.AsTwitchChannel().Roomstate.RoomId
                {
                    var l_Response = await m_HTTPClient.SendAsync(l_Query);
                    if (!l_Response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Error($"Unsuccessful status code when requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                        return;
                    }

                    JSONNode l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync());
                    if (!l_JSON["badge_sets"].IsObject)
                    {
                        Logger.Instance.Error("badge_sets was not an object.");
                        return;
                    }

                    int l_Count = 0;
                    foreach (KeyValuePair<string, JSONNode> l_KVP in l_JSON["badge_sets"])
                    {
                        string l_BadgeName = l_KVP.Key;
                        foreach (KeyValuePair<string, JSONNode> version in l_KVP.Value.AsObject["versions"].AsObject)
                        {
                            string l_BadgeVersion   = version.Key;
                            string l_FinalName      = $"{l_BadgeName}{l_BadgeVersion}";
                            string l_URI            = version.Value.AsObject["image_url_4x"].Value;

                            string l_ID = l_IsGlobal ? l_FinalName : $"{p_Category}_{l_FinalName}";
                            Resources[l_ID] = new ChatResourceData()
                            {
                                Uri         = l_URI,
                                Animation   = Animation.AnimationType.NONE,
                                Category    = EChatResourceCategory.Badge,
                                Type        = l_IsGlobal ? "TwitchGlobalBadge" : "TwitchChannelBadge"
                            };

                            l_Count++;
                        }
                    }
                    Logger.Instance.Debug($"Success caching {l_Count} Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                    return;
                }
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"An error occurred while requesting Twitch {(l_IsGlobal ? "global " : "")}badges{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                Logger.Instance.Error(l_Exception);
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
