using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus_MenuMusic.UI
{
    /// <summary>
    /// Menu music settings credit view
    /// </summary>
    internal class SettingsLeft : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsLeft>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Thanks to <b>Lunikc</b> for original idea</b></u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = " ";
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
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall Menu Music settings?", () =>
            {
                /// Reset settings
                MMConfig.Instance.Reset();
                MMConfig.Instance.Enabled = true;
                MMConfig.Instance.Save();

                /// Update main view
                Settings.Instance.OnResetButton();

                /// Reload songs
                OnReloadButton();
            });
        }
        /// <summary>
        /// Reload songs
        /// </summary>
        [UIAction("click-reload-btn-pressed")]
        private void OnReloadButton()
        {
            /// Reload songs
            MenuMusic.Instance.RefreshSongs();

            /// Start music
            MenuMusic.Instance.StartNewMusic();

            /// Show modal
            ShowMessageModal("Songs were reload!");
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
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#menu-music");
        }
    }
}
