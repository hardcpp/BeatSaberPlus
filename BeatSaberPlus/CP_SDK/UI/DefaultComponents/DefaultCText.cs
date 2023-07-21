using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CText component
    /// </summary>
    public class DefaultCText : Components.CText
    {
        private RectTransform   m_RTransform;
        private LayoutElement   m_LElement;
        private TextMeshProUGUI m_TMProUGUI;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform  => m_RTransform;
        public override LayoutElement   LElement    => m_LElement;
        public override TextMeshProUGUI TMProUGUI   => m_TMProUGUI;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_TMProUGUI)
                return;

            gameObject.layer = UISystem.UILayer;

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.minHeight = 5.0f;

            m_TMProUGUI = gameObject.AddComponent(UISystem.Override_UnityComponent_TextMeshProUGUI) as TextMeshProUGUI;
            m_TMProUGUI.font                = UISystem.Override_GetUIFont()                 ?? m_TMProUGUI.font;
            m_TMProUGUI.fontSharedMaterial  = UISystem.Override_GetUIFontSharedMaterial()   ?? m_TMProUGUI.fontSharedMaterial;
            m_TMProUGUI.margin              = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            m_TMProUGUI.fontSize            = 3.4f * UISystem.FontScale;
            m_TMProUGUI.color               = UISystem.TextColor;
            m_TMProUGUI.raycastTarget       = false;

            m_TMProUGUI.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            m_TMProUGUI.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            m_RTransform = transform as RectTransform;

            SetAlign(TextAlignmentOptions.Left);
            SetText("Default Text");
        }
    }
}
