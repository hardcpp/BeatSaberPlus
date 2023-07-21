#if CP_SDK_UNITY
using System;
using System.IO;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Texture2D helper
    /// </summary>
    public class Texture2DU
    {
        /// <summary>
        /// Load texture from byte array
        /// </summary>
        /// <param name="p_Bytes">Raw Texture 2D data</param>
        /// <returns></returns>
        public static Texture2D CreateFromRaw(byte[] p_Bytes)
        {
            if (p_Bytes != null && p_Bytes.Length > 0)
            {
                try
                {
                    var l_Texture = new Texture2D(2, 2);
                    if (l_Texture.LoadImage(p_Bytes))
                        return l_Texture;
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger?.Error("[CP_SDK.Unity][Texture2D.CreateFromRaw] Failed");
                    ChatPlexSDK.Logger?.Error(l_Exception);
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
        public static void CreateFromRawEx(byte[] p_Bytes, Action<Texture2D> p_Callback)
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

                            var l_Width     = l_Bitmap.Width;
                            var l_Height    = l_Bitmap.Height;

                            MTMainThreadInvoker.Enqueue(() =>
                            {
                                var l_Texture = null as Texture2D;

                                try
                                {
                                    l_Texture = new Texture2D(l_Width, l_Height, TextureFormat.RGBA32, false);
                                    l_Texture.wrapMode = TextureWrapMode.Clamp;

                                    l_Texture.SetPixels32(l_Colors);
                                    l_Texture.Apply(true);
                                }
                                catch (System.Exception l_Exception)
                                {
                                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][Texture2D.CreateFromRawEx] Error2:");
                                    ChatPlexSDK.Logger.Error(l_Exception);
                                }

                                try
                                {
                                    p_Callback?.Invoke(l_Texture);
                                }
                                catch (System.Exception l_Exception)
                                {
                                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][Texture2D.CreateFromRawEx] Error3:");
                                    ChatPlexSDK.Logger.Error(l_Exception);
                                }
                            });

                            return;
                        }
                    }
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][Texture2D.CreateFromRawEx] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }

            p_Callback?.Invoke(null);
        }
    }
}
#endif
