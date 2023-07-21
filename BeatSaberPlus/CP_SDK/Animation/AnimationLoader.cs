using System;
using System.Collections;
using UnityEngine;

namespace CP_SDK.Animation
{
    /// <summary>
    /// Animation type
    /// </summary>
    public enum EAnimationType
    {
        NONE,
        GIF,
        APNG,
        WEBP,
        AUTODETECT
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Animation loader
    /// </summary>
    public static class AnimationLoader
    {
        /// <summary>
        /// End of frame waiter
        /// </summary>
        private static WaitForEndOfFrame m_EndOfFrameWaiter = new WaitForEndOfFrame();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load animation
        /// </summary>
        /// <param name="p_Type">Animation type</param>
        /// <param name="p_Data">Raw data</param>
        /// <param name="p_Callback">Animated callback</param>
        /// <param name="p_StaticCallback">Static callback</param>
        public static void Load(    EAnimationType                                  p_Type,
                                    byte[]                                          p_Data,
                                    Action<Texture2D, Rect[], ushort[], int, int>   p_Callback,
                                    Action<Sprite>                                  p_StaticCallback)
        {
            switch (p_Type)
            {
                case EAnimationType.GIF:
                    GIF.GIFDecoder.Process(
                        p_Data,
                        (p_AnimationInfo) => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_ProcessLoadedAnimation(p_AnimationInfo, p_Callback))
                    );
                    break;
                case EAnimationType.APNG:
                    APNG.APNGUnityDecoder.Process(
                        p_Data,
                        (p_AnimationInfo) => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_ProcessLoadedAnimation(p_AnimationInfo, p_Callback))
                    );
                    break;
                case EAnimationType.WEBP:
                    WEBP.WEBPDecoder.Process(p_Data,
                        (p_AnimationInfo) => Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_ProcessLoadedAnimation(p_AnimationInfo, p_Callback)),
                        p_StaticCallback
                    );
                    break;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process loaded animation
        /// </summary>
        /// <param name="p_AnimationInfo">Animation infos</param>
        /// <param name="p_Callback">Callback</param>
        /// <returns></returns>
        private static IEnumerator Coroutine_ProcessLoadedAnimation(AnimationInfo p_AnimationInfo, Action<Texture2D, Rect[], ushort[], int, int> p_Callback)
        {
            if (p_AnimationInfo == null)
                p_Callback?.Invoke(null, null, null, 0, 0);

            var l_MaxAtlasTextureSize   = GetMaxAtlasTextureSize(p_AnimationInfo);
            var l_AtlasTexture          = new Texture2D(p_AnimationInfo.Width, p_AnimationInfo.Height);
            var l_SubTextures           = new Texture2D[p_AnimationInfo.Frames.Length];

            for (var l_FrameI = 0; l_FrameI < p_AnimationInfo.Frames.Length; ++l_FrameI)
            {
                var l_FrameTexture = new Texture2D(p_AnimationInfo.Width, p_AnimationInfo.Height, TextureFormat.RGBA32, false);
                l_FrameTexture.wrapMode = TextureWrapMode.Clamp;

                try
                {
                    l_FrameTexture.SetPixels32(p_AnimationInfo.Frames[l_FrameI]);
                    l_FrameTexture.Apply(l_FrameI == 0);
                }
                catch
                {
                    yield break;
                }

                l_SubTextures[l_FrameI] = l_FrameTexture;

                yield return m_EndOfFrameWaiter;
            }

            var l_UVs = l_AtlasTexture.PackTextures(l_SubTextures, 2, l_MaxAtlasTextureSize, true);

            for (int l_I = 0; l_I < l_SubTextures.Length; ++l_I)
                GameObject.Destroy(l_SubTextures[l_I]);

            p_Callback?.Invoke(l_AtlasTexture, l_UVs, p_AnimationInfo.Delays, p_AnimationInfo.Width, p_AnimationInfo.Height);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get max atlas texture size
        /// </summary>
        /// <param name="p_AnimationInfo">Animation infos</param>
        /// <returns></returns>
        private static int GetMaxAtlasTextureSize(AnimationInfo p_AnimationInfo)
        {
            var l_TestNumber        = 2;
            var l_FramesInRow       = 0;
            var l_FramesInColumn    = 0;
            var l_FrameCount        = p_AnimationInfo.Frames.Length;

            while (true)
            {
                /// Make sure the number of frames is cleanly divisible by our testNum
                if (!(l_FrameCount % l_TestNumber != 0))
                    l_FrameCount += l_FrameCount % l_TestNumber;

                l_FramesInRow       = Mathf.Max(1, l_FrameCount / l_TestNumber);
                l_FramesInColumn    = l_FrameCount / l_FramesInRow;

                if (l_FramesInRow <= l_FramesInColumn)
                    break;

                l_TestNumber += 2;
            }

            var l_TextureWidth  = Mathf.Clamp(l_FramesInRow     * p_AnimationInfo.Width,  0, 2048);
            var l_TextureHeight = Mathf.Clamp(l_FramesInColumn  * p_AnimationInfo.Height, 0, 2048);

            return Mathf.Max(l_TextureWidth, l_TextureHeight);
        }
    }
}
