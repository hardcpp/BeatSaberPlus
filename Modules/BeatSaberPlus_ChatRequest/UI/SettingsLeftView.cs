using CP_SDK.XUI;
using UnityEngine;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr =
                     "<b>Commands</b>"
            + "\n" + "- <b>!bsr #KEY/#NAME</b>\n<i><color=#CCCCCCFF>Request a song by BSR code or search by name</color></i>"
            + "\n" + "- <b>!bsrhelp</b>\n<i><color=#CCCCCCFF>Display a guide about how to request a song</color></i>"
            + "\n" + "- <b>!oops !wrongsong !wrong</b>\n<i><color=#CCCCCCFF>Remove last user requested song</color></i>"
            + "\n" + "- <b>!queue</b>\n<i><color=#CCCCCCFF>Show songs in queue</color></i>"
            + "\n" + "- <b>!queuestatus</b>\n<i><color=#CCCCCCFF>Show how many songs are in queue</color></i>"
            + "\n" + "- <b>!link</b>\n<i><color=#CCCCCCFF>Link current or last played song information in chat</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!allow #KEY</b>\n<i><color=#CCCCCCFF>Allow a map by BSR code to ignore all filters</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!att #KEY/#NAME</b>\n<i><color=#CCCCCCFF>Add any song in queue to the top by BSR code</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!block #KEY</b>\n<i><color=#CCCCCCFF>Block & remove any song in queue by BSR code</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!bsrban #USER</b>\n<i><color=#CCCCCCFF>Prevent an user from doing request</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!bsrbanmapper #USER</b>\n<i><color=#CCCCCCFF>Prevent an mapper maps to get requested</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!bsrunban #USER</b>\n<i><color=#CCCCCCFF>Remove an user from request ban list</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!bsrunbanmapper #USER</b>\n<i><color=#CCCCCCFF>Remove an mapper from the ban list</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!close</b>\n<i><color=#CCCCCCFF>Close the request queue</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!modadd #KEY/#NAME</b>\n<i><color=#CCCCCCFF>Add a song by BSR code ignoring all filters</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!mtt #KEY/#USER</b>\n<i><color=#CCCCCCFF>Move any song in queue to the top by BSR code</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!open</b>\n<i><color=#CCCCCCFF>Open the request queue</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!remap #KEY #KEY</b>\n<i><color=#CCCCCCFF>Replace all left BSR to the right BSR</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!remove #KEY/#USER</b>\n<i><color=#CCCCCCFF>Remove any song in queue by BSR code</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!sabotage on/off</b>\n<i><color=#CCCCCCFF>Enable or disable LIV streamer kit bombs</color></i>"
            + "\n" + "- <color=yellow>[Moderator]</color><b>!songmsg #KEY #MSG</b>\n<i><color=#CCCCCCFF>Allow to set a message on a request</color></i>"
            + "\n"
            + "\n" + "<b>Filters</b>"
            + "\n" + "- <b>No BeatSage</b>\n<i><color=#CCCCCCFF>Discard all auto mapped maps</color></i>"
            + "\n" + "- <b>NPS min</b>\n<i><color=#CCCCCCFF>Discard all maps with no difficulty above NotePerSecond min</color></i>"
            + "\n" + "- <b>NPS max</b>\n<i><color=#CCCCCCFF>Discard all maps with no difficulty below NotePerSecond max</color></i>"
            + "\n" + "- <b>NJS min</b>\n<i><color=#CCCCCCFF>Discard all maps with no difficulty above NoteJumpSpeed min</color></i>"
            + "\n" + "- <b>NJS max</b>\n<i><color=#CCCCCCFF>Discard all maps with no difficulty below NoteJumpSpeed max</color></i>"
            + "\n" + "- <b>Duration max</b>\n<i><color=#CCCCCCFF>Maximum duration of a map in minutes</color></i>"
            + "\n" + "- <b>Vote min</b>\n<i><color=#CCCCCCFF>Required up-vote ratio percentage</color></i>"
            + "\n" + "- <b>Upload date min</b>\n<i><color=#CCCCCCFF>Minimum upload date</color></i>"
            + "\n" + "- <b>Upload date max</b>\n<i><color=#CCCCCCFF>Maximum upload date</color></i>";

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
                    XUIPrimaryButton.Make("OBS integration",     OnOBSIntegrationButton),
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
        /// Open OBS integration button
        /// </summary>
        private void OnOBSIntegrationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL("https://github.com/hardcpp/BeatSaberPlus/wiki#chat-request---obs-integration");
        }
        /// <summary>
        /// Reset button
        /// </summary>
        private void OnResetButton()
        {
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nchat request configuration and filters?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                /// Reset config
                CRConfig.Instance.Reset();
                CRConfig.Instance.Enabled = true;
                CRConfig.Instance.Save();

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
            Application.OpenURL(ChatRequest.Instance.DocumentationURL);
        }
    }
}
