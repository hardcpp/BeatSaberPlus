using CP_SDK.XUI;
using UnityEngine;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Main left view controller
    /// </summary>
    public sealed class MainLeftView : ViewController<MainLeftView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information"),

                Templates.ScrollableInfos(50,
                    XUIText.Make($"<b>Welcome to {ChatPlexSDK.ProductName} by HardCPP#1985</b>\nVersion {ChatPlexSDK.ProductVersion}!")
                        .SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Documentation", OnDocumentationButton),
                    XUIPrimaryButton.Make("Discord", OnDiscordButton)
                ),
                Templates.ExpandedButtonsLine(
                    XUISecondaryButton.Make("Donate - Patreon", OnDonateButton)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL("https://github.com/hardcpp/BeatSaberPlus/wiki");
        }
        /// <summary>
        /// Go to discord
        /// </summary>
        private void OnDiscordButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL("https://discord.chatplex.org");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to donate
        /// </summary>
        private void OnDonateButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL("https://donate.chatplex.org");
        }
    }
}
