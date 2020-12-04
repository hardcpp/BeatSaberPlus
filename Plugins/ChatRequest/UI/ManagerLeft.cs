using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    /// <summary>
    /// Manager left window
    /// </summary>
    internal class ManagerLeft : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("QueueButton")]
        private Button m_QueueButton = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                /// Update queue status
                UpdateQueueStatus();
            }
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

            if (Plugins.ChatRequest.ChatRequest.Instance.QueueOpen)
                m_QueueButton.GetComponentInChildren<CurvedTextMeshPro>().text = "Close queue";
            else
                m_QueueButton.GetComponentInChildren<CurvedTextMeshPro>().text = "Open queue";
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
        /// <summary>
        /// Cleat queue button
        /// </summary>
        [UIAction("click-btn-clear-queue")]
        private void OnClearQueueButton()
        {
            /// Clear queue
            ChatRequest.Instance.ClearQueue();

            /// Show modal
            m_ParserParams.EmitEvent("CloseClearQueueMessageModal");
        }
    }
}
