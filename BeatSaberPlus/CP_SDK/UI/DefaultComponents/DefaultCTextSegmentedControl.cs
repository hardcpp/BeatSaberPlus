using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CTextSegmentedControl component
    /// </summary>
    public class DefaultCTextSegmentedControl : Components.CTextSegmentedControl
    {
        private RectTransform       m_RTransform;
        private ContentSizeFitter   m_CSizeFitter;
        private LayoutElement       m_LElement;
        private List<Button>        m_Controls = new List<Button>();
        private int                 m_ActiveControl = 0;

        private event Action<int> m_OnActiveChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform  => m_RTransform;
        public override ContentSizeFitter   CSizeFitter => m_CSizeFitter;
        public override LayoutElement       LElement    => m_LElement;

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

            m_CSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            m_CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            m_CSizeFitter.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.minHeight        = 5f;
            m_LElement.preferredHeight  = 5f;

            var l_Layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            l_Layout.childControlHeight = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active text changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CTextSegmentedControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnActiveChanged += p_Functor;
            else        m_OnActiveChanged -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get active text
        /// </summary>
        /// <returns></returns>
        public override int GetActiveText()
            => m_ActiveControl;
        /// <summary>
        /// Get text count
        /// </summary>
        /// <returns></returns>
        public override int GetTextCount()
            => m_Controls.Count;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set active text
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public override Components.CTextSegmentedControl SetActiveText(int p_Index, bool p_Notify = true)
        {
            if (p_Index < 0 || p_Index == m_ActiveControl || p_Index >= m_Controls.Count)
                return this;

            OnControlClicked(m_Controls[p_Index], p_Notify);
            return this;
        }
        /// <summary>
        /// Set texts
        /// </summary>
        /// <param name="p_Texts">New texts</param>
        /// <returns></returns>
        public override Components.CTextSegmentedControl SetTexts(params string[] p_Texts)
        {
            gameObject.SetActive(false);

            m_Controls.Clear();
            m_ActiveControl = 0;

            foreach (Transform l_Child in transform)
                GameObject.Destroy(l_Child.gameObject);

            if (p_Texts == null || p_Texts.Length == 0)
                p_Texts = new string[] { "Default" };

            for (var l_I = 0; l_I < p_Texts.Length; ++l_I)
            {
                var l_Control = new GameObject("Tab" + l_I.ToString(), typeof(RectTransform)).GetComponent<RectTransform>();
                l_Control.gameObject.layer = UISystem.UILayer;
                l_Control.SetParent(transform, false);
                l_Control.sizeDelta = new Vector2(0f, 5f);

                var l_Background = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
                l_Background.gameObject.layer = UISystem.UILayer;
                l_Background.rectTransform.SetParent(l_Control.transform, false);
                l_Background.rectTransform.anchorMin        = Vector3.zero;
                l_Background.rectTransform.anchorMax        = Vector3.one;
                l_Background.rectTransform.sizeDelta        = Vector2.zero;
                l_Background.rectTransform.anchoredPosition = Vector3.zero;
                l_Background.material                   = UISystem.Override_GetUIMaterial();
                l_Background.type                       = Image.Type.Sliced;
                l_Background.pixelsPerUnitMultiplier    = 1;
                l_Background.color                      = ColorU.ToUnityColor("#727272");

                if (p_Texts.Length == 1)
                    l_Background.sprite = UISystem.GetUIRoundBGSprite();
                else if (l_I == 0)
                    l_Background.sprite = UISystem.GetUIRoundRectLeftBGSprite();
                else if (l_I == (p_Texts.Length - 1))
                    l_Background.sprite = UISystem.GetUIRoundRectRightBGSprite();
                else
                    l_Background.sprite = UISystem.GetUISliderBGSprite();

                var l_Label = UISystem.TextFactory.Create("Label", l_Control.transform);
                l_Label.RTransform.anchorMin        = Vector3.zero;
                l_Label.RTransform.anchorMax        = Vector3.one;
                l_Label.RTransform.sizeDelta        = Vector2.zero;
                l_Label.RTransform.anchoredPosition = Vector3.zero;
                l_Label.SetText(p_Texts[l_I]);
                l_Label.SetAlign(TMPro.TextAlignmentOptions.Midline);
                l_Label.SetStyle(TMPro.FontStyles.Bold);

                var l_Button = l_Control.gameObject.AddComponent<Button>();
                l_Button.targetGraphic = l_Background;
                l_Button.onClick.AddListener(() => OnControlClicked(l_Button, true));

                var l_Colors = l_Button.colors;
                l_Colors.normalColor = ColorU.WithAlpha(Color.white, l_I == m_ActiveControl ? 0.75f : 0.25f);
                l_Button.colors = l_Colors;

                m_Controls.Add(l_Button);
            }

            gameObject.SetActive(true);

            try { m_OnActiveChanged?.Invoke(m_ActiveControl); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCTextSegmentedControl.SetTexts] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On control clicked
        /// </summary>
        /// <param name="p_Button">Clicked control's button</param>
        /// <param name="p_Notify">Should notify callback?</param>
        private void OnControlClicked(Button p_Button, bool p_Notify)
        {
            var l_Index = m_Controls.IndexOf(p_Button);
            if (l_Index == -1 || l_Index == m_ActiveControl)
                return;

            for (var l_I = 0; l_I < m_Controls.Count; ++l_I)
            {
                var l_Colors = m_Controls[l_I].colors;
                l_Colors.normalColor = ColorU.WithAlpha(Color.white, l_I == l_Index ? 0.75f : 0.25f);
                m_Controls[l_I].colors = l_Colors;
            }

            m_ActiveControl = l_Index;

            if (p_Notify)
            {
                try { m_OnActiveChanged?.Invoke(m_ActiveControl); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCTextSegmentedControl.OnControlClicked] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                UISystem.Override_OnClick?.Invoke(this);
            }
        }
    }
}
