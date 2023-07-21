using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr = "Original mod made by <b>Cr4</b> and <b>Uialeth</b>"
            + "\n"
            + "\n" + "<b>Commands</b>"
            + "\n" + "- <color=yellow>[Moderator]</color> <b>!er rain #EMOTE #COUNT</b>\n<i><color=#CCCCCCFF>Trigger a emote rain</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color> <b>!er toggle</b>\n<i><color=#CCCCCCFF>Disable any emote rain until a Menu/GamePlay scene change</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color> <b>!er clear</b>\n<i><color=#CCCCCCFF>Clear all raining emotes</color></i>";

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
                    XUIText.Make(s_InformationStr)
                        .SetAlign(TMPro.TextAlignmentOptions.Left)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Reset",               OnResetButton),
                    XUIPrimaryButton.Make("Web Configuration",   OnWebConfigurationButton)
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
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall chat emote rain settings?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                /// Reset settings
                CERConfig.Instance.Reset();
                CERConfig.Instance.Enabled = true;
                CERConfig.Instance.Save();

                /// Refresh values
                SettingsMainView.Instance.RefreshSettings();
                SettingsRightView.Instance.RefreshSettings();
            });
        }
        /// <summary>
        /// Open web configuration button
        /// </summary>
        private void OnWebConfigurationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            CP_SDK.Chat.Service.OpenWebConfiguration();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL(ChatEmoteRain.Instance.DocumentationURL);
        }
    }
}
