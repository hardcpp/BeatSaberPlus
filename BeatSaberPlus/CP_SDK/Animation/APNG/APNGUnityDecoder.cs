using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CP_SDK.Animation.APNG
{
    public class APNGUnityDecoder
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
        /// <param name="p_AnimationInfo">Animation info</param>
        /// <param name="p_Callback">Completion callback</param>
        private static async Task ProcessingThread(byte[] p_Raw, Action<AnimationInfo> p_Callback)
        {
            /// RUN ON RANDOM THREAD

            await Task.Yield();

            var l_APNG          = APNG.FromStream(new System.IO.MemoryStream(p_Raw));
            var l_APNGFrames    = l_APNG.Frames;
            var l_FrameCount    = l_APNG.FrameCount;
            var l_AnimationInfo = new AnimationInfo(l_APNG.IHDRChunk.Width, l_APNG.IHDRChunk.Height, (uint)l_FrameCount);

            for (int l_I = 0; l_I < l_FrameCount; ++l_I)
            {
                var l_CurrentFrame  = l_APNGFrames[l_I];
                var l_Bitmap        = l_CurrentFrame.ToBitmap();
                var l_LockBitmap    = new LockBitmap(l_Bitmap);

                l_LockBitmap.LockBits();

                for (int l_Y = 0; l_Y < l_AnimationInfo.Height; ++l_Y)
                {
                    for (int l_X = 0; l_X < l_AnimationInfo.Width; ++l_X)
                    {
                        var l_SourceColor       = l_LockBitmap.GetPixel(l_X, l_Y);
                        var l_LastFrameColor    = new Color32();

                        if (l_I > 0)
                            l_LastFrameColor = l_AnimationInfo.Frames[l_I - 1][(l_AnimationInfo.Height - l_Y - 1) * l_AnimationInfo.Width + l_X];

                        if (l_CurrentFrame.fcTLChunk.BlendOp == Chunks.EBlendOps.APNGBlendOpSource)
                            l_AnimationInfo.Frames[l_I][(l_AnimationInfo.Height - l_Y - 1) * l_AnimationInfo.Width + l_X] = new Color32(l_SourceColor.R, l_SourceColor.G, l_SourceColor.B, l_SourceColor.A);
                        if (l_CurrentFrame.fcTLChunk.BlendOp == Chunks.EBlendOps.APNGBlendOpOver)
                        {
                            var l_BlendedA = ((l_SourceColor.A / 255f + (1 - (l_SourceColor.A / 255f)) * (l_LastFrameColor.a / 255f)));
                            var l_BlendedR = ((l_SourceColor.A / 255f) * (l_SourceColor.R / 255f) + (1 - (l_SourceColor.A / 255f)) * (l_LastFrameColor.a / 255f) * (l_LastFrameColor.r / 255f)) / l_BlendedA;
                            var l_BlendedG = ((l_SourceColor.A / 255f) * (l_SourceColor.G / 255f) + (1 - (l_SourceColor.A / 255f)) * (l_LastFrameColor.a / 255f) * (l_LastFrameColor.g / 255f)) / l_BlendedA;
                            var l_BlendedB = ((l_SourceColor.A / 255f) * (l_SourceColor.B / 255f) + (1 - (l_SourceColor.A / 255f)) * (l_LastFrameColor.a / 255f) * (l_LastFrameColor.b / 255f)) / l_BlendedA;

                            l_AnimationInfo.Frames[l_I][(l_AnimationInfo.Height - l_Y - 1) * l_AnimationInfo.Width + l_X] = new Color32((byte)(l_BlendedR * 255), (byte)(l_BlendedG * 255), (byte)(l_BlendedB * 255), (byte)(l_BlendedA * 255));
                        }
                    }
                }

                l_AnimationInfo.Delays[l_I] = (ushort)l_CurrentFrame.FrameRate;
            }

            p_Callback?.Invoke(l_AnimationInfo);
        }
    }
}
