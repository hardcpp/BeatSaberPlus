using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CColorInput component
    /// </summary>
    public abstract class CColorInput : MonoBehaviour
    {
        public abstract RectTransform   RTransform  { get; }
        public abstract LayoutElement   LElement    { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CColorInput OnValueChanged(Action<Color> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get value
        /// </summary>
        /// <returns></returns>
        public abstract Color GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set alpha support
        /// </summary>
        /// <param name="p_Support">New state</param>
        /// <returns></returns>
        public abstract CColorInput SetAlphaSupport(bool p_Support);
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public abstract CColorInput SetInteractable(bool p_Interactable);
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CColorInput SetValue(Color p_Value, bool p_Notify = true);
    }
}
