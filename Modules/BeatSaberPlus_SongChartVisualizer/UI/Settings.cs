using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus_SongChartVisualizer.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("alignwithfloor-toggle")]
        public ToggleSetting m_AlignWithFloor;
        [UIComponent("showlockicon-toggle")]
        public ToggleSetting m_ShowLockIcon;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("followenvironementrotations-toggle")]
        private ToggleSetting m_FollowEnvironementRotations;
        [UIComponent("backgroundopacity-increment")]
        private IncrementSetting m_BackgroundOpacity;
        [UIComponent("cursoropacity-increment")]
        private IncrementSetting m_CursorOpacity;
        [UIComponent("lineopacity-increment")]
        private IncrementSetting m_LineOpacity;
        [UIComponent("legendopacity-increment")]
        private IncrementSetting m_LegendOpacity;
        [UIComponent("dashopacity-increment")]
        private IncrementSetting m_DashOpacity;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("shownpslegend-toggle")]
        private ToggleSetting m_ShowNPSLegend;
        [UIComponent("background-color")]
        private ColorSetting m_BackgroundColor;
        [UIComponent("cursor-color")]
        private ColorSetting m_CursorColor;
        [UIComponent("line-color")]
        private ColorSetting m_LineColor;
        [UIComponent("legend-color")]
        private ColorSetting m_LegendColor;
        [UIComponent("dash-color")]
        private ColorSetting m_DashColor;
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
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Left
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_AlignWithFloor,              l_Event,                                                        SCVConfig.Instance.AlignWithFloor,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowLockIcon,                l_Event,                                                        SCVConfig.Instance.ShowLockIcon,                 true);

            /// Center
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_FollowEnvironementRotations, l_Event,                                                        SCVConfig.Instance.FollowEnvironementRotation,   true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_BackgroundOpacity,        l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, SCVConfig.Instance.BackgroundColor.a,            true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CursorOpacity,            l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, SCVConfig.Instance.CursorColor.a,                true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_LineOpacity,              l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, SCVConfig.Instance.LineColor.a,                  true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_LegendOpacity,            l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, SCVConfig.Instance.LegendColor.a,                true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_DashOpacity,              l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, SCVConfig.Instance.DashLineColor.a,              true);

            /// Right
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ShowNPSLegend,  l_Event, SCVConfig.Instance.ShowNPSLegend,      true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_BackgroundColor, l_Event, SCVConfig.Instance.BackgroundColor,    true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_CursorColor,     l_Event, SCVConfig.Instance.CursorColor,        true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_LineColor,       l_Event, SCVConfig.Instance.LineColor,          true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_LegendColor,     l_Event, SCVConfig.Instance.LegendColor,        true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_DashColor,       l_Event, SCVConfig.Instance.DashLineColor,      true);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            SongChartVisualizer.Instance.RefreshPreview();
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            SongChartVisualizer.Instance.DestroyChart();
            SCVConfig.Instance.Save();
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
            SCVConfig.Instance.AlignWithFloor               = m_AlignWithFloor.Value;
            SCVConfig.Instance.ShowLockIcon                 = m_ShowLockIcon.Value;
            SCVConfig.Instance.FollowEnvironementRotation   = m_FollowEnvironementRotations.Value;
            SCVConfig.Instance.ShowNPSLegend                = m_ShowNPSLegend.Value;

            SCVConfig.Instance.CursorColor = new Color(
                m_BackgroundColor.CurrentColor.r,
                m_BackgroundColor.CurrentColor.g,
                m_BackgroundColor.CurrentColor.b,
                m_BackgroundOpacity.Value
            );

            SCVConfig.Instance.CursorColor = new Color(
                m_CursorColor.CurrentColor.r,
                m_CursorColor.CurrentColor.g,
                m_CursorColor.CurrentColor.b,
                m_CursorOpacity.Value
            );

            SCVConfig.Instance.LineColor = new Color(
                m_LineColor.CurrentColor.r,
                m_LineColor.CurrentColor.g,
                m_LineColor.CurrentColor.b,
                m_LineOpacity.Value
            );

            SCVConfig.Instance.LegendColor = new Color(
                m_LegendColor.CurrentColor.r,
                m_LegendColor.CurrentColor.g,
                m_LegendColor.CurrentColor.b,
                m_LegendOpacity.Value
            );

            SCVConfig.Instance.DashLineColor = new Color(
                m_DashColor.CurrentColor.r,
                m_DashColor.CurrentColor.g,
                m_DashColor.CurrentColor.b,
                m_DashOpacity.Value
            );

            SCVConfig.Instance.Save();

            /// Refresh preview
            SongChartVisualizer.Instance.RefreshPreview();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            m_PreventChanges = true;

            /// Set values
            m_AlignWithFloor.Value              = SCVConfig.Instance.AlignWithFloor;
            m_ShowLockIcon.Value                = SCVConfig.Instance.ShowLockIcon;

            /// Set values
            m_FollowEnvironementRotations.Value = SCVConfig.Instance.FollowEnvironementRotation;
            m_BackgroundOpacity.Value           = SCVConfig.Instance.BackgroundColor.a;
            m_CursorOpacity.Value               = SCVConfig.Instance.CursorColor.a;
            m_LineOpacity.Value                 = SCVConfig.Instance.LineColor.a;
            m_LegendOpacity.Value               = SCVConfig.Instance.LegendColor.a;
            m_DashOpacity.Value                 = SCVConfig.Instance.DashLineColor.a;

            /// Set values
            m_ShowNPSLegend.Value           = SCVConfig.Instance.ShowNPSLegend;
            m_BackgroundColor.CurrentColor  = SCVConfig.Instance.BackgroundColor.ColorWithAlpha(1f);
            m_CursorColor.CurrentColor      = SCVConfig.Instance.CursorColor.ColorWithAlpha(1f);
            m_LineColor.CurrentColor        = SCVConfig.Instance.LineColor.ColorWithAlpha(1f);
            m_LegendColor.CurrentColor      = SCVConfig.Instance.LegendColor.ColorWithAlpha(1f);
            m_DashColor.CurrentColor        = SCVConfig.Instance.DashLineColor.ColorWithAlpha(1f);

            m_PreventChanges = false;

            SongChartVisualizer.Instance.RefreshPreview();
        }
    }
}
