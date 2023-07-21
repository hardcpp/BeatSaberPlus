using System;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CToggle component
    /// </summary>
    public abstract class CToggle : MonoBehaviour
    {
        public abstract RectTransform RTransform    { get; }
        public abstract LayoutElement LElement      { get; }
        public abstract Toggle        Toggle        { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CToggle OnValueChanged(Action<bool> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get toggle value
        /// </summary>
        /// <returns></returns>
        public abstract bool GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public virtual CToggle SetInteractable(bool p_Interactable)
        {
            Toggle.interactable = p_Interactable;
            return this;
        }
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public abstract CToggle SetValue(bool p_Value, bool p_Notify = true);
    }
}
