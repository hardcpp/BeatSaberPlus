using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Chat filters settings
    /// </summary>
    internal class SettingsRight : SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIComponent("followenvironementrotations-toggle")]
        public ToggleSetting m_FollowEnvironementRotations;
        [UIComponent("chat-filterviewers")]
        private ToggleSetting m_ChatFitlerViewers;
        [UIComponent("chat-filterbroadcaster")]
        private ToggleSetting m_ChatFilterBroadcaster;

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
            SDK.UI.ToggleSetting.Setup(m_FollowEnvironementRotations,   l_Event, Config.Chat.FollowEnvironementRotation,    true);
            SDK.UI.ToggleSetting.Setup(m_ChatFitlerViewers,             l_Event, Config.Chat.FilterViewersCommands,         true);
            SDK.UI.ToggleSetting.Setup(m_ChatFilterBroadcaster,         l_Event, Config.Chat.FilterBroadcasterCommands,     true);

            /// Prepare
            SDK.UI.ToggleSetting.Setup(m_FollowEvents,                  l_Event, Config.Chat.ShowFollowEvents,              true);
            SDK.UI.ToggleSetting.Setup(m_SubscriptionEvents,            l_Event, Config.Chat.ShowSubscriptionEvents,        true);
            SDK.UI.ToggleSetting.Setup(m_BitsCheering,                  l_Event, Config.Chat.ShowBitsCheeringEvents,        true);
            SDK.UI.ToggleSetting.Setup(m_ChannelPoints,                 l_Event, Config.Chat.ShowChannelPointsEvent,        true);
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
            Config.Chat.FollowEnvironementRotation  = m_FollowEnvironementRotations.Value;
            Config.Chat.FilterViewersCommands       = m_ChatFitlerViewers.Value;
            Config.Chat.FilterBroadcasterCommands   = m_ChatFilterBroadcaster.Value;

            /// Set values
            Config.Chat.ShowFollowEvents            = m_FollowEvents.Value;
            Config.Chat.ShowSubscriptionEvents      = m_SubscriptionEvents.Value;
            Config.Chat.ShowBitsCheeringEvents      = m_BitsCheering.Value;
            Config.Chat.ShowChannelPointsEvent      = m_ChannelPoints.Value;

            /// Update floating view
            Chat.Instance.UpdateFloatingWindow(SDK.Game.Logic.ActiveScene, false);
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
            m_FollowEnvironementRotations.Value = Config.Chat.FollowEnvironementRotation;
            m_ChatFitlerViewers.Value           = Config.Chat.FilterViewersCommands;
            m_ChatFilterBroadcaster.Value       = Config.Chat.FilterBroadcasterCommands;

            /// Set values
            m_FollowEvents.Value                = Config.Chat.ShowFollowEvents;
            m_SubscriptionEvents.Value          = Config.Chat.ShowSubscriptionEvents;
            m_BitsCheering.Value                = Config.Chat.ShowBitsCheeringEvents;
            m_ChannelPoints.Value               = Config.Chat.ShowChannelPointsEvent;

            m_PreventChanges = false;
        }
    }
}
