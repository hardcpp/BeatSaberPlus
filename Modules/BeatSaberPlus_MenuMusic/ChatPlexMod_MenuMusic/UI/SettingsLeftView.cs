using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_MenuMusic.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        private static readonly string s_InformationStr =
                      "<b>Thanks to <b>Lunikc</b> for original idea</b>" + "\n" +
                     $"Custom music folder is: UserData/{CP_SDK.ChatPlexSDK.ProductName}Plus/MenuMusic/CustomMusic";

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
                    XUIPrimaryButton.Make("Reset", OnResetButton),
                    XUIPrimaryButton.Make("Reload", OnReloadButton)
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
            ShowConfirmationModal("<color=yellow><b>Do you really want to reset\nall Menu Music settings?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                MMConfig.Instance.Reset();
                MMConfig.Instance.Enabled = true;
                MMConfig.Instance.Save();

                SettingsMainView.Instance.OnResetButton();

                OnReloadButton();
            });
        }
        /// <summary>
        /// Reload songs
        /// </summary>
        private void OnReloadButton()
        {
            MenuMusic.Instance.UpdateMusicProvider();

            ShowMessageModal("Musics were reload!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Documentation button
        /// </summary>
        private void OnDocumentationButton()
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL(MenuMusic.Instance.DocumentationURL);
        }
    }
}
