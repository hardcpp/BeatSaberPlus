using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Info view UI controller
    /// </summary>
    internal class InfoView : SDK.UI.ResourceViewController<InfoView>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        internal GameObject m_Background = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Welcome to BeatSaberPlus by HardCPP#1985</b></u>";
        [UIValue("Line2")]
        private readonly string m_Line2 = "Version 2.0.33-Preview";
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
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to patreon
        /// </summary>
        [UIAction("click-patreon-btn-pressed")]
        private void OnPatreonButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://www.patreon.com/BeatSaberPlus");
        }
        /// <summary>
        /// Go to discord
        /// </summary>
        [UIAction("click-discord-btn-pressed")]
        private void OnDiscordButton()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            Process.Start("https://discord.gg/K4X94Ea");
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
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki");
        }
    }
}
