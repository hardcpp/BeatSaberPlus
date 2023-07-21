using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CGLayout component
    /// </summary>
    public class DefaultCGLayout : Components.CGLayout
    {
        private RectTransform       m_RTransform;
        private ContentSizeFitter   m_CSizeFitter;
        private LayoutElement       m_LElement;
        private GridLayoutGroup     m_GLayoutGroup;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform      => m_RTransform;
        public override ContentSizeFitter   CSizeFitter     => m_CSizeFitter;
        public override LayoutElement       LElement        => m_LElement;
        public override GridLayoutGroup     GLayoutGroup    => m_GLayoutGroup;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            gameObject.layer = UISystem.UILayer;

            m_RTransform = transform as RectTransform;
            m_RTransform.anchorMin = new Vector2(0f, 0f);
            m_RTransform.anchorMax = new Vector2(1f, 1f);
            m_RTransform.sizeDelta = new Vector2(0f, 0f);

            m_GLayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
            m_GLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            m_CSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            m_CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_CSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            m_LElement = gameObject.AddComponent<LayoutElement>();
        }
    }
}
