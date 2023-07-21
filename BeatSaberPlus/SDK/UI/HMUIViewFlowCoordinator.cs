using System.Collections;
using System.Linq;
using UnityEngine;

using SwitchQueue = System.Collections.Generic.Queue<(HMUI.ViewController, HMUI.ViewController, HMUI.ViewController)>;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// View flow coordinator base class
    /// </summary>
    /// <typeparam name="t_Base"></typeparam>
    public abstract class HMUIViewFlowCoordinator<t_Base> : HMUI.FlowCoordinator
        where t_Base : HMUIViewFlowCoordinator<t_Base>
    {
        private static t_Base m_Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private SwitchQueue             m_SwitchQueue           = new SwitchQueue();
        private bool                    m_IsDequeueEngaged      = false;
        private HMUI.FlowCoordinator    m_BackupFlowCoordinator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public abstract string Title { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create flow coordinator
        /// </summary>
        /// <returns></returns>
        public static t_Base Instance()
        {
            if (!m_Instance)
                m_Instance = HMUIUIUtils.CreateFlowCoordinator<t_Base>();

            return m_Instance;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public HMUIViewFlowCoordinator()
        {
            /// Bind singleton
            m_Instance = this as t_Base;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is the screen system enabling?</param>
        public override sealed void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            if (p_FirstActivation)
            {
                SetTitle(Title);
                showBackButton = true;
            }

            if (p_AddedToHierarchy)
            {
                if (!m_IsDequeueEngaged && m_SwitchQueue.Count != 0)
                {
                    var l_Current = m_SwitchQueue.Dequeue();
                    ProvideInitialViewControllers(l_Current.Item1, l_Current.Item2, l_Current.Item3);
                }
                else
                {
                    var l_Initial = GetInitialViewsController();
                    ProvideInitialViewControllers(l_Initial.Item1, l_Initial.Item2, l_Initial.Item3);
                }
            }
        }
        /// <summary>
        /// When the back button is pressed
        /// </summary>
        /// <param name="p_TopViewController">Controller instance</param>
        public override sealed void BackButtonWasPressed(HMUI.ViewController p_TopViewController)
        {
            if (OnBackButtonPressed(p_TopViewController))
                return;

            Dismiss();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get initial views controller
        /// </summary>
        /// <returns>(Middle, Left, Right)</returns>
        protected abstract (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetInitialViewsController();
        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_TopViewController">Current top view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        protected virtual bool OnBackButtonPressed(HMUI.ViewController p_TopViewController) => false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Present this ViewFlowCoordinator
        /// </summary>
        /// <param name="p_IgnoreBackuping">Ignore existing flow coordinator</param>
        public virtual void Present(bool p_IgnoreBackuping = false)
        {
            if (IsFlowCoordinatorInHierarchy(this))
                return;

            m_BackupFlowCoordinator = null;

            HMUI.FlowCoordinator l_BackupFlowCoordinator = p_IgnoreBackuping ? null : Resources.FindObjectsOfTypeAll<HMUI.FlowCoordinator>()
                .Where(x => x.isActivated && x != this && IsFlowCoordinatorInHierarchy(x)).LastOrDefault();

            try
            {
                /// Look for existing flow coordinator
                if (l_BackupFlowCoordinator != null && l_BackupFlowCoordinator)
                {
                    l_BackupFlowCoordinator.PresentFlowCoordinator(this);
                    m_BackupFlowCoordinator = l_BackupFlowCoordinator;
                }
                /// Present main view controller
                else
                    HMUIUIUtils.MainFlowCoordinator.PresentFlowCoordinator(this as HMUI.FlowCoordinator);
            }
            catch
            {

            }
        }
        /// <summary>
        /// Dismiss this ViewFlowCoordinator
        /// </summary>
        public virtual void Dismiss()
        {
            m_SwitchQueue.Clear();
            m_IsDequeueEngaged = false;

            SetLeftScreenViewController(null, HMUI.ViewController.AnimationType.None);
            SetRightScreenViewController(null, HMUI.ViewController.AnimationType.None);

            /// Restore original flow coordinator
            if (m_BackupFlowCoordinator != null)
            {
                m_SwitchQueue.Clear();
                m_IsDequeueEngaged = false;

                m_BackupFlowCoordinator.DismissFlowCoordinator(this);
                m_BackupFlowCoordinator = null;

                return;
            }

            /// Back to game main menu
            HMUIUIUtils.MainFlowCoordinator.DismissFlowCoordinator(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Hide the score board
        /// </summary>
        public void HideLeftScreen()
        {
            SetLeftScreenViewController(null, HMUI.ViewController.AnimationType.None);
        }
        /// <summary>
        /// Hide the score board
        /// </summary>
        public void HideRightScreen()
        {
            SetRightScreenViewController(null, HMUI.ViewController.AnimationType.None);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a main view controller change
        /// </summary>
        /// <param name="p_NewView">New main view controller</param>
        public void ChangeView(HMUI.ViewController p_NewView, HMUI.ViewController p_Left = null, HMUI.ViewController p_Right = null)
        {
            if (IsFlowCoordinatorInHierarchy(this) && topViewController == p_NewView)
                return;

            m_SwitchQueue.Enqueue((p_NewView, p_Left, p_Right));

            if (!IsFlowCoordinatorInHierarchy(this))
            {
                Present();
                return;
            }

            if (!m_IsDequeueEngaged && (topViewController == null || (topViewController.isActiveAndEnabled && !topViewController.isInTransition)))
                DequeueViewController();
            if (!m_IsDequeueEngaged && (!topViewController.isActiveAndEnabled || topViewController.isInTransition))
            {
                m_IsDequeueEngaged = true;
                CP_SDK.Unity.MTCoroutineStarter.Start(DequeueViewControllerWhileOldInTransition());
            }
        }
        /// <summary>
        /// Dequeue a controller change
        /// </summary>
        private void DequeueViewController()
        {
            if (m_SwitchQueue.Count == 0)
            {
                m_IsDequeueEngaged = false;
                return;
            }

            var l_Current = m_SwitchQueue.Dequeue();

            m_IsDequeueEngaged = true;

            SetLeftScreenViewController(l_Current.Item2,    HMUI.ViewController.AnimationType.None);
            SetRightScreenViewController(l_Current.Item3,   HMUI.ViewController.AnimationType.None);

            ReplaceTopViewController(l_Current.Item1, () =>
            {
                if (IsFlowCoordinatorInHierarchy(this) && isActivated)
                    DequeueViewController();
            });
        }
        /// <summary>
        /// Dequeue a controller change but wait for old one to finish transition
        /// </summary>
        private IEnumerator DequeueViewControllerWhileOldInTransition()
        {
            yield return new WaitForEndOfFrame();

            if (!IsFlowCoordinatorInHierarchy(this))
                yield break;

            yield return new WaitUntil(() => isActiveAndEnabled);

            if (!IsFlowCoordinatorInHierarchy(this))
                yield break;

            yield return new WaitUntil(() => topViewController.isActiveAndEnabled);

            if (!IsFlowCoordinatorInHierarchy(this))
                yield break;

            yield return new WaitUntil(() => !topViewController.isInTransition);

            if (!IsFlowCoordinatorInHierarchy(this))
                yield break;

            yield return new WaitForEndOfFrame();

            if (!IsFlowCoordinatorInHierarchy(this))
                yield break;

            DequeueViewController();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create view controller
        /// </summary>
        /// <typeparam name="t_ViewType"></typeparam>
        /// <returns></returns>
        public static t_ViewType CreateViewController<t_ViewType>()
            where t_ViewType : HMUI.ViewController
            => HMUIUIUtils.CreateViewController<t_ViewType>();
    }
}
