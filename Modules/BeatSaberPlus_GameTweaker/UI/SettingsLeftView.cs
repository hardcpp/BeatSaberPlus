using CP_SDK.XUI;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr = "<line-height=125%><b>Game Tweaker</b>"
            + "\n" + "<i><color=#CCCCCCFF>This module allow you to customize your game experience, remove some boring base game features, and add new cool features/tweaks</color></i>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information"),

                Templates.ScrollableInfos(50,
                    XUIText.Make(s_InformationStr)
                        .SetAlign(TMPro.TextAlignmentOptions.Left)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Reset", OnResetButton)
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
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\ngame tweaker configuration?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                /// Reset config
                GTConfig.Instance.Reset();
                GTConfig.Instance.Enabled = true;
                GTConfig.Instance.Save();

                /// Refresh values
                SettingsMainView.Instance.RefreshSettings();
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
            Application.OpenURL(GameTweaker.Instance.DocumentationURL);
        }
    }
}
