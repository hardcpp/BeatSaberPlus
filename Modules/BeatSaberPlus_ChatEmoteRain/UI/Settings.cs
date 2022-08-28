using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using UnityEngine;
using UnityEngine.UI;

using EmitterConfig = CP_SDK.Unity.Components.EnhancedImageParticleEmitter.EmitterConfig;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    /// <summary>
    /// Chat Emote Rain settings main view
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
        private static int s_EMITTER_PER_PAGE = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("TabSelector")]
        private GameObject m_TabSelector;
        private TextSegmentedControl m_TabSelector_TabSelectorControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("GeneralTab")]
        private GameObject m_GeneralTab = null;
        [UIComponent("GeneralTab_MenuRainToggle")]
        public ToggleSetting m_GeneralTab_MenuRain;
        [UIComponent("GeneralTab_MenuRainSizeSlider")]
        public SliderSetting m_GeneralTab_MenuRainSizeSlider;
        [UIComponent("GeneralTab_MenuFallSpeedSlider")]
        public SliderSetting m_GeneralTab_MenuFallSpeedSlider;
        [UIComponent("GeneralTab_SongRainToggle")]
        public ToggleSetting m_GeneralTab_SongRain;
        [UIComponent("GeneralTab_SongRainSizeSlider")]
        public SliderSetting m_GeneralTab_SongRainSizeSlider;
        [UIComponent("GeneralTab_SongFallSpeedSlider")]
        public SliderSetting m_GeneralTab_SongFallSpeedSlider;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("MenuEmittersTab")]
        private GameObject m_MenuEmittersTab = null;

        [UIComponent("MenuEmittersTab_UpButton")]
        private Button m_MenuEmittersTab_UpButton = null;
        [UIObject("MenuEmittersTab_List")]
        private GameObject m_MenuEmittersTab_ListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_MenuEmittersTab_List = null;
        [UIComponent("MenuEmittersTab_DownButton")]
        private Button m_MenuEmittersTab_DownButton = null;

        [UIObject("MenuEmittersTab_Content")]
        private GameObject m_MenuEmittersTab_Content = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("SongEmittersTab")]
        private GameObject m_SongEmittersTab = null;

        [UIComponent("SongEmittersTab_UpButton")]
        private Button m_SongEmittersTab_UpButton = null;
        [UIObject("SongEmittersTab_List")]
        private GameObject m_SongEmittersTab_ListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_SongEmittersTab_List = null;
        [UIComponent("SongEmittersTab_DownButton")]
        private Button m_SongEmittersTab_DownButton = null;

        [UIObject("SongEmittersTab_Content")]
        private GameObject m_SongEmittersTab_Content = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("CommandsTab")]
        private GameObject m_CommandsTab = null;
        [UIComponent("CommandsTab_ModeratorPowerToggle")]
        public ToggleSetting m_CommandsTab_ModeratorPowerToggle;
        [UIComponent("CommandsTab_VIPPowerToggle")]
        public ToggleSetting m_CommandsTab_VIPPowerToggle;
        [UIComponent("CommandsTab_SubscriberPowerToggle")]
        public ToggleSetting m_CommandsTab_SubscriberPowerToggle;
        [UIComponent("CommandsTab_UserPowerToggle")]
        public ToggleSetting m_CommandsTab_UserPowerToggle;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;
        /// <summary>
        /// Menu emitters current page
        /// </summary>
        private int m_MenuEmittersCurrentPage = 1;
        /// <summary>
        /// Song current selected emitter index
        /// </summary>
        private int m_MenuEmittersSelected = -1;
        /// <summary>
        /// Song emitters current page
        /// </summary>
        private int m_SongEmittersCurrentPage = 1;
        /// <summary>
        /// Menu current selected emitter index
        /// </summary>
        private int m_SongEmittersSelected = -1;
        /// <summary>
        /// Emitter widget
        /// </summary>
        private EmitterWidget m_EmitterWidget = new EmitterWidget();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_GeneralTab,              0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_MenuEmittersTab,         0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_MenuEmittersTab_Content, 0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_SongEmittersTab,         0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_SongEmittersTab_Content, 0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_CommandsTab,             0.50f);

            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Create type selector
            m_TabSelector_TabSelectorControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_TabSelector.transform as RectTransform, false);
            m_TabSelector_TabSelectorControl.SetTexts(new string[] { "General", "Menu Emitters", "Song Emitters", "Chat Commands" });
            m_TabSelector_TabSelectorControl.ReloadData();
            m_TabSelector_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            /// General tab setup
            if (true)
            {
                var l_AnchorMin = new Vector2(0.18f, -0.05f);
                var l_AnchorMax = new Vector2(0.86f, 1.05f);

                /// First row
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_GeneralTab_MenuRain,              l_Event,         CERConfig.Instance.EnableMenu,    true);
                BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_GeneralTab_MenuRainSizeSlider,    l_Event, null,   CERConfig.Instance.MenuSize,      true, true, l_AnchorMin, l_AnchorMax);
                BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_GeneralTab_MenuFallSpeedSlider,   l_Event, null,   CERConfig.Instance.MenuSpeed,     true, true, l_AnchorMin, l_AnchorMax);

                /// Second row
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_GeneralTab_SongRain,              l_Event,         CERConfig.Instance.EnableSong,   true);
                BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_GeneralTab_SongRainSizeSlider,    l_Event, null,   CERConfig.Instance.SongSize,     true, true, l_AnchorMin, l_AnchorMax);
                BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_GeneralTab_SongFallSpeedSlider,   l_Event, null,   CERConfig.Instance.SongSpeed,    true, true, l_AnchorMin, l_AnchorMax);
            }

            /// Menu emitters
            if (true)
            {
                /// Scale down up & down button
                m_MenuEmittersTab_UpButton.transform.localScale     = Vector3.one * 0.6f;
                m_MenuEmittersTab_DownButton.transform.localScale   = Vector3.one * 0.6f;

                var l_LayoutElement = m_MenuEmittersTab_ListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_MenuEmittersTab_ListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_MenuEmittersTab_ListView.GetComponentInChildren<CustomListTableData>());
                m_MenuEmittersTab_List = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_MenuEmittersTab_List.TableViewInstance        = l_BSMLTableView;
                m_MenuEmittersTab_List.CellSizeValue            = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent      += OnEmitterSelected;
                l_BSMLTableView.SetDataSource(m_MenuEmittersTab_List, false);

                /// Bind events
                m_MenuEmittersTab_UpButton.onClick.AddListener(OnEmitterPageUpPressed);
                m_MenuEmittersTab_DownButton.onClick.AddListener(OnEmitterPageDownPressed);
            }

            /// Song emitters
            if (true)
            {
                /// Scale down up & down button
                m_SongEmittersTab_UpButton.transform.localScale     = Vector3.one * 0.6f;
                m_SongEmittersTab_DownButton.transform.localScale   = Vector3.one * 0.6f;

                var l_LayoutElement = m_SongEmittersTab_ListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_SongEmittersTab_ListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_SongEmittersTab_ListView.GetComponentInChildren<CustomListTableData>());
                m_SongEmittersTab_List = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_SongEmittersTab_List.TableViewInstance        = l_BSMLTableView;
                m_SongEmittersTab_List.CellSizeValue            = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent      += OnEmitterSelected;
                l_BSMLTableView.SetDataSource(m_SongEmittersTab_List, false);

                /// Bind events
                m_SongEmittersTab_UpButton.onClick.AddListener(OnEmitterPageUpPressed);
                m_SongEmittersTab_DownButton.onClick.AddListener(OnEmitterPageDownPressed);
            }

            /// Commands tab
            if (true)
            {
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_CommandsTab_ModeratorPowerToggle,  l_Event,        CERConfig.Instance.ChatCommands.ModeratorPower,    true);
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_CommandsTab_VIPPowerToggle,        l_Event,        CERConfig.Instance.ChatCommands.VIPPower,          true);
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_CommandsTab_SubscriberPowerToggle, l_Event,        CERConfig.Instance.ChatCommands.SubscriberPower,   true);
                BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_CommandsTab_UserPowerToggle,       l_Event,        CERConfig.Instance.ChatCommands.UserPower,         true);
            }

            /// Show first tab by default
            OnTabSelected(null, 0);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Menu,       false, null);
            ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Playing,    false, null);

            CERConfig.Instance.Save();
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
            m_GeneralTab.SetActive(p_TabIndex == 0);
            m_MenuEmittersTab.SetActive(p_TabIndex == 1);
            m_SongEmittersTab.SetActive(p_TabIndex == 2);
            m_CommandsTab.SetActive(p_TabIndex == 3);

            if (p_TabIndex == 1 || p_TabIndex == 2)
            {
                m_MenuEmittersCurrentPage = 1;
                m_MenuEmittersSelected = -1;
                m_MenuEmittersCurrentPage = 1;
                m_SongEmittersSelected = -1;

                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Menu,       p_TabIndex == 1, null);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Playing,    p_TabIndex == 2, null);

                RebuildEmitterList(null);
            }
        }
        /// <summary>
        /// Emitter list page UP
        /// </summary>
        private void OnEmitterPageUpPressed()
        {
            var l_CurrentPage = m_MenuEmittersTab.activeSelf ? m_MenuEmittersCurrentPage : m_SongEmittersCurrentPage;

            /// Underflow check
            if (l_CurrentPage < 2)
                return;

            /// Decrement current page
            l_CurrentPage--;

            if (m_MenuEmittersTab.activeSelf)
                m_MenuEmittersCurrentPage = l_CurrentPage;
            else
                m_SongEmittersCurrentPage = l_CurrentPage;

            /// Rebuild list
            RebuildEmitterList(null);
        }
        /// <summary>
        /// Rebuilt emitter list
        /// </summary>
        /// <param name="p_EmitterToFocus">Emitter to focus</param>
        public void RebuildEmitterList(EmitterConfig p_EmitterToFocus)
        {
            if (!UICreated)
                return;

            var l_TargetList    = m_MenuEmittersTab.activeSelf ? m_MenuEmittersTab_List : m_SongEmittersTab_List;
            var l_DataSource    = m_MenuEmittersTab.activeSelf ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_CurrentPage   = m_MenuEmittersTab.activeSelf ? m_MenuEmittersCurrentPage : m_SongEmittersCurrentPage;

            /// Update page count
            var l_PageCount  = Math.Max(1, Mathf.CeilToInt((float)(l_DataSource.Count) / (float)(s_EMITTER_PER_PAGE)));

            if (p_EmitterToFocus != null)
            {
                var l_Index = l_DataSource.IndexOf(p_EmitterToFocus);
                if (l_Index != -1)
                    l_CurrentPage = (l_Index / s_EMITTER_PER_PAGE) + 1;
                else
                    OnEmitterSelected(null, -1);
            }

            /// Update overflow
            l_CurrentPage = Math.Max(1, Math.Min(l_CurrentPage, l_PageCount));

            /// Update UI
            if (m_MenuEmittersTab.activeSelf)
            {
                m_MenuEmittersCurrentPage = l_CurrentPage;
                m_MenuEmittersTab_UpButton.interactable     = l_CurrentPage > 1;
                m_MenuEmittersTab_DownButton.interactable   = l_CurrentPage < l_PageCount;
            }
            else
            {
                m_SongEmittersCurrentPage = l_CurrentPage;
                m_SongEmittersTab_UpButton.interactable     = l_CurrentPage > 1;
                m_SongEmittersTab_DownButton.interactable   = l_CurrentPage < l_PageCount;
            }

            /// Clear old entries
            l_TargetList.TableViewInstance.ClearSelection();
            l_TargetList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (l_CurrentPage - 1) * s_EMITTER_PER_PAGE;
                l_I < l_DataSource.Count && l_I < (l_CurrentPage * s_EMITTER_PER_PAGE);
                ++l_I)
            {
                var l_Emitter       = l_DataSource[l_I];
                var l_Name          = l_Emitter.Name;

                l_TargetList.Data.Add(("<align=\"left\">" + (l_Emitter.Enabled ? "" : "<alpha=#70><s>") + l_Name, null));

                if (l_Emitter == p_EmitterToFocus)
                    l_RelIndexToFocus = l_TargetList.Data.Count - 1;
            }

            /// Refresh
            l_TargetList.TableViewInstance.ReloadData();

            /// Update focus
            if (l_DataSource.Count == 0)
                OnEmitterSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                l_TargetList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an emitter is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void OnEmitterSelected(TableView p_List, int p_RelIndex)
        {
            var l_DataSource = m_MenuEmittersTab.activeSelf ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_CurrentPage = m_MenuEmittersTab.activeSelf ? m_MenuEmittersCurrentPage : m_SongEmittersCurrentPage;

            /// Clean up old widget
            if (m_MenuEmittersTab.activeSelf)
            {
                if (m_MenuEmittersTab_Content.transform.childCount != 0)
                    GameObject.DestroyImmediate(m_MenuEmittersTab_Content.transform.GetChild(0).gameObject);
            }
            else
            {
                if (m_SongEmittersTab_Content.transform.childCount != 0)
                    GameObject.DestroyImmediate(m_SongEmittersTab_Content.transform.GetChild(0).gameObject);
            }

            int l_EmitterIndex = ((l_CurrentPage - 1) * s_EMITTER_PER_PAGE) + p_RelIndex;
            if (p_RelIndex < 0 || l_EmitterIndex >= l_DataSource.Count)
            {
                if (m_MenuEmittersTab.activeSelf)
                    m_MenuEmittersSelected = -1;
                else
                    m_SongEmittersSelected = -1;
                return;
            }

            if (m_MenuEmittersTab.activeSelf)
                m_MenuEmittersSelected = l_EmitterIndex;
            else
                m_SongEmittersSelected = l_EmitterIndex;

            var l_Emitter = l_DataSource[l_EmitterIndex];
            if (m_MenuEmittersTab.activeSelf)
            {
                m_EmitterWidget.BuildUI(m_MenuEmittersTab_Content.transform, l_Emitter);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Menu, true, l_Emitter);
            }
            else
            {
                m_EmitterWidget.BuildUI(m_SongEmittersTab_Content.transform, l_Emitter);
                ChatEmoteRain.Instance.SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene.Playing, true, l_Emitter);
            }
        }
        /// <summary>
        /// Emitter list page DOWN
        /// </summary>
        private void OnEmitterPageDownPressed()
        {
            var l_CurrentPage = m_MenuEmittersTab.activeSelf ? m_MenuEmittersCurrentPage : m_SongEmittersCurrentPage;

            /// Increment current page
            l_CurrentPage++;

            if (m_MenuEmittersTab.activeSelf)
                m_MenuEmittersCurrentPage = l_CurrentPage;
            else
                m_SongEmittersCurrentPage = l_CurrentPage;

            /// Rebuild list
            RebuildEmitterList(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On add emitter button
        /// </summary>
        [UIAction("click-emitters-add-btn-pressed")]
        private void OnEmitterAdd()
        {
            var l_Emitter = new EmitterConfig();
            if (m_MenuEmittersTab.activeSelf)
            {
                CERConfig.Instance.MenuEmitters.Add(l_Emitter);
                ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Menu);
            }
            else
            {
                CERConfig.Instance.SongEmitters.Add(l_Emitter);
                ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Playing);
            }

            RebuildEmitterList(l_Emitter);
        }
        /// <summary>
        /// On toggle emitter button
        /// </summary>
        [UIAction("click-emitters-toggle-btn-pressed")]
        private void OnEmitterToggle()
        {
            var l_DataSource    = m_MenuEmittersTab.activeSelf ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_SelectedIndex = m_MenuEmittersTab.activeSelf ? m_MenuEmittersSelected : m_SongEmittersSelected;

            if (l_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an emitter first!");
                return;
            }

            var l_Emitter = l_DataSource[l_SelectedIndex];
            if (l_Emitter.Enabled)
            {
                ShowConfirmationModal($"Do you want to disable emitter\n\"{l_Emitter.Name}\"?", () =>
                {
                    l_Emitter.Enabled = false;
                    RebuildEmitterList(l_Emitter);

                    if (m_MenuEmittersTab.activeSelf)
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Menu);
                    else
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Playing);
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable emitter\n\"{l_Emitter.Name}\"?", () =>
                {
                    l_Emitter.Enabled = true;
                    RebuildEmitterList(l_Emitter);

                    if (m_MenuEmittersTab.activeSelf)
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Menu);
                    else
                        ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Playing);
                });
            }
        }
        /// <summary>
        /// On delete emitter button
        /// </summary>
        [UIAction("click-emitters-delete-btn-pressed")]
        private void OnEmitterDelete()
        {
            var l_DataSource    = m_MenuEmittersTab.activeSelf ? CERConfig.Instance.MenuEmitters : CERConfig.Instance.SongEmitters;
            var l_SelectedIndex = m_MenuEmittersTab.activeSelf ? m_MenuEmittersSelected : m_SongEmittersSelected;

            if (l_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an emitter first!");
                return;
            }

            var l_Emitter = l_DataSource[l_SelectedIndex];
            ShowConfirmationModal($"<color=red>Do you want to delete emitter</color>\n\"{l_Emitter.Name}\"?", () =>
            {
                OnEmitterSelected(null, -1);
                l_DataSource.Remove(l_Emitter);
                RebuildEmitterList(null);

                if (m_MenuEmittersTab.activeSelf)
                    ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Menu);
                else
                    ChatEmoteRain.Instance.UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Playing);
            });
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

            /// General tab setup
            if (true)
            {
                /// First row
                CERConfig.Instance.EnableMenu   = m_GeneralTab_MenuRain.Value;
                CERConfig.Instance.MenuSize     = m_GeneralTab_MenuRainSizeSlider.slider.value;
                CERConfig.Instance.MenuSpeed    = m_GeneralTab_MenuFallSpeedSlider.slider.value;

                /// Second row
                CERConfig.Instance.EnableSong   = m_GeneralTab_SongRain.Value;
                CERConfig.Instance.SongSize     = m_GeneralTab_SongRainSizeSlider.slider.value;
                CERConfig.Instance.SongSpeed    = m_GeneralTab_SongFallSpeedSlider.slider.value;
            }

            /// Commands tab
            if (true)
            {
                CERConfig.Instance.ChatCommands.ModeratorPower     = m_CommandsTab_ModeratorPowerToggle.Value;
                CERConfig.Instance.ChatCommands.VIPPower           = m_CommandsTab_VIPPowerToggle.Value;
                CERConfig.Instance.ChatCommands.SubscriberPower    = m_CommandsTab_SubscriberPowerToggle.Value;
                CERConfig.Instance.ChatCommands.UserPower          = m_CommandsTab_UserPowerToggle.Value;
            }

            ChatEmoteRain.Instance.OnSettingsChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// General tab setup
            if (true)
            {
                /// First row
                m_GeneralTab_MenuRain.Value = CERConfig.Instance.EnableMenu;
                BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_GeneralTab_MenuRainSizeSlider,     CERConfig.Instance.MenuSize);
                BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_GeneralTab_MenuFallSpeedSlider,    CERConfig.Instance.MenuSpeed);

                /// Second row
                m_GeneralTab_SongRain.Value = CERConfig.Instance.EnableSong;
                BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_GeneralTab_SongRainSizeSlider,     CERConfig.Instance.SongSize);
                BeatSaberPlus.SDK.UI.SliderSetting.SetValue(m_GeneralTab_SongFallSpeedSlider,    CERConfig.Instance.SongSpeed);
            }

            RebuildEmitterList(null);

            /// Commands tab
            if (true)
            {
                m_CommandsTab_ModeratorPowerToggle.Value    = CERConfig.Instance.ChatCommands.ModeratorPower;
                m_CommandsTab_VIPPowerToggle.Value          = CERConfig.Instance.ChatCommands.VIPPower;
                m_CommandsTab_SubscriberPowerToggle.Value   = CERConfig.Instance.ChatCommands.SubscriberPower;
                m_CommandsTab_UserPowerToggle.Value         = CERConfig.Instance.ChatCommands.UserPower;
            }

            m_PreventChanges = false;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }
    }
}
