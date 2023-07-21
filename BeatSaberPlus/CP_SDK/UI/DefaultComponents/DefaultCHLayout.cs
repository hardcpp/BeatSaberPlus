using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CHLayout component
    /// </summary>
    public class DefaultCHLayout : Components.CHLayout
    {
        private RectTransform           m_RTransform;
        private ContentSizeFitter       m_CSizeFitter;
        private LayoutElement           m_LElement;
        private HorizontalLayoutGroup   m_HLayoutGroup;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform                   RTransform      => m_RTransform;
        public override ContentSizeFitter               CSizeFitter     => m_CSizeFitter;
        public override LayoutElement                   LElement        => m_LElement;
        public override HorizontalOrVerticalLayoutGroup HOrVLayoutGroup => m_HLayoutGroup;
        public override HorizontalLayoutGroup           HLayoutGroup    => m_HLayoutGroup;

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

            m_CSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            m_CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            m_CSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            m_LElement = gameObject.AddComponent<LayoutElement>();

            m_HLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            m_HLayoutGroup.childForceExpandWidth    = false;
            m_HLayoutGroup.childForceExpandHeight   = false;
            m_HLayoutGroup.childScaleWidth          = false;
            m_HLayoutGroup.childScaleHeight         = false;
            m_HLayoutGroup.childControlWidth        = true;
            m_HLayoutGroup.childControlHeight       = true;
            m_HLayoutGroup.childAlignment           = TextAnchor.UpperCenter;
        }
    }
}
