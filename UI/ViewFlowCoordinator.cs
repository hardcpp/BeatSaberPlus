using BeatSaberMarkupLanguage;
using HMUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// UI flow coordinator
    /// </summary>
    public class ViewFlowCoordinator : FlowCoordinator
    {
        public static ViewFlowCoordinator Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Song detail view template
        /// </summary>
        public static GameObject SongDetailViewTemplate = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Info view
        /// </summary>
        private InfoView m_InfoView;
        /// <summary>
        /// Main view
        /// </summary>
        private MainView m_MainView;
        /// <summary>
        /// Settings view
        /// </summary>
        private SettingsView m_SettingsView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// View change queue
        /// </summary>
        private Queue<(ViewController, ViewController, ViewController)> m_SwitchQueue = new Queue<(ViewController, ViewController, ViewController)>();
        /// <summary>
        /// Is dequeue engaged ?
        /// </summary>
        private bool m_IsDequeueEngaged = false;
        /// <summary>
        /// Backup flow coordinator
        /// </summary>
        private FlowCoordinator m_BackupFlowCoordinator = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the Unity GameObject and all children are ready
        /// </summary>
        public void Awake()
        {
            /// Bind instance
            Instance = this;

            /// Backup song detail view before any mod editing
            SongDetailViewTemplate = GameObject.Instantiate(Resources.FindObjectsOfTypeAll<StandardLevelDetailView>().First(x => x.gameObject.name == "LevelDetail").gameObject);

            if (m_InfoView == null)
                m_InfoView = BeatSaberUI.CreateViewController<InfoView>();
            if (m_MainView == null)
                m_MainView = BeatSaberUI.CreateViewController<MainView>();
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<SettingsView>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to main view
        /// </summary>
        public void SwitchToMainView()
        {
            ChangeMainViewController(m_MainView, m_InfoView);
        }
        /// <summary>
        /// Switch to settings view
        /// </summary>
        public void SwitchToSettingsView()
        {
            ChangeMainViewController(m_SettingsView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a main view controller change
        /// </summary>
        /// <param name="p_NewView">New main view controller</param>
        public void ChangeMainViewController(ViewController p_NewView, ViewController p_Left = null, ViewController p_Right = null)
        {
            if (IsFlowCoordinatorInHierarchy(this) && topViewController == p_NewView)
                return;

            m_SwitchQueue.Enqueue((p_NewView, p_Left, p_Right));

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

            if (!m_IsDequeueEngaged)
                DequeueViewController();
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
            ReplaceTopViewController(l_Current.Item1, () => DequeueViewController());
            SetLeftScreenViewController(l_Current.Item2, l_Current.Item2 != null ? ViewController.AnimationType.In : ViewController.AnimationType.Out);
            SetRightScreenViewController(l_Current.Item3, l_Current.Item3 != null ? ViewController.AnimationType.In : ViewController.AnimationType.Out);
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
                SetTitle("Beat Saber Plus");
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
                    ProvideInitialViewControllers(m_MainView, m_InfoView, null);
            }
        }
        /// <summary>
        /// When the back button is pressed
        /// </summary>
        /// <param name="p_TopViewController">Controller instance</param>
        protected override void BackButtonWasPressed(ViewController p_TopViewController)
        {
            /// Restore original flow coordinator
            if (m_BackupFlowCoordinator != null)
            {
                m_BackupFlowCoordinator.DismissFlowCoordinator(this);
                m_BackupFlowCoordinator = null;

                m_SwitchQueue.Clear();

                return;
            }

            if (topViewController == m_MainView)
            {
                BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this, null);
                m_SwitchQueue.Clear();
            }
            else
                SwitchToMainView();
        }
    }
}
