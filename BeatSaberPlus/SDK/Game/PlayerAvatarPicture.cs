using CP_SDK.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Player avatar picture provider
    /// </summary>
    public static class PlayerAvatarPicture
    {
        private static Dictionary<string, Sprite> m_AvatarCache = new Dictionary<string, Sprite>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get player avatar picture
        /// </summary>
        /// <param name="p_PlayerID">ID of the player</param>
        /// <param name="p_CancellationToken">Cancellation token</param>
        /// <param name="p_Callback">Request callback</param>
        public static void GetPlayerAvatarPicture(string p_PlayerID, CancellationToken p_CancellationToken, Action<Sprite> p_Callback)
        {
            lock (m_AvatarCache)
            {
                if (m_AvatarCache.TryGetValue(p_PlayerID, out var l_Avatar))
                {
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(l_Avatar));
                    return;
                }
            }

            GetScoreSaberAvatarPicture(p_PlayerID, p_CancellationToken, p_Callback, () =>
            {
                GetBeatLeaderAvatarPicture(p_PlayerID, p_CancellationToken, p_Callback, () => p_Callback?.Invoke(null));
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get avatar picture from ScoreSaber
        /// </summary>
        /// <param name="p_PlayerID">ID of the player</param>
        /// <param name="p_CancellationToken">Cancellation token</param>
        /// <param name="p_Callback">Request callback</param>
        /// <param name="p_OnFailCallback">On error callback</param>
        private static void GetScoreSaberAvatarPicture(string p_PlayerID, CancellationToken p_CancellationToken, Action<Sprite> p_Callback, Action p_OnFailCallback)
        {
            WebClientUnity.GlobalClient.GetAsync($"https://cdn.scoresaber.com/avatars/{p_PlayerID}.jpg", p_CancellationToken, (p_AvatarResult) =>
            {
                try
                {
                    if (p_AvatarResult == null || !p_AvatarResult.IsSuccessStatusCode || p_AvatarResult.BodyBytes?.Length == 0)
                    {
                        p_OnFailCallback?.Invoke();
                        return;
                    }

                    ProcessAvatarBytes(p_PlayerID, p_Callback, p_AvatarResult.BodyBytes);
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[BeatSaberPlus.SDK.Game][PlayerAvatarPicture.GetScoreSaberAvatarPicture] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_OnFailCallback?.Invoke();
                }
            });
        }
        /// <summary>
        /// Get avatar picture from BeatLeader
        /// </summary>
        /// <param name="p_PlayerID">ID of the player</param>
        /// <param name="p_CancellationToken">Cancellation token</param>
        /// <param name="p_Callback">Request callback</param>
        /// <param name="p_OnFailCallback">On error callback</param>
        private static void GetBeatLeaderAvatarPicture(string p_PlayerID, CancellationToken p_CancellationToken, Action<Sprite> p_Callback, Action p_OnFailCallback)
        {
            WebClientUnity.GlobalClient.GetAsync($"https://api.beatleader.xyz/player/{p_PlayerID}", p_CancellationToken, (p_PlayerResult) =>
            {
                try
                {
                    if (p_PlayerResult == null || !p_PlayerResult.IsSuccessStatusCode || p_PlayerResult.BodyBytes?.Length == 0)
                    {
                        p_OnFailCallback?.Invoke();
                        return;
                    }

                    var l_JSON = JObject.Parse(p_PlayerResult.BodyString);
                    if (l_JSON == null || !l_JSON.ContainsKey("avatar"))
                    {
                        p_OnFailCallback?.Invoke();
                        return;
                    }

                    WebClientUnity.GlobalClient.GetAsync(l_JSON["avatar"].Value<string>(), p_CancellationToken, (p_AvatarResult) =>
                    {
                        try
                        {
                            if (p_AvatarResult == null || !p_AvatarResult.IsSuccessStatusCode || p_AvatarResult.BodyBytes?.Length == 0)
                            {
                                p_OnFailCallback?.Invoke();
                                return;
                            }

                            ProcessAvatarBytes(p_PlayerID, p_Callback, p_AvatarResult.BodyBytes);
                        }
                        catch (Exception l_Exception)
                        {
                            CP_SDK.ChatPlexSDK.Logger.Error("[BeatSaberPlus.SDK.Game][PlayerAvatarPicture.GetBeatLeaderAvatarPicture_2] Error:");
                            CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                            p_OnFailCallback?.Invoke();
                        }
                    });
                }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error("[BeatSaberPlus.SDK.Game][PlayerAvatarPicture.GetBeatLeaderAvatarPicture] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                    p_OnFailCallback?.Invoke();
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process received avatar body bytes
        /// </summary>
        /// <param name="p_PlayerID">ID of the player</param>
        /// <param name="p_Callback">Request callback</param>
        /// <param name="p_BodyBytes">Avatar bytes</param>
        private static void ProcessAvatarBytes(string p_PlayerID, Action<Sprite> p_Callback, byte[] p_BodyBytes)
        {
            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                var l_Texture = CP_SDK.Unity.Texture2DU.CreateFromRaw(p_BodyBytes);
                if (l_Texture == null)
                {
                    p_Callback?.Invoke(null);
                    return;
                }

                var l_Avatar = Sprite.Create(l_Texture, new Rect(0, 0, l_Texture.width, l_Texture.height), new Vector2(0.5f, 0.5f), 100);
                lock (m_AvatarCache)
                {
                    if (!m_AvatarCache.ContainsKey(p_PlayerID))
                        m_AvatarCache.Add(p_PlayerID, l_Avatar);
                }

                p_Callback?.Invoke(l_Avatar);
            });
        }
    }
}
