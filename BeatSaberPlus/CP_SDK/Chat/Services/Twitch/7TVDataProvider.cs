using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// 7TV data provider
    /// </summary>
    public class _7TVDataProvider : IChatResourceProvider<ChatResourceData>
    {
        /// <summary>
        /// Paint cached stop
        /// </summary>
        private struct CachedPaintStop
        {
            [JsonProperty] internal float Stop;
            [JsonProperty] internal Color32 StopColor;
        }
        private struct CustomPaint
        {
#pragma warning disable CS0649
            [JsonProperty] internal string Name;
            [JsonProperty] internal CachedPaintStop[] Stops;
            [JsonProperty] internal (string, string)[] Users;
#pragma warning restore CS0649
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// HTTP Client
        /// </summary>
        private HttpClient m_HTTPClient = new HttpClient();
        /// <summary>
        /// Paint per user ID
        /// </summary>
        private ConcurrentDictionary<string, CachedPaintStop[]> m_Paints = new ConcurrentDictionary<string, CachedPaintStop[]>();
        /// <summary>
        /// Painted names cache
        /// </summary>
        private ConcurrentDictionary<string, string> m_PaintCache = new ConcurrentDictionary<string, string>();

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
        public async Task TryRequestResources(string p_Category, string p_Token)
        {
            bool l_IsGlobal = string.IsNullOrEmpty(p_Category);

            try
            {
                ChatPlexSDK.Logger.Debug($"Requesting 7TV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : $" for channel {p_Category}")}.");

                using (HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, l_IsGlobal ? "https://api.7tv.app/v2/emotes/global" : $"https://api.7tv.app/v2/users/{p_Category}/emotes"))
                {
                    var l_Response = await m_HTTPClient.SendAsync(msg).ConfigureAwait(false);
                    if (l_Response.IsSuccessStatusCode)
                    {
                        JSONNode l_JSON = JSON.Parse(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                        if (l_JSON.IsArray)
                        {
                            int l_Count = 0;
                            foreach (JSONObject l_Object in l_JSON.AsArray)
                            {
                                string l_URI = $"https://cdn.7tv.app/emote/{l_Object["id"].Value}/2x";
                                string l_ID = l_IsGlobal ? l_Object["name"].Value : $"{p_Category}_{l_Object["name"].Value}";

                                Resources.TryAdd(l_ID, new ChatResourceData()
                                {
                                    Uri         = l_URI,
                                    Animation   = Animation.EAnimationType.AUTODETECT,
                                    Category    = EChatResourceCategory.Emote,
                                    Type        = l_IsGlobal ? "7TVGlobalEmote" : "7TVChannelEmote"
                                });
                                l_Count++;
                            }

                            ChatPlexSDK.Logger.Debug($"Success caching {l_Count} 7TV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                        }
                        else
                            ChatPlexSDK.Logger.Error("emotes was not an array.");
                    }
                    else
                        ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting 7TV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}. {l_Response.ReasonPhrase}");
                }
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"An error occurred while requesting 7TV {(l_IsGlobal ? "global " : "")}emotes{(l_IsGlobal ? "." : " for channel " + p_Category)}.");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            if (l_IsGlobal)
            {
                try
                {
                    ChatPlexSDK.Logger.Debug($"Requesting 7TV cosmetics");

                    using (var l_Message = new HttpRequestMessage(HttpMethod.Get, "https://api.7tv.app/v2/cosmetics?user_identifier=twitch_id"))
                    {
                        var l_Response = await m_HTTPClient.SendAsync(l_Message).ConfigureAwait(false);
                        if (l_Response.IsSuccessStatusCode)
                        {
                            var l_Cosmetics = JsonConvert.DeserializeObject<Models.Twitch._7TVCosmetics>(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false));

                            if (l_Cosmetics.paints != null)
                            {
                                for (int l_PI = 0; l_PI < l_Cosmetics.paints.Length; ++l_PI)
                                {
                                    var l_Paint         = l_Cosmetics.paints[l_PI];
                                    var l_CachedPaint   = CachePaint(l_Paint);

                                    if (l_CachedPaint != null && l_Paint.users != null)
                                    {
                                        for (int l_UI = 0; l_UI < l_Paint.users.Length; ++l_UI)
                                        {
                                            if (m_Paints.ContainsKey(l_Paint.users[l_UI]))
                                                continue;

                                            m_Paints.TryAdd(l_Paint.users[l_UI], l_CachedPaint);
                                        }
                                    }
                                }
                            }

                            ChatPlexSDK.Logger.Debug($"Success caching 7TV cosmetics ({m_Paints.Count} Paints).");
                        }
                        else
                            ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting 7TV cosmetics. {l_Response.ReasonPhrase}");
                    }
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"An error occurred while requesting 7TV cosmetics.");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                try
                {
                    ChatPlexSDK.Logger.Debug($"Requesting 7TV cosmetics");

                    using (var l_Message = new HttpRequestMessage(HttpMethod.Get, "https://mods-data.hardcpp.com/twitch_painted_names.json"))
                    {
                        var l_Response = await m_HTTPClient.SendAsync(l_Message).ConfigureAwait(false);
                        if (l_Response.IsSuccessStatusCode)
                        {
                            var l_CustomPaintings   = JsonConvert.DeserializeObject<CustomPaint[]>(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false), new Config.JsonConverters.Color32Converter());
                            var l_Count             = 0;

                            for (int l_I = 0; l_I < l_CustomPaintings.Length; ++l_I)
                            {
                                for (int l_U = 0; l_U < l_CustomPaintings[l_I].Users.Length; ++l_U)
                                {
                                    var l_UserID = l_CustomPaintings[l_I].Users[l_U].Item2;
                                    if (m_Paints.ContainsKey(l_UserID))
                                        m_Paints[l_UserID] = l_CustomPaintings[l_I].Stops;
                                    else
                                        m_Paints.TryAdd(l_UserID, l_CustomPaintings[l_I].Stops);
                                    l_Count++;
                                }
                            }

                            ChatPlexSDK.Logger.Debug($"Success caching custom cosmetics ({l_Count} Paints).");
                        }
                        else
                            ChatPlexSDK.Logger.Error($"Unsuccessful status code when requesting custom cosmetics. {l_Response.ReasonPhrase}");
                    }

                    m_PaintCache.Clear();
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"An error occurred while requesting custom cosmetics.");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try get a custom user display name
        /// </summary>
        /// <param name="p_UserID">Twitch UserID</param>
        /// <param name="p_Default">Default display name</param>
        /// <returns></returns>
        public string TryGetUserDisplayName(string p_UserID, string p_Default)
        {
            if (string.IsNullOrEmpty(p_UserID))
                return p_Default;

            if (m_PaintCache.TryGetValue(p_UserID, out var l_Cached))
                return l_Cached;
            else if (m_Paints.TryGetValue(p_UserID, out var l_Paint))
            {
                var l_PaintedName = "";
                for (int l_C = 0; l_C < p_Default.Length; ++l_C)
                {
                    var l_Progress  = (float)l_C / (float)p_Default.Length;
                    var l_StopA     = new CachedPaintStop() { Stop = -1f, StopColor = Color.black };
                    var l_StopB     = new CachedPaintStop() { Stop = -1f, StopColor = Color.black};

                    for (int l_S = 0; l_S < l_Paint.Length; ++l_S)
                    {
                        var l_CurrentStop = l_Paint[l_S];
                        if (l_CurrentStop.Stop >= l_StopA.Stop && l_CurrentStop.Stop <= l_Progress)
                            l_StopA = l_CurrentStop;
                        else if (l_CurrentStop.Stop >= l_Progress)
                        {
                            l_StopB = l_CurrentStop;
                            break;
                        }
                    }

                    var l_Color = Color.Lerp(l_StopA.StopColor, l_StopB.StopColor, (l_Progress - l_StopA.Stop) / (l_StopB.Stop - l_StopA.Stop));
                    l_PaintedName += "<color=#" + ColorUtility.ToHtmlStringRGB(l_Color) + ">" + p_Default[l_C] + "</color>";
                }

                m_PaintCache.TryAdd(p_UserID, l_PaintedName);
                return l_PaintedName;
            }

            return p_Default;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache 7TV raw paint into a cached paint
        /// </summary>
        /// <param name="p_Paint">7TV raw paint</param>
        /// <returns></returns>
        private static CachedPaintStop[] CachePaint(Models.Twitch._7TVCosmetics._7TVPaint p_Paint)
        {
            if (p_Paint.stops == null || p_Paint.stops.Length == 0/* || (!l_Paint.color.HasValue && l_Paint.stops.Length < 2)*/)
                return null;

            var l_DefaultColor  = p_Paint.color.HasValue ? ConvertColor(p_Paint.color.Value) : (Color32)Color.black;
            var l_Stops         = null as List<(float, Color32)>;

            if (p_Paint.shape == "circle")
            {
                l_Stops = new List<(float at, Color32)>();

                var l_CenterToRight = p_Paint.stops.Select(x => (x.at, ConvertColor(x.color))).ToList();

                if (p_Paint.repeat)
                {
                    var l_RepeatCount = (int)(1f / l_CenterToRight.Max(x => x.at));
                    var l_BaseCount = l_CenterToRight.Count;

                    for (int l_R = 0; l_R < l_RepeatCount - 1; ++l_R)
                    {
                        var l_Offset = (1f / (float)l_RepeatCount) * (l_R + 1);
                        for (int l_S = 0; l_S < l_BaseCount; ++l_S)
                            l_CenterToRight.Add((l_Offset + l_CenterToRight[l_S].at, l_CenterToRight[l_S].Item2));
                    }
                }

                if (!l_CenterToRight.Any(x => Mathf.Abs(0f - x.at) < 0.01))
                    l_CenterToRight.Insert(0, (0f, l_CenterToRight.First().Item2));
                if (!l_CenterToRight.Any(x => Mathf.Abs(1f - x.at) < 0.01))
                    l_CenterToRight.Add((1f, l_CenterToRight.Last().Item2));

                for (int l_Part = 0; l_Part < 2; ++l_Part)
                {
                    foreach (var l_Stop in l_CenterToRight)
                    {
                        if (l_Part == 0)
                            l_Stops.Add(((1f - l_Stop.at) / 2f, l_Stop.Item2));
                        else
                            l_Stops.Add(((l_Stop.at + 1f) / 2f, l_Stop.Item2));
                    }
                }
            }
            else
            {
                l_Stops = p_Paint.stops.Select(x => (x.at, ConvertColor(x.color))).ToList();

                if (p_Paint.repeat)
                {
                    var l_RepeatCount = (int)(1f / l_Stops.Max(x => x.Item1));
                    var l_BaseCount = l_Stops.Count;

                    for (int l_R = 0; l_R < l_RepeatCount - 1; ++l_R)
                    {
                        var l_Offset = (1f / (float)l_RepeatCount) * (l_R + 1);
                        for (int l_S = 0; l_S < l_BaseCount; ++l_S)
                            l_Stops.Add((l_Offset + l_Stops[l_S].Item1, l_Stops[l_S].Item2));
                    }
                }
            }

            return l_Stops.Distinct().OrderBy(x => x.Item1).Select(x => new CachedPaintStop() { Stop = x.Item1, StopColor = x.Item2}).ToArray();
        }
        /// <summary>
        /// Convert int color to Color object
        /// </summary>
        /// <param name="p_Color">Raw integer color</param>
        /// <returns></returns>
        public static Color32 ConvertColor(int p_Color)
        {
            return new Color32(
                (byte)((p_Color >> 24) & 0xFF),
                (byte)((p_Color >> 16) & 0xFF),
                (byte)((p_Color >>  8) & 0xFF),
                (byte)((p_Color >>  0) & 0xFF)
            );
        }
    }
}
