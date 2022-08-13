using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_NoteTweaker.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
        /// <summary>
        /// Profile line per page
        /// </summary>
        private static int EVENT_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TabSelector")]
        private GameObject m_TabSelector;
        private TextSegmentedControl m_TabSelector_TabSelectorControl = null;

        [UIObject("Tabs")] private GameObject m_Tabs;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Profiles Tab
        [UIObject("ProfilesTab")]                   private GameObject  m_ProfilesTab = null;
        [UIObject("ProfileListFrame_Background")]   private GameObject  m_ProfileListFrame_Background = null;
        [UIObject("ProfilesList")]                  private GameObject  m_ProfilesListView = null;
        [UIComponent("ProfilesUpButton")]           private Button      m_ProfilesUpButton = null;
        [UIComponent("ProfilesDownButton")]         private Button      m_ProfilesDownButton = null;

        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_EventsList = null;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Notes Tab
        [UIObject("NotesTab")]                          private GameObject          m_NotesTab = null;
        [UIComponent("NotesTab_Scale")]                 private IncrementSetting    m_NotesTab_Scale;
        [UIComponent("NotesTab_ShowPrecisonDots")]      private ToggleSetting       m_NotesTab_ShowPrecisonDots;
        [UIComponent("NotesTab_PrecisionDotsScale")]    private IncrementSetting    m_NotesTab_PrecisionDotsScale;
        [UIObject("NotesTab_InfoBackground")]           private GameObject          m_NotesTab_InfoBackground = null;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Arrows Tab
        [UIObject("ArrowsTab")]                     private GameObject          m_ArrowsTab = null;
        [UIComponent("ArrowsTab_Scale")]            private IncrementSetting    m_ArrowsTab_Scale;
        [UIComponent("ArrowsTab_Intensity")]        private IncrementSetting    m_ArrowsTab_Intensity;
        [UIComponent("ArrowsTab_OverrideColors")]   private ToggleSetting       m_ArrowsTab_OverrideColors;
        [UIComponent("ArrowsTab_LColor")]           private ColorSetting        m_ArrowsTab_LColor;
        [UIComponent("ArrowsTab_RColor")]           private ColorSetting        m_ArrowsTab_RColor;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Dots Tab
        [UIObject("DotsTab")]                   private GameObject          m_DotsTab = null;
        [UIComponent("DotsTab_Scale")]          private IncrementSetting    m_DotsTab_Scale;
        [UIComponent("DotsTab_Intensity")]      private IncrementSetting    m_DotsTab_Intensity;
        [UIComponent("DotsTab_OverrideColors")] private ToggleSetting       m_DotsTab_OverrideColors;
        [UIComponent("DotsTab_LColor")]         private ColorSetting        m_DotsTab_LColor;
        [UIComponent("DotsTab_RColor")]         private ColorSetting        m_DotsTab_RColor;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Bombs Tab
        [UIObject("BombsTab")]                  private GameObject          m_BombsTab = null;
        [UIComponent("BombsTab_Scale")]         private IncrementSetting    m_BombsTab_Scale;
        [UIComponent("BombsTab_OverrideColor")] private ToggleSetting       m_BombsTab_OverrideColor;
        [UIComponent("BombsTab_Color")]         private ColorSetting        m_BombsTab_Color;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Arcs Tab
        [UIObject("ArcsTab")]                       private GameObject          m_ArcsTab = null;
        [UIComponent("ArcsTab_Intensity")]          private IncrementSetting    m_ArcsTab_Intensity;
        [UIComponent("ArcsTab_Haptics")]            private ToggleSetting       m_ArcsTab_Haptics;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region BurstNotes Tab
        [UIObject("BurstNotesTab")]             private GameObject          m_BurstNotesTab = null;
        [UIComponent("BurstNotesTab_DotScale")] private IncrementSetting    m_BurstNotesTab_DotScale;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("ImportProfileFrame")]                  private GameObject m_ImportProfileFrame = null;
        [UIObject("ImportProfileFrame_Background")]       private GameObject m_ImportProfileFrame_Background = null;
        [UIComponent("ImportProfileFrame_DropDown")]      private DropDownListSetting m_ImportProfileFrame_DropDown;
        [UIValue("ImportProfileFrame_DropDownOptions")]   private List<object> m_ImportProfileFrame_DropDownOptions = new List<object>() { "Loading...", };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("NewKeyboard")] private ModalKeyboard m_NewEventNameKeyboard = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("RenameKeyboard")] private ModalKeyboard m_RenameKeyboard = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;
        /// <summary>
        /// Current profile list page
        /// </summary>
        private int m_CurrentProfilesPage = 1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Profile = NTConfig.Instance.GetActiveProfile();

            /// Create type selector
            m_TabSelector_TabSelectorControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TabSelector.transform as RectTransform, false);
            m_TabSelector_TabSelectorControl.SetTexts(new string[] { "<b><color=yellow>Profiles</color></b>", "Notes", "Arrows", "Dots", "Bombs", "Arcs", "BurstNotes" });
            m_TabSelector_TabSelectorControl.ReloadData();
            m_TabSelector_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            foreach (var l_Text in m_TabSelector_TabSelectorControl.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
                l_Text.richText = true;

            ////////////////////////////////////////////////////////////////////////////
            /// Prepare tabs
            ////////////////////////////////////////////////////////////////////////////

            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ProfilesTab,   0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_NotesTab,      0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ArrowsTab,     0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_DotsTab,       0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_BombsTab,      0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ArcsTab,       0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_BurstNotesTab, 0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ImportProfileFrame_Background, 0.75f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_NewEventNameKeyboard.modalView,     0.75f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_RenameKeyboard.modalView,           0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            #region Profiles Tab
            /// Scale down up & down button
            m_ProfilesUpButton.transform.localScale   = Vector3.one * 0.5f;
            m_ProfilesDownButton.transform.localScale = Vector3.one * 0.5f;

            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_ProfileListFrame_Background, 0.5f);

            /// Prepare profiles list
            var l_BSMLTableView = m_ProfilesListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_ProfilesListView.GetComponentInChildren<CustomListTableData>());
            m_EventsList                    = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
            m_EventsList.TableViewInstance  = l_BSMLTableView;
            m_EventsList.CellSizeValue      = 4.8f;
            l_BSMLTableView.didSelectCellWithIdxEvent += OnProfileSelected;
            l_BSMLTableView.SetDataSource(m_EventsList, false);

            /// Bind events
            m_ProfilesUpButton.onClick.AddListener(OnProfilesPageUpPressed);
            m_ProfilesDownButton.onClick.AddListener(OnProfilesPageDownPressed);
            #endregion

            #region Notes Tab
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_NotesTab_Scale,               l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.NotesScale,               true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_NotesTab_ShowPrecisonDots,       l_Event, l_Profile.NotesShowPrecisonDots,                                                           true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_NotesTab_PrecisionDotsScale,  l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.NotesPrecisonDotsScale,   true);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_NotesTab_InfoBackground, 0.5f);
            #endregion

            #region Arrows Tab
            var l_ArrowLColor = l_Profile.ArrowsLColor.ColorWithAlpha(1.00f);
            var l_ArrowRColor = l_Profile.ArrowsRColor.ColorWithAlpha(1.00f);

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ArrowsTab_Scale,          l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.ArrowsScale,      true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ArrowsTab_Intensity,      l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.ArrowsIntensity,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ArrowsTab_OverrideColors,    l_Event, l_Profile.ArrowsOverrideColors,                                                    true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ArrowsTab_LColor,             l_Event, l_ArrowLColor,                                                                     true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ArrowsTab_RColor,             l_Event, l_ArrowRColor,                                                                     true);
            #endregion

            #region Dots Tab
            var l_DotLColor = l_Profile.DotsLColor.ColorWithAlpha(1.00f);
            var l_DotRColor = l_Profile.DotsRColor.ColorWithAlpha(1.00f);

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_DotsTab_Scale,        l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.DotsScale,        true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_DotsTab_Intensity,    l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.DotsIntensity,    true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_DotsTab_OverrideColors,  l_Event, l_Profile.DotsOverrideColors,                                                      true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_DotsTab_LColor,           l_Event, l_DotLColor,                                                                       true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_DotsTab_RColor,           l_Event, l_DotRColor,                                                                       true);
            #endregion

            #region Bombs Tab
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_BombsTab_Scale,       l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   l_Profile.BombsScale,           true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_BombsTab_OverrideColor,  l_Event,                                                          l_Profile.BombsOverrideColor,   true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_BombsTab_Color,           l_Event,                                                          l_Profile.BombsColor,           true);
            #endregion

            #region Arcs Tab
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ArcsTab_Intensity,        l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,   l_Profile.ArcsIntensity,  true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ArcsTab_Haptics,             l_Event,                                                          l_Profile.ArcsHaptics,    true);
            #endregion

            #region BurstNotes Tab
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_BurstNotesTab_DotScale, l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, l_Profile.BurstNotesDotsScale, true);
            #endregion

            #region Import Frame
            /// Remove import event type selector label
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_ImportProfileFrame_DropDown, null, true, 0.95f);
            #endregion

            /// Hide import frame
            m_ImportProfileFrame.gameObject.SetActive(false);
            /// Show first tab by default
            OnTabSelected(null, 0);
            /// Rebuild profiles
            RebuildProfilesList();
            /// Refresh UI
            OnSettingChanged(null);
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
        /// Go to previous profiles page
        /// </summary>
        private void OnProfilesPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentProfilesPage < 2)
                return;

            /// Decrement current page
            m_CurrentProfilesPage--;

            /// Rebuild list
            RebuildProfilesList();
        }
        /// <summary>
        /// Rebuild list
        /// </summary>
        private void RebuildProfilesList()
        {
            if (!UICreated)
                return;

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(NTConfig.Instance.Profiles.Count) / (float)(EVENT_PER_PAGE)));

            /// Update overflow
            m_CurrentProfilesPage = Math.Max(1, Math.Min(m_CurrentProfilesPage, l_PageCount));

            /// Update UI
            m_ProfilesUpButton.interactable = m_CurrentProfilesPage > 1;
            m_ProfilesDownButton.interactable = m_CurrentProfilesPage < l_PageCount;

            /// Clear old entries
            m_EventsList.TableViewInstance.ClearSelection();
            m_EventsList.Data.Clear();

            for (int l_I = (m_CurrentProfilesPage - 1) * EVENT_PER_PAGE;
                l_I < NTConfig.Instance.Profiles.Count && l_I < (m_CurrentProfilesPage * EVENT_PER_PAGE);
                ++l_I)
            {
                var l_ProfileName = NTConfig.Instance.Profiles[l_I].Name;

                if (l_I == NTConfig.Instance.ActiveProfile)
                    l_ProfileName = "<b><color=yellow>" + l_ProfileName + "</color></b>";

                m_EventsList.Data.Add((l_ProfileName, null));
            }

            /// Refresh
            m_EventsList.TableViewInstance.ReloadData();
        }
        /// <summary>
        /// On profile selected
        /// </summary>
        /// <param name="p_TableView">TableView instance</param>
        /// <param name="p_RelIndex">Relative index</param>
        private void OnProfileSelected(HMUI.TableView p_TableView, int p_RelIndex)
        {
            int l_ProfileIndex = ((m_CurrentProfilesPage - 1) * EVENT_PER_PAGE) + p_RelIndex;

            if (p_RelIndex < 0 || l_ProfileIndex >= NTConfig.Instance.Profiles.Count)
                return;

            NTConfig.Instance.ActiveProfile = l_ProfileIndex;
            RefreshSettings();
            RebuildProfilesList();
        }
        /// <summary>
        /// Go to next profiles page
        /// </summary>
        private void OnProfilesPageDownPressed()
        {
            /// Increment current page
            m_CurrentProfilesPage++;

            /// Rebuild list
            RebuildProfilesList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New profile button
        /// </summary>
        [UIAction("click-new-btn-pressed")]
        private void OnNewButton()
        {
            ShowModal("OpenNewModal");
        }
        /// <summary>
        /// On new profile name keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("NewKeyboardPressed")]
        internal void NewEventNameKeyboardPressed(string p_Text)
        {
            var l_ProfileName   = string.IsNullOrEmpty(p_Text) ? "No name..." : p_Text;
            var l_NewProfile    = new NTConfig._Profile() { Name = l_ProfileName };

            NTConfig.Instance.Profiles.Add(l_NewProfile);
            NTConfig.Instance.ActiveProfile = NTConfig.Instance.Profiles.IndexOf(l_NewProfile);

            RebuildProfilesList();
            RefreshSettings();
        }
        /// <summary>
        /// Rename profile button
        /// </summary>
        [UIAction("click-rename-btn-pressed")]
        private void OnRenameButton()
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault())
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                return;
            }

            m_RenameKeyboard.SetText(NTConfig.Instance.GetActiveProfile().Name);
            ShowModal("OpenRenameModal");
        }
        /// <summary>
        /// On rename keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("RenameKeyboardPressed")]
        internal void RenameKeyboardPressed(string p_Text)
        {
            NTConfig.Instance.GetActiveProfile().Name = string.IsNullOrEmpty(p_Text) ? "No name..." : p_Text;
            RebuildProfilesList();
        }
        /// <summary>
        /// Delete profile button
        /// </summary>
        [UIAction("click-delete-btn-pressed")]
        private void OnDeleteButton()
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault())
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                return;
            }

            ShowConfirmationModal($"<color=red>Do you want to delete profile</color>\n\"{NTConfig.Instance.GetActiveProfile().Name}\"?", () =>
            {
                NTConfig.Instance.Profiles.Remove(NTConfig.Instance.GetActiveProfile());
                NTConfig.Instance.ActiveProfile = Mathf.Clamp(NTConfig.Instance.ActiveProfile - 1, 0, NTConfig.Instance.Profiles.Count);
                RebuildProfilesList();
                RefreshSettings();
            });
        }
        /// <summary>
        /// Export an profile
        /// </summary>
        [UIAction("click-export-btn-pressed")]
        private void OnExportButton()
        {
            var l_Profile = NTConfig.Instance.GetActiveProfile();

            if (l_Profile.IsDefault())
            {
                ShowMessageModal("<color=yellow>Cannot export default config!</color>");
                return;
            }

            var l_Serialized = JObject.FromObject(l_Profile);

            var l_FileName = CP_SDK.Misc.Time.UnixTimeNow() + "_" + l_Profile.Name + ".bspnt";
            l_FileName = string.Concat(l_FileName.Split(System.IO.Path.GetInvalidFileNameChars()));

            System.IO.File.WriteAllText(NoteTweaker.EXPORT_FOLDER + l_FileName, l_Serialized.ToString(Formatting.Indented), System.Text.Encoding.Unicode);

            ShowMessageModal("Event exported in\n" + NoteTweaker.EXPORT_FOLDER);
        }
        /// <summary>
        /// Import an profile
        /// </summary>
        [UIAction("click-import-btn-pressed")]
        private void OnImportButton()
        {
            m_Tabs.gameObject.SetActive(false);
            m_ImportProfileFrame.gameObject.SetActive(true);

            var l_Files = new List<object>();
            foreach (var l_File in System.IO.Directory.GetFiles(NoteTweaker.IMPORT_FOLDER, "*.bspnt"))
                l_Files.Add(System.IO.Path.GetFileNameWithoutExtension(l_File));

            m_ImportProfileFrame_DropDownOptions = l_Files;
            m_ImportProfileFrame_DropDown.values = l_Files;
            m_ImportProfileFrame_DropDown.UpdateChoices();
        }
        /// <summary>
        /// Import profile cancel button
        /// </summary>
        [UIAction("click-cancel-import-profile-btn-pressed")]
        private void OnImportProfileCancelButton()
        {
            m_ImportProfileFrame.gameObject.SetActive(false);
            m_Tabs.gameObject.SetActive(true);
        }
        /// <summary>
        /// Import profile button
        /// </summary>
        [UIAction("click-import-profile-btn-pressed")]
        private void OnImportProfileButton()
        {
            m_ImportProfileFrame.gameObject.SetActive(false);
            m_Tabs.gameObject.SetActive(true);

            var l_FileName = NoteTweaker.IMPORT_FOLDER + (string)m_ImportProfileFrame_DropDown.Value + ".bspnt";

            if (System.IO.File.Exists(l_FileName))
            {
                var l_Raw = System.IO.File.ReadAllText(l_FileName, System.Text.Encoding.Unicode);

                try
                {
                    var l_NewProfile = JsonConvert.DeserializeObject<NTConfig._Profile>(l_Raw);
                    l_NewProfile.Name += " (Imported)";

                    if (l_NewProfile != null)
                    {
                        NTConfig.Instance.Profiles.Add(l_NewProfile);
                        RebuildProfilesList();
                    }
                    else
                        ShowMessageModal("Error importing profile!");
                }
                catch
                {
                    ShowMessageModal("Invalid file!");
                }
            }
            else
                ShowMessageModal("File not found!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a tab is selected
        /// </summary>
        /// <param name="p_SegmentControl">Tab control instance</param>
        /// <param name="p_TabIndex">Tab index</param>
        private void OnTabSelected(SegmentedControl p_SegmentControl, int p_TabIndex)
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault() && p_TabIndex > 0)
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                p_SegmentControl.SelectCellWithNumber(0);
                p_TabIndex = 0;
            }

            m_ProfilesTab.SetActive(p_TabIndex == 0);
            m_NotesTab.SetActive(p_TabIndex == 1);
            m_ArrowsTab.SetActive(p_TabIndex == 2);
            m_DotsTab.SetActive(p_TabIndex == 3);
            m_BombsTab.SetActive(p_TabIndex == 4);
            m_ArcsTab.SetActive(p_TabIndex == 5);
            m_BurstNotesTab.SetActive(p_TabIndex == 6);
        }
        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            var l_Profile = NTConfig.Instance.GetActiveProfile();

            #region Notes Tab
            l_Profile.NotesScale                = m_NotesTab_Scale.Value;
            l_Profile.NotesShowPrecisonDots     = m_NotesTab_ShowPrecisonDots.Value;
            l_Profile.NotesPrecisonDotsScale    = m_NotesTab_PrecisionDotsScale.Value;
            #endregion

            #region Arrows Tab
            l_Profile.ArrowsScale           = m_ArrowsTab_Scale.Value;
            l_Profile.ArrowsIntensity       = m_ArrowsTab_Intensity.Value;
            l_Profile.ArrowsOverrideColors  = m_ArrowsTab_OverrideColors.Value;
            l_Profile.ArrowsLColor          = m_ArrowsTab_LColor.CurrentColor.ColorWithAlpha(1.0f);
            l_Profile.ArrowsRColor          = m_ArrowsTab_RColor.CurrentColor.ColorWithAlpha(1.0f);

            m_ArrowsTab_LColor.interactable = l_Profile.ArrowsOverrideColors;
            m_ArrowsTab_RColor.interactable = l_Profile.ArrowsOverrideColors;
            #endregion

            #region Dots Tab
            l_Profile.DotsScale             = m_DotsTab_Scale.Value;
            l_Profile.DotsIntensity         = m_DotsTab_Intensity.Value;
            l_Profile.DotsOverrideColors    = m_DotsTab_OverrideColors.Value;
            l_Profile.DotsLColor            = m_DotsTab_LColor.CurrentColor.ColorWithAlpha(1.0f);
            l_Profile.DotsRColor            = m_DotsTab_RColor.CurrentColor.ColorWithAlpha(1.0f);

            m_DotsTab_LColor.interactable = l_Profile.DotsOverrideColors;
            m_DotsTab_RColor.interactable = l_Profile.DotsOverrideColors;
            #endregion

            #region Bombs Tab
            l_Profile.BombsScale         = m_BombsTab_Scale.Value;
            l_Profile.BombsOverrideColor = m_BombsTab_OverrideColor.Value;
            l_Profile.BombsColor         = m_BombsTab_Color.CurrentColor.ColorWithAlpha(1.0f);

            m_BombsTab_Color.interactable = l_Profile.BombsOverrideColor;
            #endregion

            #region Arcs Tab
            l_Profile.ArcsIntensity = m_ArcsTab_Intensity.Value;
            l_Profile.ArcsHaptics   = m_ArcsTab_Haptics.Value;
            #endregion

            #region BurstNotes Tab
            l_Profile.BurstNotesDotsScale = m_BurstNotesTab_DotScale.Value;
            #endregion

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
            var l_Profile = NTConfig.Instance.GetActiveProfile();

            m_PreventChanges = true;

            #region Notes Tab
            m_NotesTab_Scale.Value              = l_Profile.NotesScale;
            m_NotesTab_ShowPrecisonDots.Value   = l_Profile.NotesShowPrecisonDots;
            m_NotesTab_PrecisionDotsScale.Value = l_Profile.NotesPrecisonDotsScale;
            #endregion

            #region Arrows Tab
            m_ArrowsTab_Scale.Value             = l_Profile.ArrowsScale;
            m_ArrowsTab_Intensity.Value         = l_Profile.ArrowsIntensity;
            m_ArrowsTab_OverrideColors.Value    = l_Profile.ArrowsOverrideColors;
            m_ArrowsTab_LColor.CurrentColor     = l_Profile.ArrowsLColor.ColorWithAlpha(1.00f);
            m_ArrowsTab_RColor.CurrentColor     = l_Profile.ArrowsRColor.ColorWithAlpha(1.00f);

            m_ArrowsTab_LColor.interactable = l_Profile.ArrowsOverrideColors;
            m_ArrowsTab_RColor.interactable = l_Profile.ArrowsOverrideColors;
            #endregion

            #region Dots Tab
            m_DotsTab_Scale.Value           = l_Profile.DotsScale;
            m_DotsTab_Intensity.Value       = l_Profile.DotsIntensity;
            m_DotsTab_OverrideColors.Value  = l_Profile.DotsOverrideColors;
            m_DotsTab_LColor.CurrentColor   = l_Profile.DotsLColor.ColorWithAlpha(1.00f);
            m_DotsTab_RColor.CurrentColor   = l_Profile.DotsRColor.ColorWithAlpha(1.00f);

            m_DotsTab_LColor.interactable = l_Profile.DotsOverrideColors;
            m_DotsTab_RColor.interactable = l_Profile.DotsOverrideColors;
            #endregion

            #region Bombs Tab
            m_BombsTab_Scale.Value          = l_Profile.BombsScale;
            m_BombsTab_OverrideColor.Value  = l_Profile.BombsOverrideColor;
            m_BombsTab_Color.CurrentColor   = l_Profile.BombsColor;

            m_BombsTab_Color.interactable   = l_Profile.BombsOverrideColor;
            #endregion

            #region Arcs Tab
            m_ArcsTab_Intensity.Value   = l_Profile.ArcsIntensity;
            m_ArcsTab_Haptics.Value     = l_Profile.ArcsHaptics;
            #endregion

            #region BurstNotes Tab
            m_BurstNotesTab_DotScale.Value = l_Profile.BurstNotesDotsScale;
            #endregion

            m_PreventChanges = false;

            SettingsRight.Instance.RefreshSettings();
        }
    }
}
