using CP_SDK.XUI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Confirmation modal
    /// </summary>
    public sealed class Confirmation : IModal
    {
        private XUIText         m_Message;
        private Action<bool>    m_Callback;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_Message != null)
                return;

            Templates.ModalRectLayout(
                XUIHLayout.Make(
                    XUIText.Make("Message...")
                        .SetAlign(TMPro.TextAlignmentOptions.Top)
                        .SetColor(Color.yellow)
                        .Bind(ref m_Message)
                )
                .OnReady(x => {
                    x.HOrVLayoutGroup.childForceExpandWidth = true;
                    x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Yes", OnYesButton).SetWidth(20.0f),
                    XUISecondaryButton.Make("No", OnNoButton).SetWidth(20.0f)
                )
                .SetPadding(0)
            )
            .BuildUI(transform);
        }
        /// <summary>
        /// On modal close
        /// </summary>
        public override void OnClose()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="p_Message">Message to display</param>
        /// <param name="p_Callback">Callback</param>
        public void Init(string p_Message, Action<bool> p_Callback)
        {
            m_Message.SetText(p_Message);
            m_Callback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On button "Yes" pressed
        /// </summary>
        private void OnYesButton()
        {
            VController.CloseModal(this);

            try { m_Callback?.Invoke(true); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Confirmation.OnYesButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On button "No" pressed
        /// </summary>
        private void OnNoButton()
        {
            VController.CloseModal(this);

            try { m_Callback?.Invoke(false); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Confirmation.OnNoButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
