using BeatSaberMarkupLanguage.Attributes;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus.Modules.SongChartVisualizer.UI
{
    /// <summary>
    /// Settings left credit view
    /// </summary>
    internal class SettingsLeft : SDK.UI.ResourceViewController<SettingsLeft>
    {
#pragma warning disable CS0414
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIValue("Line1")]
        private readonly string m_Line1 = "<u><b>Thanks to <b>Shoko84</b> for original idea</b></u>";
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
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset button
        /// </summary>
        [UIAction("click-reset-btn-pressed")]
        private void OnResetButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall SongChartVisualizer settings?", () =>
            {
                /// Reset settings
                Config.SongChartVisualizer.Reset();

                /// Update main view
                Settings.Instance.OnResetButton();
            });
        }
        /// <summary>
        /// Reset position button
        /// </summary>
        [UIAction("click-reset-position-btn-pressed")]
        private void OnResetPositionButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nSongChartVisualizer position?", () =>
            {
                /// Reset settings
                Config.SongChartVisualizer.ResetPosition();

                /// Update main view
                Settings.Instance.OnResetButton();
            });
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
            Process.Start("https://github.com/hardcpp/BeatSaberPlus/wiki#song-chart-visualizer");
        }
    }
}
