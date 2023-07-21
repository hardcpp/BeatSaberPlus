using CP_SDK.XUI;

namespace BeatSaberPlus_SongOverlay.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Song Overlay - Settings"),

                XUIText.Make("No settings available yet...")
                    .SetStyle(TMPro.FontStyles.Italic)
                    .SetAlign(TMPro.TextAlignmentOptions.Midline),

                XUIVSpacer.Make(60f)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected sealed override void OnViewDeactivation()
        {
            SOConfig.Instance.Save();
        }
    }
}
