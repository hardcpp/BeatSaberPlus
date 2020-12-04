using BeatSaberMarkupLanguage.ViewControllers;

namespace BeatSaberPlus.Plugins.SongChartVisualizer.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal class SettingsRight : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);
    }
}
