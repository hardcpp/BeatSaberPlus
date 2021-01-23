using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.Chat.Extensions
{
    /// <summary>
    /// Enhanced text mesh pro UGUI with background support
    /// </summary>
    internal class EnhancedTextMeshProUGUIWithBackground : MonoBehaviour
    {
        internal static float s_TopDownMargins = 1;
        internal static float s_LeftRightMargins = 4;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Text instance
        /// </summary>
        internal EnhancedTextMeshProUGUI Text;
        /// <summary>
        /// SubText instance
        /// </summary>
        internal EnhancedTextMeshProUGUI SubText;
        /// <summary>
        /// On rebuild complete
        /// </summary>
        internal event Action OnLatePreRenderRebuildComplete;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rect transform instance
        /// </summary>
        internal RectTransform RectTranform => transform as RectTransform;
        /// <summary>
        /// Accent color
        /// </summary>
        internal Color AccentColor
        {
            get => m_Accent.color;
            set => m_Accent.color = value;
        }
        /// <summary>
        /// Highlight color
        /// </summary>
        internal Color HighlightColor
        {
            get => m_Highlight.color;
            set => m_Highlight.color = value;
        }

        /// <summary>
        /// Enable or disable Highlight
        /// </summary>
        internal bool HighlightEnabled
        {
            get => m_Highlight.enabled;
            set => m_Highlight.enabled = value;
        }
        /// <summary>
        /// Enable or disable Accent
        /// </summary>
        internal bool AccentEnabled
        {
            get => m_Accent.enabled;
            set => m_Accent.enabled = value;
        }
        /// <summary>
        /// Enable or disable SubText
        /// </summary>
        internal bool SubTextEnabled
        {
            get => SubText.enabled;
            set => SubText.enabled = value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Highlight image
        /// </summary>
        private Image m_Highlight;
        /// <summary>
        /// Accent image
        /// </summary>
        private Image m_Accent;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the GameObject and his components is ready
        /// </summary>
        private void Awake()
        {
            /// Prepare highlight
            m_Highlight = gameObject.AddComponent<Image>();
            m_Highlight.material = SDK.Unity.Material.UINoGlowMaterial;

            RectTranform.anchorMin  = Vector2.one * 0.5f;
            RectTranform.anchorMax  = Vector2.one * 0.5f;
            RectTranform.pivot      = new Vector2(0.5f, 1f);

            /// Prepare text
            Text = new GameObject().AddComponent<EnhancedTextMeshProUGUI>();
            Text.rectTransform.anchorMin            = new Vector2(0f, 1f);
            Text.rectTransform.anchorMax            = new Vector2(0f, 1f);
            Text.rectTransform.pivot                = new Vector2(0f, 1f);
            Text.rectTransform.localPosition        = Vector3.zero;
            Text.margin                             = new Vector4(s_LeftRightMargins, s_TopDownMargins, s_LeftRightMargins, s_TopDownMargins);
            DontDestroyOnLoad(Text.gameObject);
            Text.OnLatePreRenderRebuildComplete += Text_OnLatePreRenderRebuildComplete;

            /// Prepare sub text
            SubText = new GameObject().AddComponent<EnhancedTextMeshProUGUI>();
            SubText.rectTransform.anchorMin         = new Vector2(0f, 1f);
            SubText.rectTransform.anchorMax         = new Vector2(0f, 1f);
            SubText.rectTransform.pivot             = new Vector2(0f, 1f);
            SubText.rectTransform.localPosition     = Vector3.zero;
            SubText.margin                          = new Vector4(s_LeftRightMargins, s_TopDownMargins, s_LeftRightMargins, s_TopDownMargins);
            DontDestroyOnLoad(SubText.gameObject);
            SubText.OnLatePreRenderRebuildComplete += Text_OnLatePreRenderRebuildComplete;

            /// Accent image
            m_Accent = new GameObject().AddComponent<Image>();
            DontDestroyOnLoad(m_Accent.gameObject);
            m_Accent.material   = SDK.Unity.Material.UINoGlowMaterial;
            m_Accent.color      = Color.white;

            /// Disable all sub element by default
            SubTextEnabled      = false;
            HighlightEnabled    = false;
            AccentEnabled       = false;

            /// Update accent Layout UI transform
            m_Accent.gameObject.transform.SetParent(gameObject.transform, false);
            (m_Accent.gameObject.transform as RectTransform).anchorMin  = new Vector2(0, 0.5f);
            (m_Accent.gameObject.transform as RectTransform).anchorMax  = new Vector2(0, 0.5f);
            (m_Accent.gameObject.transform as RectTransform).sizeDelta  = new Vector2(1, 10);
            (m_Accent.gameObject.transform as RectTransform).pivot      = new Vector2(0, 0.5f);

            Text.rectTransform.SetParent(gameObject.transform, false);
            SubText.rectTransform.SetParent(gameObject.transform, false);
        }
        /// <summary>
        /// When the GameObject is destroyed
        /// </summary>
        private void OnDestroy()
        {
            Text.OnLatePreRenderRebuildComplete     -= Text_OnLatePreRenderRebuildComplete;
            SubText.OnLatePreRenderRebuildComplete  -= Text_OnLatePreRenderRebuildComplete;
        }
        /// <summary>
        /// Set width
        /// </summary>
        /// <param name="p_Width"></param>
        internal void SetWidth(float p_Width)
        {
            Text.rectTransform.sizeDelta    = new Vector2(p_Width, Text.rectTransform.sizeDelta.y);
            SubText.rectTransform.sizeDelta = new Vector2(p_Width, SubText.rectTransform.sizeDelta.y);
            RectTranform.sizeDelta          = new Vector2(p_Width, RectTranform.sizeDelta.y);

            OnTextChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the text did render
        /// </summary>
        private void Text_OnLatePreRenderRebuildComplete()
        {
            OnTextChanged();
            OnLatePreRenderRebuildComplete?.Invoke();
        }
        /// <summary>
        /// On text changed, update all width/height
        /// </summary>
        private void OnTextChanged()
        {
            float l_TextHeight      = Mathf.Max(0, Text.GetRenderedValues().y + (2 * s_TopDownMargins));
            float l_SubTextHeight   = SubTextEnabled ? Mathf.Max(0, SubText.GetRenderedValues().y + (2 * s_TopDownMargins)) : 0;

            if (l_TextHeight == 0)
                l_TextHeight = Text.GetPreferredValues(" ").y;

            RectTranform.sizeDelta = new Vector2(RectTranform.sizeDelta.x, l_TextHeight + l_SubTextHeight);

            SubText.rectTransform.localPosition = new Vector3(SubText.rectTransform.localPosition.x, l_SubTextHeight, SubText.rectTransform.localPosition.z);
            m_Accent.rectTransform.sizeDelta = new Vector2(s_LeftRightMargins / 2f, RectTranform.sizeDelta.y);
        }
    }
}
