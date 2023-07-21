using System;
using System.IO;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Texture raw utilities
    /// </summary>
    public static class TextureRaw
    {
        /// <summary>
        /// Load an image into raw pixels
        /// </summary>
        /// <param name="p_Bytes">Input bytes</param>
        /// <param name="p_Width">Ouput width</param>
        /// <param name="p_Height">Output height</param>
        /// <param name="p_Pixels">Output pixels</param>
        /// <returns></returns>
        public static bool Load(byte[] p_Bytes, out int p_Width, out int p_Height, out Color[] p_Pixels)
        {
            p_Width     = 0;
            p_Height    = 0;
            p_Pixels    = null;

            if (p_Bytes == null && p_Bytes.Length == 0)
                return false;

            try
            {
                using (var l_DrawImage = System.Drawing.Image.FromStream(new MemoryStream(p_Bytes)))
                {
                    using (var l_Bitmap = new System.Drawing.Bitmap(l_DrawImage))
                    {
                        p_Pixels = new Color[l_Bitmap.Height * l_Bitmap.Width];

                        for (var l_Y = 0; l_Y < l_Bitmap.Height; l_Y++)
                        {
                            for (var l_X = 0; l_X < l_Bitmap.Width; l_X++)
                            {
                                var l_SourceColor = l_Bitmap.GetPixel(l_X, l_Y);
                                p_Pixels[(l_Bitmap.Height - l_Y - 1) * l_Bitmap.Width + l_X] = new Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                            }
                        }

                        p_Width  = l_Bitmap.Width;
                        p_Height = l_Bitmap.Height;
                    }
                }
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Unity][TextureRaw.Load] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Blur an image
        /// </summary>
        /// <param name="p_InWidth">Source width</param>
        /// <param name="p_InHeight">Source height</param>
        /// <param name="p_InPixels">Input image</param>
        /// <param name="p_Radius">Blur radius</param>
        public static void FastGaussianBlur(int p_InWidth, int p_InHeight, Color[] p_InPixels, int p_Radius)
            => GaussianBlur4(p_InWidth, p_InHeight, p_InPixels, p_Radius);
        /// <summary>
        /// Multiply image A & B into A
        /// </summary>
        /// <param name="p_ImageA">Target</param>
        /// <param name="p_ImageB">Additional image</param>
        public static void Multiply(Color[] p_ImageA, Color[] p_ImageB)
        {
            if (p_ImageA.Length != p_ImageB.Length)
                throw new System.Exception("[CP_SDK.Unity][TextureRaw.Multiply] Size differ!");

            for (var l_I = 0; l_I < p_ImageA.Length; ++l_I)
                p_ImageA[l_I] *= p_ImageB[l_I];
        }
        /// <summary>
        /// Resize an image and crop it
        /// </summary>
        /// <param name="p_InWidth">Source width</param>
        /// <param name="p_InHeight">Source height</param>
        /// <param name="p_InPixels">Input image</param>
        /// <param name="p_TargetWidth">Target width</param>
        /// <param name="p_TargetHeight">Target height</param>
        /// <param name="p_YOffsetRel">Height anchor</param>
        /// <returns></returns>
        public static Color[] ResampleAndCrop(int p_InWidth, int p_InHeight, Color[] p_InPixels, int p_TargetWidth, int p_TargetHeight, float p_YOffsetRel = 0.5f)
        {
            float l_SourceAspect = (float)p_InWidth / p_InHeight;
            float l_TargetAspect = (float)p_TargetWidth / p_TargetHeight;

            int l_XOffset = 0;
            int l_YOffset = 0;

            float l_Factor;

            /// Crop width
            if (l_SourceAspect > l_TargetAspect)
            {
                l_Factor  = (float)p_TargetHeight / p_InHeight;
                l_XOffset = (int)((p_InWidth - p_InHeight * l_TargetAspect) * 0.5f);

            }
            /// Crop height
            else
            {
                l_Factor  = (float)p_TargetWidth / p_InWidth;
                l_YOffset = (int)((p_InHeight - p_InWidth / l_TargetAspect) * (1f - p_YOffsetRel));
            }

            Color[] l_Result = new Color[p_TargetWidth * p_TargetHeight];
            for (int l_Y = 0; l_Y < p_TargetHeight; ++l_Y)
            {
                for (int l_X = 0; l_X < p_TargetWidth; ++l_X)
                {
                    var l_Pixel = new Vector2(Mathf.Clamp(l_XOffset + l_X / l_Factor, 0, p_InWidth - 1), Mathf.Clamp(l_YOffset + l_Y / l_Factor, 0, p_InHeight - 1));

                    /// Optimizations
                    var l_FX = Mathf.FloorToInt(l_Pixel.x);
                    var l_FY = Mathf.FloorToInt(l_Pixel.y);
                    var l_CX = Mathf.CeilToInt(l_Pixel.x);
                    var l_CY = Mathf.CeilToInt(l_Pixel.y);

                    /// Bilinear filtering
                    var l_C11 = p_InPixels[l_FX + p_InWidth * l_FY];
                    var l_C12 = p_InPixels[l_FX + p_InWidth * l_CY];
                    var l_C21 = p_InPixels[l_CX + p_InWidth * l_FY];
                    var l_C22 = p_InPixels[l_CX + p_InWidth * l_CY];

                    l_Result[l_X + l_Y * p_TargetWidth] = Color.Lerp(Color.Lerp(l_C11, l_C12, l_Pixel.y), Color.Lerp(l_C21, l_C22, l_Pixel.y), l_Pixel.x);
                }
            }

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// Source https://blog.ivank.net/fastest-gaussian-blur.html#results

        private static int[] BoxesForGauss(int p_Radius, int p_Count)
        {
            var l_WIdeal = Math.Sqrt((12 * p_Radius * p_Radius / p_Count) + 1);
            var l_WL = (int)Math.Floor(l_WIdeal);
            if (l_WL % 2 == 0)
                l_WL--;

            var l_WU        = l_WL + 2;
            var l_MIdeal    = (double)(12 * p_Radius * p_Radius - p_Count * l_WL * l_WL - 4 * p_Count * l_WL - 3 * p_Count) / (-4 * l_WL - 4);
            var l_M         = Math.Round(l_MIdeal);

            var l_Sizes = new int[p_Count];
            for (var l_I = 0; l_I < p_Count; ++l_I)
                l_Sizes[l_I] = l_I < l_M ? l_WL : l_WU;

            return l_Sizes;
        }
        private static void GaussianBlur4(int p_Width, int p_Height, Color[] p_InPixels, int p_Radius)
        {
            var l_Boxes = BoxesForGauss(p_Radius, 3);
            BoxBlur4(p_Width, p_Height, p_InPixels, (l_Boxes[0] - 1) / 2);
            BoxBlur4(p_Width, p_Height, p_InPixels, (l_Boxes[1] - 1) / 2);
            BoxBlur4(p_Width, p_Height, p_InPixels, (l_Boxes[2] - 1) / 2);
        }
        private static void BoxBlur4(int p_Width, int p_Height, Color[] p_InPixels, int p_Radius)
        {
            BoxBlurH4(p_Width, p_Height, p_InPixels, p_Radius);
            BoxBlurT4(p_Width, p_Height, p_InPixels, p_Radius);
        }
        private static void BoxBlurH4(int p_Width, int p_Height, Color[] p_InPixels, int p_Radius)
        {
            var l_Mult = (float)1 / (p_Radius + p_Radius + 1);
            for (var l_I = 0; l_I < p_Height; ++l_I)
            {
                var l_TI  = l_I * p_Width;
                var l_LI  = l_TI;
                var l_RI  = l_TI + p_Radius;
                var l_FV  = p_InPixels[l_TI];
                var l_LV  = p_InPixels[l_TI + p_Width - 1];
                var l_Val = (p_Radius + 1) * l_FV;

                for (var l_J = 0; l_J < p_Radius; ++l_J)
                    l_Val += p_InPixels[l_TI + l_J];

                for (var l_J = 0; l_J <= p_Radius; ++l_J)
                {
                    l_Val += p_InPixels[l_RI++] - l_FV;
                    p_InPixels[l_TI++] = l_Val * l_Mult;
                }

                for (var l_J = p_Radius + 1; l_J < p_Width - p_Radius; ++l_J)
                {
                    l_Val += p_InPixels[l_RI++] - p_InPixels[l_LI++];
                    p_InPixels[l_TI++] = l_Val * l_Mult;
                }

                for (var l_J = p_Width - p_Radius; l_J < p_Width; ++l_J)
                {
                    l_Val += l_LV - p_InPixels[l_LI++];
                    p_InPixels[l_TI++] = l_Val * l_Mult;
                }
            };
        }
        private static void BoxBlurT4(int p_Width, int p_Height, Color[] p_InPixels, int p_Radius)
        {
            var l_Mult = (float)1 / (p_Radius + p_Radius + 1);
            for (var l_I = 0; l_I < p_Width; l_I++)
            {
                var l_TI  = l_I;
                var l_LI  = l_TI;
                var l_RI  = l_TI + p_Radius * p_Width;
                var l_FV  = p_InPixels[l_TI];
                var l_LV  = p_InPixels[l_TI + p_Width * (p_Height - 1)];
                var l_Val = (p_Radius + 1) * l_FV;

                for (var l_J = 0; l_J < p_Radius; ++l_J)
                    l_Val += p_InPixels[l_TI + l_J * p_Width];

                for (var l_J = 0; l_J <= p_Radius; ++l_J)
                {
                    l_Val += p_InPixels[l_RI] - l_FV;
                    p_InPixels[l_TI] = l_Val * l_Mult;
                    l_RI += p_Width;
                    l_TI += p_Width;
                }

                for (var l_J = p_Radius + 1; l_J < p_Height - p_Radius; ++l_J)
                {
                    l_Val += p_InPixels[l_RI] - p_InPixels[l_LI];
                    p_InPixels[l_TI] = l_Val * l_Mult;
                    l_LI += p_Width;
                    l_RI += p_Width;
                    l_TI += p_Width;
                }

                for (var l_J = p_Height - p_Radius; l_J < p_Height; ++l_J)
                {
                    l_Val += l_LV - p_InPixels[l_LI];
                    p_InPixels[l_TI] = l_Val * l_Mult;
                    l_LI += p_Width;
                    l_TI += p_Width;
                }
            };
        }
    }
}
