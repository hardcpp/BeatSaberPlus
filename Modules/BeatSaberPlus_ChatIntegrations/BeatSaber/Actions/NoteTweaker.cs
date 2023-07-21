using CP_SDK.XUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Actions
{
    public class NoteTweaker_SwitchProfile
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<NoteTweaker_SwitchProfile, Models.Actions.NoteTweaker_SwitchProfile>
    {
        private XUIDropdown m_Profile   = null;
        private XUIToggle   m_Temporary = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change active NoteTweaker profile";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Choices           = new List<string>() { "<i>None</i>" };
            var l_SelectedChoice    = "<i>None</i>";
            if (ModulePresence.NoteTweaker)
            {
                l_Choices = BeatSaberPlus_NoteTweaker.NoteTweaker.Instance.GetAvailableProfiles();
                if (l_Choices.Count == 0)
                    l_Choices.Add("<i>None</i>");
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I]  != Model.Profile)
                    continue;

                l_SelectedChoice = l_Choices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Profile",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_SelectedChoice)
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Profile)
                ),

                Templates.SettingsHGroup("Temporary",
                    XUIToggle.Make()
                        .SetValue(Model.Temporary)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Temporary)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Profile   = m_Profile.Element.GetValue();
            Model.Temporary = m_Temporary.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            var l_Instance = BeatSaberPlus_NoteTweaker.NoteTweaker.Instance;
            var l_Profiles = l_Instance.GetAvailableProfiles();
            if (l_Profiles.Contains(Model.Profile))
                l_Instance.SwitchToProfile(l_Profiles.IndexOf(Model.Profile), Model.Temporary);
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:NoteTweaker_SwitchProfile Profile:{Model.Profile} not found!");
            }

            yield return null;
        }
    }
}
