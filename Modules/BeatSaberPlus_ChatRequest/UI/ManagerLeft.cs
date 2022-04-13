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
            /// Update queue status
            UpdateQueueStatus();
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            UpdateQueueStatus();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        /// Cleat queue button
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
