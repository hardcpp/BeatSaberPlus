using System;
using System.IO;
using UnityEngine;

namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Texture2D helper
    /// </summary>
    public class Texture2D
    {
        /// <summary>
        /// Load texture from byte array
        /// </summary>
        /// <param name="p_Bytes">Raw Texture 2D data</param>
        /// <returns></returns>
        public static UnityEngine.Texture2D CreateFromRaw(byte[] p_Bytes)
        {
            if (p_Bytes != null && p_Bytes.Length > 0)
            {
                try
                {
                    var l_Texture = new UnityEngine.Texture2D(2, 2);
                    if (l_Texture.LoadImage(p_Bytes))
                        return l_Texture;
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance?.Error("[SDK.Unity][Texture2D.CreateFromRaw] Failed");
                    Logger.Instance?.Error(l_Exception);
                    return null;
                }
            }

            return null;
        }
        /// <summary>
        /// Create texture from raw data using System.Drawing
        /// </summary>
        /// <param name="p_Bytes">Raw data</param>
        /// <param name="p_Callback">Callback</param>
        public static void CreateFromRawEx(byte[] p_Bytes, Action<UnityEngine.Texture2D> p_Callback)
        {
            if (p_Bytes != null && p_Bytes.Length > 0)
            {
                try
                {
                    using (var l_DrawImage = System.Drawing.Image.FromStream(new MemoryStream(p_Bytes)))
                    {
                        using (var l_Bitmap = new System.Drawing.Bitmap(l_DrawImage))
                        {
                            var l_Colors = new Color32[l_Bitmap.Height * l_Bitmap.Width];

                            for (var l_Y = 0; l_Y < l_Bitmap.Height; l_Y++)
                            {
                                for (var l_X = 0; l_X < l_Bitmap.Width; l_X++)
                                {
                                    var l_SourceColor = l_Bitmap.GetPixel(l_X, l_Y);
                                    l_Colors[(l_Bitmap.Height - l_Y - 1) * l_Bitmap.Width + l_X] = new Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                                }
                            }

                            var l_Width = l_Bitmap.Width;
                            var l_Height = l_Bitmap.Height;

                            Unity.MainThreadInvoker.Enqueue(() =>
                            {
                                var l_Texture = null as UnityEngine.Texture2D;

                                try
                                {
                                    l_Texture = new UnityEngine.Texture2D(l_Width, l_Height, UnityEngine.TextureFormat.RGBA32, false);
                                    l_Texture.wrapMode = TextureWrapMode.Clamp;

                                    l_Texture.SetPixels32(l_Colors);
                                    l_Texture.Apply(true);
                                }
                                catch (System.Exception l_Exception)
                                {
                                    Logger.Instance.Error("[SDK.Unity][Texture2D.CreateFromRawEx] Error2:");
                                    Logger.Instance.Error(l_Exception);
                                }

                                try
                                {
                                    p_Callback?.Invoke(l_Texture);
                                }
                                catch (System.Exception l_Exception)
                                {
                                    Logger.Instance.Error("[SDK.Unity][Texture2D.CreateFromRawEx] Error3:");
                                    Logger.Instance.Error(l_Exception);
                                }
                            });

                            return;
                        }
                    }
                }
                catch (System.Exception l_Exception)
                {
                    Logger.Instance.Error("[SDK.Unity][Texture2D.CreateFromRawEx] Error:");
                    Logger.Instance.Error(l_Exception);
                }
            }

            p_Callback?.Invoke(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Re sample and crop a texture
        /// </summary>
        /// <param name="p_Source">Source texture</param>
        /// <param name="p_TargetWidth">Target width</param>
        /// <param name="p_TargetHeight">Target height</param>
        /// <returns></returns>
        public static UnityEngine.Texture2D ResampleAndCrop(UnityEngine.Texture2D p_Source, int p_TargetWidth, int p_TargetHeight, float p_YOffsetRel = 0.5f)
        {
            int l_SourceWidth = p_Source.width;
            int l_SourceHeight = p_Source.height;

            float l_SourceAspect = (float)l_SourceWidth / l_SourceHeight;
            float l_TargetAspect = (float)p_TargetWidth / p_TargetHeight;

            int l_XOffset = 0;
            int l_YOffset = 0;

            float l_Factor;

            /// Crop width
            if (l_SourceAspect > l_TargetAspect)
            {
                l_Factor = (float)p_TargetHeight / l_SourceHeight;
                l_XOffset = (int)((l_SourceWidth - l_SourceHeight * l_TargetAspect) * 0.5f);

            }
            /// Crop height
            else
            {
                l_Factor = (float)p_TargetWidth / l_SourceWidth;
                l_YOffset = (int)((l_SourceHeight - l_SourceWidth / l_TargetAspect) * (1f - p_YOffsetRel));
            }

            Color32[] l_Source = p_Source.GetPixels32();
            Color32[] l_Result = new Color32[p_TargetWidth * p_TargetHeight];

            for (int l_Y = 0; l_Y < p_TargetHeight; l_Y++)
            {
                for (int l_X = 0; l_X < p_TargetWidth; l_X++)
                {
                    var l_Pixel = new Vector2(Mathf.Clamp(l_XOffset + l_X / l_Factor, 0, l_SourceWidth - 1), Mathf.Clamp(l_YOffset + l_Y / l_Factor, 0, l_SourceHeight - 1));

                    /// Bilinear filtering
                    var l_C11 = l_Source[Mathf.FloorToInt(l_Pixel.x) + l_SourceWidth * (Mathf.FloorToInt(l_Pixel.y))];
                    var l_C12 = l_Source[Mathf.FloorToInt(l_Pixel.x) + l_SourceWidth * (Mathf.CeilToInt(l_Pixel.y))];
                    var l_C21 = l_Source[Mathf.CeilToInt(l_Pixel.x) + l_SourceWidth * (Mathf.FloorToInt(l_Pixel.y))];
                    var l_C22 = l_Source[Mathf.CeilToInt(l_Pixel.x) + l_SourceWidth * (Mathf.CeilToInt(l_Pixel.y))];

                    l_Result[l_X + l_Y * p_TargetWidth] = Color.Lerp(Color.Lerp(l_C11, l_C12, l_Pixel.y), Color.Lerp(l_C21, l_C22, l_Pixel.y), l_Pixel.x);
                }
            }

            var l_ResultTexture = new UnityEngine.Texture2D(p_TargetWidth, p_TargetHeight);
            l_ResultTexture.SetPixels32(l_Result);
            l_ResultTexture.Apply(true);

            return l_ResultTexture;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Blur a Texture2D
        /// </summary>
        /// <param name="p_Image">Source</param>
        /// <param name="p_BlurSize">Blur size</param>
        /// <returns></returns>
        public static UnityEngine.Texture2D Blur(UnityEngine.Texture2D p_Image, int p_BlurSize)
        {
            UnityEngine.Texture2D l_Blurred = new UnityEngine.Texture2D(p_Image.width, p_Image.height);

            /// Look at every pixel in the blur rectangle
            for (int l_XX = 0; l_XX < p_Image.width; l_XX++)
            {
                for (int l_YY = 0; l_YY < p_Image.height; l_YY++)
                {
                    float l_AvgR = 0, l_AvgG = 0, l_AvgB = 0, l_AvgA = 0;
                    int l_BlurPixelCount = 0;

                    /// Average the color of the red, green and blue for each pixel in the
                    /// blur size while making sure you don't go outside the image bounds
                    for (int l_X = l_XX; (l_X < l_XX + p_BlurSize && l_X < p_Image.width); l_X++)
                    {
                        for (int l_Y = l_YY; (l_Y < l_YY + p_BlurSize && l_Y < p_Image.height); l_Y++)
                        {
                            Color l_Pixel = p_Image.GetPixel(l_X, l_Y);

                            l_AvgR += l_Pixel.r;
                            l_AvgG += l_Pixel.g;
                            l_AvgB += l_Pixel.b;
                            l_AvgA += l_Pixel.a;

                            l_BlurPixelCount++;
                        }
                    }

                    l_AvgR /= l_BlurPixelCount;
                    l_AvgG /= l_BlurPixelCount;
                    l_AvgB /= l_BlurPixelCount;
                    l_AvgA /= l_BlurPixelCount;

                    /// Now that we know the average for the blur size, set each pixel to that color
                    for (int l_X = l_XX; l_X < l_XX + p_BlurSize && l_X < p_Image.width; l_X++)
                    {
                        for (int l_Y = l_YY; l_Y < l_YY + p_BlurSize && l_Y < p_Image.height; l_Y++)
                            l_Blurred.SetPixel(l_X, l_Y, new Color(l_AvgR, l_AvgG, l_AvgB, l_AvgA));
                    }
                }
            }

            l_Blurred.Apply();
            return l_Blurred;
        }
    }
}
