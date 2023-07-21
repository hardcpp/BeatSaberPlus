using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CSlider component
    /// </summary>
    public abstract class CSlider : Selectable
    {
        public abstract RectTransform   RTransform  { get; }
        public abstract LayoutElement   LElement    { get; }
        public abstract CPOrSButton     DecButton   { get; }
        public abstract CPOrSButton     IncButton   { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CSlider OnValueChanged(Action<float> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get min value
        /// </summary>
        /// <returns></returns>
        public abstract float GetMinValue();
        /// <summary>
        /// Get max value
        /// </summary>
        /// <returns></returns>
        public abstract float GetMaxValue();
        /// <summary>
        /// Get increments
        /// </summary>
        /// <returns></returns>
        public abstract float GetIncrements();
        /// <summary>
        /// Get value
        /// </summary>
        /// <returns></returns>
        public abstract float GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set theme color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public abstract CSlider SetColor(Color p_Color);
        /// <summary>
        /// Set value formatter
        /// </summary>
        /// <param name="p_CustomFormatter">Custom value formatter</param>
        /// <returns></returns>
        public abstract CSlider SetFormatter(Func<float, string> p_CustomFormatter);
        /// <summary>
        /// Set integer mode
        /// </summary>
        /// <param name="p_IsInteger">Is integer?</param>
        /// <returns></returns>
        public abstract CSlider SetInteger(bool p_IsInteger);
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public virtual CSlider SetInteractable(bool p_Interactable)
        {
            interactable = p_Interactable;
            return this;
        }
        /// <summary>
        /// Set min value
        /// </summary>
        /// <param name="p_MinValue">New value</param>
        /// <returns></returns>
        public abstract CSlider SetMinValue(float p_MinValue);
        /// <summary>
        /// Set max value
        /// </summary>
        /// <param name="p_MaxValue">New value</param>
        /// <returns></returns>
        public abstract CSlider SetMaxValue(float p_MaxValue);
        /// <summary>
        /// Set increments
        /// </summary>
        /// <param name="p_Increments">New value</param>
        /// <returns></returns>
        public abstract CSlider SetIncrements(float p_Increments);
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public abstract CSlider SetValue(float p_Value, bool p_Notify = true);

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
        public abstract CSlider SwitchToColorMode(bool p_H, bool p_S, bool p_V, bool p_O);
        /// <summary>
        /// Color mode set H
        /// </summary>
        /// <param name="p_H">Is Hue mode?</param>
        /// <returns></returns>
        public abstract CSlider ColorModeSetHue(float p_H);
    }
}
