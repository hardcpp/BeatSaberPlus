using CP_SDK.UI.Data;
using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_NoteTweaker.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUITabControl       m_TabControl        = null;
        private XUIVVList           m_ProfilesTab_List  = null;

        private XUISlider           m_NotesTab_Scale                = null;
        private XUIToggle           m_NotesTab_ShowPrecisonDots     = null;
        private XUISlider           m_NotesTab_PrecisionDotsScale   = null;

        private XUISlider           m_ArrowsTab_Scale           = null;
        private XUISlider           m_ArrowsTab_Intensity       = null;
        private XUIToggle           m_ArrowsTab_OverrideColors  = null;
        private XUIColorInput       m_ArrowsTab_LColor          = null;
        private XUIColorInput       m_ArrowsTab_RColor          = null;

        private XUISlider           m_DotsTab_Scale             = null;
        private XUISlider           m_DotsTab_Intensity         = null;
        private XUIToggle           m_DotsTab_OverrideColors    = null;
        private XUIColorInput       m_DotsTab_LColor            = null;
        private XUIColorInput       m_DotsTab_RColor            = null;

        private XUISlider           m_BombsTab_Scale            = null;
        private XUIToggle           m_BombsTab_OverrideColor    = null;
        private XUIColorInput       m_BombsTab_Color            = null;

        private XUISlider           m_ArcsTab_Intensity = null;
        private XUIToggle           m_ArcsTab_Haptics   = null;

        private XUISlider           m_BurstNotesTab_DotScale = null;

        private Modals.ProfileImportModal   m_ProfileImportModal = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<TextListItem>  m_Items             = new List<TextListItem>();
        private TextListItem        m_SelectedItem      = null;
        private bool                m_PreventChanges    = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Note Tweaker | Settings"),

                XUITabControl.Make(
                    ("<b><color=yellow>Profiles</color></b>",   BuildProfilesTab()),
                    ("Notes",                                   BuildNotesTab()),
                    ("Arrows",                                  BuildArrowsTab()),
                    ("Dots",                                    BuildDotsTab()),
                    ("Bombs",                                   BuildBombsTab()),
                    ("Arcs",                                    BuildArcsTab()),
                    ("BurstNotes",                              BuildBurstNotesTab())
                )
                .OnActiveChanged(OnTabSelected)
                .Bind(ref m_TabControl)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            m_ProfileImportModal = CreateModal<Modals.ProfileImportModal>();

            ProfilesTab_Refresh();
            OnSettingChanged();
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
        /// Build profiles tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildProfilesTab()
        {
            return XUIVLayout.Make(
                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                        .OnListItemSelected(ProfilesTab_OnListItemSelect)
                        .Bind(ref m_ProfilesTab_List)
                )
                .SetHeight(50)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("New").OnClick(ProfilesTab_OnNewButton),
                    XUIPrimaryButton.Make("Rename").OnClick(ProfilesTab_OnRenameButton),
                    XUIPrimaryButton.Make("Delete").OnClick(ProfilesTab_OnDeleteButton),
                    XUISecondaryButton.Make("Export").OnClick(ProfilesTab_OnExportButton),
                    XUISecondaryButton.Make("Import").OnClick(ProfilesTab_OnImportButton)
                )
            ).OnReady(x => x.CSizeFitter.enabled = false);
        }
        /// <summary>
        /// Build notes tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildNotesTab()
        {
            return XUIVLayout.Make(
                XUIVLayout.Make(
                    XUIText.Make("Note scale"),
                    XUISlider.Make()
                        .SetMinValue(0.4f).SetMaxValue(1.2f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_NotesTab_Scale),

                    XUIText.Make("Show dot on directional notes"),
                    XUIToggle.Make()
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_NotesTab_ShowPrecisonDots),

                    XUIText.Make("Precision dot scale"),
                    XUISlider.Make()
                        .SetMinValue(0.2f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_NotesTab_PrecisionDotsScale)
                )
                .SetWidth(80.0f)
                .SetPadding(0),

                XUIVLayout.Make(
                    XUIText.Make("This module change only the visual appearance of the notes, the hitbox will stay the same as default"),
                    XUIText.Make("The scale settings can conflict with CustomNotes if not 100%")
                )
                .SetBackground(true)
            );
        }
        /// <summary>
        /// Build arrows tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildArrowsTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Arrow scale"),
                XUISlider.Make()
                    .SetMinValue(0.2f).SetMaxValue(1.4f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_ArrowsTab_Scale),

                XUIText.Make("Arrow glow intensity"),
                XUISlider.Make()
                    .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_ArrowsTab_Intensity),

                XUIText.Make("Override arrow colors"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_ArrowsTab_OverrideColors),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Arrow left color"),
                        XUIColorInput.Make()
                            .OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_ArrowsTab_LColor)
                    ),
                    XUIVLayout.Make(
                        XUIText.Make("Arrow right color"),
                        XUIColorInput.Make()
                            .OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_ArrowsTab_RColor)
                    )
                )
                .SetWidth(80.0f)
                .SetPadding(0)
            );
        }
        /// <summary>
        /// Build dots tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildDotsTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Dot scale"),
                XUISlider.Make()
                    .SetMinValue(0.2f).SetMaxValue(1.5f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_DotsTab_Scale),

                XUIText.Make("Dot glow intensity"),
                XUISlider.Make()
                    .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_DotsTab_Intensity),

                XUIText.Make("Override dot colors"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_DotsTab_OverrideColors),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Dot left color"),
                        XUIColorInput.Make()
                            .OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_DotsTab_LColor)
                    ),
                    XUIVLayout.Make(
                        XUIText.Make("Dot right color"),
                        XUIColorInput.Make()
                            .OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_DotsTab_RColor)
                    )
                )
                .SetWidth(80.0f)
                .SetPadding(0)
            );
        }
        /// <summary>
        /// Build bombs tabs
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildBombsTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Bomb scale"),
                XUISlider.Make()
                    .SetMinValue(0.4f).SetMaxValue(1.2f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_BombsTab_Scale),

                XUIText.Make("Override bomb color"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_BombsTab_OverrideColor),

                XUIText.Make("Bomb color"),
                XUIColorInput.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_BombsTab_Color)
            )
            .SetWidth(80.0f);
        }
        /// <summary>
        /// Build arcs tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildArcsTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Arcs intensity"),
                XUISlider.Make()
                    .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_ArcsTab_Intensity),

                XUIText.Make("Arcs haptics"),
                XUIToggle.Make()
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_ArcsTab_Haptics)
            )
            .SetWidth(80.0f);
        }
        /// <summary>
        /// Build burst notes tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildBurstNotesTab()
        {
            return XUIVLayout.Make(
                XUIText.Make("Dot size"),
                XUISlider.Make()
                    .SetMinValue(0.0f).SetMaxValue(3.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                    .OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_BurstNotesTab_DotScale)
            )
            .SetWidth(80.0f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a tab is selected
        /// </summary>
        /// <param name="p_TabIndex">Tab index</param>
        private void OnTabSelected(int p_TabIndex)
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault() && p_TabIndex > 0)
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                m_TabControl.SetActiveTab(0);
            }
        }
        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged()
        {
            if (m_PreventChanges)
                return;

            var l_Profile = NTConfig.Instance.GetActiveProfile();

            #region Notes Tab
            l_Profile.NotesScale                = m_NotesTab_Scale.Element.GetValue();
            l_Profile.NotesShowPrecisonDots     = m_NotesTab_ShowPrecisonDots.Element.GetValue();
            l_Profile.NotesPrecisonDotsScale    = m_NotesTab_PrecisionDotsScale.Element.GetValue();
            #endregion

            #region Arrows Tab
            l_Profile.ArrowsScale           = m_ArrowsTab_Scale.Element.GetValue();
            l_Profile.ArrowsIntensity       = m_ArrowsTab_Intensity.Element.GetValue();
            l_Profile.ArrowsOverrideColors  = m_ArrowsTab_OverrideColors.Element.GetValue();
            l_Profile.ArrowsLColor          = m_ArrowsTab_LColor.Element.GetValue();
            l_Profile.ArrowsRColor          = m_ArrowsTab_RColor.Element.GetValue();

            m_ArrowsTab_LColor.SetInteractable(l_Profile.ArrowsOverrideColors);
            m_ArrowsTab_RColor.SetInteractable(l_Profile.ArrowsOverrideColors);
            #endregion

            #region Dots Tab
            l_Profile.DotsScale             = m_DotsTab_Scale.Element.GetValue();
            l_Profile.DotsIntensity         = m_DotsTab_Intensity.Element.GetValue();
            l_Profile.DotsOverrideColors    = m_DotsTab_OverrideColors.Element.GetValue();
            l_Profile.DotsLColor            = m_DotsTab_LColor.Element.GetValue();
            l_Profile.DotsRColor            = m_DotsTab_RColor.Element.GetValue();

            m_DotsTab_LColor.SetInteractable(l_Profile.DotsOverrideColors);
            m_DotsTab_RColor.SetInteractable(l_Profile.DotsOverrideColors);
            #endregion

            #region Bombs Tab
            l_Profile.BombsScale         = m_BombsTab_Scale.Element.GetValue();
            l_Profile.BombsOverrideColor = m_BombsTab_OverrideColor.Element.GetValue();
            l_Profile.BombsColor         = m_BombsTab_Color.Element.GetValue();

            m_BombsTab_Color.SetInteractable(l_Profile.BombsOverrideColor);
            #endregion

            #region Arcs Tab
            l_Profile.ArcsIntensity = m_ArcsTab_Intensity.Element.GetValue();
            l_Profile.ArcsHaptics   = m_ArcsTab_Haptics.Element.GetValue();
            #endregion

            #region BurstNotes Tab
            l_Profile.BurstNotesDotsScale = m_BurstNotesTab_DotScale.Element.GetValue();
            #endregion

            /// Refresh preview
            SettingsRightView.Instance.RefreshSettings();
        }
        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            var l_Profile = NTConfig.Instance.GetActiveProfile();

            m_PreventChanges = true;

            #region Notes Tab
            m_NotesTab_Scale                .SetValue(l_Profile.NotesScale);
            m_NotesTab_ShowPrecisonDots     .SetValue(l_Profile.NotesShowPrecisonDots);
            m_NotesTab_PrecisionDotsScale   .SetValue(l_Profile.NotesPrecisonDotsScale);
            #endregion

            #region Arrows Tab
            m_ArrowsTab_Scale           .SetValue(l_Profile.ArrowsScale);
            m_ArrowsTab_Intensity       .SetValue(l_Profile.ArrowsIntensity);
            m_ArrowsTab_OverrideColors  .SetValue(l_Profile.ArrowsOverrideColors);
            m_ArrowsTab_LColor          .SetValue(ColorU.WithAlpha(l_Profile.ArrowsLColor, 1.00f));
            m_ArrowsTab_RColor          .SetValue(ColorU.WithAlpha(l_Profile.ArrowsRColor, 1.00f));

            m_ArrowsTab_LColor.SetInteractable(l_Profile.ArrowsOverrideColors);
            m_ArrowsTab_RColor.SetInteractable(l_Profile.ArrowsOverrideColors);
            #endregion

            #region Dots Tab
            m_DotsTab_Scale         .SetValue(l_Profile.DotsScale);
            m_DotsTab_Intensity     .SetValue(l_Profile.DotsIntensity);
            m_DotsTab_OverrideColors.SetValue(l_Profile.DotsOverrideColors);
            m_DotsTab_LColor        .SetValue(ColorU.WithAlpha(l_Profile.DotsLColor, 1.00f));
            m_DotsTab_RColor        .SetValue(ColorU.WithAlpha(l_Profile.DotsRColor, 1.00f));

            m_DotsTab_LColor.SetInteractable(l_Profile.DotsOverrideColors);
            m_DotsTab_RColor.SetInteractable(l_Profile.DotsOverrideColors);
            #endregion

            #region Bombs Tab
            m_BombsTab_Scale        .SetValue(l_Profile.BombsScale);
            m_BombsTab_OverrideColor.SetValue(l_Profile.BombsOverrideColor);
            m_BombsTab_Color        .SetValue(ColorU.WithAlpha(l_Profile.BombsColor, 1.00f));

            m_BombsTab_Color.SetInteractable(l_Profile.BombsOverrideColor);
            #endregion

            #region Arcs Tab
            m_ArcsTab_Intensity .SetValue(l_Profile.ArcsIntensity);
            m_ArcsTab_Haptics   .SetValue(l_Profile.ArcsHaptics);
            #endregion

            #region BurstNotes Tab
            m_BurstNotesTab_DotScale.SetValue(l_Profile.BurstNotesDotsScale);
            #endregion

            m_PreventChanges = false;

            SettingsRightView.Instance.RefreshSettings();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh list
        /// </summary>
        private void ProfilesTab_Refresh()
        {
            m_Items.Clear();
            for (var l_I = 0; l_I < NTConfig.Instance.Profiles.Count; ++l_I)
                m_Items.Add(new TextListItem(NTConfig.Instance.Profiles[l_I].Name, null, TMPro.TextAlignmentOptions.CaplineGeoAligned));

            m_ProfilesTab_List.SetListItems(m_Items);
            m_ProfilesTab_List.SetSelectedListItem(m_Items[NTConfig.Instance.ActiveProfile]);
        }
        /// <summary>
        /// On item selected
        /// </summary>
        /// <param name="p_ListItem">Selected item</param>
        private void ProfilesTab_OnListItemSelect(IListItem p_ListItem)
        {
            m_SelectedItem = (TextListItem)p_ListItem;

            if (m_SelectedItem != null)
                NTConfig.Instance.ActiveProfile = NTConfig.Instance.Profiles.IndexOf(NTConfig.Instance.Profiles.FirstOrDefault(x => x.Name == m_SelectedItem.Text));

            RefreshSettings();
        }
        /// <summary>
        /// New profile button
        /// </summary>
        private void ProfilesTab_OnNewButton()
        {
            ShowKeyboardModal("", (p_Name) =>
            {
                var l_ProfileName = string.IsNullOrEmpty(p_Name) ? "No name..." : p_Name;
                var l_NewProfile  = new NTConfig._Profile() { Name = l_ProfileName };

                NTConfig.Instance.Profiles.Add(l_NewProfile);
                NTConfig.Instance.ActiveProfile = NTConfig.Instance.Profiles.IndexOf(l_NewProfile);

                ProfilesTab_Refresh();
                RefreshSettings();
            });
        }
        /// <summary>
        /// Rename profile button
        /// </summary>
        private void ProfilesTab_OnRenameButton()
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault())
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                return;
            }

            ShowKeyboardModal(NTConfig.Instance.GetActiveProfile().Name, (p_NewName) =>
            {
                NTConfig.Instance.GetActiveProfile().Name = string.IsNullOrEmpty(p_NewName) ? "No name..." : p_NewName;
                RefreshSettings();
            });
        }
        /// <summary>
        /// Delete profile button
        /// </summary>
        private void ProfilesTab_OnDeleteButton()
        {
            if (NTConfig.Instance.GetActiveProfile().IsDefault())
            {
                ShowMessageModal("<color=yellow>No changes allowed on default config!</color>");
                return;
            }

            ShowConfirmationModal($"<color=red>Do you want to delete profile</color>\n\"{NTConfig.Instance.GetActiveProfile().Name}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                NTConfig.Instance.Profiles.Remove(NTConfig.Instance.GetActiveProfile());
                NTConfig.Instance.ActiveProfile = Mathf.Clamp(NTConfig.Instance.ActiveProfile - 1, 0, NTConfig.Instance.Profiles.Count);
                ProfilesTab_Refresh();
                RefreshSettings();
            });
        }
        /// <summary>
        /// Export an profile
        /// </summary>
        private void ProfilesTab_OnExportButton()
        {
            var l_Profile = NTConfig.Instance.GetActiveProfile();
            if (l_Profile.IsDefault())
            {
                ShowMessageModal("<color=yellow>Cannot export default config!</color>");
                return;
            }

            var l_Serialized = JsonConvert.SerializeObject(l_Profile, Formatting.Indented, new JsonConverter[]
            {
                new CP_SDK.Config.JsonConverters.ColorConverter()
            });

            var l_FileName = CP_SDK.Misc.Time.UnixTimeNow() + "_" + l_Profile.Name + ".bspnt";
            l_FileName = string.Concat(l_FileName.Split(System.IO.Path.GetInvalidFileNameChars()));

            System.IO.File.WriteAllText(NoteTweaker.EXPORT_FOLDER + l_FileName, l_Serialized, System.Text.Encoding.Unicode);

            ShowMessageModal("Event exported in\n" + NoteTweaker.EXPORT_FOLDER);
        }
        /// <summary>
        /// Import an profile
        /// </summary>
        private void ProfilesTab_OnImportButton()
        {
            ShowModal(m_ProfileImportModal);
            m_ProfileImportModal.Init(() => ProfilesTab_Refresh());
        }
    }
}
