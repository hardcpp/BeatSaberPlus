using CP_SDK.XUI;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Manager left window
    /// </summary>
    internal sealed class ManagerLeftView : CP_SDK.UI.ViewController<ManagerLeftView>
    {
        private XUIPrimaryButton    m_SafeModeButton;
        private XUISecondaryButton  m_QueueButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Tools"),

                XUIVLayout.Make(
                    XUIPrimaryButton.Make("Select random",      OnRandomButton),

                    XUIVSpacer.Make(5f),

                    XUIPrimaryButton.Make("ENABLE SAFE MODE",   OnSafeModeButton).Bind(ref m_SafeModeButton),
                    XUIPrimaryButton.Make("Clear queue",        OnClearQueueButton),
                    XUIPrimaryButton.Make("Reset blocklist",    OnResetBlocklistButton),

                    XUIVSpacer.Make(5f),

                    XUISecondaryButton.Make("Close queue",      OnQueueButton).Bind(ref m_QueueButton)
                )
                .SetWidth(60f)
                .SetPadding(0)
                .ForEachDirect<XUIPrimaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
                .ForEachDirect<XUISecondaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            UpdateSafeMode();
            UpdateQueueStatus();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update safe mode
        /// </summary>
        internal void UpdateSafeMode()
        {
            if (CRConfig.Instance.SafeMode2)
                m_SafeModeButton?.SetText("DISABLE SAFE MODE");
            else
                m_SafeModeButton?.SetText("ENABLE SAFE MODE");
        }
        /// <summary>
        /// Update queue status
        /// </summary>
        internal void UpdateQueueStatus()
        {
            if (ChatRequest.Instance.QueueOpen)
                m_QueueButton?.SetText("Close queue");
            else
                m_QueueButton?.SetText("Open queue");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On random button
        /// </summary>
        private void OnRandomButton()
            => ManagerMainView.Instance.SelectRandom();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Safe mode button
        /// </summary>
        private void OnSafeModeButton()
        {
            if (CRConfig.Instance.SafeMode2)
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to disable safe mode?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    CRConfig.Instance.SafeMode2 = false;
                    CRConfig.Instance.Save();
                    UpdateSafeMode();
                });
            }
            else
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to enable safe mode?\nThis will hide all song name & uploader in chat.", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    CRConfig.Instance.SafeMode2 = true;
                    CRConfig.Instance.Save();
                    UpdateSafeMode();
                });
            }
        }
        /// <summary>
        /// Clear queue button
        /// </summary>
        private void OnClearQueueButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to clear your queue?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                ChatRequest.Instance.ClearSongEntryQueue();
            });
        }
        /// <summary>
        /// Reset the blocklist button
        /// </summary>
        private void OnResetBlocklistButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset your blocklist?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                ChatRequest.Instance.ResetSongEntryBlocklist();
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On queue button
        /// </summary>
        private void OnQueueButton()
            => ChatRequest.Instance.ToggleQueueStatus();
    }
}
