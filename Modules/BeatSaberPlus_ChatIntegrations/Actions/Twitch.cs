using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using CP_SDK.Chat.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class TwitchBuilder
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
                new Twitch_AddMarker(),
                new Twitch_CreateClip()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Twitch_AddMarker : Interfaces.IAction<Twitch_AddMarker, Models.Action>
    {
        public override string Description => "Add a marker on the video";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("CurrentMarkerText")]
        private HMUI.TextPageScrollView m_CurrentMarkerText = null;

        [UIComponent("ChatInputModal")]
        protected HMUI.ModalView m_ChatInputModal = null;
        [UIComponent("ChatInputModal_Text")]
        protected TextMeshProUGUI m_ChatInputModal_Text = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_ChatInputModal, 0.75f);

            /// Update UI
            UpdateUI();
        }
        private void UpdateUI()
        {
            m_CurrentMarkerText.SetText(Model.BaseValue);
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
            var l_Message   = Model.BaseValue;
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

            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel l_TwitchChannel)
            {
                var l_URL           = $"https://api.twitch.tv/helix/streams/markers";
                var l_APIClient     = new CP_SDK.Network.APIClient("", TimeSpan.FromSeconds(10), false);
                var l_OAuthToken    = (l_Channel.Item1 as CP_SDK.Chat.Services.Twitch.TwitchService).OAuthToken.Replace("oauth:", "");

                l_APIClient.InternalClient.DefaultRequestHeaders.Add("client-id",       ChatIntegrations.s_BEATSABERPLUS_CLIENT_ID);
                l_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization",   "Bearer " + l_OAuthToken);

                var l_Content = new JObject()
                {
                    ["user_id"]     = l_TwitchChannel.Roomstate.RoomId,
                    ["description"] = l_Message
                };
                var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8, "application/json");

                l_APIClient.PostAsync(l_URL, l_ContentStr, CancellationToken.None, true).ContinueWith((x) =>
                {
                    if (x.Result != null && !x.Result.IsSuccessStatusCode)
                        Logger.Instance.Error("[ChatIntegrations.Actions][Twitch.Twitch_AddMarker] Error:" + x.Result.BodyString);
                }).ConfigureAwait(false);
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Twitch_CreateClip : Interfaces.IAction<Twitch_CreateClip, Models.Action>
    {
        public override string Description => "Create clip, and put the edit URL in beatsaberplus_clips.txt";

        public Twitch_CreateClip() => UIPlaceHolder = "<b><i>Create clip, and put the edit URL in beatsaberplus_clips.txt</i></b>";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel l_TwitchChannel)
            {
                var l_URL = $"https://api.twitch.tv/helix/clips?broadcaster_id=" + l_TwitchChannel.Roomstate.RoomId;
                var l_APIClient = new CP_SDK.Network.APIClient("", TimeSpan.FromSeconds(10), false);
                var l_OAuthToken = (l_Channel.Item1 as CP_SDK.Chat.Services.Twitch.TwitchService).OAuthToken.Replace("oauth:", "");

                l_APIClient.InternalClient.DefaultRequestHeaders.Add("client-id",       ChatIntegrations.s_BEATSABERPLUS_CLIENT_ID);
                l_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization",   "Bearer " + l_OAuthToken);

                var l_ContentStr = new StringContent("{}", Encoding.UTF8, "application/json");

                l_APIClient.PostAsync(l_URL, l_ContentStr, CancellationToken.None, true).ContinueWith((x) =>
                {
                    if (x.Result == null)
                        return;

                    if (!x.Result.IsSuccessStatusCode)
                        Logger.Instance.Error("[ChatIntegrations.Actions][Twitch.Twitch_CreateClip] Error:" + x.Result.BodyString);
                    else
                    {
                        try
                        {
                            var l_JSON = JObject.Parse(x.Result.BodyString);
                            if (l_JSON != null && l_JSON.ContainsKey("data") && (l_JSON["data"][0] as JObject).ContainsKey("edit_url"))
                                System.IO.File.AppendAllLines("beatsaberplus_clips.txt", new List<string>() { l_JSON["data"][0]["edit_url"].Value<string>() ?? "invalid" });
                        }
                        catch
                        {

                        }
                    }
                }).ConfigureAwait(false);
            }

            yield return null;
        }
    }
}
