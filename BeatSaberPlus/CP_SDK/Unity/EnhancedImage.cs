using System;
using System.IO;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Enhanced image info
    /// </summary>
    public class EnhancedImage
    {
        /// <summary>
        /// Animated gif byte pattern for fast lookup
        /// </summary>
        private static byte[] ANIMATED_GIF_PATTERN = new byte[] { 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30 };
        /// <summary>
        /// WEBPV8 byte pattern
        /// </summary>
        private static byte[] WEBPVP8_PATTERN = new byte[] { 0x57, 0x45, 0x42, 0x50, 0x56, 0x50, 0x38 };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ID of the image
        /// </summary>
        public string ImageID { get; set; }
        /// <summary>
        /// Sprite instance
        /// </summary>
        public Sprite Sprite { get; set; }
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
        /// Ensure image valid
        /// </summary>
        /// <param name="p_ForcedHeight">Forced height</param>
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// From raw
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Bytes">Result bytes</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public static void FromRawStatic(string p_ID, byte[] p_Bytes, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            SpriteU.CreateFromRawWithBordersThreaded(p_Bytes, (p_Sprite) => OnRawStaticCallback(p_ID, p_Sprite, p_Callback, p_ForcedHeight));
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
            if (p_Type == Animation.EAnimationType.AUTODETECT && p_Bytes != null && p_Bytes.Length > 0)
            {
                if (p_Bytes.Length > 3 && p_Bytes[0] == 0x47 && ContainBytePattern(p_Bytes, ANIMATED_GIF_PATTERN))
                    p_Type = Animation.EAnimationType.GIF;
                else if (p_Bytes.Length > 16 && ContainBytePattern(p_Bytes, WEBPVP8_PATTERN))
                    p_Type = Animation.EAnimationType.WEBP;
                else
                    p_Type = Animation.EAnimationType.NONE;
            }

            if (p_Type == Animation.EAnimationType.NONE)
            {
                FromRawStatic(p_ID, p_Bytes, p_Callback, p_ForcedHeight);
                return;
            }

            Animation.AnimationLoader.Load(
                p_Type,
                p_Bytes,
                (p_Texture, p_UVs, p_Delays, p_Width, p_Height) => OnRawAnimatedCallback(p_ID, p_Texture, p_UVs, p_Delays, p_Width, p_Height, p_Callback, p_ForcedHeight),
                (p_Sprite)                                      => OnRawStaticCallback(p_ID, p_Sprite, p_Callback, p_ForcedHeight)
            );
        }
        /// <summary>
        /// From file
        /// </summary>
        /// <param name="p_FileName">File name</param>
        /// <param name="p_ID">ID</param>
        /// <param name="p_Callback">On finish callback</param>
        public static void FromFile(string p_FileName, string p_ID, Action<EnhancedImage> p_Callback)
        {
            if (p_FileName.ToLower().EndsWith(".png"))
            {
                FromRawStatic(p_ID, File.ReadAllBytes(p_FileName), (p_Result) =>
                {
                    if (p_Result != null)
                    {
                        p_Result.Sprite.texture.wrapMode = TextureWrapMode.Mirror;
                        p_Callback?.Invoke(p_Result);
                    }
                    else
                        ChatPlexSDK.Logger.Error("[CP_SDK.Unity][EnhancedImage.FromFile] Failed to load image " + p_FileName);
                });
            }
            else if (p_FileName.ToLower().EndsWith(".gif"))
            {
                FromRawAnimated(
                    p_ID,
                    Animation.EAnimationType.AUTODETECT,
                    File.ReadAllBytes(p_FileName), (p_Result) =>
                    {
                        if (p_Result != null)
                            p_Callback?.Invoke(p_Result);
                        else
                            ChatPlexSDK.Logger.Error("[CP_SDK.Unity][EnhancedImage.FromFile] Failed to load image " + p_FileName);
                    });
            }
            else if (p_FileName.ToLower().EndsWith(".apng"))
            {
                EnhancedImage.FromRawAnimated(
                    p_ID,
                    Animation.EAnimationType.APNG,
                    File.ReadAllBytes(p_FileName), (p_Result) =>
                    {
                        if (p_Result != null)
                            p_Callback?.Invoke(p_Result);
                        else
                            ChatPlexSDK.Logger.Error("[CP_SDK.Unity][EnhancedImage.FromFile] Failed to load image " + p_FileName);
                    });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static void OnRawStaticCallback(string p_ID, Sprite p_Sprite, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
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
        private static void OnRawAnimatedCallback(string p_ID, Texture2D p_Texture, Rect[] p_UVs, ushort[] p_Delays, int p_Width, int p_Height, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            /// RUN ON MAIN THREAD

            if (p_Texture == null)
            {
                p_Callback?.Invoke(null);
                return;
            }

            var l_AnimControllerData    = Animation.AnimationControllerManager.Instance.Register(p_ID, p_Texture, p_UVs, p_Delays);
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
        /// Fast lookup for byte pattern
        /// </summary>
        /// <param name="p_Array">Input array</param>
        /// <param name="p_Pattern">Lookup pattern</param>
        /// <returns></returns>
        private static bool ContainBytePattern(byte[] p_Array, byte[] p_Pattern)
        {
            var l_PatternPosition = 0;
            for (int l_I = 0; l_I < p_Array.Length; ++l_I)
            {
                if (p_Array[l_I] != p_Pattern[l_PatternPosition])
                {
                    l_PatternPosition = 0;
                    continue;
                }

                l_PatternPosition++;
                if (l_PatternPosition == p_Pattern.Length)
                    return true;
            }

            return l_PatternPosition == p_Pattern.Length;
        }
    }
}