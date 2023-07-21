using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CTextInput component
    /// </summary>
    public abstract class CTextInput : MonoBehaviour
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
        public abstract CTextInput OnValueChanged(Action<string> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get value
        /// </summary>
        /// <returns></returns>
        public abstract string GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public abstract CTextInput SetInteractable(bool p_Interactable);
        /// <summary>
        /// Set is password
        /// </summary>
        /// <param name="p_IsPassword">Is password?</param>
        /// <returns></returns>
        public abstract CTextInput SetIsPassword(bool p_IsPassword);
        /// <summary>
        /// Set place holder
        /// </summary>
        /// <param name="p_PlaceHolder">New place holder</param>
        /// <returns></returns>
        public abstract CTextInput SetPlaceHolder(string p_PlaceHolder);
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CTextInput SetValue(string p_Value, bool p_Notify = true);
    }
}
