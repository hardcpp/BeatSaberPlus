using System;
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// IViewController interface
    /// </summary>
    public abstract class IViewController : MonoBehaviour
    {
        public abstract RectTransform   RTransform                  { get; }
        public abstract RectTransform   ModalContainerRTransform    { get; }
        public abstract CanvasGroup     CGroup                      { get; }
        public abstract IScreen         CurrentScreen               { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Activate
        /// </summary>
        /// <param name="p_Screen">Target screen</param>
        public abstract void __Activate(IScreen p_Screen);
        /// <summary>
        /// Deactivate
        /// </summary>
        public abstract void __Deactivate();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show a modal
        /// </summary>
        /// <param name="p_Modal">Modal to show</param>
        public abstract void ShowModal(IModal p_Modal);
        /// <summary>
        /// Close a modal
        /// </summary>
        /// <param name="p_Modal">Modal to close</param>
        public abstract void CloseModal(IModal p_Modal);
        /// <summary>
        /// Close all modals
        /// </summary>
        public abstract void CloseAllModals();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show color picker modal
        /// </summary>
        /// <param name="p_Value">Base value</param>
        /// <param name="p_Opacity">Support opacity?</param>
        /// <param name="p_Callback">On changed callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public abstract void ShowColorPickerModal(Color p_Value, bool p_Opacity, Action<Color> p_Callback, Action p_CancelCallback = null);
        /// <summary>
        /// Show the confirmation modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public abstract void ShowConfirmationModal(string p_Message, Action<bool> p_Callback);
        /// <summary>
        /// Show the dropdown modal
        /// </summary>
        /// <param name="p_Options">Available options</param>
        /// <param name="p_Selected">Selected option</param>
        /// <param name="p_Callback">Callback</param>
        public abstract void ShowDropdownModal(List<string> p_Options, string p_Selected, Action<string> p_Callback);
        /// <summary>
        /// Show the keyboard modal
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public abstract void ShowKeyboardModal(string p_Value, Action<string> p_Callback, Action p_CancelCallback = null, List<(string, Action, string)> p_CustomKeys = null);
        /// <summary>
        /// Show the loading modal
        /// </summary>
        /// <param name="p_Message">Message to show</param>
        /// <param name="p_CancelButton">Show cancel button</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public abstract void ShowLoadingModal(string p_Message = "", bool p_CancelButton = false, Action p_CancelCallback = null);
        /// <summary>
        /// Show the message modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public abstract void ShowMessageModal(string p_Message, Action p_Callback = null);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get current value
        /// </summary>
        /// <returns></returns>
        public abstract string KeyboardModal_GetValue();
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public abstract void KeyboardModal_SetValue(string p_Value);
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="p_ToAppend">Value to append</param>
        public abstract void KeyboardModal_Append(string p_ToAppend);
        /// <summary>
        /// Set message
        /// </summary>
        /// <param name="p_Message">New message</param>
        public abstract void LoadingModal_SetMessage(string p_Message);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Close color picker modal
        /// </summary>
        public abstract void CloseColorPickerModal();
        /// <summary>
        /// Close the confirmation modal
        /// </summary>
        public abstract void CloseConfirmationModal();
        /// <summary>
        /// Close the dropdown modal
        /// </summary>
        public abstract void CloseDropdownModal();
        /// <summary>
        /// Close the keyboard modal
        /// </summary>
        public abstract void CloseKeyboardModal();
        /// <summary>
        /// Close the loading modal
        /// </summary>
        public abstract void CloseLoadingModal();
        /// <summary>
        /// Close the message modal
        /// </summary>
        public abstract void CloseMessageModal();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the tooltip
        /// </summary>
        /// <param name="p_Position">World position</param>
        /// <param name="p_Text">Tooltip text</param>
        public abstract void ShowTooltip(Vector3 p_Position, string p_Text);
        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public abstract void HideTooltip();
    }
}
