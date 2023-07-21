using CP_SDK.UI.Views;
using UnityEngine;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Manager UI flow coordinator
    /// </summary>
    internal sealed class ManagerViewFlowCoordinator : CP_SDK.UI.FlowCoordinator<ManagerViewFlowCoordinator>
    {
        public override string Title => "Chat Request";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private ManagerLeftView     m_ManagerLeftView;
        private ManagerMainView     m_ManagerMainView;
        private ManagerRightView    m_ManagerRightView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public override void Init()
        {
            m_ManagerLeftView   = CP_SDK.UI.UISystem.CreateViewController<ManagerLeftView>();
            m_ManagerMainView   = CP_SDK.UI.UISystem.CreateViewController<ManagerMainView>();
            m_ManagerRightView  = CP_SDK.UI.UISystem.CreateViewController<ManagerRightView>();
        }
        /// <summary>
        /// On destroy
        /// </summary>
        internal void OnDestroy()
        {
            CP_SDK.UI.UISystem.DestroyUI(ref m_ManagerLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_ManagerMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_ManagerRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected override sealed (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetInitialViewsController()
            => (m_ManagerMainView, m_ManagerLeftView, m_ManagerRightView);
    }
}
