using BeatSaberMarkupLanguage.Animations;
using BeatSaberPlusChatCore.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.SDK.Chat
{
    /*
       Code from https://github.com/brian91292/EnhancedStreamChat-v3

       MIT License

       Copyright (c) 2020 brian91292

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    /// <summary>
    /// Image provider for twitch
    /// </summary>
    public class ImageProvider
    {
        /// <summary>
        /// Forced emote height
        /// </summary>
        private static int m_ForcedHeight = 110;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cache folder
        /// </summary>
        private static string m_CacheFolder = "UserData/BeatSaberPlus/Cache/Chat/";
        /// <summary>
        /// Cached image info
        /// </summary>
        private static ConcurrentDictionary<string, Unity.EnhancedImage> m_CachedImageInfo = new ConcurrentDictionary<string, Unity.EnhancedImage>();
        /// <summary>
        /// Download queue
        /// </summary>
        private static ConcurrentDictionary<string, Action<byte[]>> m_ActiveDownloads = new ConcurrentDictionary<string, Action<byte[]>>();
        /// <summary>
        /// Cache for sprite sheets
        /// </summary>
        private static ConcurrentDictionary<string, Texture2D> m_CachedSpriteSheets = new ConcurrentDictionary<string, Texture2D>();
        /// <summary>
        /// Cached images info
        /// </summary>
        private static ReadOnlyDictionary<string, Unity.EnhancedImage> m_CachedImageInfoProxy = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cached images info
        /// </summary>
        public static ReadOnlyDictionary<string, Unity.EnhancedImage> CachedImageInfo { get {
            if (m_CachedImageInfoProxy == null)
                m_CachedImageInfoProxy = new ReadOnlyDictionary<string, Unity.EnhancedImage>(m_CachedImageInfo);

            return m_CachedImageInfoProxy;
        } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear cache
        /// </summary>
        internal static void ClearCache()
        {
            if (m_CachedImageInfo.Count > 0)
            {
                foreach (var l_Current in m_CachedImageInfo.Values)
                    MonoBehaviour.Destroy(l_Current.Sprite);

                m_CachedImageInfo.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pre-cache animated image
        /// </summary>
        /// <param name="p_URI">URI of the image</param>
        /// <param name="p_ID">ID of the image</param>
        /// <returns></returns>
        public static void PrecacheAnimatedImage(string p_URI, string p_ID)//, int p_ForcedHeight = -1)
        {
            TryCacheSingleImage(p_ID, p_URI, true, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try to cache single image
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_IsAnimated">Is and animation image</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <returns></returns>
        public static void TryCacheSingleImage(string p_ID, string p_URI, bool p_IsAnimated, Action<Unity.EnhancedImage> p_Finally = null)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var p_Info))
            {
                p_Finally?.Invoke(p_Info);
                return;
            }

            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                SharedCoroutineStarter.instance.StartCoroutine(DownloadContent(p_URI, (p_Bytes) =>
                {
                    OnSingleImageCached(p_ID, p_Bytes, p_IsAnimated, p_Finally, m_ForcedHeight);
                }));
            });
        }
        /// <summary>
        /// Try to cache sprite sheet
        /// </summary>
        /// <param name="p_ID">ID of the sprite sheet</param>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <returns></returns>
        public static void TryCacheSpriteSheetImage(string p_ID, string p_URI, ImageRect p_Rect, Action<Unity.EnhancedImage> p_Finally = null)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var l_Info))
            {
                p_Finally?.Invoke(l_Info);
                return;
            }

            if (m_CachedSpriteSheets.TryGetValue(p_URI, out var l_Texture))
            {
                SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    CacheSpriteSheetImage(p_ID, l_Texture, p_Rect, p_Finally, m_ForcedHeight);
                });
            }
            else
            {
                SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    SharedCoroutineStarter.instance.StartCoroutine(DownloadContent(p_URI, (p_Bytes) =>
                    {
                        //Logger.Instance.Info($"Finished download content for emote {p_ID}!");
                        l_Texture = SDK.Unity.Texture2D.CreateFromRaw(p_Bytes);
                        m_CachedSpriteSheets[p_URI] = l_Texture;

                        CacheSpriteSheetImage(p_ID, l_Texture, p_Rect, p_Finally, m_ForcedHeight);
                    }));
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves the requested content from the provided Uri. Don't yield to this function, as it may return instantly if the download is already queued when you request it.
        /// <para>
        /// The <paramref name="p_Finally"/> callback will *always* be called for this function. If it returns an empty byte array, that should be considered a failure.
        /// </para>
        /// </summary>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_IsRetry">Is a retry attempt</param>
        private static IEnumerator DownloadContent(string p_URI, Action<byte[]> p_Finally, bool p_IsRetry = false)
        {
            if (string.IsNullOrEmpty(p_URI))
            {
                Logger.Instance.Error($"[SDK.Chat][ImageProvider.DownloadContent] URI is null or empty in request for resource {p_URI}. Aborting!");
                p_Finally?.Invoke(null);
                yield break;
            }

            string l_CacheID = "Emote_" + SDK.Cryptography.SHA1.GetHashString(p_URI) + ".dat";
            if (!p_IsRetry && File.Exists(m_CacheFolder + l_CacheID))
            {
                p_Finally?.Invoke(File.ReadAllBytes(m_CacheFolder + l_CacheID));
                yield break;
            }

            if (!p_IsRetry && m_ActiveDownloads.TryGetValue(p_URI, out var activeDownload))
            {
                Logger.Instance.Info($"[SDK.Chat][ImageProvider.DownloadContent] Request already active for {p_URI}");
                yield break;
            }

            if (!m_ActiveDownloads.ContainsKey(p_URI))
                m_ActiveDownloads.TryAdd(p_URI, p_Finally);

            m_ActiveDownloads[p_URI] -= p_Finally;
            m_ActiveDownloads[p_URI] += p_Finally;

            using (UnityWebRequest l_Request = UnityWebRequest.Get(p_URI))
            {
                yield return l_Request.SendWebRequest();

                /// Failed to download due to HTTP error, don't retry
                if (l_Request.isHttpError)
                {
                    m_ActiveDownloads[p_URI]?.Invoke(new byte[0]);
                    m_ActiveDownloads.TryRemove(p_URI, out var d1);
                    yield break;
                }

                if (l_Request.isNetworkError)
                {
                    if (!p_IsRetry)
                    {
                        Logger.Instance.Error($"[SDK.Chat][ImageProvider.DownloadContent] A network error occurred during request to {p_URI}. Retrying in 3 seconds... {l_Request.error}");
                        yield return new WaitForSeconds(3);

                        SharedCoroutineStarter.instance.StartCoroutine(DownloadContent(p_URI, p_Finally, true));

                        yield break;
                    }
                    m_ActiveDownloads[p_URI]?.Invoke(new byte[0]);
                    m_ActiveDownloads.TryRemove(p_URI, out var d2);

                    yield break;
                }

                var l_Data = l_Request.downloadHandler.data;

                if (!Directory.Exists(m_CacheFolder))
                    Directory.CreateDirectory(m_CacheFolder);

                File.WriteAllBytes(m_CacheFolder + l_CacheID, l_Data);

                m_ActiveDownloads[p_URI]?.Invoke(l_Data);
                m_ActiveDownloads.TryRemove(p_URI, out var d3);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On single image cached
        /// </summary>
        /// <param name="p_ID"></param>
        /// <param name="p_Bytes">Result bytes</param>
        /// <param name="p_IsAnimated">Is and animation image</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        private static void OnSingleImageCached(string p_ID, byte[] p_Bytes, bool p_IsAnimated, Action<Unity.EnhancedImage> p_Finally = null, int p_ForcedHeight = -1)
        {
            if (p_IsAnimated)
            {
                SDK.Unity.EnhancedImage.FromRawAnimated(p_ID, AnimationType.GIF, p_Bytes, (p_Result) =>
                {
                    if (p_Result != null)
                        m_CachedImageInfo[p_ID] = p_Result;

                    p_Finally?.Invoke(p_Result);
                }, p_ForcedHeight);
            }
            else
            {
                var l_Result = SDK.Unity.EnhancedImage.FromRawStatic(p_ID, p_Bytes, p_ForcedHeight);
                if (l_Result != null)
                    m_CachedImageInfo[p_ID] = l_Result;

                p_Finally?.Invoke(l_Result);
            }
        }
        /// <summary>
        /// On sprite sheet cached
        /// </summary>
        /// <param name="p_ID">ID of the sprite sheet</param>
        /// <param name="p_Texture">Result texture</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        private static void CacheSpriteSheetImage(string p_ID, Texture2D p_Texture, ImageRect p_Rect, Action<Unity.EnhancedImage> p_Finally = null, int p_ForcedHeight = -1)
        {
            var l_Result = Unity.EnhancedImage.FromSpriteSheetImage(p_ID, p_Texture, new Rect(p_Rect.x, p_Rect.y, p_Rect.width, p_Rect.height), p_ForcedHeight);

            if (l_Result != null)
                m_CachedImageInfo[p_ID] = l_Result;

            p_Finally?.Invoke(l_Result);
        }
    }
}
