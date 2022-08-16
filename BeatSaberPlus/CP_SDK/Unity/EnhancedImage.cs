#if CP_SDK_UNITY
using System;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Enhanced image info
    /// </summary>
    public class EnhancedImage
    {
        /// <summary>
        /// ID of the image
        /// </summary>
        public string ImageID { get; set; }
        /// <summary>
        /// Sprite instance
        /// </summary>
        public UnityEngine.Sprite Sprite { get; set; }
        /// <summary>
        /// Width
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Height
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Animation controller data
        /// </summary>
        public Animation.AnimationControllerInstance AnimControllerData { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// From raw
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Bytes">Result bytes</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public static void FromRawStatic(string p_ID, byte[] p_Bytes, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            SpriteU.CreateFromRawEx(p_Bytes, (p_Sprite) => OnRawStaticCallback(p_ID, p_Sprite, p_Callback, p_ForcedHeight));
        }
        /// <summary>
        /// From raw animated
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Type">Animation type</param>
        /// <param name="p_Bytes">Result bytes</param>
        /// <param name="p_Callback">A callback that occurs after the resource is retrieved. This will always occur even if the resource is already cached.</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public static void FromRawAnimated(string p_ID, Animation.EAnimationType p_Type, byte[] p_Bytes, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            Animation.AnimationLoader.Load(
                p_Type,
                p_Bytes,
                (p_Texture, p_UVs, p_Delays, p_Width, p_Height) => OnRawAnimatedCallback(p_ID, p_Texture, p_UVs, p_Delays, p_Width, p_Height, p_Callback, p_ForcedHeight),
                (p_Sprite)                                      => OnRawStaticCallback(p_ID, p_Sprite, p_Callback, p_ForcedHeight)
            );
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static void OnRawStaticCallback(string p_ID, UnityEngine.Sprite p_Sprite, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            /// RUN ON MAIN THREAD

            int l_SpriteWidth = 0;
            int l_SpriteHeight = 0;

            if (p_Sprite != null)
            {
                l_SpriteWidth   = p_Sprite.texture.width;
                l_SpriteHeight  = p_Sprite.texture.height;
            }

            EnhancedImage l_Result = null;
            if (p_Sprite != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref l_SpriteWidth, ref l_SpriteHeight, p_ForcedHeight);

                l_Result = new EnhancedImage()
                {
                    ImageID = p_ID,
                    Sprite  = p_Sprite,
                    Width   = l_SpriteWidth,
                    Height  = l_SpriteHeight,
                    AnimControllerData = null
                };

                if (p_ForcedHeight != -1)
                    l_Result.EnsureValidForHeight(p_ForcedHeight);
            }

            p_Callback?.Invoke(l_Result);
        }
        private static void OnRawAnimatedCallback(string p_ID, UnityEngine.Texture2D p_Texture, Rect[] p_UVs, ushort[] p_Delays, int p_Width, int p_Height, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            /// RUN ON MAIN THREAD

            if (p_Texture == null)
            {
                p_Callback?.Invoke(null);
                return;
            }

            var l_AnimControllerData    = Animation.AnimationControllerManager.instance.Register(p_ID, p_Texture, p_UVs, p_Delays);
            var l_AnimResult            = null as EnhancedImage;

            if (l_AnimControllerData != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref p_Width, ref p_Height, p_ForcedHeight);

                l_AnimResult = new EnhancedImage()
                {
                    ImageID             = p_ID,
                    Sprite              = l_AnimControllerData.FirstFrame,
                    Width               = p_Width,
                    Height              = p_Height,
                    AnimControllerData  = l_AnimControllerData
                };

                if (p_ForcedHeight != -1)
                    l_AnimResult.EnsureValidForHeight(p_ForcedHeight);
            }

            p_Callback?.Invoke(l_AnimResult);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Compute image size for specific height
        /// </summary>
        /// <param name="p_SpriteWidth">Base width</param>
        /// <param name="p_SpriteHeight">Base height</param>
        /// <param name="p_Height">Desired height</param>
        private static void ComputeImageSizeForHeight(ref int p_SpriteWidth, ref int p_SpriteHeight, int p_Height)
        {
            /// Quick exit
            if (p_SpriteHeight == p_Height)
                return;

            /// 1:1 ratio quick case
            if (p_SpriteHeight == p_SpriteWidth)
            {
                p_SpriteWidth   = p_Height;
                p_SpriteHeight  = p_Height;
            }
            else
            {
                double l_Scale = ((double)p_Height) / ((double)p_SpriteHeight);

                p_SpriteWidth   = (int)(l_Scale * ((double)p_SpriteWidth));
                p_SpriteHeight  = (int)(l_Scale * ((double)p_SpriteHeight));
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ensure image valid
        /// </summary>
        public void EnsureValidForHeight(int p_ForcedHeight)
        {
            if (Height < 0 || Width < 0)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Unity][EnhancedImageInfo.EnsureValidForHeight] Invalid emote ImageID {ImageID} Width {Width} height {Height}");
                Width   = p_ForcedHeight;
                Height  = p_ForcedHeight;
                return;
            }

            if (Height > p_ForcedHeight)
                Height = p_ForcedHeight;

            if (Width > (6 * p_ForcedHeight))
            {
                Width = p_ForcedHeight;
                ChatPlexSDK.Logger.Error($"[CP_SDK.Unity][EnhancedImageInfo.EnsureValidForHeight] Too wide emote ImageID {ImageID} Width {Width} height {Height}");
            }
        }
    }
}
#endif