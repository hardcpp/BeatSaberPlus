using BeatSaberMarkupLanguage.Animations;
using BeatSaberPlusChatCore.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.Utils
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
    public class ChatImageProvider : PersistentSingleton<ChatImageProvider>
    {
        /// <summary>
        /// Cached images info
        /// </summary>
        public ReadOnlyDictionary<string, EnhancedImageInfo> CachedImageInfo { get; internal set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cached image info
        /// </summary>
        private ConcurrentDictionary<string, EnhancedImageInfo> m_CachedImageInfo = new ConcurrentDictionary<string, EnhancedImageInfo>();
        /// <summary>
        /// Download queue
        /// </summary>
        private ConcurrentDictionary<string, Action<byte[]>> m_ActiveDownloads = new ConcurrentDictionary<string, Action<byte[]>>();
        /// <summary>
        /// Cache for sprite sheets
        /// </summary>
        private ConcurrentDictionary<string, Texture2D> m_CachedSpriteSheets = new ConcurrentDictionary<string, Texture2D>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On GameObject awake
        /// </summary>
        private void Awake()
        {
            CachedImageInfo = new ReadOnlyDictionary<string, EnhancedImageInfo>(m_CachedImageInfo);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear cache
        /// </summary>
        internal static void ClearCache()
        {
            if (instance.m_CachedImageInfo.Count > 0)
            {
                foreach (var l_Current in instance.m_CachedImageInfo.Values)
                    Destroy(l_Current.Sprite);

                instance.m_CachedImageInfo.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pre-cache animated image
        /// </summary>
        /// <param name="p_URI">URI of the image</param>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_ForcedHeight">Force height</param>
        /// <returns></returns>
        public IEnumerator PrecacheAnimatedImage(string p_URI, string p_ID, int p_ForcedHeight = -1)
        {
            yield return TryCacheSingleImage(p_ID, p_URI, true);
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
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public IEnumerator TryCacheSingleImage(string p_ID, string p_URI, bool p_IsAnimated, Action<EnhancedImageInfo> p_Finally = null, int p_ForcedHeight = -1)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var p_Info))
            {
                p_Finally?.Invoke(p_Info);
                yield break;
            }

            StartCoroutine(ChatImageProvider.instance.DownloadContent(p_URI, (p_Bytes) =>
            {
                //Logger.Instance.Info($"Finished download content for emote {p_ID}!");
                StartCoroutine(OnSingleImageCached(p_ID, p_Bytes, p_IsAnimated, p_Finally, p_ForcedHeight));
            }));
        }
        /// <summary>
        /// Try to cache sprite sheet
        /// </summary>
        /// <param name="p_ID">ID of the sprite sheet</param>
        /// <param name="p_URI">The resource location</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public void TryCacheSpriteSheetImage(string p_ID, string p_URI, ImageRect p_Rect, Action<EnhancedImageInfo> p_Finally = null, int p_ForcedHeight = -1)
        {
            if (m_CachedImageInfo.TryGetValue(p_ID, out var info))
            {
                p_Finally?.Invoke(info);
                return;
            }

            if (m_CachedSpriteSheets.TryGetValue(p_URI, out var l_Texture))
                CacheSpriteSheetImage(p_ID, l_Texture, p_Rect, p_Finally, p_ForcedHeight);
            else
            {
                StartCoroutine(ChatImageProvider.instance.DownloadContent(p_URI, (p_Bytes) =>
                {
                    //Logger.Instance.Info($"Finished download content for emote {p_ID}!");
                    l_Texture = BeatSaberPlus.Utils.UnityTexture.LoadTextureRaw(p_Bytes);
                    m_CachedSpriteSheets[p_URI] = l_Texture;

                    CacheSpriteSheetImage(p_ID, l_Texture, p_Rect, p_Finally, p_ForcedHeight);
                }));
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
        private IEnumerator DownloadContent(string p_URI, Action<byte[]> p_Finally, bool p_IsRetry = false)
        {
            if (string.IsNullOrEmpty(p_URI))
            {
                Logger.Instance.Error($"URI is null or empty in request for resource {p_URI}. Aborting!");
                p_Finally?.Invoke(null);
                yield break;
            }

            if (!p_IsRetry && m_ActiveDownloads.TryGetValue(p_URI, out var activeDownload))
            {
                Logger.Instance.Info($"Request already active for {p_URI}");
                yield break;
            }

            if (!m_ActiveDownloads.ContainsKey(p_URI))
            {
                m_ActiveDownloads.TryAdd(p_URI, p_Finally);
            }

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
                        Logger.Instance.Error($"A network error occurred during request to {p_URI}. Retrying in 3 seconds... {l_Request.error}");
                        yield return new WaitForSeconds(3);

                        StartCoroutine(DownloadContent(p_URI, p_Finally, true));

                        yield break;
                    }
                    m_ActiveDownloads[p_URI]?.Invoke(new byte[0]);
                    m_ActiveDownloads.TryRemove(p_URI, out var d2);

                    yield break;
                }

                var l_Data = l_Request.downloadHandler.data;

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
        private IEnumerator OnSingleImageCached(string p_ID, byte[] p_Bytes, bool p_IsAnimated,Action<EnhancedImageInfo> p_Finally = null, int p_ForcedHeight = -1)
        {
            int l_SpriteWidth = 0;
            int l_SpriteHeight = 0;

            Sprite l_Sprite = null;

            AnimationControllerData l_AnimControllerData = null;

            if (p_IsAnimated)
            {
                AnimationLoader.Process(AnimationType.GIF, p_Bytes, (p_Texture, p_Atlas, p_Delays, p_Width, p_Height) =>
                {
                    l_AnimControllerData    = AnimationController.instance.Register(p_ID, p_Texture, p_Atlas, p_Delays);
                    l_Sprite                = l_AnimControllerData.sprite;
                    l_SpriteWidth           = p_Width;
                    l_SpriteHeight          = p_Height;
                });

                yield return new WaitUntil(() => l_AnimControllerData != null);
            }
            else
            {
                try
                {
                    l_Sprite        = BeatSaberPlus.Utils.UnityTexture.LoadSpriteRaw(p_Bytes);
                    l_SpriteWidth   = l_Sprite.texture.width;
                    l_SpriteHeight  = l_Sprite.texture.height;
                }
                catch (Exception p_Exception)
                {
                    Logger.Instance.Error("Error in OnSingleImageCached");
                    Logger.Instance.Error(p_Exception);
                    l_Sprite = null;
                }
            }

            EnhancedImageInfo l_Result = null;
            if (l_Sprite != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref l_SpriteWidth, ref l_SpriteHeight, p_ForcedHeight);

                l_Result = new EnhancedImageInfo()
                {
                    ImageID             = p_ID,
                    Sprite              = l_Sprite,
                    Width               = l_SpriteWidth,
                    Height              = l_SpriteHeight,
                    AnimControllerData  = l_AnimControllerData
                };

                m_CachedImageInfo[p_ID] = l_Result;
            }

            p_Finally?.Invoke(l_Result);
        }
        /// <summary>
        /// On sprite sheet cached
        /// </summary>
        /// <param name="p_ID">ID of the sprite sheet</param>
        /// <param name="p_Texture">Result texture</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_Finally">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        private void CacheSpriteSheetImage(string p_ID, Texture2D p_Texture, ImageRect p_Rect, Action<EnhancedImageInfo> p_Finally = null, int p_ForcedHeight = -1)
        {
            int l_SpriteWidth = p_Rect.width;
            int l_SpriteHeight = p_Rect.height;

            Sprite l_Sprite = Sprite.Create(p_Texture, new Rect(p_Rect.x, p_Texture.height - p_Rect.y - l_SpriteHeight, l_SpriteWidth, l_SpriteHeight), new Vector2(0, 0));
            l_Sprite.texture.wrapMode = TextureWrapMode.Clamp;

            EnhancedImageInfo l_Result = null;
            if (l_Sprite != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref l_SpriteWidth, ref l_SpriteHeight, p_ForcedHeight);

                l_Result = new EnhancedImageInfo()
                {
                    ImageID             = p_ID,
                    Sprite              = l_Sprite,
                    Width               = l_SpriteWidth,
                    Height              = l_SpriteHeight,
                    AnimControllerData  = null
                };

                m_CachedImageInfo[p_ID] = l_Result;
            }

            p_Finally?.Invoke(l_Result);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Compute image size for specific height
        /// </summary>
        /// <param name="p_SpriteHeight">Base height</param>
        /// <param name="p_SpriteWidth">Base width</param>
        /// <param name="p_Height">Desired height</param>
        private void ComputeImageSizeForHeight(ref int p_SpriteHeight, ref int p_SpriteWidth, int p_Height)
        {
            float l_Scale = 1.0f;

            if (p_SpriteHeight != (float)p_Height)
                l_Scale = (float)p_Height / p_SpriteHeight;

            p_SpriteWidth   = (int)(l_Scale * p_SpriteWidth);
            p_SpriteHeight  = (int)(l_Scale * p_SpriteHeight);
        }
    }
}
