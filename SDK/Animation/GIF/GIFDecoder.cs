using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    /// <summary>
    /// GIF decoder
    /// </summary>
    public class GIFDecoder
    {
        /// <summary>
        /// Async decode GIF image
        /// </summary>
        /// <param name="p_Raw">Raw data</param>
        /// <param name="p_Callback">Completion callback</param>
        public static void Process(byte[] p_Raw, Action<AnimationInfo> p_Callback)
            => Task.Run(() => ProcessingThread(p_Raw, new AnimationInfo(), p_Callback));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decoding task
        /// </summary>
        /// <param name="p_Raw">Raw data</param>
        /// <param name="p_AnimationInfo">Animation info</param>
        /// <param name="p_Callback">Completion callback</param>
        private static void ProcessingThread(byte[] p_Raw, AnimationInfo p_AnimationInfo, Action<AnimationInfo> p_Callback)
        {
            var l_GifImage      = System.Drawing.Image.FromStream(new MemoryStream(p_Raw));
            var l_Dimension     = new FrameDimension(l_GifImage.FrameDimensionsList[0]);
            var l_FrameCount    = l_GifImage.GetFrameCount(l_Dimension);

            p_AnimationInfo.frameCount = l_FrameCount;

            var l_PropertySlice = 0;
            var l_FirstDelayValue = -1;

            for (var l_I = 0; l_I < l_FrameCount; l_I++)
            {
                l_GifImage.SelectActiveFrame(l_Dimension, l_I);

                var l_Bitmap = new System.Drawing.Bitmap(l_GifImage.Width, l_GifImage.Height);

                System.Drawing.Graphics.FromImage(l_Bitmap).DrawImage(l_GifImage, System.Drawing.Point.Empty);

                LockBitmap l_BitmapFrame = new LockBitmap(l_Bitmap);
                l_BitmapFrame.LockBits();

                var l_CurrentFrame = new FrameInfo(l_Bitmap.Width, l_Bitmap.Height);
                if (l_CurrentFrame.colors == null)
                    l_CurrentFrame.colors = new Color32[l_BitmapFrame.Height * l_BitmapFrame.Width];

                for (var l_X = 0; l_X < l_BitmapFrame.Width; l_X++)
                {
                    for (var l_Y = 0; l_Y < l_BitmapFrame.Height; l_Y++)
                    {
                        var l_SourceColor = l_BitmapFrame.GetPixel(l_X, l_Y);
                        l_CurrentFrame.colors[(l_BitmapFrame.Height - l_Y - 1) * l_BitmapFrame.Width + l_X] = new Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                    }
                }

                var l_DelayPropertyValue = BitConverter.ToInt32(l_GifImage.GetPropertyItem(20736).Value, l_PropertySlice);
                l_PropertySlice += 4;

                if (l_FirstDelayValue == -1)
                    l_FirstDelayValue = l_DelayPropertyValue;

                l_CurrentFrame.delay = l_DelayPropertyValue * 10;
                p_AnimationInfo.frames.Add(l_CurrentFrame);
            }

            Unity.MainThreadInvoker.Enqueue(() => p_Callback?.Invoke(p_AnimationInfo));
        }
    }
}
