using System;
using System.Reflection;
using UnityEngine;

namespace ChatPlexMod_MenuMusic.Utils
{
    /// <summary>
    /// Art provider for the player floating panel
    /// </summary>
    internal class ArtProvider
    {
        private static Color[] m_BackgroundMask = null;
        private static Color[] m_CoverMask = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prepare
        /// </summary>
        /// <param name="p_RawByte">Input raw bytes</param>
        /// <param name="p_CancellationToken">Cancellation token</param>
        /// <param name="p_Callback">Result callback</param>
        public static void Prepare(byte[] p_RawByte, CP_SDK.Misc.FastCancellationToken p_CancellationToken, Action<Sprite, Sprite> p_Callback)
            => CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => PrepareImpl(p_RawByte, p_CancellationToken, p_Callback));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prepare implementation
        /// </summary>
        /// <param name="p_RawByte">Input raw byte</param>
        /// <param name="p_CancellationToken">Cancellation token</param>
        /// <param name="p_Callback">Result callback</param>
        private static void PrepareImpl(byte[] p_RawByte, CP_SDK.Misc.FastCancellationToken p_CancellationToken, Action<Sprite, Sprite> p_Callback)
        {
            try
            {
                var l_StartSerial = p_CancellationToken?.Serial ?? 0;

                if (m_BackgroundMask == null)
                {
                    var l_Bytes = CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "ChatPlexMod_MenuMusic.Resources.BackgroundMask.png");
                    CP_SDK.Unity.TextureRaw.Load(l_Bytes, out _, out _, out m_BackgroundMask);
                }

                if (m_CoverMask == null)
                {
                    var l_Bytes = CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "ChatPlexMod_MenuMusic.Resources.CoverMask.png");
                    CP_SDK.Unity.TextureRaw.Load(l_Bytes, out _, out _, out m_CoverMask);
                }

                if (p_CancellationToken?.IsCancelled(l_StartSerial) ?? false)
                    return;

                if (!CP_SDK.Unity.TextureRaw.Load(p_RawByte, out var l_OGWidth, out var l_OGHeight, out var l_OGPixels))
                {
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(null, null));
                    return;
                }

                var l_CoverSize         = new Vector2Int(18 * 4 * 10, 18 * 4 * 10);
                var l_BackgroundSize    = new Vector2Int(80 * 1 * 10, 20 * 1 * 10);
                var l_CoverPixels       = CP_SDK.Unity.TextureRaw.ResampleAndCrop(l_OGWidth,        l_OGHeight,    l_OGPixels,     l_CoverSize.x,       l_CoverSize.y);
                var l_BackgroundPixels  = CP_SDK.Unity.TextureRaw.ResampleAndCrop(l_CoverSize.x, l_CoverSize.y, l_CoverPixels, l_BackgroundSize.x, l_BackgroundSize.y);

                if (p_CancellationToken?.IsCancelled(l_StartSerial) ?? false)
                    return;

                CP_SDK.Unity.TextureRaw.FastGaussianBlur(l_BackgroundSize.x, l_BackgroundSize.y, l_BackgroundPixels, 4);

                if (p_CancellationToken?.IsCancelled(l_StartSerial) ?? false)
                    return;

                CP_SDK.Unity.TextureRaw.Multiply(l_CoverPixels,         m_CoverMask);
                CP_SDK.Unity.TextureRaw.Multiply(l_BackgroundPixels,    m_BackgroundMask);

                if (p_CancellationToken?.IsCancelled(l_StartSerial) ?? false)
                    return;

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    try
                    {
                        if (p_CancellationToken?.IsCancelled(l_StartSerial) ?? false)
                            return;

                        var l_CoverTexture = new Texture2D(l_CoverSize.x, l_CoverSize.y, TextureFormat.RGBA32, false);
                        l_CoverTexture.wrapMode = TextureWrapMode.Clamp;
                        l_CoverTexture.SetPixels(l_CoverPixels);
                        l_CoverTexture.Apply(true);

                        var l_BackgroundTexture = new Texture2D(l_BackgroundSize.x, l_BackgroundSize.y, TextureFormat.RGBA32, false);
                        l_BackgroundTexture.wrapMode = TextureWrapMode.Clamp;
                        l_BackgroundTexture.SetPixels(l_BackgroundPixels);
                        l_BackgroundTexture.Apply(true);

                        p_Callback?.Invoke(
                            CP_SDK.Unity.SpriteU.CreateFromTexture(l_CoverTexture),
                            CP_SDK.Unity.SpriteU.CreateFromTexture(l_BackgroundTexture)
                        );
                    }
                    catch (System.Exception l_Exception)
                    {
                        Logger.Instance.Error("[ChatPlexMod_MenuMusic.Utils][ArtProvider.PrepareImpl] Error:");
                        Logger.Instance.Error(l_Exception);
                    }
                });

                return;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_MenuMusic.Utils][ArtProvider.PrepareImpl] Error:");
                Logger.Instance.Error(l_Exception);
            }

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => p_Callback?.Invoke(null, null));
        }
    }
}
