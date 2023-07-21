using CP_SDK.XUI;
using UnityEngine;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal sealed class SettingsRightView : ViewController<SettingsRightView>
    {
        private XUITabControl   m_TabControl;

        private XUIToggle       m_OBSTab_Enabled;
        private XUITextInput    m_OBSTab_Server;
        private XUITextInput    m_OBSTab_Password;
        private XUIText         m_OBSTab_Status;

        private XUIToggle       m_EmotesTab_BBTVEnabled;
        private XUIToggle       m_EmotesTab_FFZEnabled;
        private XUIToggle       m_EmotesTab_7TVEnabled;
        private XUIToggle       m_EmotesTab_EmojisEnabled;
        private XUIToggle       m_EmotesTab_ParseTemporaryChannels;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Other settings"),

                XUITabControl.Make(
                    ("OBS",     BuildOBSTab()),
                    ("Emotes",  BuildEmotesTab())
                )
                .Bind(ref m_TabControl)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            OnValueChanged();
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            CPConfig.Instance.Save();
            Chat.ChatModSettings.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build OBS tab
        /// </summary>
        /// <returns></returns>
        private XUIVLayout BuildOBSTab()
        {
            var l_OBSConfig = OBS.OBSModSettings.Instance;
            return XUIVLayout.Make(
                XUIText.Make("Status: X")
                    .SetAlign(TMPro.TextAlignmentOptions.CaplineGeoAligned)
                    .SetColor(Color.yellow)
                    .Bind(ref m_OBSTab_Status),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Enabled"),
                        XUIText.Make("Server"),
                        XUIText.Make("Password")
                    )
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)),

                    XUIVLayout.Make(
                        XUIToggle.Make()
                            .SetValue(l_OBSConfig.Enabled)
                            .Bind(ref m_OBSTab_Enabled),
                        XUITextInput.Make("Server address")
                            .SetValue(l_OBSConfig.Server)
                            .Bind(ref m_OBSTab_Server),
                        XUITextInput.Make("Password")
                            .SetIsPassword(true)
                            .SetValue(l_OBSConfig.Password)
                            .Bind(ref m_OBSTab_Password)
                    )
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
                    .ForEachDirect<XUITextInput>(x => x.OnValueChanged(_ => OnValueChanged()))
                ),

                XUIVSpacer.Make(10f),

                XUIPrimaryButton.Make("Apply", OnOBSTabApplyButton)
                    .SetWidth(60f)
                    .SetHeight(8f)
            );
        }
        /// <summary>
        /// Build emotes tab
        /// </summary>
        /// <returns></returns>
        private XUIVLayout BuildEmotesTab()
        {
            var l_EmotesConfig = Chat.ChatModSettings.Instance.Emotes;

            return XUIVLayout.Make(
                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Parse BBTV Emotes"),
                        XUIText.Make("Parse FFZ Emotes"),
                        XUIText.Make("Parse 7TV Emotes"),
                        XUIText.Make("Parse Emojis Emotes"),
                        XUIText.Make("Parse emotes from temporary channels")
                    )
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)),

                    XUIVLayout.Make(
                        XUIToggle.Make()
                            .SetValue(l_EmotesConfig.ParseBTTVEmotes)
                            .Bind(ref m_EmotesTab_BBTVEnabled),
                        XUIToggle.Make()
                            .SetValue(l_EmotesConfig.ParseFFZEmotes)
                            .Bind(ref m_EmotesTab_FFZEnabled),
                        XUIToggle.Make()
                            .SetValue(l_EmotesConfig.Parse7TVEmotes)
                            .Bind(ref m_EmotesTab_7TVEnabled),
                        XUIToggle.Make()
                            .SetValue(l_EmotesConfig.ParseEmojis)
                            .Bind(ref m_EmotesTab_EmojisEnabled),
                        XUIToggle.Make()
                            .SetValue(l_EmotesConfig.ParseTemporaryChannels)
                            .Bind(ref m_EmotesTab_ParseTemporaryChannels)
                    )
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
                ),

                XUIVSpacer.Make(5f),

                XUIPrimaryButton.Make("Apply / Recache emotes", OnEmotesTabApplyButton)
                    .SetWidth(60f)
                    .SetHeight(8f)
            );
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (m_TabControl.Element.GetActiveTab() == 0)
            {
                var l_Status = OBS.Service.Status;
                var l_Text = "Status: ";

                switch (l_Status)
                {
                    case OBS.Service.EStatus.Disconnected:
                    case OBS.Service.EStatus.Connecting:
                        l_Text += "<color=blue>";
                        break;

                    case OBS.Service.EStatus.Authing:
                        l_Text += "<color=yellow>";
                        break;

                    case OBS.Service.EStatus.Connected:
                        l_Text += "<color=green>";
                        break;

                    case OBS.Service.EStatus.AuthRejected:
                        l_Text += "<color=red>";
                        break;
                }

                l_Text += l_Status;

                if (m_OBSTab_Status.Element.TMProUGUI.text != l_Text)
                    m_OBSTab_Status.SetText(l_Text);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On setting changed
        /// </summary>
        private void OnValueChanged()
        {
            if (m_PreventChanges)
                return;

            var l_OBSConfig = OBS.OBSModSettings.Instance;
            l_OBSConfig.Enabled     = m_OBSTab_Enabled.Element.GetValue();
            l_OBSConfig.Server      = m_OBSTab_Server.Element.GetValue();
            l_OBSConfig.Password    = m_OBSTab_Password.Element.GetValue();

            m_OBSTab_Server  .SetInteractable(l_OBSConfig.Enabled);
            m_OBSTab_Password.SetInteractable(l_OBSConfig.Enabled);

            var l_EmotesConfig = Chat.ChatModSettings.Instance.Emotes;
            l_EmotesConfig.ParseBTTVEmotes          = m_EmotesTab_BBTVEnabled.Element.GetValue();
            l_EmotesConfig.ParseFFZEmotes           = m_EmotesTab_FFZEnabled.Element.GetValue();
            l_EmotesConfig.Parse7TVEmotes           = m_EmotesTab_7TVEnabled.Element.GetValue();
            l_EmotesConfig.ParseEmojis              = m_EmotesTab_EmojisEnabled.Element.GetValue();
            l_EmotesConfig.ParseTemporaryChannels   = m_EmotesTab_ParseTemporaryChannels.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On apply setting button
        /// </summary>
        private void OnOBSTabApplyButton()
            => OBS.Service.ApplyConf();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On apply setting button
        /// </summary>
        private void OnEmotesTabApplyButton()
        {
            Chat.Service.RecacheEmotes();
            ShowMessageModal("OK!");
        }
    }
}
