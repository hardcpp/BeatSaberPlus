using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using UnityEngine;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Loading modal
    /// </summary>
    public sealed class Loading : IModal
    {
        private XUIText             m_Message       = null;
        private XUISecondaryButton  m_CancelButton  = null;

        private Action m_CancelCallback = null;

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
                XUIImage.Make()
                    .SetWidth(20.0f).SetHeight(20.0f)
                    .SetEnhancedImage(UISystem.GetLoadingAnimation()),

                XUIHLayout.Make(
                    XUIText.Make("Message...")
                        .SetAlign(TMPro.TextAlignmentOptions.Top)
                        .SetColor(Color.yellow)
                        .Bind(ref m_Message)
                )
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained),

                XUISecondaryButton.Make("Cancel", OnCancelButton)
                    .SetWidth(40f)
                    .Bind(ref m_CancelButton)
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
        /// <param name="p_CancelButton">Show cancel button</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public void Init(string p_Message, bool p_CancelButton = false, Action p_CancelCallback = null)
        {
            m_Message.SetText(p_Message);
            m_CancelButton.SetActive(p_CancelButton);

            m_CancelCallback = p_CancelCallback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set message
        /// </summary>
        /// <param name="p_Message">New message</param>
        public void SetMessage(string p_Message)
        {
            m_Message.SetText(p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel button
        /// </summary>
        private void OnCancelButton()
        {
            VController.CloseModal(this);

            try { m_CancelCallback?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Loading.OnCancelButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
