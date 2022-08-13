using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace CP_SDK.Animation.WEBP
{
    /// <summary>
    /// WEBP decoder
    /// </summary>
    public class WEBPDecoder
    {
        /// <summary>
        /// Async decode WEBP image
        /// </summary>
        /// <param name="p_Raw">Raw data</param>
        /// <param name="p_Callback">Completion callback</param>
        /// <param name="p_StaticCallback">On static frame completion callback</param>
        public static void Process(byte[] p_Raw, Action<AnimationInfo> p_Callback, Action<UnityEngine.Sprite> p_StaticCallback)
            => ProcessingThread(p_Raw, p_Callback, p_StaticCallback).ConfigureAwait(false);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decoding task
        /// </summary>
        /// <param name="p_Raw">Raw data</param>
        /// <param name="p_Callback">Completion callback</param>
        /// <param name="p_StaticCallback">On static frame completion callback</param>
        private static async Task ProcessingThread( byte[]                      p_Raw,
                                                    Action<AnimationInfo>       p_Callback,
                                                    Action<UnityEngine.Sprite>  p_StaticCallback)
        {
            /// RUN ON RANDOM THREAD

            await Task.Yield();

            var l_Features = new Natives.WEBP.WebPBitstreamFeatures();
            if (Natives.WEBP.WebPGetFeatures(p_Raw, ref l_Features) != Natives.WEBP.VP8StatusCode.VP8_STATUS_OK)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Failed to get WebPFeatures");
                p_Callback?.Invoke(null);
                return;
            }

            if (l_Features.has_animation == 0)
            {
                var l_Bitmap        = null as Bitmap;
                var l_BitmapData    = null as BitmapData;

                try
                {
                    if (l_Features.has_alpha == 1)
                        l_Bitmap = new Bitmap(l_Features.width, l_Features.height, PixelFormat.Format32bppArgb);
                    else
                        l_Bitmap = new Bitmap(l_Features.width, l_Features.height, PixelFormat.Format24bppRgb);
                    l_BitmapData = l_Bitmap.LockBits(new Rectangle(0, 0, l_Features.width, l_Features.height), ImageLockMode.WriteOnly, l_Bitmap.PixelFormat);

                    int l_ResultSize = 0;
                    if (l_Bitmap.PixelFormat == PixelFormat.Format24bppRgb)
                        l_ResultSize = Natives.WEBP.WebPDecodeBGRInto(p_Raw, l_BitmapData.Scan0, l_BitmapData.Stride * l_Features.height, l_BitmapData.Stride);
                    else
                        l_ResultSize = Natives.WEBP.WebPDecodeBGRAInto(p_Raw, l_BitmapData.Scan0, l_BitmapData.Stride * l_Features.height, l_BitmapData.Stride);

                    if (l_ResultSize == 0)
                        throw new Exception("Can't decode WebP");

                    l_Bitmap.UnlockBits(l_BitmapData);

                    var l_Colors = new UnityEngine.Color32[l_Bitmap.Height * l_Bitmap.Width];
                    for (var l_Y = 0; l_Y < l_Bitmap.Height; l_Y++)
                    {
                        for (var l_X = 0; l_X < l_Bitmap.Width; l_X++)
                        {
                            var l_SourceColor = l_Bitmap.GetPixel(l_X, l_Y);
                            l_Colors[(l_Bitmap.Height - l_Y - 1) * l_Bitmap.Width + l_X] = new UnityEngine.Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                        }
                    }

                    var l_Width     = l_Bitmap.Width;
                    var l_Height    = l_Bitmap.Height;

                    Unity.MTMainThreadInvoker.Enqueue(() =>
                    {
                        var l_Texture = null as UnityEngine.Texture2D;

                        try
                        {
                            l_Texture = new UnityEngine.Texture2D(l_Width, l_Height, UnityEngine.TextureFormat.RGBA32, false);
                            l_Texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;

                            l_Texture.SetPixels32(l_Colors);
                            l_Texture.Apply(true);
                        }
                        catch (System.Exception l_Exception)
                        {
                            ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Error2:");
                            ChatPlexSDK.Logger.Error(l_Exception);
                        }

                        try
                        {
                            p_StaticCallback?.Invoke(Unity.SpriteU.CreateFromTexture(l_Texture));
                        }
                        catch (System.Exception l_Exception)
                        {
                            ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Error3:");
                            ChatPlexSDK.Logger.Error(l_Exception);
                        }
                    });
                }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Error4:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                    p_StaticCallback?.Invoke(null);
                }
            }
            else
            {
                ///var l_Options = new Natives.WEBPDemux.WebPAnimDecoderOptions();
                ///l_Options.use_threads = 0;
                ///l_Options.color_mode = Natives.WEBP.WEBP_CSP_MODE.MODE_RGBA;

                var l_Decoder = Natives.WEBPDemux.WebPAnimDecoderNew(p_Raw/*, ref l_Options*/);
                if (l_Decoder != IntPtr.Zero)
                {
                    var l_Infos = new Natives.WEBPDemux.WebPAnimInfo();
                    if (Natives.WEBPDemux.WebPAnimDecoderGetInfo(l_Decoder, ref l_Infos))
                    {
                        var l_Buffer        = new byte[4 * l_Infos.canvas_width * l_Infos.canvas_height];
                        var l_TimeStamp     = 0;
                        var l_PrevTimeStamp = 0;
                        var l_AnimationInfo = new AnimationInfo((int)l_Infos.canvas_width, (int)l_Infos.canvas_height, l_Infos.frame_count);
                        var l_FrameI        = 0;

                        while (Natives.WEBPDemux.WebPAnimDecoderHasMoreFrames(l_Decoder))
                        {
                            if (!Natives.WEBPDemux.WebPAnimDecoderGetNext(l_Decoder, l_Buffer, ref l_TimeStamp))
                            {
                                ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Failed to decode next frame");
                                break;
                            }

                            var l_TargetArray = l_AnimationInfo.Frames[l_FrameI];
                            for (int l_Line = 0; l_Line < l_Infos.canvas_height; ++l_Line)
                            {
                                for (int l_X = 0; l_X < l_Infos.canvas_width; ++l_X)
                                {
                                    var l_DestOffset = ((l_Infos.canvas_height - (l_Line + 1)) * l_Infos.canvas_width) + l_X;
                                    var l_SourOffset = (l_Line * l_Infos.canvas_width * 4) + (l_X * 4);
                                    l_TargetArray[l_DestOffset].r = l_Buffer[l_SourOffset + 0];
                                    l_TargetArray[l_DestOffset].g = l_Buffer[l_SourOffset + 1];
                                    l_TargetArray[l_DestOffset].b = l_Buffer[l_SourOffset + 2];
                                    l_TargetArray[l_DestOffset].a = l_Buffer[l_SourOffset + 3];
                                }
                            }

                            l_AnimationInfo.Delays[l_FrameI]    = (ushort)(l_TimeStamp - l_PrevTimeStamp);
                            l_PrevTimeStamp                     = l_TimeStamp;

                            l_FrameI++;
                        }

                        Natives.WEBPDemux.WebPAnimDecoderDelete(l_Decoder);

                        p_Callback?.Invoke(l_AnimationInfo);
                    }
                    else
                    {
                        ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Failed to decode animated informations");
                        p_Callback?.Invoke(null);
                    }
                }
                else
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Animation.WEBP][WEBP.ProcessingThread] Failed to decode");
                    p_Callback?.Invoke(null);
                }
            }
        }
    }
}
