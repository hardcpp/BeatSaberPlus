using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace CP_SDK.Chat
{
    /// <summary>
    /// Image provider for twitch
    /// </summary>
    public class ChatImageProvider
    {
        /// <summary>
        /// Forced emote height
        /// </summary>
        private static int m_ForcedHeight = 110;
        /// <summary>
        /// Cache folder
        /// </summary>
        private static string m_CacheFolder;
        /// <summary>
        /// Is caching enabled
        /// </summary>
        private static bool m_CacheEnabled = true;
        /// <summary>
        /// Network client
        /// </summary>
        private static Network.WebClient m_WebClient;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cached image info
        /// </summary>
        private static ConcurrentDictionary<string, Unity.EnhancedImage> m_CachedImageInfo = new ConcurrentDictionary<string, Unity.EnhancedImage>();
        /// <summary>
        /// Cached emote info
        /// </summary>
        private static ConcurrentDictionary<string, Unity.EnhancedImage> m_CachedEmoteInfo = new ConcurrentDictionary<string, Unity.EnhancedImage>();
        /// <summary>
        /// Download queue
        /// </summary>
        private static ConcurrentDictionary<string, Action<byte[]>> m_ActiveDownloads = new ConcurrentDictionary<string, Action<byte[]>>();
        /// <summary>
        /// Cached images info
        /// </summary>
        private static ReadOnlyDictionary<string, Unity.EnhancedImage> m_CachedImageInfoProxy = null;
        /// <summary>
        /// Cached emotes info
        /// </summary>
        private static ReadOnlyDictionary<string, Unity.EnhancedImage> m_CachedEmoteInfoProxy = null;

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
        /// <summary>
        /// Cached emotes info
        /// </summary>
        public static ReadOnlyDictionary<string, Unity.EnhancedImage> CachedEmoteInfo { get {
            if (m_CachedEmoteInfoProxy == null)
                m_CachedEmoteInfoProxy = new ReadOnlyDictionary<string, Unity.EnhancedImage>(m_CachedEmoteInfo);

            return m_CachedEmoteInfoProxy;
        } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init image provider
        /// </summary>
        internal static void Init()
        {
            m_CacheFolder   = Path.Combine(ChatPlexSDK.BasePath, $"UserData/{ChatPlexSDK.ProductName}/Cache/Chat/");
            m_WebClient     = new Network.WebClient("", TimeSpan.FromSeconds(10));
            m_WebClient.Timeout = 10;

            try
            {
                if (!Directory.Exists(m_CacheFolder))
                    Directory.CreateDirectory(m_CacheFolder);

                m_CacheEnabled = true;
            }
            catch (Exception l_Exception)
            {
                m_CacheEnabled = false;

                ChatPlexSDK.Logger.Error($"[CP_SDK.Chat][ImageProvider.Init] Error creating cache folder, disabling caching:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// Clear cache
        /// </summary>
        internal static void ClearCache()
        {
            if (m_CachedImageInfo.Count > 0)
            {
                foreach (var l_Current in m_CachedImageInfo.Values)
                {
                    if (l_Current != null)
                        MonoBehaviour.Destroy(l_Current.Sprite);
                }

                m_CachedImageInfo.Clear();
                m_CachedEmoteInfo.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try to cache single image
        /// </summary>
        /// <param name="p_Category">Category</param>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_URL">The resource location</param>
        /// <param name="p_Animation">Is and animated image</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <returns></returns>
        public static void TryCacheSingleImage(Interfaces.EChatResourceCategory p_Category, string p_ID, string p_URL, Animation.EAnimationType p_Animation, Action<Unity.EnhancedImage> p_Finally = null)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var p_Info))
            {
                p_Finally?.Invoke(p_Info);
                return;
            }

            if (string.IsNullOrEmpty(p_URL))
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Chat][ChatImageProvider.TryCacheSingleImage] URI is null or empty in request for resource {p_URL} ID:{p_ID}. Aborting!");

                m_CachedImageInfo[p_URL] = null;
                p_Finally?.Invoke(null);

                return;
            }

            string l_CacheID = "";
            if (m_CacheEnabled)
                l_CacheID = "Emote_" + Cryptography.SHA1.GetHashString(p_URL) + ".dat";

            LoadFromCacheOrDownload(p_URL, l_CacheID, (p_Bytes) =>
            {
                if (m_CachedImageInfo.TryGetValue(p_ID, out var p_Info2))
                {
                    p_Finally?.Invoke(p_Info2);
                    return;
                }

                if (p_Animation != Animation.EAnimationType.NONE)
                {
                    Unity.EnhancedImage.FromRawAnimated(p_ID, p_Animation, p_Bytes, (p_Result) =>
                    {
                        m_CachedImageInfo[p_ID] = p_Result;

                        if (p_Result != null && p_Category == Interfaces.EChatResourceCategory.Emote)
                            m_CachedEmoteInfo[p_ID] = p_Result;

                        p_Finally?.Invoke(p_Result);
                    }, m_ForcedHeight);
                }
                else
                {
                    Unity.EnhancedImage.FromRawStatic(p_ID, p_Bytes, (p_Result) =>
                    {
                        m_CachedImageInfo[p_ID] = p_Result;

                        if (p_Result != null && p_Category == Interfaces.EChatResourceCategory.Emote)
                            m_CachedEmoteInfo[p_ID] = p_Result;

                        p_Finally?.Invoke(p_Result);
                    }, m_ForcedHeight);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load from cache or download
        /// </summary>
        /// <param name="p_URL">The resource location</param>
        /// <param name="p_CacheID">Cache ID</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        private static void LoadFromCacheOrDownload(string p_URL, string p_CacheID, Action<byte[]> p_Finally)
        {
            Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                if (m_CacheEnabled)
                {
                    if (File.Exists(m_CacheFolder + p_CacheID))
                    {
                        var l_Content = File.ReadAllBytes(m_CacheFolder + p_CacheID);
                        p_Finally?.Invoke(l_Content);
                        return;
                    }
                }

                DownloadContent(p_URL, p_CacheID, p_Finally);
            });
        }
        /// <summary>
        /// Retrieves the requested content from the provided Uri. Don't yield to this function, as it may return instantly if the download is already queued when you request it.
        /// <para>
        /// The <paramref name="p_Finally"/> callback will *always* be called for this function. If it returns an empty byte array, that should be considered a failure.
        /// </para>
        /// </summary>
        /// <param name="p_URL">The resource location</param>
        /// <param name="p_CacheID">Cache ID</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_IsRetry">Is a retry attempt</param>
        private static void DownloadContent(string p_URL, string p_CacheID, Action<byte[]> p_Finally)
        {
            if (m_ActiveDownloads.TryGetValue(p_URL, out var l_Active))
            {
                l_Active -= p_Finally;
                l_Active += p_Finally;
                return;
            }

            if (!m_ActiveDownloads.ContainsKey(p_URL))
                m_ActiveDownloads.TryAdd(p_URL, p_Finally);

            m_ActiveDownloads[p_URL] -= p_Finally;
            m_ActiveDownloads[p_URL] += p_Finally;

            m_WebClient.GetAsync(p_URL, System.Threading.CancellationToken.None, (p_Result) =>
            {
                if (p_Result == null)
                {
                    m_ActiveDownloads[p_URL]?.Invoke(null);
                    m_ActiveDownloads.TryRemove(p_URL, out var _);
                    return;
                }

                if (m_CacheEnabled && p_Result?.BodyBytes != null && p_Result.BodyBytes.Length > 0)
                {
                    Unity.MTThreadInvoker.EnqueueOnThread(() =>
                    {
                        if (!Directory.Exists(m_CacheFolder))
                            Directory.CreateDirectory(m_CacheFolder);

                        File.WriteAllBytes(m_CacheFolder + p_CacheID, p_Result.BodyBytes);
                    });
                }

                m_ActiveDownloads[p_URL]?.Invoke(p_Result.BodyBytes);
                m_ActiveDownloads.TryRemove(p_URL, out var _);
            });
        }
    }
}
