using CP_SDK.Unity.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CToggle component
    /// </summary>
    public class DefaultCToggle : Components.CToggle
    {
        [Flags]
        private enum AnimationState
        {
            Idle            = 0 << 0,
            SwitchingOn     = 1 << 0,
            SwitchingOff    = 1 << 1,
            HighlightingOn  = 1 << 2,
            HighlightingOff = 1 << 3,
            DisablingOn     = 1 << 4,
            DisablingOff    = 1 << 5
        }
        private class ColorBlock
        {
            public Color KnobColor          = Color.white;
            public Color BackgroundColor    = Color.white;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private RectTransform               m_RTransform;
        private LayoutElement               m_LElement;
        private Subs.SubToggleWithCallbacks m_Toggle;
        private Image                       m_BackgroundImage;
        private Components.CText            m_OffText;
        private Components.CText            m_OnText;
        private Image                       m_KnobImage;
        private bool                        m_PreventChange = false;
        private event Action<bool>          m_OnChange;

        private float           m_SwitchAnimationSmooth     = 16f;
        private float           m_HorizontalStretchAmount   = 0.8f;
        private float           m_VerticalStretchAmount     = 0.2f;

        private AnimationState  m_AnimationState;
        private float           m_SwitchAmount;
        private float           m_HighlightAmount;
        private float           m_DisabledAmount;
        private float           m_OriginalKnobWidth;
        private float           m_OriginalKnobHeight;

        private ColorBlock      m_OnColors;
        private ColorBlock      m_OffColors;
        private ColorBlock      m_OnHighlightedColors;
        private ColorBlock      m_OffHighlightedColors;
        private ColorBlock      m_DisabledColors;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform RTransform    => m_RTransform;
        public override LayoutElement LElement      => m_LElement;
        public override Toggle        Toggle        => m_Toggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            m_OnColors = new ColorBlock() {
                KnobColor       = ColorU.WithAlpha(UISystem.PrimaryColor, 200.0f / 255.0f),
                BackgroundColor = ColorU.WithAlpha(UISystem.PrimaryColor, 100.0f / 255.0f)
            };
            m_OffColors = new ColorBlock() {
                KnobColor       = ColorU.WithAlpha(UISystem.SecondaryColor, 200.0f / 255.0f),
                BackgroundColor = ColorU.WithAlpha(UISystem.SecondaryColor, 100.0f / 255.0f)
            };
            m_OnHighlightedColors = new ColorBlock() {
                KnobColor       = ColorU.WithAlpha(UISystem.PrimaryColor, 255.0f / 255.0f),
                BackgroundColor = ColorU.WithAlpha(UISystem.PrimaryColor, 150.0f / 255.0f)
            };
            m_OffHighlightedColors = new ColorBlock() {
                KnobColor       = ColorU.WithAlpha(UISystem.SecondaryColor, 255.0f / 255.0f),
                BackgroundColor = ColorU.WithAlpha(UISystem.SecondaryColor, 150.0f / 255.0f)
            };
            m_DisabledColors = new ColorBlock() {
                KnobColor       = new Color32(  0,   0,   0,  64),
                BackgroundColor = new Color32(  0,   0,   0,  68)
            };

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
            l_View.anchorMin        = 0.5f * Vector2.one;
            l_View.anchorMax        = 0.5f * Vector2.one;
            l_View.sizeDelta        = new Vector2(15f, 5f);

            m_Toggle = l_View.gameObject.AddComponent<Subs.SubToggleWithCallbacks>();
            m_Toggle.onValueChanged.RemoveAllListeners();
            m_Toggle.onValueChanged.AddListener(Toggle_onValueChanged);
            m_Toggle.StateDidChangeEvent += Toggle_StateDidChange;

            m_BackgroundImage = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_BackgroundImage.gameObject.layer = UISystem.UILayer;
            m_BackgroundImage.rectTransform.SetParent(l_View, false);
            m_BackgroundImage.rectTransform.anchorMin = 0.5f * Vector2.one;
            m_BackgroundImage.rectTransform.anchorMax = 0.5f * Vector2.one;
            m_BackgroundImage.rectTransform.sizeDelta = new Vector2(15f, 5f);
            m_BackgroundImage.sprite     = UISystem.GetUIRoundBGSprite();
            m_BackgroundImage.color      = new Color(0f, 0f, 0f, 0.5f);
            m_BackgroundImage.type       = Image.Type.Sliced;
            m_BackgroundImage.material   = UISystem.Override_GetUIMaterial();

            m_OffText = UISystem.TextFactory.Create("Off", m_BackgroundImage.rectTransform);
            m_OffText.SetText("0").SetAlign(TMPro.TextAlignmentOptions.Capline);
            m_OffText.SetAlpha(0.5f);
            m_OffText.RTransform.anchorMin      = new Vector2( 0.50f, 0.00f);
            m_OffText.RTransform.anchorMax      = new Vector2( 0.50f, 1.00f);
            m_OffText.RTransform.sizeDelta      = new Vector2( 6.00f, 0.00f);
            m_OffText.RTransform.localPosition  = new Vector2( 3.25f, 0.00f);

            m_OnText = UISystem.TextFactory.Create("On", m_BackgroundImage.rectTransform);
            m_OnText.SetText("I").SetAlign(TMPro.TextAlignmentOptions.Capline);
            m_OnText.RTransform.anchorMin       = new Vector2( 0.50f, 0.00f);
            m_OnText.RTransform.anchorMax       = new Vector2( 0.50f, 1.00f);
            m_OnText.RTransform.sizeDelta       = new Vector2( 6.00f, 0.00f);
            m_OnText.RTransform.localPosition   = new Vector2(-3.25f, 0.00f);

            var l_Knob = new GameObject("Knob", typeof(RectTransform)).transform as RectTransform;
            l_Knob.gameObject.layer = UISystem.UILayer;
            l_Knob.SetParent(m_BackgroundImage.rectTransform, false);
            l_Knob.anchorMin        = new Vector2( 0.50f, 0.0f);
            l_Knob.anchorMax        = new Vector2( 0.50f, 1.0f);
            l_Knob.sizeDelta        = new Vector2( 7.50f, 0.0f);
            l_Knob.localPosition    = new Vector2( 0.00f, 0.0f);

            m_KnobImage = new GameObject("Image", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_KnobImage.gameObject.layer = UISystem.UILayer;
            m_KnobImage.rectTransform.SetParent(l_Knob, false);
            m_KnobImage.rectTransform.anchorMin     = new Vector2( 0.00f, 0.5f);
            m_KnobImage.rectTransform.anchorMax     = new Vector2( 0.00f, 0.5f);
            m_KnobImage.rectTransform.sizeDelta     = new Vector2( 6.55f, 4.0f);
            m_KnobImage.rectTransform.localPosition = new Vector2(-3.75f, 0.0f);
            m_KnobImage.sprite          = UISystem.GetUIRoundBGSprite();
            m_KnobImage.color           = new Color(0f, 0f, 0f, 0.5f);
            m_KnobImage.type            = Image.Type.Sliced;
            m_KnobImage.material        = UISystem.Override_GetUIMaterial();
            m_KnobImage.raycastTarget   = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Component first frame
        /// </summary>
        private void Start()
        {
            m_SwitchAmount      = (m_Toggle.isOn ? 1f : 0f);

            m_HighlightAmount   = 0f;
            m_DisabledAmount    = (m_Toggle.IsInteractable() ? 0f : 1f);

            m_AnimationState    = AnimationState.Idle;

            m_OriginalKnobWidth     = m_KnobImage.rectTransform.sizeDelta.x;
            m_OriginalKnobHeight    = m_KnobImage.rectTransform.sizeDelta.y;

            LerpColors(m_SwitchAmount, m_HighlightAmount, m_DisabledAmount);
            LerpPosition(m_SwitchAmount);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_AnimationState == AnimationState.Idle)
            {
                enabled = false;
                return;
            }

            if (m_AnimationState.HasFlag(AnimationState.SwitchingOn))
            {
                m_SwitchAmount = Mathf.Lerp(m_SwitchAmount, 1f, Time.deltaTime * m_SwitchAnimationSmooth);
                if (m_SwitchAmount >= 0.99f)
                {
                    m_SwitchAmount = 1f;
                    m_AnimationState &= ~AnimationState.SwitchingOn;
                }
            }
            else if (m_AnimationState.HasFlag(AnimationState.SwitchingOff))
            {
                m_SwitchAmount = Mathf.Lerp(m_SwitchAmount, 0f, Time.deltaTime * m_SwitchAnimationSmooth);
                if (m_SwitchAmount <= 0.01f)
                {
                    m_SwitchAmount = 0f;
                    m_AnimationState &= ~AnimationState.SwitchingOff;
                }
            }

            if (m_AnimationState.HasFlag(AnimationState.HighlightingOn) && m_DisabledAmount <= 0f)
            {
                m_HighlightAmount = 1f;
                m_AnimationState &= ~AnimationState.HighlightingOn;
            }
            else if (m_AnimationState.HasFlag(AnimationState.HighlightingOff))
            {
                m_HighlightAmount = 0f;
                m_AnimationState &= ~AnimationState.HighlightingOff;
            }

            if (m_AnimationState.HasFlag(AnimationState.DisablingOn))
            {
                m_DisabledAmount = 1f;
                m_AnimationState &= ~AnimationState.DisablingOn;
            }
            else if (m_AnimationState.HasFlag(AnimationState.DisablingOff))
            {
                m_DisabledAmount = 0f;
                m_AnimationState &= ~AnimationState.DisablingOff;
            }

            LerpColors(m_SwitchAmount, m_HighlightAmount, m_DisabledAmount);
            LerpPosition(m_SwitchAmount);

            if (!Mathf.Approximately(0f, m_HorizontalStretchAmount))
                LerpStretch(m_SwitchAmount);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CToggle OnValueChanged(Action<bool> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnChange += p_Functor;
            else        m_OnChange -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get toggle value
        /// </summary>
        /// <returns></returns>
        public override bool GetValue()
            => m_Toggle.isOn;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public override Components.CToggle SetValue(bool p_Value, bool p_Notify = true)
        {
            m_PreventChange = !p_Notify;
            m_Toggle.isOn = p_Value;
            m_PreventChange = false;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggle event onValueChanged
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void Toggle_onValueChanged(bool p_Value)
        {
            if (p_Value)
            {
                m_AnimationState |= AnimationState.SwitchingOn;
                m_AnimationState &= ~AnimationState.SwitchingOff;
            }
            else
            {
                m_AnimationState |= AnimationState.SwitchingOff;
                m_AnimationState &= ~AnimationState.SwitchingOn;
            }

            enabled = true;

            try
            {
                if (!m_PreventChange)
                    m_OnChange?.Invoke(p_Value);
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCToggle.Toggle_onValueChanged] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            UISystem.Override_OnClick?.Invoke(this);
        }
        /// <summary>
        /// Toggle event stateDidChangeEvent
        /// </summary>
        /// <param name="p_SelectionState">New state</param>
        private void Toggle_StateDidChange(Subs.SubToggleWithCallbacks.SelectionState p_SelectionState)
        {
            if (p_SelectionState == Subs.SubToggleWithCallbacks.SelectionState.Disabled)
            {
                m_AnimationState |= AnimationState.DisablingOn;
                m_AnimationState &= ~AnimationState.DisablingOff;
            }
            else
            {
                m_AnimationState |= AnimationState.DisablingOff;
                m_AnimationState &= ~AnimationState.DisablingOn;
            }

            if (   p_SelectionState == Subs.SubToggleWithCallbacks.SelectionState.Highlighted
                || p_SelectionState == Subs.SubToggleWithCallbacks.SelectionState.Pressed
                || p_SelectionState == Subs.SubToggleWithCallbacks.SelectionState.Selected)
            {
                if (m_DisabledAmount <= 0f || m_AnimationState.HasFlag(AnimationState.DisablingOff))
                {
                    m_AnimationState |= AnimationState.HighlightingOn;
                    m_AnimationState &= ~AnimationState.HighlightingOff;
                }
            }
            else
            {
                m_AnimationState |= AnimationState.HighlightingOff;
                m_AnimationState &= ~AnimationState.HighlightingOn;
            }

            enabled = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lerp position
        /// </summary>
        /// <param name="p_SwitchAmount">Switch amount</param>
        private void LerpPosition(float p_SwitchAmount)
        {
            var l_AnchorMin = m_KnobImage.rectTransform.anchorMin;
            var l_AnchorMax = m_KnobImage.rectTransform.anchorMax;

            l_AnchorMin.x = p_SwitchAmount;
            l_AnchorMax.x = p_SwitchAmount;

            m_KnobImage.rectTransform.anchorMin = l_AnchorMin;
            m_KnobImage.rectTransform.anchorMax = l_AnchorMax;
        }
        /// <summary>
        /// Lerp stretching
        /// </summary>
        /// <param name="p_SwitchAmount">Switch amount</param>
        private void LerpStretch(float p_SwitchAmount)
        {
            var l_Factor    = 1f - Mathf.Abs(p_SwitchAmount - 0.5f) * 2f;
            var l_Width     = m_OriginalKnobWidth  * (1f + m_HorizontalStretchAmount * l_Factor);
            var l_Height    = m_OriginalKnobHeight * (1f - m_VerticalStretchAmount   * l_Factor);

            var l_Size = m_KnobImage.rectTransform.sizeDelta;
            l_Size.x = l_Width;
            l_Size.y = l_Height;
            m_KnobImage.rectTransform.sizeDelta = l_Size;
        }
        /// <summary>
        /// Lerp all colors
        /// </summary>
        /// <param name="p_SwitchAmount">Switch amount</param>
        /// <param name="p_HighlightAmount">Highlight amount</param>
        /// <param name="p_DisabledAmount">Disabled amount</param>
        private void LerpColors(float p_SwitchAmount, float p_HighlightAmount, float p_DisabledAmount)
        {
            m_BackgroundImage.color = LerpColor(p_SwitchAmount, p_HighlightAmount, p_DisabledAmount, (ColorBlock x) => x.BackgroundColor);
            m_KnobImage.color       = LerpColor(p_SwitchAmount, p_HighlightAmount, p_DisabledAmount, (ColorBlock x) => x.KnobColor);

            m_OnText.SetAlpha(p_SwitchAmount);
            m_OffText.SetAlpha((1f - p_SwitchAmount) * 0.5f);
        }
        /// <summary>
        /// Lerp specific color
        /// </summary>
        /// <param name="p_SwitchAmount">Switch amount</param>
        /// <param name="p_HighlightAmount">Highlight amount</param>
        /// <param name="p_DisabledAmount">Disabled amount</param>
        /// <param name="p_Delegate">Get specific sub color delegate</param>
        /// <returns></returns>
        private Color LerpColor(float p_SwitchAmount, float p_HighlightAmount, float p_DisabledAmount, Func<ColorBlock, Color> p_Delegate)
        {
            var l_A = Color.Lerp(p_Delegate(m_OffColors),              p_Delegate(m_OnColors),              p_SwitchAmount);
            var l_B = Color.Lerp(p_Delegate(m_OffHighlightedColors),   p_Delegate(m_OnHighlightedColors),   p_SwitchAmount);

            return Color.Lerp(Color.Lerp(l_A, l_B, p_HighlightAmount), p_Delegate(m_DisabledColors), p_DisabledAmount);
        }
    }
}
