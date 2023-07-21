using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// CP_SDK.UI.IViewController bridge component
    /// </summary>
    public class CP_SDK_UI_IViewControllerBridge : CP_SDK.UI.IViewController
    {
        public override RectTransform       RTransform                  => GetComponent<IHMUIViewController>()?.RTransform;
        public override RectTransform       ModalContainerRTransform    => GetComponent<IHMUIViewController>()?.ModalContainerRTransform;
        public override CanvasGroup         CGroup                      => GetComponent<IHMUIViewController>()?.CGroup;
        public override CP_SDK.UI.IScreen   CurrentScreen               => null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Activate
        /// </summary>
        /// <param name="p_Screen">Target screen</param>
        public override void __Activate(CP_SDK.UI.IScreen p_Screen) { }
        /// <summary>
        /// Deactivate
        /// </summary>
        public override void __Deactivate() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show a modal
        /// </summary>
        /// <param name="p_Modal">Modal to show</param>
        public override void ShowModal(CP_SDK.UI.IModal p_Modal)
            => GetComponent<IHMUIViewController>()?.ShowModal(p_Modal);
        /// <summary>
        /// Close a modal
        /// </summary>
        /// <param name="p_Modal">Modal to close</param>
        public override void CloseModal(CP_SDK.UI.IModal p_Modal)
            => GetComponent<IHMUIViewController>()?.CloseModal(p_Modal);
        /// <summary>
        /// Close all modals
        /// </summary>
        public override void CloseAllModals()
            => GetComponent<IHMUIViewController>()?.CloseAllModals();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show color picker modal
        /// </summary>
        /// <param name="p_Value">Base value</param>
        /// <param name="p_Opacity">Support opacity?</param>
        /// <param name="p_Callback">On changed callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public override void ShowColorPickerModal(Color p_Value, bool p_Opacity, Action<Color> p_Callback, Action p_CancelCallback = null)
            => GetComponent<IHMUIViewController>()?.ShowColorPickerModal(p_Value, p_Opacity, p_Callback, p_CancelCallback);
        /// <summary>
        /// Show the confirmation modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowConfirmationModal(string p_Message, Action<bool> p_Callback)
            => GetComponent<IHMUIViewController>()?.ShowConfirmationModal(p_Message, p_Callback);
        /// <summary>
        /// Show the dropdown modal
        /// </summary>
        /// <param name="p_Options">Available options</param>
        /// <param name="p_Selected">Selected option</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowDropdownModal(List<string> p_Options, string p_Selected, Action<string> p_Callback)
            => GetComponent<IHMUIViewController>()?.ShowDropdownModal(p_Options, p_Selected, p_Callback);
        /// <summary>
        /// Show the keyboard modal
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public override void ShowKeyboardModal(string p_Value, Action<string> p_Callback, Action p_CancelCallback = null, List<(string, Action, string)> p_CustomKeys = null)
            => GetComponent<IHMUIViewController>()?.ShowKeyboardModal(p_Value, p_Callback, p_CancelCallback, p_CustomKeys);
        /// <summary>
        /// Show the loading modal
        /// </summary>
        /// <param name="p_Message">Message to show</param>
        /// <param name="p_CancelButton">Show cancel button</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public override void ShowLoadingModal(string p_Message = "", bool p_CancelButton = false, Action p_CancelCallback = null)
            => GetComponent<IHMUIViewController>()?.ShowLoadingModal(p_Message, p_CancelButton, p_CancelCallback);
        /// <summary>
        /// Show the message modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowMessageModal(string p_Message, Action p_Callback = null)
            => GetComponent<IHMUIViewController>()?.ShowMessageModal(p_Message, p_Callback);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get current value
        /// </summary>
        /// <returns></returns>
        public override string KeyboardModal_GetValue()
            => GetComponent<IHMUIViewController>()?.KeyboardModal_GetValue();
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public override void KeyboardModal_SetValue(string p_Value)
            => GetComponent<IHMUIViewController>()?.KeyboardModal_SetValue(p_Value);
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="p_ToAppend">Value to append</param>
        public override void KeyboardModal_Append(string p_ToAppend)
            => GetComponent<IHMUIViewController>()?.KeyboardModal_Append(p_ToAppend);
        /// <summary>
        /// Set message
        /// </summary>
        /// <param name="p_Message">New message</param>
        public override void LoadingModal_SetMessage(string p_Message)
            => GetComponent<IHMUIViewController>()?.LoadingModal_SetMessage(p_Message);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Close color picker modal
        /// </summary>
        public override void CloseColorPickerModal()
            => GetComponent<IHMUIViewController>()?.CloseColorPickerModal();
        /// <summary>
        /// Close the confirmation modal
        /// </summary>
        public override void CloseConfirmationModal()
            => GetComponent<IHMUIViewController>()?.CloseConfirmationModal();
        /// <summary>
        /// Close the dropdown modal
        /// </summary>
        public override void CloseDropdownModal()
            => GetComponent<IHMUIViewController>()?.CloseDropdownModal();
        /// <summary>
        /// Close the keyboard modal
        /// </summary>
        public override void CloseKeyboardModal()
            => GetComponent<IHMUIViewController>()?.CloseKeyboardModal();
        /// <summary>
        /// Close the loading modal
        /// </summary>
        public override void CloseLoadingModal()
            => GetComponent<IHMUIViewController>()?.CloseLoadingModal();
        /// <summary>
        /// Close the message modal
        /// </summary>
        public override void CloseMessageModal()
            => GetComponent<IHMUIViewController>()?.CloseMessageModal();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the tooltip
        /// </summary>
        /// <param name="p_Position">World position</param>
        /// <param name="p_Text">Tooltip text</param>
        public override void ShowTooltip(Vector3 p_Position, string p_Text)
            => GetComponent<IHMUIViewController>()?.ShowTooltip(p_Position, p_Text);
        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public override void HideTooltip()
            => GetComponent<IHMUIViewController>()?.HideTooltip();
    }
}
