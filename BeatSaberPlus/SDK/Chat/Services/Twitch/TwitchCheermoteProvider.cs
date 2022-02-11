using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using BeatSaberPlus.SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch Cheermote provider
    /// </summary>
    public class TwitchCheermoteProvider : IChatResourceProvider<TwitchCheermoteData>
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
        public ConcurrentDictionary<string, TwitchCheermoteData> Resources { get; } = new ConcurrentDictionary<string, TwitchCheermoteData>();

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
                Logger.Instance.Debug($"Requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");
                using (HttpRequestMessage l_Query = new HttpRequestMessage(HttpMethod.Get, $"https://api.twitch.tv/v5/bits/actions?client_id={TwitchService.TWITCH_CLIENT_ID}&channel_id={p_Category}&include_sponsored=1"))
                {
                    var l_Response = await m_HTTPClient.SendAsync(l_Query);
                    if (!l_Response.IsSuccessStatusCode)
                    {
                        Logger.Instance.Error($"Unsuccessful status code when requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                        return;
                    }

                    JSONNode l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync());
                    if (!l_JSON["actions"].IsArray)
                    {
                        Logger.Instance.Error("badge_sets was not an object.");
                        return;
                    }

                    int l_Count = 0;
                    foreach (JSONNode l_Node in l_JSON["actions"].AsArray.Values)
                    {
                        var l_Cheermote = new TwitchCheermoteData();
                        var l_Prefix    = l_Node["prefix"].Value.ToLower();

                        foreach (JSONNode l_Tier in l_Node["tiers"].Values)
                        {
                            var l_NewTier = new CheermoteTier();
                            l_NewTier.MinBits   = l_Tier["min_bits"].AsInt;
                            l_NewTier.Color     = l_Tier["color"].Value;
                            l_NewTier.CanCheer  = l_Tier["can_cheer"].AsBool;
                            l_NewTier.Uri       = l_Tier["images"]["dark"]["animated"]["4"].Value;

                            l_Cheermote.Tiers.Add(l_NewTier);
                        }

                        l_Cheermote.Prefix = l_Prefix;
                        l_Cheermote.Tiers = l_Cheermote.Tiers.OrderBy(t => t.MinBits).ToList();

                        string l_ID = l_IsGlobal ? l_Prefix : $"{p_Category}_{l_Prefix}";
                        Resources[l_ID] = l_Cheermote;
                        l_Count++;
                    }

                    Logger.Instance.Debug($"Success caching {l_Count} Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                    return;
                }
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"An error occurred while requesting Twitch {(l_IsGlobal ? "global " : "")}cheermotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
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
