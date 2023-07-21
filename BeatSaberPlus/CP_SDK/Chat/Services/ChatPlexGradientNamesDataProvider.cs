using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models;
using CP_SDK.Unity.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace CP_SDK.Chat.Services
{
    public class ChatPlexGradientNamesDataProvider : IChatResourceProvider<ChatResourceData>
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
            [JsonProperty] internal object[][] Stops;
            [JsonProperty] internal string[] Users;
#pragma warning restore CS0649
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private string                                          m_GradientNamesURL  = "";
        private HttpClient                                      m_HTTPClient        = new HttpClient();
        private ConcurrentDictionary<string, CachedPaintStop[]> m_Paints            = new ConcurrentDictionary<string, CachedPaintStop[]>();
        private ConcurrentDictionary<string, string>            m_PaintCache        = new ConcurrentDictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public ConcurrentDictionary<string, ChatResourceData> Resources { get; } = new ConcurrentDictionary<string, ChatResourceData>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_GradientNamesURL">Gradient names URL</param>
        public ChatPlexGradientNamesDataProvider(string p_GradientNamesURL)
        {
            m_GradientNamesURL = p_GradientNamesURL;
        }

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
            var l_IsGlobal = string.IsNullOrEmpty(p_ChannelID);
            if (!l_IsGlobal)
                return;

            try
            {
                await RequestChatPlexGradientNames();
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Chat.Services][ChatPlexGlobalDataProvider.TryRequestResources] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
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
            p_Data = null;
            return false;
        }
        /// <summary>
        /// Try get a custom user display name
        /// </summary>
        /// <param name="p_UserID">UserID</param>
        /// <param name="p_Default">Default display name</param>
        /// <param name="p_PaintedName">Output painted name</param>
        /// <returns></returns>
        public bool TryGetUserDisplayName(string p_UserID, string p_Default, out string p_PaintedName)
        {
            p_PaintedName = p_Default;
            if (string.IsNullOrEmpty(p_UserID))
                return false;

            if (m_PaintCache.TryGetValue(p_UserID, out var l_Cached))
            {
                p_PaintedName = l_Cached;
                return true;
            }
            else if (m_Paints.TryGetValue(p_UserID, out var l_Paint))
            {
                var l_PaintedName = "";
                for (int l_C = 0; l_C < p_Default.Length; ++l_C)
                {
                    var l_Progress = (float)l_C / (float)p_Default.Length;
                    var l_StopA = new CachedPaintStop() { Stop = -1f, StopColor = Color.black };
                    var l_StopB = new CachedPaintStop() { Stop = -1f, StopColor = Color.black };

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
                    l_PaintedName += "<color=" + ColorU.ToHexRGBA(l_Color) + ">" + p_Default[l_C] + "</color>";
                }

                m_PaintCache.TryAdd(p_UserID, l_PaintedName);
                p_PaintedName = l_PaintedName;

                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request ChatPlex gradient names
        /// </summary>
        /// <returns></returns>
        private async Task RequestChatPlexGradientNames()
        {
            if (string.IsNullOrEmpty(m_GradientNamesURL))
                return;

            ChatPlexSDK.Logger.Debug($"[CP_SDK.Chat.Services][ChatPlexGlobalDataProvider.TryRequestResources] {m_GradientNamesURL}");

            using (var l_Message = new HttpRequestMessage(HttpMethod.Get, m_GradientNamesURL))
            {
                var l_Response = await m_HTTPClient.SendAsync(l_Message).ConfigureAwait(false);
                if (l_Response.IsSuccessStatusCode)
                {
                    var l_CustomPaintings = JsonConvert.DeserializeObject<CustomPaint[]>(await l_Response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    var l_Count = 0;

                    for (int l_I = 0; l_I < l_CustomPaintings.Length; ++l_I)
                    {
                        var l_CustomPainting    = l_CustomPaintings[l_I];
                        var l_StopsS            = l_CustomPainting.Stops[0];
                        var l_StopsC            = l_CustomPainting.Stops[1];
                        var l_Converted         = new List<CachedPaintStop>();

                        for (int l_SI = 0; l_SI < l_StopsS.Length; ++l_SI)
                        {
                            l_Converted.Add(new CachedPaintStop()
                            {
                                Stop        = float.Parse(l_StopsS[l_SI].ToString()),
                                StopColor   = ColorU.ToUnityColor(((string)l_StopsC[l_SI]))
                            });
                        }

                        var l_AsArray = l_Converted.ToArray();
                        for (int l_UI = 0; l_UI < l_CustomPainting.Users.Length; ++l_UI)
                        {
                            var l_UserID = l_CustomPainting.Users[l_UI];
                            if (m_Paints.ContainsKey(l_UserID))
                                m_Paints[l_UserID] = l_AsArray;
                            else
                                m_Paints.TryAdd(l_UserID, l_AsArray);
                            l_Count++;
                        }
                    }

                    ChatPlexSDK.Logger.Debug($"[CP_SDK.Chat.Services][ChatPlexGlobalDataProvider.TryRequestResources] Success caching {l_Count} gradient names.");
                }
                else
                    ChatPlexSDK.Logger.Error($"[CP_SDK.Chat.Services][ChatPlexGlobalDataProvider.TryRequestResources] Unsuccessful status code when requesting gradient names: {l_Response.ReasonPhrase}");
            }

            m_PaintCache.Clear();
        }
    }
}
