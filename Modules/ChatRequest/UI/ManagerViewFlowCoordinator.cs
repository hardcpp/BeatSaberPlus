using HMUI;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatRequest.UI
{
    /// <summary>
    /// Manager UI flow coordinator
    /// </summary>
    class ManagerViewFlowCoordinator : SDK.UI.ViewFlowCoordinator<ManagerViewFlowCoordinator>
    {
        /// <summary>
        /// Title
        /// </summary>
        public override string Title => "Chat Request";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main view
        /// </summary>
        private ManagerMain m_MainView;
        /// <summary>
        /// Left view
        /// </summary>
        private ManagerLeft m_LeftView;
        /// <summary>
        /// Details view
        /// </summary>
        private ManagerRight m_RightView;

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
        internal ManagerViewFlowCoordinator()
        {
            m_MainView  = CreateViewController<ManagerMain>();
            m_LeftView  = CreateViewController<ManagerLeft>();
            m_RightView = CreateViewController<ManagerRight>();
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
