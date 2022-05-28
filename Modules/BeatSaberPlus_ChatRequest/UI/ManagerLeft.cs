using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Manager left window
    /// </summary>
    internal class ManagerLeft : BeatSaberPlus.SDK.UI.ResourceViewController<ManagerLeft>
    {
#pragma warning disable CS0649
        [UIComponent("SafeButton")]
        private Button m_SafeButton = null;

        [UIComponent("QueueButton")]
        private Button m_QueueButton = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            UpdateSafeMode();
            UpdateQueueStatus();
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
            if (m_SafeButton == null)
                return;

            if (CRConfig.Instance.SafeMode)
                m_SafeButton.GetComponentInChildren<CurvedTextMeshPro>().text = "DISABLE SAFE MODE";
            else
                m_SafeButton.GetComponentInChildren<CurvedTextMeshPro>().text = "ENABLE SAFE MODE";
        }
        /// <summary>
        /// Update queue status
        /// </summary>
        internal void UpdateQueueStatus()
        {
            if (m_QueueButton == null)
                return;

            if (ChatRequest.Instance.QueueOpen)
                m_QueueButton.GetComponentInChildren<CurvedTextMeshPro>().text = "Close queue";
            else
                m_QueueButton.GetComponentInChildren<CurvedTextMeshPro>().text = "Open queue";
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On random button
        /// </summary>
        [UIAction("click-btn-random")]
        private void OnRandomButton()
        {
            ManagerMain.Instance.SelectRandom();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Safe mode button
        /// </summary>
        [UIAction("click-btn-safe")]
        private void OnSafeModeButton()
        {
            if (CRConfig.Instance.SafeMode)
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to disable safe mode?", () =>
                {
                    CRConfig.Instance.SafeMode = false;
                    UpdateSafeMode();
                });
            }
            else
            {
                ShowConfirmationModal("<color=yellow><b>Do you really want to enable safe mode?\nThis will hide all song name & uploader in chat.", () =>
                {
                    CRConfig.Instance.SafeMode = true;
                    UpdateSafeMode();
                });
            }
        }
        /// <summary>
        /// Clear queue button
        /// </summary>
        [UIAction("click-clear-queue-btn-pressed")]
        private void OnClearQueueButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset clear the song queue?", () =>
            {
                ChatRequest.Instance.ClearQueue();
            });
        }
        /// <summary>
        /// Cleat queue button
        /// </summary>
        [UIAction("click-reset-blacklist-btn-pressed")]
        private void OnResetBlacklistButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset your blacklist?", () =>
            {
                ChatRequest.Instance.ResetBlacklist();
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On queue button
        /// </summary>
        [UIAction("click-btn-queue")]
        private void OnQueueButton()
        {
            ChatRequest.Instance.ToggleQueueStatus();
        }
    }
}
