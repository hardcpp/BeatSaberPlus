using HMUI;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Moderation UI flow coordinator
    /// </summary>
    internal class ModerationViewFlowCoordinator : SDK.UI.ViewFlowCoordinator<ModerationViewFlowCoordinator>
    {
        /// <summary>
        /// Title
        /// </summary>
        public override string Title => "Chat Moderation";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main view
        /// </summary>
        private ModerationMain m_MainView;
        /// <summary>
        /// Left view
        /// </summary>
        private ModerationLeft m_LeftView;
        /// <summary>
        /// Details view
        /// </summary>
        private ModerationRight m_RightView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected override sealed (ViewController, ViewController, ViewController) GetInitialViewsController() => (m_MainView, m_LeftView, m_RightView);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        internal ModerationViewFlowCoordinator()
        {
            m_MainView  = CreateViewController<ModerationMain>();
            m_LeftView  = CreateViewController<ModerationLeft>();
            m_RightView = CreateViewController<ModerationRight>();
        }
        /// <summary>
        /// On destroy
        /// </summary>
        internal void OnDestroy()
        {
            if (m_MainView != null)
            {
                GameObject.Destroy(m_MainView.gameObject);
                m_MainView = null;
            }
            if (m_LeftView != null)
            {
                GameObject.Destroy(m_LeftView.gameObject);
                m_LeftView = null;
            }
            if (m_RightView != null)
            {
                GameObject.Destroy(m_RightView.gameObject);
                m_RightView = null;
            }
        }
    }
}
