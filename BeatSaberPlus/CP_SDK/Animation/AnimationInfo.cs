using UnityEngine;

namespace CP_SDK.Animation
{
    /// <summary>
    /// Animation frame info
    /// </summary>
    public class AnimationInfo
    {
        /// <summary>
        /// Width
        /// </summary>
        public int Width;
        /// <summary>
        /// Height
        /// </summary>
        public int Height;
        /// <summary>
        /// Frames
        /// </summary>
        public Color32[][] Frames;
        /// <summary>
        /// Delay
        /// </summary>
        public ushort[] Delays;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <param name="p_Height">Height</param>
        /// <param name="p_FrameCount">Animation frame count</param>
        public AnimationInfo(int p_Width, int p_Height, uint p_FrameCount)
        {
            Width   = p_Width;
            Height  = p_Height;
            Frames  = new Color32[p_FrameCount][];
            Delays  = new ushort[p_FrameCount];

            for (int l_I = 0; l_I <p_FrameCount; ++l_I)
                Frames[l_I] = new Color32[p_Width * p_Height];
        }
    }
}
