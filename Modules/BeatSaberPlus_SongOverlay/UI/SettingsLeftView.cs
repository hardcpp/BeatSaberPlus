using CP_SDK.XUI;
using UnityEngine;

namespace BeatSaberPlus_SongOverlay.UI
{
    /// <summary>
    /// Settings left view
    /// </summary>
    internal sealed class SettingsLeftView : CP_SDK.UI.ViewController<SettingsLeftView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Information"),

                Templates.ScrollableInfos(55,
                    XUIText.Make($"This module provide a server for Web based song overlays!")
                        .SetAlign(TMPro.TextAlignmentOptions.Left)
                ),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Documentation", OnDocumentationButton)
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
            Application.OpenURL(SongOverlay.Instance.DocumentationURL);
        }
    }
}
