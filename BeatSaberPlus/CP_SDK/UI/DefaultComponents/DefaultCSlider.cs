using CP_SDK.Unity.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CSlider component
    /// </summary>
    public class DefaultCSlider : Components.CSlider, IBeginDragHandler, IEventSystemHandler, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
    {
        private enum EColorMode
        {
            None,
            H,
            S,
            V,
            O
        }

        private RectTransform               m_RTransform;
        private LayoutElement               m_LElement;
        private Image                       m_BG;
        private Image                       m_BGSub1;
        private Image                       m_BGSub2;
        private Components.CPOrSButton      m_DecButton;
        private Components.CPOrSButton      m_IncButton;
        private RectTransform               m_SlidingArea;
        private Image                       m_Handle;
        private Components.CText            m_ValueText;
        private DrivenRectTransformTracker  m_DrivenRectTransformTracker;
        private Color                       m_OnColor                   = UISystem.PrimaryColor;
        private Color                       m_OffColor                  = UISystem.SecondaryColor;
        private EColorMode                  m_ColorMode                 = EColorMode.None;
        private bool                        m_EnableDragging            = true;
        private bool                        m_IsInteger                 = false;
        private float                       m_HandleSize                = 1.5f;
        private float                       m_ValueSize                 = 20f;
        private float                       m_SeparatorSize             = 0.50f;
        private int                         m_NumberOfSteps             = 100;
        private float                       m_NormalizedValue           = 0.5f;
        private float                       m_MinValue                  = 0.0f;
        private float                       m_MaxValue                  = 1.0f;
        private float                       m_Increments                = 0.1f;
        private float                       m_DragSmoothing             = 1.0f;
        private bool                        m_LeftMouseButtonPressed;
        private float                       m_DragStartTime;
        private float                       m_DragTargetValue;
        private float                       m_DragCurrentValue;
        private PointerEventData            m_LastPointerEvent          = null;

        private         Func<float, string> m_CustomFormatter;
        private event   Action<float>       m_OnChange;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => m_RTransform;
        public override LayoutElement           LElement    => m_LElement;
        public override Components.CPOrSButton  DecButton   => m_DecButton;
        public override Components.CPOrSButton  IncButton   => m_IncButton;

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
            m_LElement.minWidth         = 30f;
            m_LElement.minHeight        = 5f;
            m_LElement.preferredHeight  = 5f;
            m_LElement.flexibleWidth    = 150f;

            m_BG = new GameObject("BG", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_BG.gameObject.layer = UISystem.UILayer;
            m_BG.rectTransform.SetParent(transform, false);
            m_BG.rectTransform.pivot                        = new Vector2(  0.50f,  0.50f);
            m_BG.rectTransform.anchorMin                    = new Vector2(  0.00f,  0.00f);
            m_BG.rectTransform.anchorMax                    = new Vector2(  1.00f,  1.00f);
            m_BG.rectTransform.anchoredPosition             = new Vector2(  0.00f,  0.00f);
            m_BG.rectTransform.sizeDelta                    = new Vector2(-10.00f,  0.00f);
            m_BG.material                   = UISystem.Override_GetUIMaterial();
            m_BG.color                      = ColorU.WithAlpha(m_OnColor, 110f / 255f);
            m_BG.type                       = Image.Type.Sliced;
            m_BG.pixelsPerUnitMultiplier    = 1;
            m_BG.sprite                     = UISystem.GetUISliderBGSprite();

            m_DecButton = UISystem.PrimaryButtonFactory.Create("Dec", m_RTransform);
            m_DecButton.RTransform.pivot                        = new Vector2(0.0f,  0.5f);
            m_DecButton.RTransform.anchorMin                    = new Vector2(0.0f,  0.0f);
            m_DecButton.RTransform.anchorMax                    = new Vector2(0.0f,  1.0f);
            m_DecButton.RTransform.anchoredPosition             = new Vector2(0.0f,  0.0f);
            m_DecButton.RTransform.sizeDelta                    = new Vector2(6.0f, -1.0f);
            m_DecButton.IconImageC.rectTransform.localEulerAngles       = new Vector3(0.0f,  0.0f, -90.0f);
            m_DecButton.IconImageC.rectTransform.localScale             = new Vector3(0.6f,  0.4f,   0.6f);
            m_DecButton.LElement.minWidth                               = 5f;
            m_DecButton.LElement.minHeight                              = 5f;
            m_DecButton.LElement.preferredWidth                         = 5f;
            m_DecButton.LElement.preferredHeight                        = 5f;
            m_DecButton.SetText(string.Empty).SetBackgroundSprite(UISystem.GetUIRoundRectLeftBGSprite()).SetIconSprite(UISystem.GetUIDownArrowSprite()).OnClick(() =>
            {
                SetNormalizedValue(GetSteppedNormalizedValue() - ((m_NumberOfSteps > 0) ? (1f / (float)m_NumberOfSteps) : 0.1f));
            });

            m_IncButton = UISystem.PrimaryButtonFactory.Create("Inc", m_RTransform);
            m_IncButton.RTransform.pivot                        = new Vector2( 1.0f,  0.5f);
            m_IncButton.RTransform.anchorMin                    = new Vector2( 1.0f,  0.0f);
            m_IncButton.RTransform.anchorMax                    = new Vector2( 1.0f,  1.0f);
            m_IncButton.RTransform.anchoredPosition             = new Vector2(-0.0f,  0.0f);
            m_IncButton.RTransform.sizeDelta                    = new Vector2( 6.0f, -1.0f);
            m_IncButton.IconImageC.rectTransform.localEulerAngles       = new Vector3( 0.0f,  0.0f,  90.0f);
            m_IncButton.IconImageC.rectTransform.localScale             = new Vector3( 0.6f,  0.4f,   0.6f);
            m_IncButton.LElement.minWidth                               = 5f;
            m_IncButton.LElement.minHeight                              = 5f;
            m_IncButton.LElement.preferredWidth                         = 5f;
            m_IncButton.LElement.preferredHeight                        = 5f;
            m_IncButton.SetText(string.Empty).SetBackgroundSprite(UISystem.GetUIRoundRectRightBGSprite()).SetIconSprite(UISystem.GetUIDownArrowSprite()).OnClick(() =>
            {
                SetNormalizedValue(GetSteppedNormalizedValue() + ((m_NumberOfSteps > 0) ? (1f / (float)m_NumberOfSteps) : 0.1f));
            });

            m_SlidingArea = new GameObject("SlidingArea", typeof(RectTransform)).GetComponent<RectTransform>();
            m_SlidingArea.gameObject.layer = UISystem.UILayer;
            m_SlidingArea.SetParent(transform, false);
            m_SlidingArea.pivot               = new Vector2(  0.5f,  0.5f);
            m_SlidingArea.anchorMin           = new Vector2(  0.0f,  0.0f);
            m_SlidingArea.anchorMax           = new Vector2(  1.0f,  1.0f);
            m_SlidingArea.anchoredPosition    = new Vector2(  0.0f,  0.0f);
            m_SlidingArea.sizeDelta           = new Vector2(-11.5f, -1.0f);

            m_Handle = new GameObject("Handle", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
            m_Handle.gameObject.layer = UISystem.UILayer;
            m_Handle.rectTransform.SetParent(m_SlidingArea, false);
            m_Handle.rectTransform.pivot                        = new Vector2(0.5f,  0.5f);
            m_Handle.rectTransform.anchorMin                    = new Vector2(0.0f,  0.0f);
            m_Handle.rectTransform.anchorMax                    = new Vector2(0.0f,  1.0f);
            m_Handle.rectTransform.anchoredPosition             = new Vector2(0.0f,  0.0f);
            m_Handle.rectTransform.sizeDelta                    = new Vector2(1.5f, -1.0f);
            m_Handle.material                   = UISystem.Override_GetUIMaterial();
            m_Handle.color                      = new Color32(255, 255, 255, 210);
            m_Handle.type                       = Image.Type.Simple;
            m_Handle.pixelsPerUnitMultiplier    = 15;
            m_Handle.sprite                     = UISystem.GetUISliderHandleSprite();

            m_ValueText = UISystem.TextFactory.Create("Value", m_SlidingArea);

            SetIncrements(0.01f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CSlider OnValueChanged(Action<float> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnChange += p_Functor;
            else        m_OnChange -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get min value
        /// </summary>
        /// <returns></returns>
        public override float GetMinValue() => m_MinValue;
        /// <summary>
        /// Get max value
        /// </summary>
        /// <returns></returns>
        public override float GetMaxValue() => m_MaxValue;
        /// <summary>
        /// Get increments
        /// </summary>
        /// <returns></returns>
        public override float GetIncrements() => m_Increments;
        /// <summary>
        /// Get value
        /// </summary>
        /// <returns></returns>
        public override float GetValue() => ConvertFromSteppedNormalizedValue(GetSteppedNormalizedValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set theme color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public override Components.CSlider SetColor(Color p_Color)
        {
            m_OnColor   = p_Color;
            m_OffColor  = p_Color;

            m_DecButton.SetColor(p_Color);
            m_IncButton.SetColor(p_Color);

            UpdateStyle();

            return this;
        }
        /// <summary>
        /// Set value formatter
        /// </summary>
        /// <param name="p_CustomFormatter">Custom value formatter</param>
        /// <returns></returns>
        public override Components.CSlider SetFormatter(Func<float, string> p_CustomFormatter)
        {
            m_CustomFormatter = p_CustomFormatter;
            return this;
        }
        /// <summary>
        /// Set integer mode
        /// </summary>
        /// <param name="p_IsInteger">Is integer?</param>
        /// <returns></returns>
        public override Components.CSlider SetInteger(bool p_IsInteger)
        {
            m_IsInteger = p_IsInteger;
            return this;
        }
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public override Components.CSlider SetInteractable(bool p_Interactable)
        {
            base.SetInteractable(p_Interactable);
            UpdateStyle();

            return this;
        }
        /// <summary>
        /// Set min value
        /// </summary>
        /// <param name="p_MinValue">New value</param>
        /// <returns></returns>
        public override Components.CSlider SetMinValue(float p_MinValue)
        {
            if (m_MinValue != p_MinValue)
            {
                m_MinValue = p_MinValue;
                UpdateVisuals();
            }

            return this;
        }
        /// <summary>
        /// Set max value
        /// </summary>
        /// <param name="p_MaxValue">New value</param>
        /// <returns></returns>
        public override Components.CSlider SetMaxValue(float p_MaxValue)
        {
            if (m_MaxValue != p_MaxValue)
            {
                m_MaxValue = p_MaxValue;
                UpdateVisuals();
            }

            return this;
        }
        /// <summary>
        /// Set increments
        /// </summary>
        /// <param name="p_Increments">New value</param>
        /// <returns></returns>
        public override Components.CSlider SetIncrements(float p_Increments)
        {
            m_Increments    = p_Increments;
            m_NumberOfSteps = (int)Math.Round((m_MaxValue - m_MinValue) / m_Increments) + 1;
            UpdateVisuals();

            return this;
        }
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public override Components.CSlider SetValue(float p_Value, bool p_Notify = true)
        {
            p_Value = m_IsInteger ? Mathf.Round((int)p_Value) : p_Value;
            SetNormalizedValue((p_Value - m_MinValue) / (m_MaxValue - m_MinValue), p_Notify);
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to color mode
        /// </summary>
        /// <param name="p_H">Is Hue mode?</param>
        /// <param name="p_S">Is saturation mode?</param>
        /// <param name="p_V">Is value mode?</param>
        /// <param name="p_O">Is opacity mode?</param>
        /// <returns></returns>
        public override Components.CSlider SwitchToColorMode(bool p_H, bool p_S, bool p_V, bool p_O)
        {
            if (m_ColorMode != EColorMode.None) return this;

            if (!p_O)
            {
                m_BGSub2 = new GameObject("BGSub2", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
                m_BGSub2.rectTransform.SetParent(m_BG.transform, false);
                m_BGSub2.rectTransform.pivot            = new Vector2(0.50f, 0.50f);
                m_BGSub2.rectTransform.anchorMin        = new Vector2(0.00f, 0.00f);
                m_BGSub2.rectTransform.anchorMax        = new Vector2(1.00f, 1.00f);
                m_BGSub2.rectTransform.anchoredPosition = new Vector2(0.00f, 0.00f);
                m_BGSub2.rectTransform.sizeDelta        = new Vector2(0.00f, 0.00f);
                m_BGSub2.material                   = UISystem.Override_GetUIMaterial();
                m_BGSub2.color                      = Color.white;
                m_BGSub2.type                       = Image.Type.Simple;
                m_BGSub2.pixelsPerUnitMultiplier    = 1;

                m_BG.enabled = false;
            }

            m_ValueText.TMProUGUI.enabled = false;

            if (p_H)
            {
                m_BGSub2.sprite = UISystem.GetUIColorPickerHBGSprite();
                m_ColorMode     = EColorMode.H;
            }
            else if (p_S)
            {
                m_BGSub1 = new GameObject("BGSub1", UISystem.Override_UnityComponent_Image).GetComponent(UISystem.Override_UnityComponent_Image) as Image;
                m_BGSub1.rectTransform.SetParent(m_BG.transform, false);
                m_BGSub1.rectTransform.pivot            = new Vector2(0.50f, 0.50f);
                m_BGSub1.rectTransform.anchorMin        = new Vector2(0.00f, 0.00f);
                m_BGSub1.rectTransform.anchorMax        = new Vector2(1.00f, 1.00f);
                m_BGSub1.rectTransform.anchoredPosition = new Vector2(0.00f, 0.00f);
                m_BGSub1.rectTransform.sizeDelta        = new Vector2(0.00f, 0.00f);
                m_BGSub1.material                   = UISystem.Override_GetUIMaterial();
                m_BGSub1.color                      = Color.white;
                m_BGSub1.type                       = Image.Type.Simple;
                m_BGSub1.pixelsPerUnitMultiplier    = 1;
                m_BGSub1.sprite                     = UISystem.GetUIColorPickerFBGSprite();
                m_BGSub1.transform.SetAsFirstSibling();

                m_BGSub2.sprite = UISystem.GetUIColorPickerSBGSprite();
                m_ColorMode     = EColorMode.S;
            }
            else if (p_V)
            {
                m_BGSub2.sprite = UISystem.GetUIColorPickerVBGSprite();
                m_ColorMode     = EColorMode.V;
            }
            else
                m_ColorMode = EColorMode.O;

            UpdateStyle();

            return this;
        }
        /// <summary>
        /// Color mode set H
        /// </summary>
        /// <param name="p_H">Is Hue mode?</param>
        /// <returns></returns>
        public override Components.CSlider ColorModeSetHue(float p_H)
        {
            if (m_ColorMode == EColorMode.None) return this;

            if (m_ColorMode == EColorMode.S) m_BGSub1.color = Color.HSVToRGB(p_H, 1f, 1f);
            if (m_ColorMode == EColorMode.V) m_BGSub2.color = Color.HSVToRGB(p_H, 1f, 1f);

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component enable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            SetNormalizedValue(m_NormalizedValue, p_Notify: false);
            UpdateVisuals();
        }
        /// <summary>
        /// On component disable
        /// </summary>
        protected override void OnDisable()
        {
            m_DrivenRectTransformTracker.Clear();
            base.OnDisable();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame (late)
        /// </summary>
        private void LateUpdate()
        {
            if (!m_LeftMouseButtonPressed)
                return;

            var l_Delta         = m_DragSmoothing * Time.deltaTime * Mathf.Clamp01((Time.time - m_DragStartTime) / 2.0f);
            m_DragCurrentValue  = Mathf.Lerp(m_DragCurrentValue, m_DragTargetValue, l_Delta);

            SetNormalizedValue(m_DragCurrentValue, true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// May drag based on pointer event data
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        /// <returns></returns>
        private bool MayDrag(PointerEventData p_EventData)
        {
            if (!IsActive() || !IsInteractable())
                return false;

            return p_EventData.button == PointerEventData.InputButton.Left;
        }
        /// <summary>
        /// Initialize a potential drag
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public void OnInitializePotentialDrag(PointerEventData p_EventData)
        {
            p_EventData.useDragThreshold = false;
        }
        /// <summary>
        /// On drag start
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public void OnBeginDrag(PointerEventData p_EventData)
        {
            if (!MayDrag(p_EventData) || !m_EnableDragging)
                return;

            _ = m_SlidingArea == null;
        }
        /// <summary>
        /// On mouse drag
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public void OnDrag(PointerEventData p_EventData)
        {
            if (!MayDrag(p_EventData) || !m_EnableDragging || m_SlidingArea == null)
                return;

            UpdateDrag(p_EventData);
        }
        /// <summary>
        /// Update drag
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        private void UpdateDrag(PointerEventData p_EventData)
        {
            if (p_EventData.button != PointerEventData.InputButton.Left
                || m_SlidingArea == null
                || p_EventData.hovered.Count == 0
                || !RectTransformUtility.ScreenPointToLocalPointInRectangle(m_SlidingArea, p_EventData.position, p_EventData.pressEventCamera, out var l_LocalPoint)
                || float.IsNaN(l_LocalPoint.x)
                || float.IsNaN(l_LocalPoint.y))
                return;

            var l_HandleRectTransform = m_Handle.rectTransform;

            var l_Point = l_LocalPoint - m_SlidingArea.rect.position - new Vector2(l_HandleRectTransform.rect.width * 0.5f, 0f) - (l_HandleRectTransform.rect.size - l_HandleRectTransform.sizeDelta) * 0.5f;
            var l_Value = m_SlidingArea.rect.width * (1f - m_HandleSize / m_SlidingArea.rect.width);

            m_DragTargetValue = (l_Point.x / l_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On pointer enter
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public override void OnPointerEnter(PointerEventData p_EventData)
        {
            base.OnPointerEnter(p_EventData);

            if (IsInteractable())
            {
                m_LastPointerEvent = p_EventData;
                UpdateStyle();
            }
        }
        /// <summary>
        /// On pointer exit
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public override void OnPointerExit(PointerEventData p_EventData)
        {
            base.OnPointerExit(p_EventData);

            m_LastPointerEvent = null;
            UpdateStyle();
        }
        /// <summary>
        /// On pointer button down
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public override void OnPointerDown(PointerEventData p_EventData)
        {
            if (!MayDrag(p_EventData))
                return;

            base.OnPointerDown(p_EventData);

            m_DragCurrentValue          = m_DragTargetValue = GetSteppedNormalizedValue();
            m_LeftMouseButtonPressed    = true;
            m_DragStartTime             = Time.time;

            UpdateDrag(p_EventData);
            UpdateStyle();
        }
        /// <summary>
        /// On pointer button up
        /// </summary>
        /// <param name="p_EventData">Event data</param>
        public override void OnPointerUp(PointerEventData p_EventData)
        {
            base.OnPointerUp(p_EventData);

            if (m_LeftMouseButtonPressed)
            {
                m_LeftMouseButtonPressed = false;
                SetNormalizedValue(m_DragCurrentValue, true);
            }

            UpdateStyle();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set normalized value
        /// </summary>
        /// <param name="p_Value">Normalized value</param>
        /// <param name="p_Notify">Should notify?</param>
        public virtual void SetNormalizedValue(float p_Value, bool p_Notify = true)
        {
            var l_Original = m_NormalizedValue;
            m_NormalizedValue = Mathf.Clamp01(p_Value);

            if (l_Original == GetSteppedNormalizedValue())
                return;

            UpdateVisuals();

            if (p_Notify)
            {
                try { m_OnChange?.Invoke(ConvertFromSteppedNormalizedValue(GetSteppedNormalizedValue())); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCSlider.SetNormalizedValue] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On rect transform dimensions changed
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (IsActive())
                UpdateVisuals();
        }
        /// <summary>
        /// Update visual style
        /// </summary>
        private void UpdateStyle()
        {
            var l_IsInteractable = IsInteractable();
            m_DecButton.SetColor(l_IsInteractable ? m_OnColor : m_OffColor);
            m_IncButton.SetColor(l_IsInteractable ? m_OnColor : m_OffColor);

            if (m_LeftMouseButtonPressed || m_LastPointerEvent != null)
            {
                m_DecButton.SetInteractable(!m_LeftMouseButtonPressed);
                m_IncButton.SetInteractable(!m_LeftMouseButtonPressed);
                m_BG.color = ColorU.WithAlpha(l_IsInteractable ? m_OnColor : m_OffColor, l_IsInteractable ? 200f / 255f : 50f / 255f);
            }
            else
            {
                m_DecButton.SetInteractable(l_IsInteractable);
                m_IncButton.SetInteractable(l_IsInteractable);
                m_BG.color = ColorU.WithAlpha(l_IsInteractable ? m_OnColor : m_OffColor, l_IsInteractable ? 110f / 255f : 50f / 255f);
            }

        }
        /// <summary>
        /// Update visuals
        /// </summary>
        private void UpdateVisuals()
        {
            if (!m_Handle)
                return;

            m_DrivenRectTransformTracker.Clear();

            var l_HandleRectTransform   = m_Handle.rectTransform;
            var l_ValueRectTransform    = m_ValueText.RTransform;

            var l_Width     = m_SlidingArea.rect.width;
            var l_PosX      = GetSteppedNormalizedValue() * (l_Width - m_HandleSize);
            var l_AnchorMin = new Vector2(0f, 0f);
            var l_AnchorMax = new Vector2(0f, 1f);

            m_DrivenRectTransformTracker.Add(this, l_HandleRectTransform, DrivenTransformProperties.AnchorMax);
            m_DrivenRectTransformTracker.Add(this, l_HandleRectTransform, DrivenTransformProperties.AnchorMin);
            m_DrivenRectTransformTracker.Add(this, l_HandleRectTransform, DrivenTransformProperties.SizeDelta);
            m_DrivenRectTransformTracker.Add(this, l_HandleRectTransform, DrivenTransformProperties.Pivot);
            m_DrivenRectTransformTracker.Add(this, l_HandleRectTransform, DrivenTransformProperties.AnchoredPosition);

            l_HandleRectTransform.anchorMin = l_AnchorMin;
            l_HandleRectTransform.anchorMax = l_AnchorMax;
            l_HandleRectTransform.sizeDelta = new Vector2(m_HandleSize, 0f);
            l_HandleRectTransform.pivot = new Vector2(0f, 0.5f);
            l_HandleRectTransform.anchoredPosition = new Vector2(l_PosX, 0f);

            m_DrivenRectTransformTracker.Add(this, l_ValueRectTransform, DrivenTransformProperties.AnchorMax);
            m_DrivenRectTransformTracker.Add(this, l_ValueRectTransform, DrivenTransformProperties.AnchorMin);
            m_DrivenRectTransformTracker.Add(this, l_ValueRectTransform, DrivenTransformProperties.SizeDelta);
            m_DrivenRectTransformTracker.Add(this, l_ValueRectTransform, DrivenTransformProperties.Pivot);
            m_DrivenRectTransformTracker.Add(this, l_ValueRectTransform, DrivenTransformProperties.AnchoredPosition);

            l_ValueRectTransform.anchorMin = l_AnchorMin;
            l_ValueRectTransform.anchorMax = l_AnchorMax;
            l_ValueRectTransform.sizeDelta = new Vector2(m_ValueSize, 0f);

            if (GetSteppedNormalizedValue() > 0.5f)
            {
                l_ValueRectTransform.pivot              = new Vector2(1f, 0.5f);
                l_ValueRectTransform.anchoredPosition   = new Vector2(l_PosX - m_SeparatorSize, 0f);
                m_ValueText.SetAlign(TextAlignmentOptions.CaplineRight);
            }
            else
            {
                l_ValueRectTransform.pivot              = new Vector2(0f, 0.5f);
                l_ValueRectTransform.anchoredPosition   = new Vector2(l_PosX + m_HandleSize + m_SeparatorSize, 0f);
                m_ValueText.SetAlign(TextAlignmentOptions.CaplineLeft);
            }

            m_ValueText.SetText(GetTextValue(ConvertFromSteppedNormalizedValue(GetSteppedNormalizedValue())));
        }
        /// <summary>
        /// On state transition
        /// </summary>
        /// <param name="p_State">New state</param>
        /// <param name="p_Instant">Is instant?</param>
        protected override void DoStateTransition(SelectionState p_State, bool p_Instant)
        {
            base.DoStateTransition(p_State, p_Instant);

            m_DecButton?.SetInteractable(interactable);
            m_IncButton?.SetInteractable(interactable);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On canvas rebuild
        /// </summary>
        /// <param name="p_Executing">Executing update</param>
        public virtual void Rebuild(CanvasUpdate p_Executing)
        {
        }
        /// <summary>
        /// On layout rebuild
        /// </summary>
        public virtual void LayoutComplete()
        {
        }
        /// <summary>
        /// On graphic update complete
        /// </summary>
        public virtual void GraphicUpdateComplete()
        {
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get stepped rounded normalized value
        /// </summary>
        /// <returns></returns>
        private float GetSteppedNormalizedValue()
        {
            var l_Result = m_NormalizedValue;
            if (m_NumberOfSteps > 1)
                l_Result = Mathf.Round(l_Result * (float)(m_NumberOfSteps - 1)) / (float)(m_NumberOfSteps - 1);

            return l_Result;
        }
        /// <summary>
        /// Convert stepped normalized value to value
        /// </summary>
        /// <param name="p_NormalizedValue">Normalized value</param>
        /// <returns></returns>
        private float ConvertFromSteppedNormalizedValue(float p_NormalizedValue)
        {
            var l_Value = p_NormalizedValue * (m_MaxValue - m_MinValue) + m_MinValue;
            return m_IsInteger ? (int)l_Value : l_Value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get text for value
        /// </summary>
        /// <param name="p_ConvertedValue">Value to convert</param>
        /// <returns></returns>
        private string GetTextValue(float p_ConvertedValue)
        {
            if (m_CustomFormatter != null)
                return m_CustomFormatter(m_IsInteger ? (int)p_ConvertedValue : p_ConvertedValue);

            return m_IsInteger ? ((int)p_ConvertedValue).ToString() : p_ConvertedValue.ToString("0.00");
        }
    }
}
