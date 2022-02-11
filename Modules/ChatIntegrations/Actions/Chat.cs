using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using BeatSaberPlus_ChatIntegrations.Models;
using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class ChatBuilder
    {
        internal static List<Interfaces.IActionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            switch (p_Event)
            {
                default:
                    break;
            }

            return new List<Interfaces.IActionBase>()
            {
                new Chat_SendMessage(),
                new Chat_ToggleEmoteOnly()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Chat_SendMessage : Interfaces.IAction<Chat_SendMessage, Models.Actions.Chat_SendMessage>
    {
        public override string Description => "Send a message in the chat";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("CurrentMessageText")]
        private HMUI.TextPageScrollView m_CurrentMessageText = null;
        [UIComponent("AddTTSPrefixToggle")]
        private ToggleSetting m_AddTTSPrefixToggle = null;

        [UIComponent("ChatInputModal")]
        protected HMUI.ModalView m_ChatInputModal = null;
        [UIComponent("ChatInputModal_Text")]
        protected TextMeshProUGUI m_ChatInputModal_Text = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_AddTTSPrefixToggle, l_Event, Model.AddTTSPefix, false);

            /// Change opacity
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_ChatInputModal, 0.75f);

            /// Update UI
            UpdateUI();
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.AddTTSPefix = m_AddTTSPrefixToggle.Value;
        }
        private void UpdateUI()
        {
            m_CurrentMessageText.SetText(Model.BaseValue);
        }

        [UIAction("click-set-game-btn-pressed")]
        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == IValueType.String || x.Item1 == IValueType.Integer || x.Item1 == IValueType.Floating).ToArray();
            var l_Keys = new System.Collections.Generic.List<(string, System.Action)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => UI.Settings.Instance.UIInputKeyboardAppend("$" + l_Var.Item2)));

            UI.Settings.Instance.UIShowInputKeyboard(Model.BaseValue, (p_Result) =>
            {
                Model.BaseValue = p_Result;

                /// Update UI
                UpdateUI();

            }, l_Keys);
        }
        [UIAction("click-set-chat-btn-pressed")]
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == IValueType.String || x.Item1 == IValueType.Integer || x.Item1 == IValueType.Floating).ToArray();
            var l_Message   = "Please input a message in chat with your streaming account.\nProvided values:\n";
            l_Message      += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

            m_ChatInputModal_Text.text = l_Message;

            m_ParserParams.EmitEvent("ShowChatInputModal");
        }
        private void Instance_OnBroadcasterChatMessage(IChatMessage p_Message)
        {
            Model.BaseValue = p_Message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            m_ParserParams.EmitEvent("CloseChatInputModal");

            UpdateUI();
        }
        [UIAction("click-cancel-set-chat-btn-pressed")]
        private void OnCancelSetFromChatButton()
        {
            m_ParserParams.EmitEvent("CloseChatInputModal");
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            /// Temp
            Model.BaseValue = Model.BaseValue.Replace("$SenderName", "$UserName");

            var l_Message   = (Model.AddTTSPefix ? "! " : "") +  Model.BaseValue;
            var l_Variables = p_Context.GetValues(IValueType.String, IValueType.Integer, IValueType.Floating);

            foreach (var l_Var in l_Variables)
            {
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == IValueType.String ? "" : "0";

                if (l_Var.Item1 == IValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == IValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == IValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = l_StringVal;

                l_Message = l_Message.Replace(l_Key, l_ReplaceValue);
            }

            if (p_Context.ChatService != null && p_Context.Channel != null)
                p_Context.ChatService.SendTextMessage(p_Context.Channel, l_Message);
            else
                BeatSaberPlus.SDK.Chat.Service.BroadcastMessage(l_Message);

            yield return null;
        }
    }

    public class Chat_ToggleEmoteOnly : Interfaces.IAction<Chat_ToggleEmoteOnly, Models.Action>
    {
        public override string Description => "Enable or disable emote only in the chat";

        public Chat_ToggleEmoteOnly() => UIPlaceHolder = "Enable or disable emote only in the chat";

        public override IEnumerator Eval(EventContext p_Context)
        {
            var l_Channel = BeatSaberPlus.SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is BeatSaberPlus.SDK.Chat.Models.Twitch.TwitchChannel l_TwitchChannel)
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
}
