using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CVLayout component
    /// </summary>
    public class DefaultCVLayout : Components.CVLayout
    {
        private RectTransform           m_RTransform;
        private ContentSizeFitter       m_ContentSizeFitter;
        private LayoutElement           m_LElement;
        private VerticalLayoutGroup     m_VLayoutGroup;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform                   RTransform      => m_RTransform;
        public override ContentSizeFitter               CSizeFitter     => m_ContentSizeFitter;
        public override LayoutElement                   LElement        => m_LElement;
        public override HorizontalOrVerticalLayoutGroup HOrVLayoutGroup => m_VLayoutGroup;
        public override VerticalLayoutGroup             VLayoutGroup    => m_VLayoutGroup;

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
            m_RTransform.anchorMin = new Vector2(0.0f, 0.0f);
            m_RTransform.anchorMax = new Vector2(1.0f, 1.0f);
            m_RTransform.sizeDelta = new Vector2(0.0f, 0.0f);

            m_ContentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            m_ContentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_ContentSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            m_LElement = gameObject.AddComponent<LayoutElement>();

            m_VLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            m_VLayoutGroup.childForceExpandWidth    = false;
            m_VLayoutGroup.childForceExpandHeight   = false;
            m_VLayoutGroup.childScaleWidth          = false;
            m_VLayoutGroup.childScaleHeight         = false;
            m_VLayoutGroup.childControlWidth        = true;
            m_VLayoutGroup.childControlHeight       = true;
            m_VLayoutGroup.childAlignment           = TextAnchor.UpperCenter;
        }
    }
}
