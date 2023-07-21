namespace CP_SDK.UI.FlowCoordinators
{
    /// <summary>
    /// UI flow coordinator
    /// </summary>
    public sealed class MainFlowCoordinator : FlowCoordinator<MainFlowCoordinator>
    {
        public override string Title => $"{ChatPlexSDK.ProductName} V{ChatPlexSDK.ProductVersion}";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Views.MainLeftView  m_MainLeftView  = null;
        private Views.MainMainView  m_MainMainView  = null;
        private Views.MainRightView m_MainRightView = null;

        private Views.SettingsLeftView  m_SettingsLeftView  = null;
        private Views.SettingsMainView  m_SettingsMainView  = null;
        private Views.SettingsRightView m_SettingsRightView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        public override void Init()
        {
            m_MainLeftView   = UISystem.CreateViewController<Views.MainLeftView>();
            m_MainMainView   = UISystem.CreateViewController<Views.MainMainView>();
            m_MainRightView  = UISystem.CreateViewController<Views.MainRightView>();

            m_SettingsLeftView  = UISystem.CreateViewController<Views.SettingsLeftView>();
            m_SettingsMainView  = UISystem.CreateViewController<Views.SettingsMainView>();
            m_SettingsRightView = UISystem.CreateViewController<Views.SettingsRightView>();
        }
        /// <summary>
        /// Stop
        /// </summary>
        private void OnDestroy()
        {
            UISystem.DestroyUI(ref m_SettingsRightView);
            UISystem.DestroyUI(ref m_SettingsMainView);
            UISystem.DestroyUI(ref m_SettingsLeftView);

            UISystem.DestroyUI(ref m_MainRightView);
            UISystem.DestroyUI(ref m_MainMainView);
            UISystem.DestroyUI(ref m_MainLeftView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected override sealed (IViewController, IViewController, IViewController) GetInitialViewsController()
            => (m_MainMainView, m_MainLeftView, m_MainRightView);
        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_MainViewController">Current main view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        public override sealed bool OnBackButtonPressed(IViewController p_MainViewController)
        {
            if (p_MainViewController != m_MainMainView)
            {
                SwitchToMainView();
                return true;
            }

            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to main view
        /// </summary>
        public void SwitchToMainView()
            => ChangeViewControllers(m_MainMainView, m_MainLeftView, m_MainRightView);
        /// <summary>
        /// Switch to settings view
        /// </summary>
        public void SwitchToSettingsView()
            => ChangeViewControllers(m_SettingsMainView, m_SettingsLeftView, m_SettingsRightView);
    }
}
