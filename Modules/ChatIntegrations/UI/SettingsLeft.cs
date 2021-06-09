using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.UI
{
    internal class SettingsLeft : SDK.UI.ResourceViewController<SettingsLeft>
    {
        private static readonly string s_InformationsStr = "<line-height=125%>"
            + "\n<b>Special thanks to HypersonicSharkz#3301 for help on TwitchAPI and some Actions code!"
            + "\nThis module allow you execute actions on your game when triggered by events."
            + "\n"
            + "\n<b><u>Events</u></b>"
            + "\n- <color=yellow><b>ChatBits</b></color>\n<i><color=#CCCCCCFF>When someone spends bits your channel!</color></i>"
            + "\n- <color=yellow><b>ChatCommand</b></color>\n<i><color=#CCCCCCFF>Allow you to create chat commands and execute actions with them</color></i>"
            + "\n- <color=yellow><b>ChatFollow</b></color>\n<i><color=#CCCCCCFF>When someone follows your channel</color></i>"
            + "\n- <color=yellow><b>ChatPointsReward</b></color>\n<i><color=#CCCCCCFF>Allow you to create channel points rewards and fully configure them and bind some actions to them</color></i>"
            + "\n- <color=yellow><b>ChatSubscription</b></color>\n<i><color=#CCCCCCFF>When someone subscribes or subgifts</color></i>"
            + "\n- <color=yellow><b>Dummy</b></color>\n<i><color=#CCCCCCFF>Dummy event that can get triggered by other events</color></i>"
            + "\n- <color=yellow><b>LevelEnded</b></color>\n<i><color=#CCCCCCFF>When you exit a map</color></i>"
            + "\n- <color=yellow><b>LevelStarted</b></color>\n<i><color=#CCCCCCFF>When you enter a map</color></i>"
            + "\n- <color=yellow><b>VoiceAttackCommand</b></color>\n<i><color=#CCCCCCFF>Bind VoiceAttack commands to BS+</color></i>"
            + "\n"
            + "\n";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("Informations")]
        private HMUI.TextPageScrollView m_Informations = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
            m_Informations.SetText(s_InformationsStr);
            m_Informations.UpdateVerticalScrollIndicator(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Open web configuration button
        /// </summary>
        [UIAction("click-open-web-configuration-btn-pressed")]
        private void OnWebConfigurationButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            SDK.Chat.Service.OpenWebConfigurator();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        [UIAction("click-documentation-btn-pressed")]
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations");
        }
    }
}
