using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Flow coordinator interface
    /// </summary>
    public abstract class IFlowCoordinator : MonoBehaviour
    {
        private bool                m_FirstActivation       = true;
        private IFlowCoordinator    m_BackupFlowCoordinator = null;
        private IViewController     m_LeftViewController    = null;
        private IViewController     m_MainViewController    = null;
        private IViewController     m_RightViewController   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public abstract string Title                { get; }
        public IViewController LeftViewController   => m_LeftViewController;
        public IViewController MainViewController   => m_MainViewController;
        public IViewController RightViewController  => m_RightViewController;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Activate
        /// </summary>
        public void __Activate()
        {
            ChangeViewControllers(m_MainViewController, m_LeftViewController, m_RightViewController);
        }
        /// <summary>
        /// Deactivate
        /// </summary>
        public void __Deactivate()
        {
            if (m_LeftViewController && m_LeftViewController.CurrentScreen)
                m_LeftViewController.CurrentScreen.SetViewController(null);

            if (m_MainViewController && m_MainViewController.CurrentScreen)
                m_MainViewController.CurrentScreen.SetViewController(null);

            if (m_RightViewController && m_RightViewController.CurrentScreen)
                m_RightViewController.CurrentScreen.SetViewController(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        public virtual void Init() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Present this FlowCoordinator
        /// </summary>
        /// <param name="p_IgnoreBackuping">Ignore existing flow coordinator</param>
        public void Present(bool p_IgnoreBackuping = false)
        {
            if (ScreenSystem.Instance.CurrentFlowCoordinator == this)
                return;

            m_BackupFlowCoordinator = p_IgnoreBackuping ? null : ScreenSystem.Instance.CurrentFlowCoordinator;

            ScreenSystem.Instance.Present();
            ScreenSystem.Instance.SetFlowCoordinator(this, true);

            if (m_FirstActivation)
            {
                try { Init(); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI][IFlowCoordinator.Present] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                var l_Initial = GetInitialViewsController();
                ChangeViewControllers(l_Initial.Item1, l_Initial.Item2, l_Initial.Item3);
                m_FirstActivation = false;
            }
            else
                ChangeViewControllers(m_MainViewController, m_LeftViewController, m_RightViewController);
        }
        /// <summary>
        /// Dismiss this FlowCoordinator
        /// </summary>
        public void Dismiss()
        {
            if (m_BackupFlowCoordinator)
            {
                ScreenSystem.Instance.SetFlowCoordinator(m_BackupFlowCoordinator, false);
                m_BackupFlowCoordinator = null;
                return;
            }

            ScreenSystem.Instance.Dismiss();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Main, Left, Right)</returns>
        protected abstract (IViewController, IViewController, IViewController) GetInitialViewsController();
        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_MainViewController">Current main view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        public virtual bool OnBackButtonPressed(IViewController p_MainViewController) => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change view controllers
        /// </summary>
        /// <param name="p_MainViewController">New main view controller</param>
        /// <param name="p_LeftViewController">New left view controller</param>
        /// <param name="p_RightViewController">New right view controller</param>
        public void ChangeViewControllers(IViewController p_MainViewController, IViewController p_LeftViewController = null, IViewController p_RightViewController = null)
        {
            if (ScreenSystem.Instance.CurrentFlowCoordinator != this)
                return;

            SetLeftViewController(p_LeftViewController);
            SetMainViewController(p_MainViewController);
            SetRightViewController(p_RightViewController);
        }
        /// <summary>
        /// Set left view controller
        /// </summary>
        /// <param name="p_ViewController">New view controller</param>
        protected void SetLeftViewController(IViewController p_ViewController)
        {
            if (ScreenSystem.Instance.CurrentFlowCoordinator != this)
                return;

            m_LeftViewController = p_ViewController;
            try { ScreenSystem.Instance.LeftScreen.SetViewController(p_ViewController); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][IFlowCoordinator.SetLeftViewController] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// Set main view controller
        /// </summary>
        /// <param name="p_ViewController">New view controller</param>
        protected void SetMainViewController(IViewController p_ViewController)
        {
            if (ScreenSystem.Instance.CurrentFlowCoordinator != this)
                return;

            m_MainViewController = p_ViewController;
            try { ScreenSystem.Instance.MainScreen.SetViewController(p_ViewController); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][IFlowCoordinator.SetMainViewController] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// Set left view controller
        /// </summary>
        /// <param name="p_ViewController">New view controller</param>
        protected void SetRightViewController(IViewController p_ViewController)
        {
            if (ScreenSystem.Instance.CurrentFlowCoordinator != this)
                return;

            m_RightViewController = p_ViewController;
            try { ScreenSystem.Instance.RightScreen.SetViewController(p_ViewController); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][IFlowCoordinator.SetRightViewController] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
