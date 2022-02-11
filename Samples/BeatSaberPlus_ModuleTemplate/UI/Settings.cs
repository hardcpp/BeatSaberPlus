using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;

namespace BeatSaberPlus_ModuleTemplate.UI
{
    /// <summary>
    /// ModuleTemplate settings view controller
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("templatesetting-toggle")]
        private ToggleSetting m_TemplateSetting;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(Settings.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_TemplateSetting, l_Event, MTConfig.Instance.TemplateSetting, true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected sealed override void OnViewDeactivation()
        {
            MTConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            /// Update config
            MTConfig.Instance.TemplateSetting = m_TemplateSetting.Value;
        }
    }
}
