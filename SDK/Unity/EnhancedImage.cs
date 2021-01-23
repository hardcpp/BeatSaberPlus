using BeatSaberMarkupLanguage.Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Unity
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
        public AnimationControllerData AnimControllerData { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ensure image valid
        /// </summary>
        public void EnsureValidForHeight(int p_ForcedHeight)
        {
            if (Height < 0 || Width < 0)
            {
                Logger.Instance.Error($"[SDK.Unity][EnhancedImageInfo.EnsureValidForHeight] Invalid emote ImageID {ImageID} Width {Width} height {Height}");
                Width = p_ForcedHeight;
                Height = p_ForcedHeight;
                return;
            }

            if (Height > p_ForcedHeight)
                Height = p_ForcedHeight;

            if (Width > (3 * p_ForcedHeight))
            {
                Width = p_ForcedHeight;
                Logger.Instance.Error($"[SDK.Unity][EnhancedImageInfo.EnsureValidForHeight] Too wide emote ImageID {ImageID} Width {Width} height {Height}");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// From raw
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Bytes">Result bytes</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        /// <returns></returns>
        public static EnhancedImage FromRawStatic(string p_ID, byte[] p_Bytes, int p_ForcedHeight = -1)
        {
            int l_SpriteWidth = 0;
            int l_SpriteHeight = 0;

            UnityEngine.Sprite l_Sprite = null;

            try
            {
                l_Sprite        = SDK.Unity.Sprite.CreateFromRaw(p_Bytes);
                l_SpriteWidth   = l_Sprite.texture.width;
                l_SpriteHeight  = l_Sprite.texture.height;
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("[SDK.Unity][EnhancedImageInfo.FromRaw] Error in FromRaw");
                Logger.Instance.Error(p_Exception);
                l_Sprite = null;
            }

            Unity.EnhancedImage l_Result = null;
            if (l_Sprite != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref l_SpriteWidth, ref l_SpriteHeight, p_ForcedHeight);

                l_Result = new Unity.EnhancedImage()
                {
                    ImageID             = p_ID,
                    Sprite              = l_Sprite,
                    Width               = l_SpriteWidth,
                    Height              = l_SpriteHeight,
                    AnimControllerData  = null
                };

                if (p_ForcedHeight != -1)
                    l_Result.EnsureValidForHeight(p_ForcedHeight);
            }

            return l_Result;
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
        public static void FromRawAnimated(string p_ID, AnimationType p_Type, byte[] p_Bytes, Action<EnhancedImage> p_Callback, int p_ForcedHeight = -1)
        {
            AnimationLoader.Process(p_Type, p_Bytes, (p_Texture, p_Atlas, p_Delays, p_Width, p_Height) =>
            {
                var l_AnimControllerData = AnimationController.instance.Register(p_ID, p_Texture, p_Atlas, p_Delays);

                Unity.EnhancedImage l_AnimResult = null;
                if (l_AnimControllerData.sprite != null)
                {
                    if (p_ForcedHeight != -1)
                        ComputeImageSizeForHeight(ref p_Width, ref p_Height, p_ForcedHeight);

                    l_AnimResult = new Unity.EnhancedImage()
                    {
                        ImageID             = p_ID,
                        Sprite              = l_AnimControllerData.sprite,
                        Width               = p_Width,
                        Height              = p_Height,
                        AnimControllerData  = l_AnimControllerData
                    };

                    if (p_ForcedHeight != -1)
                        l_AnimResult.EnsureValidForHeight(p_ForcedHeight);
                }

                p_Callback?.Invoke(l_AnimResult);
            });
        }
        /// <summary>
        /// From sprite sheet
        /// </summary>
        /// <param name="p_ID">ID of the image</param>
        /// <param name="p_Texture">Result texture</param>
        /// <param name="p_Rect">Sheet rect</param>
        /// <param name="p_ForcedHeight">Forced height</param>
        public static EnhancedImage FromSpriteSheetImage(string p_ID, UnityEngine.Texture2D p_Texture, UnityEngine.Rect p_Rect, int p_ForcedHeight = -1)
        {
            int l_SpriteWidth   = (int)p_Rect.width;
            int l_SpriteHeight  = (int)p_Rect.height;

            var l_Sprite = UnityEngine.Sprite.Create(p_Texture,
                                                     new UnityEngine.Rect(p_Rect.x, p_Texture.height - p_Rect.y - l_SpriteHeight, l_SpriteWidth, l_SpriteHeight),
                                                     new UnityEngine.Vector2(0, 0));
            l_Sprite.texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;

            EnhancedImage l_Result = null;
            if (l_Sprite != null)
            {
                if (p_ForcedHeight != -1)
                    ComputeImageSizeForHeight(ref l_SpriteWidth, ref l_SpriteHeight, p_ForcedHeight);

                l_Result = new Unity.EnhancedImage()
                {
                    ImageID             = p_ID,
                    Sprite              = l_Sprite,
                    Width               = l_SpriteWidth,
                    Height              = l_SpriteHeight,
                    AnimControllerData  = null
                };

                if (p_ForcedHeight != -1)
                    l_Result.EnsureValidForHeight(p_ForcedHeight);
            }

            return l_Result;
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
                p_SpriteWidth = p_Height;
                p_SpriteHeight = p_Height;
            }
            else
            {
                double l_Scale = ((double)p_Height) / ((double)p_SpriteHeight);

                p_SpriteWidth   = (int)(l_Scale * ((double)p_SpriteWidth));
                p_SpriteHeight  = (int)(l_Scale * ((double)p_SpriteHeight));
            }
        }
    }
}
