using HMUI;
using UnityEngine;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Moderation UI flow coordinator
    /// </summary>
    internal class ModerationViewFlowCoordinator : BeatSaberPlus.SDK.UI.ViewFlowCoordinator<ModerationViewFlowCoordinator>
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
        /// <summary>
        /// Moderation shortcut view
        /// </summary>
        private ModerationShortcut m_ShortcutMainView;

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
            m_MainView          = CreateViewController<ModerationMain>();
            m_LeftView          = CreateViewController<ModerationLeft>();
            m_RightView         = CreateViewController<ModerationRight>();
            m_ShortcutMainView  = CreateViewController<ModerationShortcut>();
        }
        /// <summary>
        /// On destroy
        /// </summary>
        internal void OnDestroy()
        {
            if (m_ShortcutMainView != null)
            {
                GameObject.Destroy(m_ShortcutMainView.gameObject);
                m_ShortcutMainView = null;
            }
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to shortcut view
        /// </summary>
        internal void SwitchToShortcut()
            => ChangeView(m_ShortcutMainView);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On back button pressed
        /// </summary>
        /// <param name="p_TopViewController">Current top view controller</param>
        /// <returns>True if the event is catched, false if we should dismiss the flow coordinator</returns>
        protected override sealed bool OnBackButtonPressed(HMUI.ViewController p_TopViewController)
        {
            if (p_TopViewController == m_ShortcutMainView)
            {
                ChangeView(m_MainView, m_LeftView, m_RightView);
                return true;
            }

            return false;
        }
    }
}
