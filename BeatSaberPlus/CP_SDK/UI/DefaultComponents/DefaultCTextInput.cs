using CP_SDK.Unity.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CTextInput component
    /// </summary>
    public class DefaultCTextInput : Components.CTextInput
    {
        private RectTransform               m_RTransform;
        private LayoutElement               m_LElement;
        private Image                       m_BG;
        private Components.CText            m_Icon;
        private Components.CText            m_ValueText;
        private Button                      m_Button;
        private bool                        m_IsPassword;
        private string                      m_PlaceHolder   = string.Empty;
        private string                      m_Value         = string.Empty;

        private event Action<string> m_OnChange;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => m_RTransform;
        public override LayoutElement           LElement    => m_LElement;

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
            m_RTransform.sizeDelta = new Vector2(60f, 5.5f);

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.minWidth         = 40f;
            m_LElement.minHeight        = 5f;
            m_LElement.preferredHeight  = 5f;
            m_LElement.flexibleWidth    = 150f;

            m_BG = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_BG.gameObject.layer = UISystem.UILayer;
            m_BG.rectTransform.SetParent(transform, false);
            m_BG.rectTransform.pivot                        = new Vector2(  0.50f,  0.50f);
            m_BG.rectTransform.anchorMin                    = Vector2.zero;
            m_BG.rectTransform.anchorMax                    = Vector2.one;
            m_BG.rectTransform.anchoredPosition             = Vector2.zero;
            m_BG.rectTransform.sizeDelta                    = Vector2.zero;
            m_BG.material                   = UISystem.Override_GetUIMaterial();
            m_BG.color                      = new Color32(37, 140, 255, 255);
            m_BG.type                       = Image.Type.Sliced;
            m_BG.pixelsPerUnitMultiplier    = 1;
            m_BG.sprite                     = UISystem.GetUIRoundBGSprite();

            m_Icon = UISystem.TextFactory.Create("Icon", RTransform);
            m_Icon.SetText("📝");
            m_Icon.SetMargins(0, 0, 0, 0);
            m_Icon.SetAlign(TMPro.TextAlignmentOptions.Midline);
            m_Icon.RTransform.pivot             = new Vector2( 1.0f,  0.5f);
            m_Icon.RTransform.anchorMin         = new Vector2( 1.0f,  0.5f);
            m_Icon.RTransform.anchorMax         = new Vector2( 1.0f,  0.5f);
            m_Icon.RTransform.anchoredPosition  = new Vector2( 0.0f,  0.0f);
            m_Icon.RTransform.sizeDelta         = new Vector2( 5.0f,  5.0f);

            m_ValueText = UISystem.TextFactory.Create("Value", RTransform);
            m_ValueText.SetMargins(1, 0, 1, 0);
            m_ValueText.SetOverflowMode(TMPro.TextOverflowModes.Ellipsis);
            m_ValueText.SetAlign(TMPro.TextAlignmentOptions.MidlineLeft);
            m_ValueText.RTransform.anchorMin        = Vector2.zero;
            m_ValueText.RTransform.anchorMax        = Vector2.one;
            m_ValueText.RTransform.anchoredPosition = new Vector2(-2.5f, 0);
            m_ValueText.RTransform.sizeDelta        = new Vector2(-5f, 0);

            m_Button = gameObject.AddComponent<Button>();
            m_Button.targetGraphic  = m_BG;
            m_Button.transition     = Selectable.Transition.ColorTint;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(Button_OnClick);

            var l_Colors = m_Button.colors;
            l_Colors.normalColor        = new Color32(255, 255, 255, 127);
            l_Colors.highlightedColor   = new Color32(255, 255, 255, 255);
            l_Colors.pressedColor       = new Color32(200, 200, 200, 255);
            l_Colors.selectedColor      = l_Colors.normalColor;
            l_Colors.disabledColor      = new Color32(255, 255, 255,  48);
            l_Colors.fadeDuration       = 0.05f;
            m_Button.colors = l_Colors;

            Refresh();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CTextInput OnValueChanged(Action<string> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnChange += p_Functor;
            else        m_OnChange -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get value
        /// </summary>
        /// <returns></returns>
        public override string GetValue()
            => m_Value;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public override Components.CTextInput SetInteractable(bool p_Interactable)
        {
            m_Button.interactable = p_Interactable;

            m_ValueText.SetColor(ColorU.WithAlpha(Color.white, p_Interactable ? 1f : 0.5f));
            m_Icon.SetColor(ColorU.WithAlpha(Color.white, p_Interactable ? 1f : 0.5f));

            return this;
        }
        /// <summary>
        /// Set is password
        /// </summary>
        /// <param name="p_IsPassword">Is password?</param>
        /// <returns></returns>
        public override Components.CTextInput SetIsPassword(bool p_IsPassword)
        {
            m_IsPassword = p_IsPassword;
            Refresh();
            return this;
        }
        /// <summary>
        /// Set place holder
        /// </summary>
        /// <param name="p_PlaceHolder">New place holder</param>
        /// <returns></returns>
        public override Components.CTextInput SetPlaceHolder(string p_PlaceHolder)
        {
            m_PlaceHolder = p_PlaceHolder;
            Refresh();
            return this;
        }
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public override Components.CTextInput SetValue(string p_Value, bool p_Notify = true)
        {
            m_Value = p_Value;
            Refresh();

            if (p_Notify)
                Notify();

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh displayed value
        /// </summary>
        private void Refresh()
        {
            m_Icon.SetAlign(TMPro.TextAlignmentOptions.Midline);

            if (string.IsNullOrEmpty(m_Value))
            {
                m_ValueText.TMProUGUI.text = "<i><alpha=#AA>" + m_PlaceHolder;
                return;
            }

            if (m_IsPassword)
                m_ValueText.TMProUGUI.text = "******";
            else
                m_ValueText.TMProUGUI.text = m_Value;
        }
        /// <summary>
        /// Notify
        /// </summary>
        private void Notify()
        {
            try { m_OnChange?.Invoke(m_Value); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCTextInput.Notify] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On button click
        /// </summary>
        private void Button_OnClick()
        {
            UISystem.Override_OnClick?.Invoke(this);

            var l_OwningView = GetComponentInParent<IViewController>();
            if (l_OwningView == null)
                return;

            l_OwningView.ShowKeyboardModal(m_Value, (x) =>
            {
                m_Value = x;
                Refresh();

                Notify();
            });
        }
    }
}
