using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Chat filters settings
    /// </summary>
    internal class SettingsRight : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIComponent("alignwithfloor-toggle")]
        public ToggleSetting m_AlignWithFloor;
        [UIComponent("showlockicon-toggle")]
        public ToggleSetting m_ShowLockIcon;
        [UIComponent("followenvironementrotations-toggle")]
        public ToggleSetting m_FollowEnvironementRotations;
        [UIComponent("chat-viewercount")]
        private ToggleSetting m_ChatViewerCount;
        [UIComponent("chat-filterviewers")]
        private ToggleSetting m_ChatFitlerViewers;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("followevent-toggle")]
        public ToggleSetting m_FollowEvents;
        [UIComponent("subscriptionevents-toggle")]
        public ToggleSetting m_SubscriptionEvents;
        [UIComponent("bitscheering-toggle")]
        public ToggleSetting m_BitsCheering;
        [UIComponent("channelpoints-toggle")]
        public ToggleSetting m_ChannelPoints;
        [UIComponent("chat-filterbroadcaster")]
        private ToggleSetting m_ChatFilterBroadcaster;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Prepare
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_AlignWithFloor,                l_Event, CConfig.Instance.AlignWithFloor,                true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowLockIcon,                  l_Event, CConfig.Instance.ShowLockIcon,                  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_FollowEnvironementRotations,   l_Event, CConfig.Instance.FollowEnvironementRotation,    true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ChatViewerCount,               l_Event, CConfig.Instance.ShowViewerCount,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ChatFitlerViewers,             l_Event, CConfig.Instance.FilterViewersCommands,         true);

            /// Prepare
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_FollowEvents,                  l_Event, CConfig.Instance.ShowFollowEvents,              true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_SubscriptionEvents,            l_Event, CConfig.Instance.ShowSubscriptionEvents,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_BitsCheering,                  l_Event, CConfig.Instance.ShowBitsCheeringEvents,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ChannelPoints,                 l_Event, CConfig.Instance.ShowChannelPointsEvent,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ChatFilterBroadcaster,         l_Event, CConfig.Instance.FilterBroadcasterCommands,     true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update config
            CConfig.Instance.AlignWithFloor              = m_AlignWithFloor.Value;
            CConfig.Instance.ShowLockIcon                = m_ShowLockIcon.Value;
            CConfig.Instance.FollowEnvironementRotation  = m_FollowEnvironementRotations.Value;
            CConfig.Instance.ShowViewerCount             = m_ChatViewerCount.Value;
            CConfig.Instance.FilterViewersCommands       = m_ChatFitlerViewers.Value;

            /// Set values
            CConfig.Instance.ShowFollowEvents            = m_FollowEvents.Value;
            CConfig.Instance.ShowSubscriptionEvents      = m_SubscriptionEvents.Value;
            CConfig.Instance.ShowBitsCheeringEvents      = m_BitsCheering.Value;
            CConfig.Instance.ShowChannelPointsEvent      = m_ChannelPoints.Value;
            CConfig.Instance.FilterBroadcasterCommands   = m_ChatFilterBroadcaster.Value;

            /// Update floating view
            Chat.Instance.UpdateFloatingWindow(CP_SDK.ChatPlexSDK.ActiveGenericScene, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Set values
            m_AlignWithFloor.Value              = CConfig.Instance.AlignWithFloor;
            m_ShowLockIcon.Value                = CConfig.Instance.ShowLockIcon;
            m_FollowEnvironementRotations.Value = CConfig.Instance.FollowEnvironementRotation;
            m_ChatViewerCount.Value             = CConfig.Instance.ShowViewerCount;
            m_ChatFitlerViewers.Value           = CConfig.Instance.FilterViewersCommands;

            /// Set values
            m_FollowEvents.Value                = CConfig.Instance.ShowFollowEvents;
            m_SubscriptionEvents.Value          = CConfig.Instance.ShowSubscriptionEvents;
            m_BitsCheering.Value                = CConfig.Instance.ShowBitsCheeringEvents;
            m_ChannelPoints.Value               = CConfig.Instance.ShowChannelPointsEvent;
            m_ChatFilterBroadcaster.Value       = CConfig.Instance.FilterBroadcasterCommands;

            m_PreventChanges = false;
        }
    }
}
