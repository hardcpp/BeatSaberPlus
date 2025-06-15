using CP_SDK.Chat.Interfaces;
using CP_SDK.XUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class ChatRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("Chat_SendMessage",       () => new Chat_SendMessage());
            ChatIntegrations.RegisterActionType("Chat_InternalMessage",   () => new Chat_InternalMessage());
            ChatIntegrations.RegisterActionType("Chat_ToggleEmoteOnly",   () => new Chat_ToggleEmoteOnly());
            ChatIntegrations.RegisterActionType("Chat_ToggleVisibility",  () => new Chat_ToggleVisibility());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Chat_SendMessage
        : Interfaces.IAction<Chat_SendMessage, Models.Actions.Chat_SendMessage>
    {
        private XUIText   m_Message         = null;
        private XUIToggle m_AddTTSPrefix    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Send a message in the chat";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Message",
                    XUIText.Make(Model.BaseValue)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_Message)
                ),

                Templates.SettingsHGroup("Add TTS prefix",
                    XUIToggle.Make()
                        .SetValue(Model.AddTTSPefix).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_AddTTSPrefix)
                ),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Set from game", OnSetFromGameButton),
                    XUIPrimaryButton.Make("Set from chat", OnSetFromChatButton)
                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .ForEachDirect<XUIPrimaryButton>  (y => {
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.AddTTSPefix = m_AddTTSPrefix.Element.GetValue();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Keys = new List<(string, System.Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.BaseValue, (p_Result) =>
            {
                Model.BaseValue = p_Result;
                m_Message.SetText(Model.BaseValue);
            }, null, l_Keys);
        }
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Message = "Please input a message in chat with your streaming account.\nProvided values:\n";
            l_Message += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

            View.ShowLoadingModal(l_Message, true, () =>
            {
                ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Instance_OnBroadcasterChatMessage(IChatMessage p_Message)
        {
            Model.BaseValue = p_Message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            View.CloseLoadingModal();

            m_Message.SetText(Model.BaseValue);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            var l_Message   = (Model.AddTTSPefix ? "! " : "") +  Model.BaseValue;
            var l_Variables = p_Context.GetValues(Interfaces.EValueType.String, Interfaces.EValueType.Integer, Interfaces.EValueType.Floating);

            foreach (var l_Var in l_Variables)
            {
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.EValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.EValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = l_StringVal;

                l_Message = l_Message.Replace(l_Key, l_ReplaceValue);
            }

            if (p_Context.ChatService != null && p_Context.Channel != null)
                p_Context.ChatService.SendTextMessage(p_Context.Channel, l_Message);
            else
                CP_SDK.Chat.Service.BroadcastMessage(l_Message);

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Chat_InternalMessage
        : Interfaces.IAction<Chat_InternalMessage, Models.Action>
    {
        private XUIText   m_Message         = null;
        private XUIToggle m_AddTTSPrefix    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Display a internal message in the chat";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Message",
                    XUIText.Make(Model.BaseValue)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_Message)
                ),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Set from game", OnSetFromGameButton),
                    XUIPrimaryButton.Make("Set from chat", OnSetFromChatButton)
                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .ForEachDirect<XUIPrimaryButton>  (y => {
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
            };

            BuildUIAuto(parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Keys      = new List<(string, System.Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.BaseValue, (result) =>
            {
                Model.BaseValue = result;
                m_Message.SetText(Model.BaseValue);
            }, null, l_Keys);
        }
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Message = "Please input a message in chat with your streaming account.\nProvided values:\n";
            l_Message += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

            View.ShowLoadingModal(l_Message, true, () =>
            {
                ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Instance_OnBroadcasterChatMessage(IChatMessage message)
        {
            Model.BaseValue = message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            View.CloseLoadingModal();

            m_Message.SetText(Model.BaseValue);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext context)
        {
            var l_Message   = Model.BaseValue;
            var l_Variables = context.GetValues(Interfaces.EValueType.String, Interfaces.EValueType.Integer, Interfaces.EValueType.Floating);

            foreach (var l_Var in l_Variables)
            {
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.EValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.EValueType.Integer && context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.Floating && context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.String && context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = l_StringVal;

                l_Message = l_Message.Replace(l_Key, l_ReplaceValue);
            }

            CP_SDK.Chat.Service.Multiplexer.InternalBroadcastSystemMessage(l_Message);

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Chat_ToggleEmoteOnly
        : Interfaces.IAction<Chat_ToggleEmoteOnly, Models.Action>
    {
        public override string Description      => "Enable or disable emote only in the chat";
        public override string UIPlaceHolder    => "Enable or disable emote only in the chat";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel l_TwitchChannel)
            {
                if (l_TwitchChannel.Roomstate.EmoteOnly)
                    l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonlyoff");
                else
                    l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonly");
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Chat_ToggleVisibility
        : Interfaces.IAction<Chat_ToggleVisibility, Models.Actions.Chat_ToggleVisibility>
    {
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Show or hide the chat ingame";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(Enums.Toggle.S).SetValue(Enums.Toggle.ToStr(Model.ToggleType)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.ToggleType = Enums.Toggle.ToInt(m_ChangeType.Element.GetValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.Chat)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("Chat: Action failed, Chat module is missing!");
                yield break;
            }

            switch ((Enums.Toggle.E)Model.ToggleType)
            {
                case Enums.Toggle.E.Toggle:
                    ChatPlexMod_Chat.Chat.Instance?.ToggleVisibility();
                    break;
                case Enums.Toggle.E.Enable:
                    ChatPlexMod_Chat.Chat.Instance?.SetVisible(true);
                    break;
                case Enums.Toggle.E.Disable:
                    ChatPlexMod_Chat.Chat.Instance?.SetVisible(false);
                    break;
            }

            yield return null;
        }
    }
}
