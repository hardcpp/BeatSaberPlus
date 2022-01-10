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
            SDK.UI.IncrementSetting.Setup(m_UserRequest,            l_Event, null,                                  CRConfig.Instance.UserMaxRequest,          true);
            SDK.UI.IncrementSetting.Setup(m_VIPBonusRequest,        l_Event, null,                                  CRConfig.Instance.VIPBonusRequest,         true);
            SDK.UI.IncrementSetting.Setup(m_SubscriberBonusRequest, l_Event, null,                                  CRConfig.Instance.SubscriberBonusRequest,  true);
            SDK.UI.IncrementSetting.Setup(m_HistorySize,            l_Event, null,                                  CRConfig.Instance.HistorySize,             true);

            /// Right
            SDK.UI.ToggleSetting.Setup(m_PlayPreviewMusic,          l_Event,                                        CRConfig.Instance.PlayPreviewMusic,        true);
            SDK.UI.ToggleSetting.Setup(m_ModeratorPower,            l_Event,                                        CRConfig.Instance.ModeratorPower,          true);
            SDK.UI.IncrementSetting.Setup(m_QueueSize,              l_Event, null,                                  CRConfig.Instance.QueueCommandShowSize,    true);
            SDK.UI.IncrementSetting.Setup(m_QueueCooldown,          l_Event, SDK.UI.BSMLSettingFormartter.Seconds,  CRConfig.Instance.QueueCommandCooldown,    true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            CRConfig.Instance.Save();
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
            CRConfig.Instance.UserMaxRequest           = (int)m_UserRequest.Value;
            CRConfig.Instance.VIPBonusRequest          = (int)m_VIPBonusRequest.Value;
            CRConfig.Instance.SubscriberBonusRequest   = (int)m_SubscriberBonusRequest.Value;
            CRConfig.Instance.HistorySize              = (int)m_HistorySize.Value;

            /// Right
            CRConfig.Instance.PlayPreviewMusic         = m_PlayPreviewMusic.Value;
            CRConfig.Instance.ModeratorPower           = m_ModeratorPower.Value;
            CRConfig.Instance.QueueCommandShowSize     = (int)m_QueueSize.Value;
            CRConfig.Instance.QueueCommandCooldown     = (int)m_QueueCooldown.Value;
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
            m_UserRequest.Value             = CRConfig.Instance.UserMaxRequest;
            m_VIPBonusRequest.Value         = CRConfig.Instance.VIPBonusRequest;
            m_SubscriberBonusRequest.Value  = CRConfig.Instance.SubscriberBonusRequest;
            m_HistorySize.Value             = CRConfig.Instance.HistorySize;

            /// Right
            m_PlayPreviewMusic.Value        = CRConfig.Instance.PlayPreviewMusic;
            m_ModeratorPower.Value          = CRConfig.Instance.ModeratorPower;
            m_QueueSize.Value               = CRConfig.Instance.QueueCommandShowSize;
            m_QueueCooldown.Value           = CRConfig.Instance.QueueCommandCooldown;

            m_PreventChanges = false;
        }
    }
}
