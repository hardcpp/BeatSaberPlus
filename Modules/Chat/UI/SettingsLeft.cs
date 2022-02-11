using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus_Chat.UI
{
    /// <summary>
    /// Stream chat credits
    /// </summary>
    internal class SettingsLeft : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsLeft>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        internal GameObject m_Background = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Thanks to <b>brian</b> for original ChatCore lib</b></u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = " - https://github.com/brian91292/ChatCore";
        [UIValue("Line3")]
        private readonly string m_Line3 = " ";
        [UIValue("Line4")]
        private readonly string m_Line4 = " ";
        [UIValue("Line5")]
        private readonly string m_Line5 = " ";
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset button
        /// </summary>
        [UIAction("click-reset-btn-pressed")]
        private void OnResetButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset all chat settings?", () =>
            {
                /// Reset settings
                CConfig.Instance.Reset();
                CConfig.Instance.Enabled = true;
                CConfig.Instance.Save();

                /// Refresh values
                Settings.Instance.RefreshSettings();
                SettingsRight.Instance.RefreshSettings();

                /// Update floating view
                Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.SDK.Game.Logic.ActiveScene, true);
            });
        }
        /// <summary>
        /// Reset position button
        /// </summary>
        [UIAction("click-reset-position-btn-pressed")]
        private void OnResetPositionButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset chat position?", () =>
            {
                /// Reset position settings
                CConfig.Instance.ResetPosition();
                CConfig.Instance.Save();

                /// Refresh values
                Settings.Instance.RefreshSettings();
                SettingsRight.Instance.RefreshSettings();

                /// Update floating view
                Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.SDK.Game.Logic.ActiveScene, true);
            });
        }
        /// <summary>
        /// Open web configuration button
        /// </summary>
        [UIAction("click-open-web-configuration-btn-pressed")]
        private void OnWebConfigurationButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            BeatSaberPlus.SDK.Chat.Service.OpenWebConfigurator();
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
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#chat");
        }
    }
}
