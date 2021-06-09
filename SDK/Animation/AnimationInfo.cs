using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.SDK.Animation
{
    public class AnimationInfo
    {
        public List<FrameInfo> frames = new List<FrameInfo>();
        public int frameCount = 0;
    }

    public struct FrameInfo
    {
        public int width, height;
        public Color32[] colors;
        public int delay;
        public FrameInfo(int width, int height)
        {
            this.width = width;
            this.height = height;
            colors = new Color32[width * height];
            this.delay = 0;
        }
    }
}
