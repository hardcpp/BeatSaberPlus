using BeatSaberMarkupLanguage;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// View controller base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ViewFlowCoordinator<T> : HMUI.FlowCoordinator
        where T : ViewFlowCoordinator<T>
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static T m_Instance = null;
        /// <summary>
        /// View change queue
        /// </summary>
        private Queue<(HMUI.ViewController, HMUI.ViewController, HMUI.ViewController)> m_SwitchQueue = new Queue<(HMUI.ViewController, HMUI.ViewController, HMUI.ViewController)>();
        /// <summary>
        /// Is dequeue engaged ?
        /// </summary>
        private bool m_IsDequeueEngaged = false;
        /// <summary>
        /// Backup flow coordinator
        /// </summary>
        private HMUI.FlowCoordinator m_BackupFlowCoordinator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Title
        /// </summary>
        public abstract string Title { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create flow coordinator
        /// </summary>
        /// <returns></returns>
        public static T Instance()
        {
            if (m_Instance == null)
                m_Instance = BeatSaberUI.CreateFlowCoordinator<T>();

            return m_Instance;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewFlowCoordinator()
        {
            /// Bind singleton
            m_Instance = this as T;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_ActivationType">Activation type</param>
        protected override sealed void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool screenSystemEnabling)
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
        protected override sealed void BackButtonWasPressed(HMUI.ViewController p_TopViewController)
        {
            /// Look for existing flow coordinator
            if (m_BackupFlowCoordinator != null)
            {
                Dismiss();
                return;
            }

            if (!OnBackButtonPressed(p_TopViewController))
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
        public void Present(bool p_IgnoreBackuping = false)
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
                    BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(this as HMUI.FlowCoordinator);
            }
            catch
            {

            }
        }
        /// <summary>
        /// Dismiss this ViewFlowCoordinator
        /// </summary>
        public void Dismiss()
        {
            m_SwitchQueue.Clear();
            m_IsDequeueEngaged = false;

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
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null);
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

            if (!m_IsDequeueEngaged && topViewController.isActiveAndEnabled && !topViewController.isInTransition)
                DequeueViewController();
            if (!m_IsDequeueEngaged && (!topViewController.isActiveAndEnabled || topViewController.isInTransition))
            {
                if (topViewController is SDK.UI.IViewController)
                    (topViewController as SDK.UI.IViewController).ShowViewTransitionLoading();

                m_IsDequeueEngaged = true;
                SharedCoroutineStarter.instance.StartCoroutine(DequeueViewControllerWhileOldInTransition());
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
            ReplaceTopViewController(l_Current.Item1, () =>
            {
                if (IsFlowCoordinatorInHierarchy(this) && isActivated)
                    DequeueViewController();
            });
            SetLeftScreenViewController(l_Current.Item2,    HMUI.ViewController.AnimationType.None);
            SetRightScreenViewController(l_Current.Item3,   HMUI.ViewController.AnimationType.None);
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
        /// <typeparam name="V"></typeparam>
        /// <returns></returns>
        public static V CreateViewController<V>() where V : HMUI.ViewController
        {
            return BeatSaberUI.CreateViewController<V>();
        }
    }
}
