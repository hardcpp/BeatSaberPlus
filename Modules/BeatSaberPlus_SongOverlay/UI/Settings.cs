using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus_SongOverlay.UI
{
    /// <summary>
    /// ModuleTemplate settings view controller
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            
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
