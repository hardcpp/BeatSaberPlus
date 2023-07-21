using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CSlider XUI Element
    /// </summary>
    public class XUISlider
        : IXUIElement, IXUIElementReady<XUISlider, UI.Components.CSlider>, IXUIBindable<XUISlider>
    {
        private UI.Components.CSlider m_Element = null;

        private event Action<UI.Components.CSlider> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => Element?.RTransform;
        public          UI.Components.CSlider   Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUISlider(string p_Name)
            : base(p_Name) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public static XUISlider Make()
            => new XUISlider("XUISlider");
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        public static XUISlider Make(string p_Name)
            => new XUISlider(p_Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.SliderFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUISlider.BuildUI] Error OnReady:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ready, append callback functor
        /// </summary>
        /// <param name="p_Functor">Functor to add</param>
        /// <returns></returns>
        public XUISlider OnReady(Action<UI.Components.CSlider> p_Functor)
        {
            if (m_Element)    p_Functor?.Invoke(m_Element);
            else m_OnReady += p_Functor;
            return this;
        }
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        public XUISlider Bind(ref XUISlider p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUISlider OnValueChanged(Action<float> p_Functor, bool p_Add = true) => OnReady(x => x.OnValueChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUISlider SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set theme color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public XUISlider SetColor(Color p_Color) => OnReady(x => x.SetColor(p_Color));
        /// <summary>
        /// Set value formatter
        /// </summary>
        /// <param name="p_CustomFormatter">Custom value formatter</param>
        /// <returns></returns>
        public XUISlider SetFormatter(Func<float, string> p_CustomFormatter) => OnReady(x => x.SetFormatter(p_CustomFormatter));
        /// <summary>
        /// Set integer mode
        /// </summary>
        /// <param name="p_IsInteger">Is integer?</param>
        /// <returns></returns>
        public XUISlider SetInteger(bool p_IsInteger) => OnReady(x => x.SetInteger(p_IsInteger));
        /// <summary>
        /// Set min value
        /// </summary>
        /// <param name="p_MinValue">New value</param>
        /// <returns></returns>
        public XUISlider SetMinValue(float p_MinValue) => OnReady(x => x.SetMinValue(p_MinValue));
        /// <summary>
        /// Set max value
        /// </summary>
        /// <param name="p_MaxValue">New value</param>
        /// <returns></returns>
        public XUISlider SetMaxValue(float p_MaxValue) => OnReady(x => x.SetMaxValue(p_MaxValue));
        /// <summary>
        /// Set increments
        /// </summary>
        /// <param name="p_Increments">New value</param>
        /// <returns></returns>
        public XUISlider SetIncrements(float p_Increments) => OnReady(x => x.SetIncrements(p_Increments));
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUISlider SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public XUISlider SetValue(float p_Value, bool p_Notify = true) => OnReady(x => x.SetValue(p_Value, p_Notify));

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
        public XUISlider SwitchToColorMode(bool p_H, bool p_S, bool p_V, bool p_O) => OnReady(x => x.SwitchToColorMode(p_H, p_S, p_V, p_O));
        /// <summary>
        /// Color mode set H
        /// </summary>
        /// <param name="p_H">Is Hue mode?</param>
        /// <returns></returns>
        public XUISlider ColorModeSetHue(float p_H) => OnReady(x => x.ColorModeSetHue(p_H));
    }
}
