using CP_SDK.XUI;

namespace ChatPlexMod_SongChartVisualizer.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal sealed class SettingsRightView : CP_SDK.UI.ViewController<SettingsRightView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Preview"),

                XUIVSpacer.Make(65f)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
    }
}
