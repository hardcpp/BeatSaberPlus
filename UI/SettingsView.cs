using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Settings view controller
    /// </summary>
    internal class SettingsView : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ToggleSetting creator
        /// </summary>
        private BeatSaberMarkupLanguage.Tags.ToggleSettingTag m_ToggleSettingCreator = null;
        /// <summary>
        /// Plugin setting
        /// </summary>
        private Dictionary<Plugins.PluginBase, ToggleSetting> m_PluginsSetting = new Dictionary<Plugins.PluginBase, ToggleSetting>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("SettingGrid")]
        private GameObject m_SettingGrid;
        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// Initial setup
            if (p_FirstActivation)
            {
                m_ToggleSettingCreator = new BeatSaberMarkupLanguage.Tags.ToggleSettingTag();

                foreach (var l_Plugin in Plugin.Instance.Plugins)
                {
                    var l_Setting = AddSetting(l_Plugin.Name, l_Plugin.IsEnabled, (x) => {
                        try
                        {
                            l_Plugin.SetEnabled(x);
                            CheckChatTutorial(l_Plugin);
                        }
                        catch (System.Exception p_InitException) { Logger.Instance.Error("Error on plugin init " + l_Plugin.Name); Logger.Instance.Error(p_InitException); }
                    });
                    m_PluginsSetting.Add(l_Plugin, l_Setting);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a setting to the grid
        /// </summary>
        /// <param name="p_Name">Button caption</param>
        /// <param name="p_Action">Button callback</param>
        private ToggleSetting AddSetting(string p_Name, bool p_Enabled, Action<bool> p_Action)
        {
            var l_Setting = m_ToggleSettingCreator.CreateObject(m_SettingGrid.transform);

            l_Setting.gameObject.SetActive(false);
            var l_Toggle = l_Setting.GetComponent<ToggleSetting>();

            l_Toggle.Text   = p_Name;
            l_Toggle.Value  = p_Enabled;
            l_Toggle.toggle.onValueChanged.AddListener((x) => { p_Action(x); });
            l_Setting.gameObject.SetActive(true);

            return l_Toggle;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for chat tutorial
        /// </summary>
        /// <param name="p_Plugin">Plugin instance</param>
        private void CheckChatTutorial(Plugins.PluginBase p_Plugin)
        {
            if (!(p_Plugin is Plugins.Chat.Chat) && !(p_Plugin is Plugins.ChatRequest.ChatRequest) && !(p_Plugin is Plugins.ChatEmoteRain.ChatEmoteRain))
                return;

            if (Config.FirstChatCoreRun)
            {
                ShowMessageModal("Hey it's seems that this is the first time you use a chat module!\nThe configuration page has been opened in your browser!");
                Utils.ChatService.OpenWebConfigurator();

                Config.FirstChatCoreRun = false;
            }
        }
    }
}
