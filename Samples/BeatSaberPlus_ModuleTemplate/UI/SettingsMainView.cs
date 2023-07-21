using CP_SDK.XUI;

namespace BeatSaberPlus_ModuleTemplate.UI
{
    /// <summary>
    /// ModuleTemplate settings view controller
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUIToggle m_TemplateSetting = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Module Template | Settings"),

                XUIText.Make("Template setting"),
                XUIToggle.Make()
                    .SetValue(MTConfig.Instance.TemplateSetting)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_TemplateSetting)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
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
        private void OnSettingChanged()
        {
            /// Update config
            MTConfig.Instance.TemplateSetting = m_TemplateSetting.Element.GetValue();
        }
    }
}
