using CP_SDK.XUI;
using System.Collections;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Actions
{
    public class SongChartVisualizer_ToggleVisibility
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<SongChartVisualizer_ToggleVisibility, Models.Actions.SongChartVisualizer_ToggleVisibility>
    {
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Show or hide the SongChartVisualizer ingame";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(ChatPlexMod_ChatIntegrations.Enums.Toggle.S).SetValue(ChatPlexMod_ChatIntegrations.Enums.Toggle.ToStr(Model.ChangeType))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.SongChartVisualizer)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("SongChartVisualizer: Action failed, SongChartVisualizer module is missing!");
                yield break;
            }

            var l_Instance = ChatPlexMod_SongChartVisualizer.SongChartVisualizer.Instance;
            switch (Model.ChangeType)
            {
                case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                    l_Instance?.ToggleVisibility();
                    break;
                case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                    l_Instance?.SetVisible(true);
                    break;
                case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                    l_Instance?.SetVisible(false);
                    break;
            }

            yield return null;
        }
    }
}
