using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus.Modules.NoteTweaker.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
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
            SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);

            var l_Event         = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));
            var l_ArrowLColor   = new Color(Config.NoteTweaker.ArrowLR, Config.NoteTweaker.ArrowLG, Config.NoteTweaker.ArrowLB, 1f);
            var l_ArrowRColor   = new Color(Config.NoteTweaker.ArrowRR, Config.NoteTweaker.ArrowRG, Config.NoteTweaker.ArrowRB, 1f);
            var l_DotLColor     = new Color(Config.NoteTweaker.DotLR,   Config.NoteTweaker.DotLG,   Config.NoteTweaker.DotLB,   1f);
            var l_DotRColor     = new Color(Config.NoteTweaker.DotRR,   Config.NoteTweaker.DotRG,   Config.NoteTweaker.DotRB,   1f);

            SDK.UI.ToggleSetting.Setup(m_ShowDotOnDirectionalNoteToggle,    l_Event,                                            Config.NoteTweaker.ShowDotsWithArrow,   true);
            SDK.UI.ToggleSetting.Setup(m_OverrideArrowColors,               l_Event,                                            Config.NoteTweaker.OverrideArrowColors, true);
            SDK.UI.ToggleSetting.Setup(m_OverrideDotColors,                 l_Event,                                            Config.NoteTweaker.OverrideDotColors,   true);
            SDK.UI.IncrementSetting.Setup(m_NoteScaleIncrement,             l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.Scale,               true);

            SDK.UI.IncrementSetting.Setup(m_ArrowScaleIncrement,            l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.ArrowScale,          true);
            SDK.UI.ColorSetting.Setup(m_ArrowLColor,                        l_Event,                                            l_ArrowLColor,                          true);
            SDK.UI.ColorSetting.Setup(m_ArrowRColor,                        l_Event,                                            l_ArrowRColor,                          true);
            SDK.UI.IncrementSetting.Setup(m_ArrowIntensity,                 l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.ArrowA,              true);

            SDK.UI.IncrementSetting.Setup(m_DotScaleIncrement,              l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.DotScale,            true);
            SDK.UI.ColorSetting.Setup(m_DotLColor,                          l_Event,                                            l_DotLColor,                            true);
            SDK.UI.ColorSetting.Setup(m_DotRColor,                          l_Event,                                            l_DotRColor,                            true);
            SDK.UI.IncrementSetting.Setup(m_DotIntensity,                   l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.DotA,                true);

            SDK.UI.IncrementSetting.Setup(m_PrecisionDotScaleIncrement,     l_Event, SDK.UI.BSMLSettingFormartter.Percentage,   Config.NoteTweaker.PrecisionDotScale,   true);
            SDK.UI.ToggleSetting.Setup(m_OverrideBombColorToggle,           l_Event,                                            Config.NoteTweaker.OverrideBombColor,   true);
            SDK.UI.ColorSetting.Setup(m_BombColor,                          l_Event,                                            Config.NoteTweaker.BombColor,           true);

            m_ArrowLColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_ArrowRColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_DotLColor.interactable    = Config.NoteTweaker.OverrideDotColors;
            m_DotRColor.interactable    = Config.NoteTweaker.OverrideDotColors;
            m_BombColor.interactable    = Config.NoteTweaker.OverrideBombColor;
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
            Config.NoteTweaker.ShowDotsWithArrow    = m_ShowDotOnDirectionalNoteToggle.Value;
            Config.NoteTweaker.OverrideArrowColors  = m_OverrideArrowColors.Value;
            Config.NoteTweaker.OverrideDotColors    = m_OverrideDotColors.Value;
            Config.NoteTweaker.Scale                = m_NoteScaleIncrement.Value;

            Config.NoteTweaker.ArrowScale           = m_ArrowScaleIncrement.Value;
            Config.NoteTweaker.ArrowLR              = m_ArrowLColor.CurrentColor.r;
            Config.NoteTweaker.ArrowLG              = m_ArrowLColor.CurrentColor.g;
            Config.NoteTweaker.ArrowLB              = m_ArrowLColor.CurrentColor.b;
            Config.NoteTweaker.ArrowRR              = m_ArrowRColor.CurrentColor.r;
            Config.NoteTweaker.ArrowRG              = m_ArrowRColor.CurrentColor.g;
            Config.NoteTweaker.ArrowRB              = m_ArrowRColor.CurrentColor.b;
            Config.NoteTweaker.ArrowA               = m_ArrowIntensity.Value;

            Config.NoteTweaker.DotScale             = m_DotScaleIncrement.Value;
            Config.NoteTweaker.DotLR                = m_DotLColor.CurrentColor.r;
            Config.NoteTweaker.DotLG                = m_DotLColor.CurrentColor.g;
            Config.NoteTweaker.DotLB                = m_DotLColor.CurrentColor.b;
            Config.NoteTweaker.DotRR                = m_DotRColor.CurrentColor.r;
            Config.NoteTweaker.DotRG                = m_DotRColor.CurrentColor.g;
            Config.NoteTweaker.DotRB                = m_DotRColor.CurrentColor.b;
            Config.NoteTweaker.DotA                 = m_DotIntensity.Value;

            Config.NoteTweaker.PrecisionDotScale    = m_PrecisionDotScaleIncrement.Value;
            Config.NoteTweaker.OverrideBombColor    = m_OverrideBombColorToggle.Value;
            Config.NoteTweaker.BombR                = m_BombColor.CurrentColor.r;
            Config.NoteTweaker.BombG                = m_BombColor.CurrentColor.g;
            Config.NoteTweaker.BombB                = m_BombColor.CurrentColor.b;

            m_ArrowLColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_ArrowRColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_DotLColor.interactable    = Config.NoteTweaker.OverrideDotColors;
            m_DotRColor.interactable    = Config.NoteTweaker.OverrideDotColors;
            m_BombColor.interactable    = Config.NoteTweaker.OverrideBombColor;

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
            m_ShowDotOnDirectionalNoteToggle.Value  = Config.NoteTweaker.ShowDotsWithArrow;
            m_OverrideArrowColors.Value             = Config.NoteTweaker.OverrideArrowColors;
            m_OverrideDotColors.Value               = Config.NoteTweaker.OverrideDotColors;
            m_NoteScaleIncrement.Value              = Config.NoteTweaker.Scale;

            m_ArrowScaleIncrement.Value             = Config.NoteTweaker.ArrowScale;
            m_ArrowLColor.CurrentColor              = new Color(Config.NoteTweaker.ArrowLR, Config.NoteTweaker.ArrowLG, Config.NoteTweaker.ArrowLB, 1f);
            m_ArrowRColor.CurrentColor              = new Color(Config.NoteTweaker.ArrowRR, Config.NoteTweaker.ArrowRG, Config.NoteTweaker.ArrowRB, 1f);
            m_ArrowIntensity.Value                  = Config.NoteTweaker.ArrowA;

            m_DotScaleIncrement.Value               = Config.NoteTweaker.DotScale;
            m_DotLColor.CurrentColor                = new Color(Config.NoteTweaker.DotLR, Config.NoteTweaker.DotLG, Config.NoteTweaker.DotLB, 1f);
            m_DotRColor.CurrentColor                = new Color(Config.NoteTweaker.DotRR, Config.NoteTweaker.DotRG, Config.NoteTweaker.DotRB, 1f);
            m_DotIntensity.Value                    = Config.NoteTweaker.DotA;

            m_PrecisionDotScaleIncrement.Value      = Config.NoteTweaker.PrecisionDotScale;
            m_OverrideBombColorToggle.Value         = Config.NoteTweaker.OverrideBombColor;
            m_BombColor.CurrentColor                = Config.NoteTweaker.BombColor;

            m_ArrowLColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_ArrowRColor.interactable  = Config.NoteTweaker.OverrideArrowColors;
            m_DotLColor.interactable    = Config.NoteTweaker.OverrideDotColors;
            m_DotRColor.interactable    = Config.NoteTweaker.OverrideDotColors;

            m_PreventChanges = false;

            SettingsRight.Instance.RefreshSettings();
        }
    }
}
