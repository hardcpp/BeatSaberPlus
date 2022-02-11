using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("OverrideArrowColors")]
        private ToggleSetting m_OverrideArrowColors;
        [UIComponent("OverrideDotColors")]
        private ToggleSetting m_OverrideDotColors;
        [UIComponent("ShowDotOnDirectionalNoteToggle")]
        private ToggleSetting m_ShowDotOnDirectionalNoteToggle;
        [UIComponent("NoteScaleIncrement")]
        private IncrementSetting m_NoteScaleIncrement;

        [UIComponent("ArrowScaleIncrement")]
        private IncrementSetting m_ArrowScaleIncrement;
        [UIComponent("ArrowLColor")]
        private ColorSetting m_ArrowLColor;
        [UIComponent("ArrowRColor")]
        private ColorSetting m_ArrowRColor;
        [UIComponent("ArrowIntensity")]
        private IncrementSetting m_ArrowIntensity;

        [UIComponent("DotScaleIncrement")]
        private IncrementSetting m_DotScaleIncrement;
        [UIComponent("DotLColor")]
        private ColorSetting m_DotLColor;
        [UIComponent("DotRColor")]
        private ColorSetting m_DotRColor;
        [UIComponent("DotIntensity")]
        private IncrementSetting m_DotIntensity;

        [UIComponent("PrecisionDotScaleIncrement")]
        private IncrementSetting m_PrecisionDotScaleIncrement;
        [UIComponent("OverrideBombColorToggle")]
        private ToggleSetting m_OverrideBombColorToggle;
        [UIComponent("BombColor")]
        private ColorSetting m_BombColor;

        [UIObject("InfoBackground")]
        private GameObject m_InfoBackground = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);

            var l_Event         = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_ArrowLColor   = NTConfig.Instance.ArrowLColor.ColorWithAlpha(1.00f);
            var l_ArrowRColor   = NTConfig.Instance.ArrowRColor.ColorWithAlpha(1.00f);
            var l_DotLColor     = NTConfig.Instance.DotLColor.ColorWithAlpha(1.00f);
            var l_DotRColor     = NTConfig.Instance.DotRColor.ColorWithAlpha(1.00f);

            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowDotOnDirectionalNoteToggle,    l_Event,                                                          NTConfig.Instance.ShowDotsWithArrow,   true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_OverrideArrowColors,               l_Event,                                                          NTConfig.Instance.OverrideArrowColors, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_OverrideDotColors,                 l_Event,                                                          NTConfig.Instance.OverrideDotColors,   true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_NoteScaleIncrement,             l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.Scale,               true);

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ArrowScaleIncrement,            l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.ArrowScale,          true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ArrowLColor,                        l_Event,                                                          l_ArrowLColor,                         true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ArrowRColor,                        l_Event,                                                          l_ArrowRColor,                         true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ArrowIntensity,                 l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.ArrowAlpha,          true);

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_DotScaleIncrement,              l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.DotScale,            true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_DotLColor,                          l_Event,                                                          l_DotLColor,                           true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_DotRColor,                          l_Event,                                                          l_DotRColor,                           true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_DotIntensity,                   l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.DotAlpha,            true);

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_PrecisionDotScaleIncrement,     l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   NTConfig.Instance.PrecisionDotScale,   true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_OverrideBombColorToggle,           l_Event,                                                          NTConfig.Instance.OverrideBombColor,   true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_BombColor,                          l_Event,                                                          NTConfig.Instance.BombColor,           true);

            m_ArrowLColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_ArrowRColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_DotLColor.interactable    = NTConfig.Instance.OverrideDotColors;
            m_DotRColor.interactable    = NTConfig.Instance.OverrideDotColors;
            m_BombColor.interactable    = NTConfig.Instance.OverrideBombColor;
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            NTConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update config
            NTConfig.Instance.ShowDotsWithArrow    = m_ShowDotOnDirectionalNoteToggle.Value;
            NTConfig.Instance.OverrideArrowColors  = m_OverrideArrowColors.Value;
            NTConfig.Instance.OverrideDotColors    = m_OverrideDotColors.Value;
            NTConfig.Instance.Scale                = m_NoteScaleIncrement.Value;

            NTConfig.Instance.ArrowScale           = m_ArrowScaleIncrement.Value;
            NTConfig.Instance.ArrowAlpha           = m_ArrowIntensity.Value;
            NTConfig.Instance.ArrowLColor          = m_ArrowLColor.CurrentColor.ColorWithAlpha(1.0f);
            NTConfig.Instance.ArrowRColor          = m_ArrowRColor.CurrentColor.ColorWithAlpha(1.0f);

            NTConfig.Instance.DotScale             = m_DotScaleIncrement.Value;
            NTConfig.Instance.DotAlpha             = m_DotIntensity.Value;
            NTConfig.Instance.DotLColor            = m_DotLColor.CurrentColor.ColorWithAlpha(1.0f);
            NTConfig.Instance.DotRColor            = m_DotRColor.CurrentColor.ColorWithAlpha(1.0f);

            NTConfig.Instance.PrecisionDotScale    = m_PrecisionDotScaleIncrement.Value;
            NTConfig.Instance.OverrideBombColor    = m_OverrideBombColorToggle.Value;
            NTConfig.Instance.BombColor            = m_BombColor.CurrentColor.ColorWithAlpha(1.0f);

            m_ArrowLColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_ArrowRColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_DotLColor.interactable    = NTConfig.Instance.OverrideDotColors;
            m_DotRColor.interactable    = NTConfig.Instance.OverrideDotColors;
            m_BombColor.interactable    = NTConfig.Instance.OverrideBombColor;

            /// Refresh preview
            SettingsRight.Instance.RefreshSettings();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Set values
            m_ShowDotOnDirectionalNoteToggle.Value  = NTConfig.Instance.ShowDotsWithArrow;
            m_OverrideArrowColors.Value             = NTConfig.Instance.OverrideArrowColors;
            m_OverrideDotColors.Value               = NTConfig.Instance.OverrideDotColors;
            m_NoteScaleIncrement.Value              = NTConfig.Instance.Scale;

            m_ArrowScaleIncrement.Value             = NTConfig.Instance.ArrowScale;
            m_ArrowLColor.CurrentColor              = NTConfig.Instance.ArrowLColor.ColorWithAlpha(1.00f);
            m_ArrowRColor.CurrentColor              = NTConfig.Instance.ArrowRColor.ColorWithAlpha(1.00f);
            m_ArrowIntensity.Value                  = NTConfig.Instance.ArrowAlpha;

            m_DotScaleIncrement.Value               = NTConfig.Instance.DotScale;
            m_DotLColor.CurrentColor                = NTConfig.Instance.DotLColor.ColorWithAlpha(1.00f);
            m_DotRColor.CurrentColor                = NTConfig.Instance.DotRColor.ColorWithAlpha(1.00f);
            m_DotIntensity.Value                    = NTConfig.Instance.DotAlpha;

            m_PrecisionDotScaleIncrement.Value      = NTConfig.Instance.PrecisionDotScale;
            m_OverrideBombColorToggle.Value         = NTConfig.Instance.OverrideBombColor;
            m_BombColor.CurrentColor                = NTConfig.Instance.BombColor;

            m_ArrowLColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_ArrowRColor.interactable  = NTConfig.Instance.OverrideArrowColors;
            m_DotLColor.interactable    = NTConfig.Instance.OverrideDotColors;
            m_DotRColor.interactable    = NTConfig.Instance.OverrideDotColors;

            m_PreventChanges = false;

            SettingsRight.Instance.RefreshSettings();
        }
    }
}
