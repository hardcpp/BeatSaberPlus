using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.ChatRequest.UI
{
    /// <summary>
    /// Chat request settings view
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0649

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
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            if (p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Left
                Utils.GameUI.PrepareIncrementSetting(m_UserRequest,             l_Event, null, Config.ChatRequest.UserMaxRequest,           true);
                Utils.GameUI.PrepareIncrementSetting(m_VIPBonusRequest,         l_Event, null, Config.ChatRequest.VIPBonusRequest,          true);
                Utils.GameUI.PrepareIncrementSetting(m_SubscriberBonusRequest,  l_Event, null, Config.ChatRequest.SubscriberBonusRequest,   true);
                Utils.GameUI.PrepareIncrementSetting(m_HistorySize,             l_Event, null, Config.ChatRequest.HistorySize,              true);

                /// Right
                Utils.GameUI.PrepareToggleSetting(m_PlayPreviewMusic,   l_Event,                                  Config.ChatRequest.PlayPreviewMusic,        true);
                Utils.GameUI.PrepareToggleSetting(m_ModeratorPower,     l_Event,                                  Config.ChatRequest.ModeratorPower,          true);
                Utils.GameUI.PrepareIncrementSetting(m_QueueSize,       l_Event, null,                            Config.ChatRequest.QueueCommandShowSize,    true);
                Utils.GameUI.PrepareIncrementSetting(m_QueueCooldown,   l_Event, Utils.GameUI.Formatter_Seconds,  Config.ChatRequest.QueueCommandCooldown,    true);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
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
        /// Open web configuration tool
        /// </summary>
        [UIAction("click-open-web-configuration-tool-btn-pressed")]
        private void OnOpenWebConfigurationToolButton()
        {
            ShowMessageModal("We did open the configuration tool in your desktop browser!");
            BeatSaberPlus.Utils.ChatService.OpenWebConfigurator();
        }
    }
}
