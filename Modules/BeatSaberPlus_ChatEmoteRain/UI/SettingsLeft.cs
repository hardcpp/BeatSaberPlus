using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    /// <summary>
    /// Emote rain settings credits view
    /// </summary>
    internal class SettingsLeft : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsLeft>
    {
        private static readonly string s_InformationsStr = "<line-height=125%>Original mod made by <b>Cr4</b> and <b>Uialeth</b>"
            + "\n"
            + "\n" + "<b><u>Commands</u></b>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!er toggle</b>\n<i><color=#CCCCCCFF>Disable any emote rain until a Menu/GamePlay scene change</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!er rain #EMOTE #COUNT</b>\n<i><color=#CCCCCCFF>Trigger a emote rain</color></i>"
            + "\n"
            + "\n"
            + "\n"
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
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
            m_Informations.SetText(s_InformationsStr);
            m_Informations.UpdateVerticalScrollIndicator(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset button
        /// </summary>
        [UIAction("click-reset-btn-pressed")]
        private void OnResetButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall chat emote rain settings?", () =>
            {
                /// Reset settings
                CERConfig.Instance.Reset();
                CERConfig.Instance.Enabled = true;
                CERConfig.Instance.Save();

                /// Refresh values
                Settings.Instance.RefreshSettings();
                SettingsRight.Instance.RefreshSettings();
            });
        }
        /// <summary>
        /// Open web configuration button
        /// </summary>
        [UIAction("click-open-web-configuration-btn-pressed")]
        private void OnWebConfigurationButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            CP_SDK.Chat.Service.OpenWebConfigurator();
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
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#chat-emote-rain");
        }
    }
}
