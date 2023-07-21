using CP_SDK.XUI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Message modal
    /// </summary>
    public sealed class Message : IModal
    {
        private XUIText m_Message;
        private Action  m_Callback;

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
                .OnReady((x) => {
                    x.HOrVLayoutGroup.childForceExpandWidth = true;
                    x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("OK", OnOKButton).SetWidth(20f)
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
        public void Init(string p_Message, Action p_Callback)
        {
            m_Message.SetText(p_Message);
            m_Callback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On button "OK" pressed
        /// </summary>
        private void OnOKButton()
        {
            VController.CloseModal(this);

            try { m_Callback?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Message.OnOKButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
