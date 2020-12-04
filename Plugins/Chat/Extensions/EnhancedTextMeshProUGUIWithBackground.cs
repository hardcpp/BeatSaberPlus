using BeatSaberPlus.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.Chat.Extensions
{
    /*
       Code from https://github.com/brian91292/EnhancedStreamChat-v3

       MIT License

       Copyright (c) 2020 brian91292

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    /// <summary>
    /// Enhanced text mesh pro UGUI with background support
    /// </summary>
    internal class EnhancedTextMeshProUGUIWithBackground : MonoBehaviour
    {
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
            set
            {
                m_Highlight.enabled = value;
                m_VerticalLayoutGroup.padding = value ? new RectOffset(5, 5, 2, 2) : new RectOffset(5, 5, 1, 1);
            }
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
            set
            {
                SubText.enabled = value;
                SubText.rectTransform.SetParent(value ? gameObject.transform : null, false);
            }
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
        /// <summary>
        /// UI Layout group
        /// </summary>
        private VerticalLayoutGroup m_VerticalLayoutGroup;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the GameObject and his components is ready
        /// </summary>
        private void Awake()
        {
            /// Prepare highlight
            m_Highlight = gameObject.AddComponent<Image>();
            m_Highlight.material = UnityMaterial.UINoGlowMaterial;

            /// Prepare text
            Text = new GameObject().AddComponent<EnhancedTextMeshProUGUI>();
            DontDestroyOnLoad(Text.gameObject);
            Text.OnLatePreRenderRebuildComplete += Text_OnLatePreRenderRebuildComplete;

            /// Prepare sub text
            SubText = new GameObject().AddComponent<EnhancedTextMeshProUGUI>();
            DontDestroyOnLoad(SubText.gameObject);
            SubText.OnLatePreRenderRebuildComplete += Text_OnLatePreRenderRebuildComplete;

            /// Accent image
            m_Accent = new GameObject().AddComponent<Image>();
            DontDestroyOnLoad(m_Accent.gameObject);
            m_Accent.material = UnityMaterial.UINoGlowMaterial;
            m_Accent.color = Color.yellow;

            /// Setup UI layout
            m_VerticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            m_VerticalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
            m_VerticalLayoutGroup.spacing = 1;

            /// Make the highlight color to fix the display
            var l_HighlightFitter = m_Accent.gameObject.AddComponent<LayoutElement>();
            l_HighlightFitter.ignoreLayout = true;

            /// Make the text fit the display
            var l_TextFitter = Text.gameObject.AddComponent<ContentSizeFitter>();
            l_TextFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            /// Make the background color to fix the display
            var l_BackgroundFitter = gameObject.AddComponent<ContentSizeFitter>();
            l_BackgroundFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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
        }
        /// <summary>
        /// When the GameObject is destroyed
        /// </summary>
        private void OnDestroy()
        {
            Text.OnLatePreRenderRebuildComplete     -= Text_OnLatePreRenderRebuildComplete;
            SubText.OnLatePreRenderRebuildComplete  -= Text_OnLatePreRenderRebuildComplete;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the text did render
        /// </summary>
        private void Text_OnLatePreRenderRebuildComplete()
        {
            (m_Accent.gameObject.transform as RectTransform).sizeDelta = new Vector2(1, (transform as RectTransform).sizeDelta.y);
            OnLatePreRenderRebuildComplete?.Invoke();
        }
    }
}
