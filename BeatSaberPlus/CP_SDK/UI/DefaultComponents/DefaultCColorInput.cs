using CP_SDK.Unity.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CColorInput component
    /// </summary>
    public class DefaultCColorInput : Components.CColorInput
    {
        private RectTransform               m_RTransform;
        private LayoutElement               m_LElement;
        private Image                       m_BG;
        private Components.CPrimaryButton   m_Icon;
        private Button                      m_Button;
        private Color                       m_Value         = Color.red;
        private Color                       m_OriginalValue;
        private bool                        m_AlphaSupport  = false;

        private event Action<Color> m_OnChange;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform  => m_RTransform;
        public override LayoutElement   LElement    => m_LElement;

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
            m_RTransform.sizeDelta = new Vector2(15f, 5f);

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.preferredWidth   = 15.0f;
            m_LElement.preferredHeight  =  5.0f;
            m_LElement.minWidth         = 15.0f;
            m_LElement.minHeight        =  5.0f;

            var l_View = new GameObject("View", typeof(RectTransform)).transform as RectTransform;
            l_View.gameObject.layer = UISystem.UILayer;
            l_View.SetParent(transform, false);
            l_View.anchorMin = 0.5f * Vector2.one;
            l_View.anchorMax = 0.5f * Vector2.one;
            l_View.sizeDelta = new Vector2(15f, 5f);

            m_BG = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_BG.gameObject.layer = UISystem.UILayer;
            m_BG.rectTransform.SetParent(l_View, false);
            m_BG.rectTransform.pivot                        = new Vector2(  0.50f,  0.50f);
            m_BG.rectTransform.anchorMin                    = new Vector2(  0.00f,  0.00f);
            m_BG.rectTransform.anchorMax                    = new Vector2(  1.00f,  1.00f);
            m_BG.rectTransform.anchoredPosition             = new Vector2( -2.50f,  0.00f);
            m_BG.rectTransform.sizeDelta                    = new Vector2( -5.00f,  0.00f);
            m_BG.material                   = UISystem.Override_GetUIMaterial();
            m_BG.color                      = m_Value;
            m_BG.type                       = Image.Type.Sliced;
            m_BG.pixelsPerUnitMultiplier    = 1;
            m_BG.sprite                     = UISystem.GetUIRoundRectLeftBGSprite();

            m_Icon = UISystem.PrimaryButtonFactory.Create("Inc", l_View);
            m_Icon.RTransform.pivot                        = new Vector2( 1.0f,  0.5f);
            m_Icon.RTransform.anchorMin                    = new Vector2( 1.0f,  0.0f);
            m_Icon.RTransform.anchorMax                    = new Vector2( 1.0f,  1.0f);
            m_Icon.RTransform.anchoredPosition             = new Vector2(-0.0f,  0.0f);
            m_Icon.RTransform.sizeDelta                    = new Vector2( 6.0f, -1.0f);
            m_Icon.LElement.minWidth                               = 5f;
            m_Icon.LElement.minHeight                              = 5f;
            m_Icon.LElement.preferredWidth                         = 5f;
            m_Icon.LElement.preferredHeight                        = 5f;
            m_Icon.SetText("🖌");
            m_Icon.SetBackgroundSprite(UISystem.GetUIRoundRectRightBGSprite());
            m_Icon.OnClick(Button_OnClick);

            m_Button = l_View.gameObject.AddComponent<Button>();
            m_Button.targetGraphic  = m_BG;
            m_Button.transition     = Selectable.Transition.ColorTint;
            m_Button.onClick.RemoveAllListeners();
            m_Button.onClick.AddListener(Button_OnClick);

            var l_Colors = m_Button.colors;
            l_Colors.normalColor        = new Color32(255, 255, 255, 255);
            l_Colors.highlightedColor   = new Color32(255, 255, 255, 127);
            l_Colors.pressedColor       = new Color32(255, 255, 255, 255);
            l_Colors.selectedColor      = l_Colors.normalColor;
            l_Colors.disabledColor      = new Color32(255, 255, 255,  48);
            l_Colors.fadeDuration       = 0.05f;
            m_Button.colors = l_Colors;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CColorInput OnValueChanged(Action<Color> p_Functor, bool p_Add = true)
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
        public override Color GetValue()
            => m_Value;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Set alpha support
        /// </summary>
        /// <param name="p_Support">New state</param>
        /// <returns></returns>
        public override Components.CColorInput SetAlphaSupport(bool p_Support)
        {
            m_AlphaSupport = p_Support;
            return this;
        }
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public override Components.CColorInput SetInteractable(bool p_Interactable)
        {
            m_Button.interactable = p_Interactable;
            m_Icon.SetInteractable(p_Interactable);

            return this;
        }
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public override Components.CColorInput SetValue(Color p_Value, bool p_Notify = true)
        {
            m_Value     = p_Value;
            m_BG.color  = ColorU.WithAlpha(p_Value, 1.0f);

            if (p_Notify)
                Notify();

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notify
        /// </summary>
        private void Notify()
        {
            try { m_OnChange?.Invoke(m_Value); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCColorInput.Notify] Error:");
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

            m_OriginalValue = m_Value;
            l_OwningView.ShowColorPickerModal(m_Value, m_AlphaSupport, (x) => SetValue(x, true), () => SetValue(m_OriginalValue, true));
        }
    }
}
