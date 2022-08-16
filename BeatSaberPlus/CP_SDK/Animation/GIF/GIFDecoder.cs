using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CP_SDK.Animation.GIF
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
            => ProcessingThread(p_Raw, p_Callback).ConfigureAwait(false);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Decoding task
        /// </summary>
        /// <param name="p_Raw">Raw data</param>
        /// <param name="p_Callback">Completion callback</param>
        private static async Task ProcessingThread(byte[] p_Raw, Action<AnimationInfo> p_Callback)
        {
            /// RUN ON RANDOM THREAD

            await Task.Yield();

            var l_GifImage          = System.Drawing.Image.FromStream(new MemoryStream(p_Raw));
            var l_Dimension         = new FrameDimension(l_GifImage.FrameDimensionsList[0]);
            var l_FrameCount        = l_GifImage.GetFrameCount(l_Dimension);
            var l_PropertySlice     = 0;
            var l_FirstDelayValue   = -1;
            var l_AnimationInfo     = null as AnimationInfo;

            for (var l_I = 0; l_I < l_FrameCount; l_I++)
            {
                l_GifImage.SelectActiveFrame(l_Dimension, l_I);

                var l_Bitmap = new System.Drawing.Bitmap(l_GifImage.Width, l_GifImage.Height);

                System.Drawing.Graphics.FromImage(l_Bitmap).DrawImage(l_GifImage, System.Drawing.Point.Empty);

                var l_BitmapFrame = new LockBitmap(l_Bitmap);
                l_BitmapFrame.LockBits();

                if (l_AnimationInfo == null)
                    l_AnimationInfo = new AnimationInfo(l_Bitmap.Width, l_Bitmap.Height, (uint)l_FrameCount);

                var l_TargetArray = l_AnimationInfo.Frames[l_I];

                for (var l_X = 0; l_X < l_BitmapFrame.Width; l_X++)
                {
                    for (var l_Y = 0; l_Y < l_BitmapFrame.Height; l_Y++)
                    {
                        var l_SourceColor = l_BitmapFrame.GetPixel(l_X, l_Y);
                        l_TargetArray[(l_BitmapFrame.Height - l_Y - 1) * l_BitmapFrame.Width + l_X] = new Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                    }
                }

                var l_DelayPropertyValue = BitConverter.ToInt32(l_GifImage.GetPropertyItem(20736).Value, l_PropertySlice);
                l_PropertySlice += 4;

                if (l_FirstDelayValue == -1)
                    l_FirstDelayValue = l_DelayPropertyValue;

                l_AnimationInfo.Delays[l_I] = (ushort)(l_DelayPropertyValue * 10);
            }

            p_Callback?.Invoke(l_AnimationInfo);
        }
    }
}
