using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models.Twitch;
using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch Cheermote provider
    /// </summary>
    public class TwitchCheermoteProvider : IChatResourceProvider<TwitchCheermoteData>
    {
        /// <summary>
        /// Resource cache
        /// </summary>
        public ConcurrentDictionary<string, TwitchCheermoteData> Resources { get; } = new ConcurrentDictionary<string, TwitchCheermoteData>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try request resources
        /// </summary>
        /// <param name="p_Category">Category / Channel</param>
        /// <returns></returns>
        public async Task TryRequestResources(string p_Category, string p_Token)
        {
            Network.APIClient m_APIClient = new Network.APIClient("https://api.twitch.tv/helix/", TimeSpan.FromSeconds(10), true);
            if (!m_APIClient.InternalClient.DefaultRequestHeaders.Contains("client-id"))
                m_APIClient.InternalClient.DefaultRequestHeaders.Add("client-id", TwitchService.TWITCH_CLIENT_ID);

            if (m_APIClient.InternalClient.DefaultRequestHeaders.Contains("Authorization"))
                m_APIClient.InternalClient.DefaultRequestHeaders.Remove("Authorization");

            m_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + p_Token.Replace("oauth:", ""));

            bool l_IsGlobal = string.IsNullOrEmpty(p_Category);
            try
            {
                ChatPlexSDK.Logger.Debug($"Requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");

                var l_Response = await m_APIClient.GetAsync("bits/cheermotes" + (l_IsGlobal ? "" : "?broadcaster_id=" + p_Category), CancellationToken.None).ConfigureAwait(false);
                if (l_Response == null || !l_Response.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                    return;
                }

                JSONNode l_JSON = JSON.Parse(l_Response.BodyString);
                if (!l_JSON["data"].IsArray)
                {
                    ChatPlexSDK.Logger.Error("badge_sets was not an object.");
                    return;
                }

                int l_Count = 0;
                foreach (JSONNode l_Node in l_JSON["data"].AsArray.Values)
                {
                    var l_Cheermote = new TwitchCheermoteData();
                    var l_Prefix    = l_Node["prefix"].Value.ToLower();

                    foreach (JSONNode l_Tier in l_Node["tiers"].Values)
                    {
                        var l_NewTier = new CheermoteTier();
                        l_NewTier.MinBits   = l_Tier["min_bits"].AsInt;
                        l_NewTier.Color     = l_Tier["color"].Value;
                        l_NewTier.Uri       = l_Tier["images"]["dark"]["animated"]["3"].Value;

                        l_Cheermote.Tiers.Add(l_NewTier);
                    }

                    l_Cheermote.Prefix = l_Prefix;
                    l_Cheermote.Tiers = l_Cheermote.Tiers.OrderBy(t => t.MinBits).ToList();

                    string l_ID = l_IsGlobal ? l_Prefix : $"{p_Category}_{l_Prefix}";
                    Resources[l_ID] = l_Cheermote;
                    l_Count++;
                }

                ChatPlexSDK.Logger.Debug($"Success caching {l_Count} Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");

            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An error occurred while requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
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
        public bool TryGetResource(string p_Identifier, string p_Category, out TwitchCheermoteData p_Data)
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
