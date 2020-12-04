using BeatSaberMarkupLanguage;
using HMUI;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    /// <summary>
    /// Manager UI flow coordinator
    /// </summary>
    class ManagerViewFlowCoordinator : FlowCoordinator
    {
        /// <summary>
        /// Main view accessor
        /// </summary>
        internal ManagerMain MainView => m_MainView;
        /// <summary>
        /// Left view accessor
        /// </summary>
        internal ManagerLeft LeftView => m_LeftView;
        /// <summary>
        /// Detail view accessor
        /// </summary>
        internal ManagerDetail DetailView => m_DetailView;

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
        private ManagerDetail m_DetailView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Backup flow coordinator
        /// </summary>
        private FlowCoordinator m_BackupFlowCoordinator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the Unity GameObject and all children are ready
        /// </summary>
        internal void Awake()
        {
            if (m_MainView == null)
                m_MainView = BeatSaberUI.CreateViewController<ManagerMain>();
            if (m_LeftView == null)
                m_LeftView = BeatSaberUI.CreateViewController<ManagerLeft>();
            if (m_DetailView == null)
                m_DetailView = BeatSaberUI.CreateViewController<ManagerDetail>();

            m_MainView.FlowCoordinator = this;
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
            if (m_DetailView != null)
            {
                GameObject.Destroy(m_DetailView.gameObject);
                m_DetailView = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to main view
        /// </summary>
        internal void Show()
        {
            if (IsFlowCoordinatorInHierarchy(this))
                return;

            m_BackupFlowCoordinator = null;

            if (!this.IsFlowCoordinatorInHierarchy(this))
            {
                FlowCoordinator l_GameCoordinator = Resources.FindObjectsOfTypeAll<FlowCoordinator>().Where(x => x.isActivated).LastOrDefault();
                if (l_GameCoordinator != null && this.IsFlowCoordinatorInHierarchy(l_GameCoordinator))
                {
                    l_GameCoordinator.PresentFlowCoordinator(this);
                    m_BackupFlowCoordinator = l_GameCoordinator;
                }

                return;
            }
        }
        /// <summary>
        /// Hide flow coordinator
        /// </summary>
        internal void Hide()
        {
            BackButtonWasPressed(null);
        }

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
            if (p_FirstActivation)
            {
                SetTitle("Chat Request");
                showBackButton = true;
            }

            if (p_AddedToHierarchy)
                ProvideInitialViewControllers(m_MainView, m_LeftView, m_DetailView);
        }
        /// <summary>
        /// When the back button is pressed
        /// </summary>
        /// <param name="p_TopViewController">Controller instance</param>
        protected override void BackButtonWasPressed(ViewController p_TopViewController)
        {
            /// Restore original flow coordinator
            if (m_BackupFlowCoordinator != null)
                m_BackupFlowCoordinator.DismissFlowCoordinator(this);
            else
                BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null);

            m_BackupFlowCoordinator = null;
        }
    }
}
