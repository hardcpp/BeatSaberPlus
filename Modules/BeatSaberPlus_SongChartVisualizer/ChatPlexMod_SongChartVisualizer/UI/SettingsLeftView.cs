using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_SongChartVisualizer.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr =
                     "<b>Thanks to <b>Shoko84 & Opzon</b> for the original idea</b>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information / Credits"),

                Templates.ScrollableInfos(50,
                    XUIText.Make(s_InformationStr).SetAlign(TMPro.TextAlignmentOptions.Left)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Reset",           OnResetButton),
                    XUIPrimaryButton.Make("Reset Position",  OnResetPositionButton)
                ),
                Templates.ExpandedButtonsLine(
                    XUISecondaryButton.Make("Documentation", OnDocumentationButton)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset button
        /// </summary>
        private void OnResetButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall SongChartVisualizer settings?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                /// Reset settings
                SCVConfig.Instance.Reset();
                SCVConfig.Instance.Enabled = true;
                SCVConfig.Instance.Save();

                /// Update main view
                SettingsMainView.Instance.OnResetButton();
            });
        }
        /// <summary>
        /// Reset position button
        /// </summary>
        private void OnResetPositionButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nSongChartVisualizer position?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                /// Reset settings
                SCVConfig.Instance.ResetPosition();
                SCVConfig.Instance.Save();

                /// Update main view
                SettingsMainView.Instance.OnResetButton();
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL(SongChartVisualizer.Instance.DocumentationURL);
        }
    }
}
