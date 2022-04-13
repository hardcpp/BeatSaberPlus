using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Settings right view controller
    /// </summary>
    internal class SettingsRightView : SDK.UI.ResourceViewController<SettingsRightView>
    {
#pragma warning disable CS0649
        [UIObject("TabSelector")] private GameObject m_TabSelector;
        private TextSegmentedControl m_TabSelector_TabSelectorControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Twitch Tab
        [UIObject("TwitchTab")]                             private GameObject m_TwitchTab = null;
        [UIComponent("TwitchTab_BBTVEnabled")]              private ToggleSetting m_TwitchTab_BBTVEnabled;
        [UIComponent("TwitchTab_FFZEnabled")]               private ToggleSetting m_TwitchTab_FFZEnabled;
        [UIComponent("TwitchTab_7TVEnabled")]               private ToggleSetting m_TwitchTab_7TVEnabled;
        [UIComponent("TwitchTab_TwitchEnabled")]            private ToggleSetting m_TwitchTab_TwitchEnabled;
        [UIComponent("TwitchTab_TwitchCheermotesEnabled")]  private ToggleSetting m_TwitchTab_TwitchCheermotesEnabled;
        [UIComponent("TwitchTab_EmojisEnabled")]            private ToggleSetting m_TwitchTab_EmojisEnabled;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        #region Tools Tab
        [UIObject("OBSTab")]                    private GameObject m_OBSTab = null;
        [UIComponent("OBSTab_Enabled")]         private ToggleSetting m_OBSTab_Enabled;
        [UIComponent("OBSTab_Server")]          private TextMeshProUGUI m_OBSTab_Server;
        [UIComponent("OBSTab_ChangeServer")]    private Button m_OBSTab_ChangeServer;
        [UIComponent("OBSTab_ChangePassword")]  private Button m_OBSTab_ChangePassword;
        [UIComponent("OBSTab_Status")]          private TextMeshProUGUI m_OBSTab_Status;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("InputKeyboard")]
        private ModalKeyboard m_InputKeyboard = null;
        [UIValue("InputKeyboardValue")]
        private string m_InputKeyboardValue = "";
        private Action<string> m_InputKeyboardCallback;
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
            var l_Event = new BSMLAction(this, this.GetType().GetMethod(nameof(SettingsRightView.OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Create type selector
            m_TabSelector_TabSelectorControl = SDK.UI.TextSegmentedControl.Create(m_TabSelector.transform as RectTransform, false);
            m_TabSelector_TabSelectorControl.SetTexts(new string[] { "Twitch", "OBS" });
            m_TabSelector_TabSelectorControl.ReloadData();
            m_TabSelector_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            ////////////////////////////////////////////////////////////////////////////
            /// Prepare tabs
            ////////////////////////////////////////////////////////////////////////////

            SDK.UI.Backgroundable.SetOpacity(m_TwitchTab,   0.50f);
            SDK.UI.Backgroundable.SetOpacity(m_OBSTab,      0.50f);
            SDK.UI.ModalView.SetOpacity(m_InputKeyboard.modalView, 0.75f);

            #region Twitch Tab
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_BBTVEnabled,             l_Event, BSPConfig.Instance.Twitch.ParseBTTVEmotes,     false);
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_FFZEnabled,              l_Event, BSPConfig.Instance.Twitch.ParseFFZEmotes,      false);
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_7TVEnabled,              l_Event, BSPConfig.Instance.Twitch.Parse7TVEmotes,      false);
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_TwitchEnabled,           l_Event, BSPConfig.Instance.Twitch.ParseTwitchEmotes,   false);
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_TwitchCheermotesEnabled, l_Event, BSPConfig.Instance.Twitch.ParseCheermotes,     false);
            SDK.UI.ToggleSetting.Setup(m_TwitchTab_EmojisEnabled,           l_Event, BSPConfig.Instance.Twitch.ParseEmojis,         false);
            #endregion

            #region Tools Tab
            SDK.UI.ToggleSetting.Setup(m_OBSTab_Enabled, l_Event, BSPConfig.Instance.OBS.Enabled, true);

            m_OBSTab_Server.text = BSPConfig.Instance.OBS.Server;
            #endregion

            /// Show first tab by default
            OnTabSelected(null, 0);
            /// Refresh UI
            OnSettingChanged(null);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            BSPConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_OBSTab.activeSelf)
            {
                var l_Status = SDK.OBS.Service.Status;
                var l_Text = "Status: ";

                switch (l_Status)
                {
                    case SDK.OBS.Service.EStatus.Disconnected:
                    case SDK.OBS.Service.EStatus.Connecting:
                        l_Text += "<color=blue>";
                        break;

                    case SDK.OBS.Service.EStatus.Authing:
                        l_Text += "<color=yellow>";
                        break;

                    case SDK.OBS.Service.EStatus.Connected:
                        l_Text += "<color=green>";
                        break;

                    case SDK.OBS.Service.EStatus.AuthRejected:
                        l_Text += "<color=red>";
                        break;
                }

                l_Text += l_Status;

                if (m_OBSTab_Status.text != l_Text)
                    m_OBSTab_Status.text = l_Text;
            }
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
            m_TwitchTab.SetActive(p_TabIndex == 0);
            m_OBSTab.SetActive(p_TabIndex == 1);

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_TwitchTab.transform.parent.transform as RectTransform);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            #region Twitch Tab
            BSPConfig.Instance.Twitch.ParseBTTVEmotes   = m_TwitchTab_BBTVEnabled.Value;
            BSPConfig.Instance.Twitch.ParseFFZEmotes    = m_TwitchTab_FFZEnabled.Value;
            BSPConfig.Instance.Twitch.Parse7TVEmotes    = m_TwitchTab_7TVEnabled.Value;
            BSPConfig.Instance.Twitch.ParseTwitchEmotes = m_TwitchTab_TwitchEnabled.Value;
            BSPConfig.Instance.Twitch.ParseCheermotes   = m_TwitchTab_TwitchCheermotesEnabled.Value;
            BSPConfig.Instance.Twitch.ParseEmojis       = m_TwitchTab_EmojisEnabled.Value;
            #endregion

            #region OBS Tab
            BSPConfig.Instance.OBS.Enabled = m_OBSTab_Enabled.Value;

            m_OBSTab_ChangeServer.interactable      = BSPConfig.Instance.OBS.Enabled;
            m_OBSTab_ChangePassword.interactable    = BSPConfig.Instance.OBS.Enabled;
            #endregion
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On apply setting button
        /// </summary>
        [UIAction("TwitchTab_ApplyButton")]
        private void TwitchTab_ApplyButton()
        {
            var l_TwitchService = SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is SDK.Chat.Services.Twitch.TwitchService);

            if (l_TwitchService != null)
            {
                (l_TwitchService as SDK.Chat.Services.Twitch.TwitchService).OnCredentialsUpdated(true);
                ShowMessageModal("OK!");
            }
            else
                ShowMessageModal("No Twitch service connected!");
        }
        /// <summary>
        /// On open web configuration button
        /// </summary>
        [UIAction("TwitchTab_WebConfiguration")]
        private void TwitchTab_WebConfiguration()
        {
            ShowMessageModal("URL opened in your desktop browser.");
            SDK.Chat.Service.OpenWebConfigurator();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change server button
        /// </summary>
        [UIAction("OBSTab_ChangeServerButton")]
        private void OBSTab_ChangeServerButton()
        {
            UIShowInputKeyboard(BSPConfig.Instance.OBS.Server, (x) =>
            {
                BSPConfig.Instance.OBS.Server = x;
                m_OBSTab_Server.text = BSPConfig.Instance.OBS.Server;
            });
        }
        /// <summary>
        /// Change password button
        /// </summary>
        [UIAction("OBSTab_ChangePasswordButton")]
        private void OBSTab_ChangePasswordButton()
        {
            UIShowInputKeyboard(BSPConfig.Instance.OBS.Pssword, (x) =>
            {
                BSPConfig.Instance.OBS.Pssword = x;
            });
        }
        /// <summary>
        /// On apply setting button
        /// </summary>
        [UIAction("OBSTab_ApplyButton")]
        private void OBSTab_ApplyButton()
            => SDK.OBS.Service.ApplyConf();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show input keyboard
        /// </summary>
        /// <param name="p_Value">Start value</param>
        /// <param name="p_Callback">On enter callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public void UIShowInputKeyboard(string p_Value, Action<string> p_Callback)
        {
            m_InputKeyboardValue = p_Value;

            /// Show keyboard
            m_InputKeyboardCallback = p_Callback;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardText.enableAutoSizing = true;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.enableAutoSizing = true;
            m_InputKeyboard.modalView.Show(true);
        }
        /// <summary>
        /// Append value to current keyboard input
        /// </summary>
        /// <param name="p_Value">Value to append</param>
        public void UIInputKeyboardAppend(string p_Value)
        {
            m_InputKeyboard.keyboard.KeyboardText.text += p_Value;
        }
        /// <summary>
        /// On input keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("InputKeyboardEnterPressed")]
        private void InputKeyboardEnterPressed(string p_Text)
        {
            m_InputKeyboardCallback?.Invoke(p_Text);
        }
    }
}
