using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal class SettingsRight : BeatSaberPlus.SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIObject("MenuPanel")]
        private GameObject m_MenuPanel;
        [UIObject("ToolsDevPanel")]
        private GameObject m_ToolsDevPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("disablebeatmapeditorbuttononmainmenu-toggle")]
        private ToggleSetting m_DisableBeatMapEditorButtonInMainMenu;
        [UIComponent("removenewcontentpromotional-toggle")]
        private ToggleSetting m_RemoveNewContentPromotional;

        [UIComponent("reorderplayersettings-toggle")]
        private ToggleSetting m_ReorderPlayerSettings;
        [UIComponent("addoverridelightintensityoption-toggle")]
        private ToggleSetting m_AddOverrideLightIntensityOption;
        [UIComponent("removebasegamefilterbutton-toggle")]
        private ToggleSetting m_RemoveBaseGameFilterButton;
        [UIComponent("mergelightpressetoptions-toggle")]
        private ToggleSetting m_MergeLightPressetOptions;
        [UIComponent("deletesongbutton-toggle")]
        private ToggleSetting m_DeleteSongButton;
        [UIComponent("removesongbrowsertrashcan-toggle")]
        private ToggleSetting m_RemoveSongBrowserTrashcan;
        [UIComponent("highlightplayedsong-toggle")]
        private ToggleSetting m_HighlightPlayedSong;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("RemoveOldLogsToggle")]
        private ToggleSetting m_RemoveOldLogsToggle;
        [UIComponent("LogEntriesToKeepIncrement")]
        private IncrementSetting m_LogEntriesToKeepIncrement;

        [UIComponent("fpfcescape-toggle")]
        private ToggleSetting m_FPFCEscape;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;
        /// <summary>
        /// Type segment control
        /// </summary>
        private TextSegmentedControl m_TypeSegmentControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(SettingsRight.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Create type selector
            m_TypeSegmentControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false);
            m_TypeSegmentControl.SetTexts(new string[] { "Menu", "Tools / Dev" });
            m_TypeSegmentControl.ReloadData();
            m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DisableBeatMapEditorButtonInMainMenu,  l_Event, GTConfig.Instance.DisableBeatMapEditorButtonOnMainMenu,   true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveNewContentPromotional,           l_Event, GTConfig.Instance.RemoveNewContentPromotional,            true);

            /// Level selection
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ReorderPlayerSettings,                 l_Event, GTConfig.Instance.ReorderPlayerSettings,                  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_AddOverrideLightIntensityOption,       l_Event, GTConfig.Instance.AddOverrideLightIntensityOption,        true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveBaseGameFilterButton,            l_Event, GTConfig.Instance.RemoveBaseGameFilterButton,             true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_MergeLightPressetOptions,              l_Event, GTConfig.Instance.MergeLightPressetOptions,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DeleteSongButton,                      l_Event, GTConfig.Instance.DeleteSongButton,                       true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveSongBrowserTrashcan,             l_Event, GTConfig.Instance.DeleteSongBrowserTrashcan,              true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_HighlightPlayedSong,                   l_Event, GTConfig.Instance.HighlightPlayedSong,                    true);

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Logs
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_RemoveOldLogsToggle,           l_Event,         GTConfig.Instance.RemoveOldLogs,                          true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_LogEntriesToKeepIncrement,  l_Event, null,   GTConfig.Instance.LogEntriesToKeep,                       true);

            /// FPFC escape
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_FPFCEscape,                    l_Event,         GTConfig.Instance.FPFCEscape,                             false);

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// Force change to tab GamePlay
            OnTypeChanged(null, 0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the type is changed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Index">Tab index</param>
        private void OnTypeChanged(SegmentedControl p_Sender, int p_Index)
        {
            m_MenuPanel.SetActive(p_Index == 0);
            m_ToolsDevPanel.SetActive(p_Index == 1);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            GTConfig.Instance.DisableBeatMapEditorButtonOnMainMenu = m_DisableBeatMapEditorButtonInMainMenu.Value;
            GTConfig.Instance.RemoveNewContentPromotional          = m_RemoveNewContentPromotional.Value;

            /// Level selection
            GTConfig.Instance.ReorderPlayerSettings                = m_ReorderPlayerSettings.Value;
            GTConfig.Instance.AddOverrideLightIntensityOption      = m_AddOverrideLightIntensityOption.Value;
            GTConfig.Instance.RemoveBaseGameFilterButton           = m_RemoveBaseGameFilterButton.Value;
            GTConfig.Instance.MergeLightPressetOptions             = m_MergeLightPressetOptions.Value;
            GTConfig.Instance.DeleteSongButton                     = m_DeleteSongButton.Value;
            GTConfig.Instance.DeleteSongBrowserTrashcan            = m_RemoveSongBrowserTrashcan.Value;
            GTConfig.Instance.HighlightPlayedSong                  = m_HighlightPlayedSong.Value;

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Logs
            GTConfig.Instance.RemoveOldLogs                        = m_RemoveOldLogsToggle.Value;
            GTConfig.Instance.LogEntriesToKeep                     = (int)m_LogEntriesToKeepIncrement.Value;

            /// FPFC escape
            GTConfig.Instance.FPFCEscape                           = m_FPFCEscape.Value;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// Update patches
            GameTweaker.Instance.UpdatePatches(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            m_DisableBeatMapEditorButtonInMainMenu.Value    = GTConfig.Instance.DisableBeatMapEditorButtonOnMainMenu;
            m_RemoveNewContentPromotional.Value             = GTConfig.Instance.RemoveNewContentPromotional;

            /// Level selection
            m_ReorderPlayerSettings.Value                   = GTConfig.Instance.ReorderPlayerSettings;
            m_AddOverrideLightIntensityOption.Value         = GTConfig.Instance.AddOverrideLightIntensityOption;
            m_RemoveBaseGameFilterButton.Value              = GTConfig.Instance.RemoveBaseGameFilterButton;
            m_MergeLightPressetOptions.Value                = GTConfig.Instance.MergeLightPressetOptions;
            m_DeleteSongButton.Value                        = GTConfig.Instance.DeleteSongButton;
            m_RemoveSongBrowserTrashcan.Value               = GTConfig.Instance.DeleteSongBrowserTrashcan;
            m_HighlightPlayedSong.Value                     = GTConfig.Instance.HighlightPlayedSong;

            ////////////////////////////////////////////////////////////////////////////
            /// Tools / Dev
            ////////////////////////////////////////////////////////////////////////////

            /// Logs
            m_RemoveOldLogsToggle.Value                     = GTConfig.Instance.RemoveOldLogs;
            m_LogEntriesToKeepIncrement.Value               = GTConfig.Instance.LogEntriesToKeep;

            /// FPFC escape
            m_FPFCEscape.Value                              = GTConfig.Instance.FPFCEscape;

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged(null);
        }
    }
}
