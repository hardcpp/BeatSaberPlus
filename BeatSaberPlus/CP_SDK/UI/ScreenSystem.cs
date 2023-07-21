using System;
using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Screen system
    /// </summary>
    public class ScreenSystem : MonoBehaviour
    {
        private static ScreenSystem m_Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Transform                   m_ScreenContainer               = null;
        private IFlowCoordinator            m_CurrentFlowCoordinator        = null;
        private Components.CFloatingPanel   m_LeftScreen                    = null;
        private Components.CFloatingPanel   m_TopScreen                     = null;
        private Components.CFloatingPanel   m_MainScreen                    = null;
        private Components.CFloatingPanel   m_RightScreen                   = null;
        private Views.TopNavigationView     m_TopNavigationViewController   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static ScreenSystem Instance => m_Instance;

        public static event Action OnCreated;
        public static event Action OnPresent;
        public static event Action OnDismiss;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public IFlowCoordinator          CurrentFlowCoordinator => m_CurrentFlowCoordinator;
        public Transform                 ScreenContainer        => m_ScreenContainer;
        public Components.CFloatingPanel LeftScreen             => m_LeftScreen;
        public Components.CFloatingPanel TopScreen              => m_TopScreen;
        public Components.CFloatingPanel MainScreen             => m_MainScreen;
        public Components.CFloatingPanel RightScreen            => m_RightScreen;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create the screen system
        /// </summary>
        internal static void Create()
        {
            if (m_Instance)
                return;

            m_Instance = new GameObject("[CP_SDK.UI.ScreenSystem]", typeof(ScreenSystem)).GetComponent<ScreenSystem>();
            GameObject.DontDestroyOnLoad(m_Instance.gameObject);
        }
        /// <summary>
        /// Destroy
        /// </summary>
        internal static void Destroy()
        {
            if (!m_Instance)
                return;

            UISystem.DestroyUI(ref m_Instance.m_RightScreen);
            UISystem.DestroyUI(ref m_Instance.m_MainScreen);
            UISystem.DestroyUI(ref m_Instance.m_TopScreen, ref m_Instance.m_TopNavigationViewController);
            UISystem.DestroyUI(ref m_Instance.m_LeftScreen);

            GameObject.Destroy(m_Instance.gameObject);
            m_Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Present the screen system
        /// </summary>
        public void Present()
        {
            if (!m_ScreenContainer)
                Init();

            if (gameObject.activeSelf)
                return;

            try { OnPresent?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ScreenSystem.Present] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            gameObject.SetActive(true);
        }
        /// <summary>
        /// Dismiss the screen system
        /// </summary>
        public void Dismiss()
        {
            if (!gameObject.activeSelf)
                return;

            SetFlowCoordinator(null, false);

            gameObject.SetActive(false);

            try { OnDismiss?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ScreenSystem.Dismiss] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set current flow coordinator
        /// </summary>
        /// <param name="p_FlowCoordinator">New flow coordinator</param>
        /// <param name="p_OnPresent">On present?</param>
        public void SetFlowCoordinator(IFlowCoordinator p_FlowCoordinator, bool p_OnPresent)
        {
            if (m_CurrentFlowCoordinator)
                m_CurrentFlowCoordinator.__Deactivate();

            m_LeftScreen.SetViewController(null);
            m_MainScreen.SetViewController(null);
            m_RightScreen.SetViewController(null);

            m_CurrentFlowCoordinator = p_FlowCoordinator;

            if (m_CurrentFlowCoordinator && !p_OnPresent)
                m_CurrentFlowCoordinator.__Activate();

            if (m_CurrentFlowCoordinator)
                m_TopNavigationViewController.SetTitle(m_CurrentFlowCoordinator.Title);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init the screen system
        /// </summary>
        private void Init()
        {
            transform.position   = Vector3.zero;
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            m_ScreenContainer = new GameObject("[CP_SDK.UI.ScreenSystem.ScreenContainer]").transform;
            m_ScreenContainer.SetParent(transform, false);
            m_ScreenContainer.localPosition = new Vector3(0.00f, 0.85f, 2.90f);

            m_LeftScreen = UISystem.FloatingPanelFactory.Create("[CP_SDK.UI.ScreenSystem.ScreenContainer.LeftScreen]", m_ScreenContainer);
            m_LeftScreen.SetTransformDirect(new Vector3(-2.60f, 0.00f, -0.815f), new Vector3(0.00f, -40.00f, 0.00f));
            m_LeftScreen.SetSize(new Vector2(120.0f, 80.0f));
            m_LeftScreen.SetRadius(0.0f);
            m_LeftScreen.SetBackground(false);

            m_TopScreen = UISystem.FloatingPanelFactory.Create("[CP_SDK.UI.ScreenSystem.ScreenContainer.TopScreen]", m_ScreenContainer);
            m_TopScreen.SetTransformDirect(new Vector3(0.00f, 0.9f, 0.00f), new Vector3(0.00f, 0.00f, 0.00f));
            m_TopScreen.SetSize(new Vector2(150.0f, 8.0f));
            m_TopScreen.SetRadius(0.0f);
            m_TopScreen.SetBackground(false);

            m_MainScreen = UISystem.FloatingPanelFactory.Create("[CP_SDK.UI.ScreenSystem.ScreenContainer.MainScreen]", m_ScreenContainer);
            m_MainScreen.SetTransformDirect(new Vector3(0.00f, 0.00f, 0.00f), new Vector3(0.00f, 0.00f, 0.00f));
            m_MainScreen.SetSize(new Vector2(150.0f, 80.0f));
            m_MainScreen.SetRadius(0.0f);
            m_MainScreen.SetBackground(false);

            m_RightScreen = UISystem.FloatingPanelFactory.Create("[CP_SDK.UI.ScreenSystem.ScreenContainer.RightScreen]", m_ScreenContainer);
            m_RightScreen.SetTransformDirect(new Vector3(2.60f, 0.00f, -0.815f), new Vector3(0.00f, 40.00f, 0.00f));
            m_RightScreen.SetSize(new Vector2(120.0f, 80.0f));
            m_RightScreen.SetRadius(0.0f);
            m_RightScreen.SetBackground(false);

            m_TopNavigationViewController = UISystem.CreateViewController<Views.TopNavigationView>();
            m_TopNavigationViewController.OnBackButton += TopNavigationViewController_OnBackButton;
            m_TopScreen.SetViewController(m_TopNavigationViewController);

            try { OnCreated?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI][ScreenSystem.Init] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            gameObject.SetActive(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On back button pressed
        /// </summary>
        private void TopNavigationViewController_OnBackButton()
        {
            if (!m_CurrentFlowCoordinator
                || !m_CurrentFlowCoordinator.OnBackButtonPressed(m_MainScreen.CurrentViewController))
            {
                Dismiss();
                return;
            }

            return;
        }
    }
}
