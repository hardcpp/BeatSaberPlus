using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using UnityEngine;

namespace BeatSaberPlus.Modules.GameTweaker.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal class SettingsRight : SDK.UI.ResourceViewController<SettingsRight>
    {
#pragma warning disable CS0649
        [UIObject("TypeSegmentPanel")]
        private GameObject m_TypeSegmentPanel;
        [UIObject("MenuPanel")]
        private GameObject m_MenuPanel;
        [UIObject("DevToolsTestingPanel")]
        private GameObject m_DevToolsTestingPanel;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("disablebeatmapeditorbuttononmainmenu-toggle")]
        private ToggleSetting m_DisableBeatMapEditorButtonInMainMenu;
        [UIComponent("showplayerstatisticsonmainmenu-toggle")]
        private ToggleSetting m_ShowPlayerStatisticsInMainMenu;
        [UIComponent("removenewcontentpromotional-toggle")]
        private ToggleSetting m_RemoveNewContentPromotional;

        [UIComponent("reorderplayersettings-toggle")]
        private ToggleSetting m_ReorderPlayerSettings;
        [UIComponent("addoverridelightintensityoption-toggle")]
        private ToggleSetting m_AddOverrideLightIntensityOption;
        [UIComponent("removebasegamefilterbutton-toggle")]
        private ToggleSetting m_RemoveBaseGameFilterButton;
        [UIComponent("deletesongbutton-toggle")]
        private ToggleSetting m_DeleteSongButton;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            m_TypeSegmentControl = SDK.UI.TextSegmentedControl.Create(m_TypeSegmentPanel.transform as RectTransform, false);
            m_TypeSegmentControl.SetTexts(new string[] { "Menu", "Dev / Testing" });
            m_TypeSegmentControl.ReloadData();
            m_TypeSegmentControl.didSelectCellEvent += OnTypeChanged;

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            SDK.UI.ToggleSetting.Setup(m_DisableBeatMapEditorButtonInMainMenu,  l_Event, Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu,   true);
            SDK.UI.ToggleSetting.Setup(m_ShowPlayerStatisticsInMainMenu,        l_Event, Config.GameTweaker.ShowPlayerStatisticsOnMainMenu,         true);
            SDK.UI.ToggleSetting.Setup(m_RemoveNewContentPromotional,           l_Event, Config.GameTweaker.RemoveNewContentPromotional,            true);

            /// Level selection
            SDK.UI.ToggleSetting.Setup(m_ReorderPlayerSettings,                 l_Event, Config.GameTweaker.ReorderPlayerSettings,                  true);
            SDK.UI.ToggleSetting.Setup(m_AddOverrideLightIntensityOption,       l_Event, Config.GameTweaker.AddOverrideLightIntensityOption,        true);
            SDK.UI.ToggleSetting.Setup(m_RemoveBaseGameFilterButton,            l_Event, Config.GameTweaker.RemoveBaseGameFilterButton,             true);
            SDK.UI.ToggleSetting.Setup(m_DeleteSongButton,                      l_Event, Config.GameTweaker.DeleteSongButton,                       true);

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Prepare sliders
            SDK.UI.ToggleSetting.Setup(m_FPFCEscape, l_Event, Config.GameTweaker.FPFCEscape, false);

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
            m_DevToolsTestingPanel.SetActive(p_Index == 1);
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
            Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu = m_DisableBeatMapEditorButtonInMainMenu.Value;
            Config.GameTweaker.ShowPlayerStatisticsOnMainMenu       = m_ShowPlayerStatisticsInMainMenu.Value;
            Config.GameTweaker.RemoveNewContentPromotional          = m_RemoveNewContentPromotional.Value;

            /// Level selection
            Config.GameTweaker.ReorderPlayerSettings                = m_ReorderPlayerSettings.Value;
            Config.GameTweaker.AddOverrideLightIntensityOption      = m_AddOverrideLightIntensityOption.Value;
            Config.GameTweaker.RemoveBaseGameFilterButton           = m_RemoveBaseGameFilterButton.Value;
            Config.GameTweaker.DeleteSongButton                     = m_DeleteSongButton.Value;

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Update config
            Config.GameTweaker.FPFCEscape = m_FPFCEscape.Value;

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
            m_DisableBeatMapEditorButtonInMainMenu.Value    = Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu;
            m_ShowPlayerStatisticsInMainMenu.Value          = Config.GameTweaker.ShowPlayerStatisticsOnMainMenu;
            m_RemoveNewContentPromotional.Value             = Config.GameTweaker.RemoveNewContentPromotional;

            /// Level selection
            m_ReorderPlayerSettings.Value                   = Config.GameTweaker.ReorderPlayerSettings;
            m_AddOverrideLightIntensityOption.Value         = Config.GameTweaker.AddOverrideLightIntensityOption;
            m_RemoveBaseGameFilterButton.Value              = Config.GameTweaker.RemoveBaseGameFilterButton;
            m_DeleteSongButton.Value                        = Config.GameTweaker.DeleteSongButton;

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////
            m_FPFCEscape.Value                              = Config.GameTweaker.FPFCEscape;

            m_PreventChanges = false;

            /// Refresh UI
            OnSettingChanged(null);
        }
    }
}
