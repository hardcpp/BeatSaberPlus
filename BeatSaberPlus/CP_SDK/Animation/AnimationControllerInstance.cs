using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.Animation
{
    /// <summary>
    /// Animation controller data object
    /// </summary>
    public class AnimationControllerInstance
    {
        /// <summary>
        /// Is frame delays consistent
        /// </summary>
        private bool m_IsDelayConsistent = true;
        /// <summary>
        /// Last frame change
        /// </summary>
        private long m_LastFrameChange;
        /// <summary>
        /// Active images
        /// </summary>
        private List<Image> m_ActiveImages = new List<Image>(50);
        /// <summary>
        /// Active images count
        /// </summary>
        private int m_ActiveCount = 0;
        /// <summary>
        /// Atlas UVs
        /// </summary>
        private Rect[] m_UVs;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// First frame
        /// </summary>
        public Sprite FirstFrame;
        /// <summary>
        /// Frames
        /// </summary>
        public Sprite[] Frames;
        /// <summary>
        /// Frame delays
        /// </summary>
        public ushort[] Delays;
        /// <summary>
        /// Current frame index
        /// </summary>
        public uint CurrentFrameIndex = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Texture">Texture atlas</param>
        /// <param name="p_UVs">UVs of sub sprites</param>
        /// <param name="p_Delays">Delays of frames</param>
        public AnimationControllerInstance(Texture2D p_Texture, Rect[] p_UVs, ushort[] p_Delays)
        {
            int l_FirstDelay = -1;

            Frames  = new Sprite[p_UVs.Length];
            m_UVs   = p_UVs;
            Delays  = p_Delays;

            for (int l_Frame = 0; l_Frame < p_UVs.Length; ++l_Frame)
            {
                Frames[l_Frame] = Sprite.Create(p_Texture,
                    new Rect(p_UVs[l_Frame].x * p_Texture.width, p_UVs[l_Frame].y * p_Texture.height, p_UVs[l_Frame].width * p_Texture.width, p_UVs[l_Frame].height * p_Texture.height),
                    Vector2.zero,
                    100f,
                    0,
                    SpriteMeshType.FullRect
                );

                if (l_Frame == 0)
                    l_FirstDelay = p_Delays[l_Frame];

                if (p_Delays[l_Frame] != l_FirstDelay)
                    m_IsDelayConsistent = false;
            }

            FirstFrame = Frames[0];

            m_LastFrameChange = (long)(Time.realtimeSinceStartup * 1000f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register an image
        /// </summary>
        /// <param name="p_TargetImage">Target</param>
        public void Register(Image p_TargetImage)
        {
            if (!m_ActiveImages.Contains(p_TargetImage))
                m_ActiveImages.Add(p_TargetImage);

            m_ActiveCount = m_ActiveImages.Count;
        }
        /// <summary>
        /// Unregister an image
        /// </summary>
        /// <param name="p_TargetImage">Target</param>
        public void Unregister(Image p_TargetImage)
        {
            m_ActiveImages.Remove(p_TargetImage);
            m_ActiveCount = m_ActiveImages.Count;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check if we should move to the next frame
        /// </summary>
        /// <param name="p_Now">Current time in MS</param>
        internal void CheckForNextFrame(long p_Now)
        {
            if (m_ActiveCount == 0)
                return;

            long l_DeltaT = p_Now - m_LastFrameChange;
            if (l_DeltaT < Delays[CurrentFrameIndex])
                return;

            /// Bump animations with consistently 10ms or lower frame timings to 100ms
            if (m_IsDelayConsistent && Delays[CurrentFrameIndex] <= 10 && l_DeltaT < 100)
                return;

            m_LastFrameChange = p_Now;
            do
            {
                CurrentFrameIndex++;
                if (CurrentFrameIndex >= m_UVs.Length)
                    CurrentFrameIndex = 0;
            }
            while (!m_IsDelayConsistent && Delays[CurrentFrameIndex] == 0);

            var l_NewSprite = Frames[CurrentFrameIndex];
            for (int l_I = 0; l_I < m_ActiveCount; ++l_I)
                m_ActiveImages[l_I].sprite = l_NewSprite;
        }
    }
}
