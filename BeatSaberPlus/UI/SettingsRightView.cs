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

        #region Emotes Tab
        [UIObject("EmotesTab")]                     private GameObject      m_EmotesTab = null;
        [UIComponent("EmotesTab_BBTVEnabled")]      private ToggleSetting   m_EmotesTab_BBTVEnabled;
        [UIComponent("EmotesTab_FFZEnabled")]       private ToggleSetting   m_EmotesTab_FFZEnabled;
        [UIComponent("EmotesTab_7TVEnabled")]       private ToggleSetting   m_EmotesTab_7TVEnabled;
        [UIComponent("EmotesTab_EmojisEnabled")]    private ToggleSetting   m_EmotesTab_EmojisEnabled;
        #endregion

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        //#region Twitch Tab
        //[UIObject("TwitchTab")]                             private GameObject m_TwitchTab = null;
        //[UIComponent("TwitchTab_TwitchEnabled")]            private ToggleSetting m_TwitchTab_TwitchEnabled;
        //[UIComponent("TwitchTab_TwitchCheermotesEnabled")]  private ToggleSetting m_TwitchTab_TwitchCheermotesEnabled;
        //#endregion

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
            m_TabSelector_TabSelectorControl.SetTexts(new string[] { "OBS", "Emotes" });
            m_TabSelector_TabSelectorControl.ReloadData();
            m_TabSelector_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            ////////////////////////////////////////////////////////////////////////////
            /// Prepare tabs
            ////////////////////////////////////////////////////////////////////////////

            SDK.UI.Backgroundable.SetOpacity(m_OBSTab,      0.50f);
            SDK.UI.Backgroundable.SetOpacity(m_EmotesTab,   0.50f);
            SDK.UI.ModalView.SetOpacity(m_InputKeyboard.modalView, 0.75f);

            #region Emotes Tab
            SDK.UI.ToggleSetting.Setup(m_EmotesTab_BBTVEnabled,             l_Event, CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseBTTVEmotes,     false);
            SDK.UI.ToggleSetting.Setup(m_EmotesTab_FFZEnabled,              l_Event, CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseFFZEmotes,      false);
            SDK.UI.ToggleSetting.Setup(m_EmotesTab_7TVEnabled,              l_Event, CP_SDK.Chat.ChatModSettings.Instance.Emotes.Parse7TVEmotes,      false);
            SDK.UI.ToggleSetting.Setup(m_EmotesTab_EmojisEnabled,           l_Event, CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseEmojis,         false);
            #endregion

            #region Tools Tab
            SDK.UI.ToggleSetting.Setup(m_OBSTab_Enabled, l_Event, CP_SDK.OBS.OBSModSettings.Instance.Enabled, true);

            m_OBSTab_Server.text = CP_SDK.OBS.OBSModSettings.Instance.Server;
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
            CP_SDK.Chat.ChatModSettings.Instance.Save();
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
                var l_Status = CP_SDK.OBS.Service.Status;
                var l_Text = "Status: ";

                switch (l_Status)
                {
                    case CP_SDK.OBS.Service.EStatus.Disconnected:
                    case CP_SDK.OBS.Service.EStatus.Connecting:
                        l_Text += "<color=blue>";
                        break;

                    case CP_SDK.OBS.Service.EStatus.Authing:
                        l_Text += "<color=yellow>";
                        break;

                    case CP_SDK.OBS.Service.EStatus.Connected:
                        l_Text += "<color=green>";
                        break;

                    case CP_SDK.OBS.Service.EStatus.AuthRejected:
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
            m_OBSTab.SetActive(p_TabIndex == 0);
            m_EmotesTab.SetActive(p_TabIndex == 1);

            LayoutRebuilder.ForceRebuildLayoutImmediate(m_EmotesTab.transform.parent.transform as RectTransform);
        }
        /// <summary>
        /// On setting changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            #region OBS Tab
            CP_SDK.OBS.OBSModSettings.Instance.Enabled = m_OBSTab_Enabled.Value;

            m_OBSTab_ChangeServer.interactable      = CP_SDK.OBS.OBSModSettings.Instance.Enabled;
            m_OBSTab_ChangePassword.interactable    = CP_SDK.OBS.OBSModSettings.Instance.Enabled;
            #endregion

            #region Emotes Tab
            CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseBTTVEmotes   = m_EmotesTab_BBTVEnabled.Value;
            CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseFFZEmotes    = m_EmotesTab_FFZEnabled.Value;
            CP_SDK.Chat.ChatModSettings.Instance.Emotes.Parse7TVEmotes    = m_EmotesTab_7TVEnabled.Value;
            CP_SDK.Chat.ChatModSettings.Instance.Emotes.ParseEmojis       = m_EmotesTab_EmojisEnabled.Value;
            #endregion
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change server button
        /// </summary>
        [UIAction("OBSTab_ChangeServerButton")]
        private void OBSTab_ChangeServerButton()
        {
            UIShowInputKeyboard(CP_SDK.OBS.OBSModSettings.Instance.Server, (x) =>
            {
                CP_SDK.OBS.OBSModSettings.Instance.Server = x;
                m_OBSTab_Server.text = CP_SDK.OBS.OBSModSettings.Instance.Server;
            });
        }
        /// <summary>
        /// Change password button
        /// </summary>
        [UIAction("OBSTab_ChangePasswordButton")]
        private void OBSTab_ChangePasswordButton()
        {
            UIShowInputKeyboard(CP_SDK.OBS.OBSModSettings.Instance.Password, (x) =>
            {
                CP_SDK.OBS.OBSModSettings.Instance.Password = x;
            });
        }
        /// <summary>
        /// On apply setting button
        /// </summary>
        [UIAction("OBSTab_ApplyButton")]
        private void OBSTab_ApplyButton()
            => CP_SDK.OBS.Service.ApplyConf();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On apply setting button
        /// </summary>
        [UIAction("EmotesTab_ApplyButton")]
        private void EmotesTab_ApplyButton()
        {
            CP_SDK.Chat.Service.Multiplexer.RecacheEmotes();
            ShowMessageModal("OK!");
        }

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