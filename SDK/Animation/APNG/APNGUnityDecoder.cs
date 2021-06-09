using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    public class APNGUnityDecoder
    {
        public static void Process(byte[] apngData, Action<AnimationInfo> callback)
        {
            Task.Run(() => ProcessingThread(apngData, new AnimationInfo(), callback));
        }

        private static void ProcessingThread(byte[] apngData, AnimationInfo animationInfo, Action<AnimationInfo> callback)
        {
            APNG.APNG apng = APNG.APNG.FromStream(new System.IO.MemoryStream(apngData));
            int frameCount = apng.FrameCount;
            animationInfo.frameCount = frameCount;

            for (int i = 0; i < frameCount; i++)
            {
                Frame frame = apng.Frames[i];
                System.Drawing.Bitmap bitmap = frame.ToBitmap();
                LockBitmap lockBitmap = new LockBitmap(bitmap);
                lockBitmap.LockBits();
                FrameInfo frameInfo = new FrameInfo(bitmap.Width, bitmap.Height);
                for (int x = 0; x < frameInfo.width; x++)
                {
                    for (int y = 0; y < frameInfo.height; y++)
                    {
                        System.Drawing.Color sourceColor = lockBitmap.GetPixel(x, y);
                        Color32 lastFrame = new Color32();
                        if (i>0)
                            lastFrame = animationInfo.frames.Last().colors[(frameInfo.height - y - 1) * frameInfo.width + x];

                        if (frame.fcTLChunk.BlendOp == APNG.Chunks.BlendOps.APNGBlendOpSource)
                            frameInfo.colors[(frameInfo.height - y - 1) * frameInfo.width + x] = new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A);
                        if (frame.fcTLChunk.BlendOp == APNG.Chunks.BlendOps.APNGBlendOpOver)
                        {
                            float blendedA = ((sourceColor.A / 255f + (1 - (sourceColor.A / 255f)) * (lastFrame.a / 255f)));
                            float blendedR = ((sourceColor.A / 255f) * (sourceColor.R / 255f) + (1 - (sourceColor.A / 255f)) * (lastFrame.a / 255f) * (lastFrame.r / 255f)) / blendedA;
                            float blendedG = ((sourceColor.A / 255f) * (sourceColor.G / 255f) + (1 - (sourceColor.A / 255f)) * (lastFrame.a / 255f) * (lastFrame.g / 255f)) / blendedA;
                            float blendedB = ((sourceColor.A / 255f) * (sourceColor.B / 255f) + (1 - (sourceColor.A / 255f)) * (lastFrame.a / 255f) * (lastFrame.b / 255f)) / blendedA;

                            frameInfo.colors[(frameInfo.height - y - 1) * frameInfo.width + x] = new Color32((byte)(blendedR * 255), (byte)(blendedG * 255), (byte)(blendedB * 255), (byte)(blendedA * 255));
                        }
                    }
                }
                frameInfo.delay = frame.FrameRate;
                animationInfo.frames.Add(frameInfo);
            }

            Unity.MainThreadInvoker.Enqueue(() => callback?.Invoke(animationInfo));
        }
    }
}
