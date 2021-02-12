using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus.Modules.ChatRequest.UI
{
    /// <summary>
    /// Chat request settings view
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("use-request")]
        private IncrementSetting m_UserRequest;
        [UIComponent("vip-request")]
        private IncrementSetting m_VIPBonusRequest;
        [UIComponent("sub-request")]
        private IncrementSetting m_SubscriberBonusRequest;
        [UIComponent("his-size")]
        private IncrementSetting m_HistorySize;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("pre-toggle")]
        private ToggleSetting m_PlayPreviewMusic;
        [UIComponent("mod-toggle")]
        private ToggleSetting m_ModeratorPower;
        [UIComponent("que-size")]
        private IncrementSetting m_QueueSize;
        [UIComponent("que-cool")]
        private IncrementSetting m_QueueCooldown;
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

            /// Left
            SDK.UI.IncrementSetting.Setup(m_UserRequest,            l_Event, null,                                  Config.ChatRequest.UserMaxRequest,          true);
            SDK.UI.IncrementSetting.Setup(m_VIPBonusRequest,        l_Event, null,                                  Config.ChatRequest.VIPBonusRequest,         true);
            SDK.UI.IncrementSetting.Setup(m_SubscriberBonusRequest, l_Event, null,                                  Config.ChatRequest.SubscriberBonusRequest,  true);
            SDK.UI.IncrementSetting.Setup(m_HistorySize,            l_Event, null,                                  Config.ChatRequest.HistorySize,             true);

            /// Right
            SDK.UI.ToggleSetting.Setup(m_PlayPreviewMusic,          l_Event,                                        Config.ChatRequest.PlayPreviewMusic,        true);
            SDK.UI.ToggleSetting.Setup(m_ModeratorPower,            l_Event,                                        Config.ChatRequest.ModeratorPower,          true);
            SDK.UI.IncrementSetting.Setup(m_QueueSize,              l_Event, null,                                  Config.ChatRequest.QueueCommandShowSize,    true);
            SDK.UI.IncrementSetting.Setup(m_QueueCooldown,          l_Event, SDK.UI.BSMLSettingFormartter.Seconds,   Config.ChatRequest.QueueCommandCooldown,    true);
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

            /// Left
            Config.ChatRequest.UserMaxRequest           = (int)m_UserRequest.Value;
            Config.ChatRequest.VIPBonusRequest          = (int)m_VIPBonusRequest.Value;
            Config.ChatRequest.SubscriberBonusRequest   = (int)m_SubscriberBonusRequest.Value;
            Config.ChatRequest.HistorySize              = (int)m_HistorySize.Value;

            /// Right
            Config.ChatRequest.PlayPreviewMusic         = m_PlayPreviewMusic.Value;
            Config.ChatRequest.ModeratorPower           = m_ModeratorPower.Value;
            Config.ChatRequest.QueueCommandShowSize     = (int)m_QueueSize.Value;
            Config.ChatRequest.QueueCommandCooldown     = (int)m_QueueCooldown.Value;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Left
            m_UserRequest.Value             = Config.ChatRequest.UserMaxRequest;
            m_VIPBonusRequest.Value         = Config.ChatRequest.VIPBonusRequest;
            m_SubscriberBonusRequest.Value  = Config.ChatRequest.SubscriberBonusRequest;
            m_HistorySize.Value             = Config.ChatRequest.HistorySize;

            /// Right
            m_PlayPreviewMusic.Value        = Config.ChatRequest.PlayPreviewMusic;
            m_ModeratorPower.Value          = Config.ChatRequest.ModeratorPower;
            m_QueueSize.Value               = Config.ChatRequest.QueueCommandShowSize;
            m_QueueCooldown.Value           = Config.ChatRequest.QueueCommandCooldown;

            m_PreventChanges = false;
        }
    }
}
