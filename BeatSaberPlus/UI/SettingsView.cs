using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.UI
{
    /// <summary>
    /// Settings view controller
    /// </summary>
    internal class SettingsView : SDK.UI.ResourceViewController<SettingsView>
    {
        /// <summary>
        /// Module setting
        /// </summary>
        private Dictionary<CP_SDK.IModuleBase, ToggleSetting> m_ModulesSetting = new Dictionary<CP_SDK.IModuleBase, ToggleSetting>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("SettingGrid")]
        private GameObject m_SettingGrid;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Layout = m_SettingGrid.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            l_Layout.constraint         = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            l_Layout.constraintCount    = 3;

            foreach (var l_Module in CP_SDK.ChatPlexSDK.GetModules().Where(x => x.Type == CP_SDK.EIModuleBaseType.Integrated))
            {
                try
                {
                    var l_Setting = SDK.UI.ToggleSetting.Create(m_SettingGrid.transform, l_Module.FancyName, l_Module.IsEnabled, (x) =>
                    {
                        try
                        {
                            l_Module.SetEnabled(x);
                            CheckChatTutorial(l_Module);
                        }
                        catch (Exception p_InitException)
                        {
                            CP_SDK.ChatPlexSDK.Logger.Error($"[UI][SettingsView.OnViewCreation] Error on module \"{l_Module.FancyName}\" init");
                            CP_SDK.ChatPlexSDK.Logger.Error(p_InitException);
                        }
                    }, l_Module.Description);
                    l_Setting.text.fontSize = 2.5f;

                    m_ModulesSetting.Add(l_Module, l_Setting);
                }
                catch (Exception p_InitException)
                {
                    CP_SDK.ChatPlexSDK.Logger.Error($"[UI][SettingsView.OnViewCreation] Error on module \"{l_Module.FancyName}\" init");
                    CP_SDK.ChatPlexSDK.Logger.Error(p_InitException);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for chat tutorial
        /// </summary>
        /// <param name="p_Plugin">Plugin instance</param>
        private void CheckChatTutorial(CP_SDK.IModuleBase p_Plugin)
        {
#if DEBUG
            if (p_Plugin.UseChatFeatures && true)
#else
            if (p_Plugin.UseChatFeatures && BSPConfig.Instance.FirstChatCoreRun)
#endif
            {
                ShowMessageModal("Hey it's seems that this is the first time\nyou use a chat module!\n<b><color=yellow>The configuration page has been opened in your browser!</color></b>");

                CP_SDK.Chat.Service.OpenWebConfigurator();

                BSPConfig.Instance.FirstChatCoreRun = false;
                BSPConfig.Instance.Save();
            }
        }
    }
}
