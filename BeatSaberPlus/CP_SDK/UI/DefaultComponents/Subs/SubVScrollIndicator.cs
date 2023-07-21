using UnityEngine;

namespace CP_SDK.UI.DefaultComponents.Subs
{
    /// <summary>
    /// Vertical scroll indicator component
    /// </summary>
    public class SubVScrollIndicator : MonoBehaviour
    {
        private RectTransform   m_Handle = null;
        private float           m_Padding = 0.25f;
        private float           m_Progress;
        private float           m_NormalizedPageHeight = 1f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle transform
        /// </summary>
        public RectTransform Handle
        {
            get => Handle;
            set {
                if (m_Handle == value) return;
                m_Handle = value;
                RefreshHandle();
            }
        }
        /// <summary>
        /// Progress
        /// </summary>
        public float Progress
        {
            get => m_Progress;
            set {
                if (m_Progress == value) return;
                m_Progress = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }
        /// <summary>
        /// Normalized page height
        /// </summary>
        public float NormalizedPageHeight
        {
            get => m_NormalizedPageHeight;
            set {
                if (m_NormalizedPageHeight == value) return;
                m_NormalizedPageHeight = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh handle
        /// </summary>
        public void RefreshHandle()
        {
            var l_Progress  = (transform as RectTransform).rect.size.y - 2f * m_Padding;
            var l_PosY      = (0f - m_Progress) * (1f - m_NormalizedPageHeight) * l_Progress - m_Padding;

            if (float.IsNaN(l_PosY))
                l_PosY = 0.0f;

            m_Handle.sizeDelta           = new Vector2(0f, m_NormalizedPageHeight * l_Progress);
            m_Handle.anchoredPosition    = new Vector2(0f, l_PosY);
        }
    }
}
