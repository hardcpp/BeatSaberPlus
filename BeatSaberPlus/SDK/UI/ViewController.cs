using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// View controller base class
    /// </summary>
    /// <typeparam name="t_Base"></typeparam>
    public abstract class ViewController<t_Base> : IHMUIViewController
        where t_Base : ViewController<t_Base>
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static t_Base Instance = null;
        /// <summary>
        /// Can UI be updated
        /// </summary>
        public static bool CanBeUpdated => Instance != null && Instance && Instance.isInViewControllerHierarchy && Instance.isActiveAndEnabled && Instance.UICreated;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private RectTransform                   m_RTransform;
        private CanvasGroup                     m_CGroup;
        private int                             m_ModalShowCount;
        private CP_SDK.UI.Components.CHLayout   m_ModalContainer;
        private CP_SDK.UI.Modals.ColorPicker    m_ColorPickerModal;
        private CP_SDK.UI.Modals.Confirmation   m_ConfirmationModal;
        private CP_SDK.UI.Modals.Dropdown       m_DropdownModal;
        private CP_SDK.UI.Modals.Keyboard       m_KeyboardModal;
        private CP_SDK.UI.Modals.Loading        m_LoadingModal;
        private CP_SDK.UI.Modals.Message        m_MessageModal;
        private CP_SDK.UI.Tooltip               m_Tooltip;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform                  => m_RTransform;
        public override RectTransform   ModalContainerRTransform    => m_ModalContainer.RTransform;
        public override CanvasGroup     CGroup                      => m_CGroup;
        public bool                     UICreated                   { get; private set; } = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        public override sealed void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Bind singleton
            Instance = this as t_Base;

            if (p_FirstActivation)
            {
                if (!GetComponent<CP_SDK_UI_IViewControllerBridge>())
                    gameObject.AddComponent<CP_SDK_UI_IViewControllerBridge>();

                /// Get components
                m_RTransform    = GetComponent<RectTransform>();
                m_CGroup        = GetComponent<CanvasGroup>();

                /// Create modal container
                m_ModalContainer = CP_SDK.UI.UISystem.HLayoutFactory.Create("ModalContainer", transform);
                m_ModalContainer.HOrVLayoutGroup.enabled    = false;
                m_ModalContainer.CSizeFitter.enabled        = false;
                m_ModalContainer.RTransform.sizeDelta       = new Vector2(-10.0f, 0.0f);
                m_ModalContainer.gameObject.SetActive(false);

                var l_ModalContainerCanvasGroup = m_ModalContainer.gameObject.AddComponent<CanvasGroup>();
                l_ModalContainerCanvasGroup.ignoreParentGroups = true;

                m_Tooltip = CP_SDK.UI.Tooltip.Create(transform as RectTransform);

                ////////////////////////////////////////////////////////////////////////////

                /// Call implementation
                OnViewCreation();

                /// Change state
                UICreated = true;
            }

            /// Call implementation
            OnViewActivation();
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemDisabling">Is screen system disabling</param>
        public override sealed void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Close all remaining modals
            CloseAllModals();

            /// Call implementation
            OnViewDeactivation();
        }
        /// <summary>
        /// On destruction
        /// </summary>
        public override sealed void OnDestroy()
        {
            /// Call implementation
            OnViewDestruction();

            /// Forward event
            base.OnDestroy();

            /// Unbind singleton
            Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected virtual void OnViewCreation() { }
        /// <summary>
        /// On view activation
        /// </summary>
        protected virtual void OnViewActivation() { }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected virtual void OnViewDeactivation() { }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected virtual void OnViewDestruction() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a modal of type t_ModalType
        /// </summary>
        /// <typeparam name="t_ModalType">Modal type</typeparam>
        /// <returns></returns>
        public t_ModalType CreateModal<t_ModalType>()
            where t_ModalType : CP_SDK.UI.IModal
        {
            var l_GameObject = new GameObject(typeof(t_ModalType).FullName, typeof(RectTransform), typeof(t_ModalType), CP_SDK.UI.UISystem.Override_UnityComponent_Image);
            var l_Modal      = l_GameObject.GetComponent<t_ModalType>();

            l_Modal.RTransform.SetParent(m_ModalContainer.RTransform, false);
            l_Modal.RTransform.anchorMin        = new Vector2(0.0f, 0.0f);
            l_Modal.RTransform.anchorMax        = new Vector2(1.0f, 1.0f);
            l_Modal.RTransform.pivot            = new Vector2(0.5f, 0.5f);
            l_Modal.RTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            l_Modal.RTransform.sizeDelta        = new Vector2(0.0f, 0.0f);

            var l_Background = l_GameObject.GetComponent(CP_SDK.UI.UISystem.Override_UnityComponent_Image) as Image;
            l_Background.material                   = CP_SDK.UI.UISystem.Override_GetUIMaterial();
            l_Background.raycastTarget              = true;
            l_Background.pixelsPerUnitMultiplier    = 1;
            l_Background.type                       = Image.Type.Sliced;
            l_Background.sprite                     = CP_SDK.UI.UISystem.GetUIRoundBGSprite();
            l_Background.color                      = ColorU.WithAlpha(Color.black, 0.80f);

            l_Modal.gameObject.SetActive(false);

            return l_Modal;
        }
        /// <summary>
        /// Show a modal
        /// </summary>
        /// <param name="p_Modal">Modal to show</param>
        public override void ShowModal(CP_SDK.UI.IModal p_Modal)
        {
            if (!p_Modal || p_Modal.RTransform.parent != m_ModalContainer.RTransform)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[BeatSaberPlus.SDK.UI][ViewController<{typeof(t_Base).FullName}>.ShowModal] Null or invalid parented modal, not showing!");
                return;
            }

            if (!p_Modal.gameObject.activeSelf)
            {
                m_ModalShowCount++;

                if (m_ModalShowCount == 1)
                {
                    m_ModalContainer.RTransform.SetAsLastSibling();
                    m_ModalContainer.gameObject.SetActive(true);

                    m_CGroup.enabled        = true;
                    m_CGroup.blocksRaycasts = false;
                }

                p_Modal.transform.SetAsLastSibling();
                p_Modal.VController = GetComponent<CP_SDK_UI_IViewControllerBridge>();
                p_Modal.gameObject.SetActive(true);

                try { p_Modal.OnShow(); }
                catch (Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[BeatSaberPlus.SDK.UI][ViewController<{typeof(t_Base).FullName}>.ShowModal] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }
            }
        }
        /// <summary>
        /// Close a modal
        /// </summary>
        /// <param name="p_Modal">Modal to close</param>
        public override void CloseModal(CP_SDK.UI.IModal p_Modal)
        {
            if (!p_Modal || p_Modal.RTransform.parent != m_ModalContainer.RTransform)
            {
                CP_SDK.ChatPlexSDK.Logger.Error($"[BeatSaberPlus.SDK.UI][ViewController<{typeof(t_Base).FullName}>.CloseModal] Null or invalid parented modal, not closing!");
                return;
            }

            if (p_Modal.gameObject.activeSelf)
            {
                try { p_Modal.OnClose(); }
                catch (System.Exception l_Exception)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[BeatSaberPlus.SDK.UI][ViewController<{typeof(t_Base).FullName}>.CloseModal] Error:");
                    CP_SDK.ChatPlexSDK.Logger.Error(l_Exception);
                }

                p_Modal.gameObject.SetActive(false);

                m_ModalShowCount--;

                if (m_ModalShowCount <= 0)
                    CloseAllModals();
            }
        }
        /// <summary>
        /// Close all modals
        /// </summary>
        public override void CloseAllModals()
        {
            foreach (Transform l_Child in m_ModalContainer.RTransform)
            {
                var l_Modal = l_Child.GetComponent<CP_SDK.UI.IModal>();

                if (!l_Modal || !l_Modal.gameObject.activeSelf)
                    continue;

                l_Modal.OnClose();
                l_Modal.gameObject.SetActive(false);
            }

            m_ModalShowCount = 0;

            m_ModalContainer.gameObject.SetActive(false);

            m_CGroup.blocksRaycasts = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the color picker modal
        /// </summary>
        /// <param name="p_Value">Base value</param>
        /// <param name="p_Opacity">Support opacity?</param>
        /// <param name="p_Callback">On changed callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public override void ShowColorPickerModal(Color p_Value, bool p_Opacity, Action<Color> p_Callback, Action p_CancelCallback)
        {
            if (!m_ColorPickerModal)
                m_ColorPickerModal = CreateModal<CP_SDK.UI.Modals.ColorPicker>();

            ShowModal(m_ColorPickerModal);
            m_ColorPickerModal.Init(p_Value, p_Opacity, p_Callback, p_CancelCallback);
        }
        /// <summary>
        /// Show the confirmation modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowConfirmationModal(string p_Message, Action<bool> p_Callback)
        {
            if (!m_ConfirmationModal)
                m_ConfirmationModal = CreateModal<CP_SDK.UI.Modals.Confirmation>();

            ShowModal(m_ConfirmationModal);
            m_ConfirmationModal.Init(p_Message, p_Callback);
        }
        /// <summary>
        /// Show the dropdown modal
        /// </summary>
        /// <param name="p_Options">Available options</param>
        /// <param name="p_Selected">Selected option</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowDropdownModal(List<string> p_Options, string p_Selected, Action<string> p_Callback)
        {
            if (!m_DropdownModal)
                m_DropdownModal = CreateModal<CP_SDK.UI.Modals.Dropdown>();

            ShowModal(m_DropdownModal);
            m_DropdownModal.Init(p_Options, p_Selected, p_Callback);
        }
        /// <summary>
        /// Show the keyboard modal
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public override void ShowKeyboardModal(string p_Value, Action<string> p_Callback, Action p_CancelCallback = null, List<(string, Action, string)> p_CustomKeys = null)
        {
            if (!m_KeyboardModal)
                m_KeyboardModal = CreateModal<CP_SDK.UI.Modals.Keyboard>();

            ShowModal(m_KeyboardModal);
            m_KeyboardModal.Init(p_Value, p_Callback, p_CancelCallback, p_CustomKeys);
        }
        /// <summary>
        /// Show the loading modal
        /// </summary>
        /// <param name="p_Message">Message to show</param>
        /// <param name="p_CancelButton">Show cancel button</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public override void ShowLoadingModal(string p_Message = "", bool p_CancelButton = false, Action p_CancelCallback = null)
        {
            if (!m_LoadingModal)
                m_LoadingModal = CreateModal<CP_SDK.UI.Modals.Loading>();

            ShowModal(m_LoadingModal);
            m_LoadingModal.Init(p_Message, p_CancelButton, p_CancelCallback);
        }
        /// <summary>
        /// Show the message modal
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public override void ShowMessageModal(string p_Message, Action p_Callback = null)
        {
            if (!m_MessageModal)
                m_MessageModal = CreateModal<CP_SDK.UI.Modals.Message>();

            ShowModal(m_MessageModal);
            m_MessageModal.Init(p_Message, p_Callback);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get current value
        /// </summary>
        /// <returns></returns>
        public override string KeyboardModal_GetValue() => m_KeyboardModal.GetValue();
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public override void KeyboardModal_SetValue(string p_Value) => m_KeyboardModal.SetValue(p_Value);
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="p_ToAppend">Value to append</param>
        public override void KeyboardModal_Append(string p_ToAppend) => m_KeyboardModal.Append(p_ToAppend);
        /// <summary>
        /// Set message
        /// </summary>
        /// <param name="p_Message">New message</param>
        public override void LoadingModal_SetMessage(string p_Message) => m_LoadingModal.SetMessage(p_Message);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Close the color picker modal
        /// </summary>
        public override void CloseColorPickerModal() => CloseModal(m_ColorPickerModal);
        /// <summary>
        /// Close the confirmation modal
        /// </summary>
        public override void CloseConfirmationModal() => CloseModal(m_ConfirmationModal);
        /// <summary>
        /// Close the dropdown modal
        /// </summary>
        public override void CloseDropdownModal() => CloseModal(m_DropdownModal);
        /// <summary>
        /// Close the keyboard modal
        /// </summary>
        public override void CloseKeyboardModal() => CloseModal(m_KeyboardModal);
        /// <summary>
        /// Close the loading modal
        /// </summary>
        public override void CloseLoadingModal() => CloseModal(m_LoadingModal);
        /// <summary>
        /// Close the message modal
        /// </summary>
        public override void CloseMessageModal() => CloseModal(m_MessageModal);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show the tooltip
        /// </summary>
        /// <param name="p_Position">World position</param>
        /// <param name="p_Text">Tooltip text</param>
        public override void ShowTooltip(Vector3 p_Position, string p_Text)
        {
            m_Tooltip.transform.SetAsLastSibling();
            m_Tooltip.Show(p_Position, p_Text);
        }
        /// <summary>
        /// Hide the tooltip
        /// </summary>
        public override void HideTooltip()
            => m_Tooltip.Hide();
    }
}
