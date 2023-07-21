using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CDropdown component
    /// </summary>
    public abstract class CDropdown : MonoBehaviour
    {
        public abstract RectTransform RTransform { get; }
        public abstract LayoutElement LElement   { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CDropdown OnValueChanged(Action<int, string> p_Functor, bool p_Add = true);

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
        public abstract CDropdown SetInteractable(bool p_Interactable);
        /// <summary>
        /// Set available options
        /// </summary>
        /// <param name="p_Options">New options list</param>
        /// <returns></returns>
        public abstract CDropdown SetOptions(List<string> p_Options);
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Select option</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CDropdown SetValue(string p_Value, bool p_Notify = true);
    }
}
