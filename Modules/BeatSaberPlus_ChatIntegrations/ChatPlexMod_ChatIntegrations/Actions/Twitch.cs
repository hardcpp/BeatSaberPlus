using CP_SDK.Chat.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class TwitchRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("Twitch_AddMarker",  () => new Twitch_AddMarker());
            ChatIntegrations.RegisterActionType("Twitch_CreateClip", () => new Twitch_CreateClip());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Twitch_AddMarker
        : Interfaces.IAction<Twitch_AddMarker, Models.Action>
    {
        private XUIText m_MarkerName = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add a marker on the video";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Marker name",
                    XUIText.Make(Model.BaseValue)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_MarkerName)
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

        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Keys      = new List<(string, Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.BaseValue, (p_Result) =>
            {
                Model.BaseValue = p_Result;
                m_MarkerName.SetText(Model.BaseValue);
            }, null, l_Keys);
        }
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.EValueType.String || x.Item1 == Interfaces.EValueType.Integer || x.Item1 == Interfaces.EValueType.Floating).ToArray();
            var l_Message   = "Please input a message in chat with your streaming account.\nProvided values:\n";
            l_Message      += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

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

            m_MarkerName.SetText(Model.BaseValue);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            var l_Message   = Model.BaseValue;
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

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                var l_HelixAPI = (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI;
                l_HelixAPI.CreateMarker(l_Message, null);
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Twitch_CreateClip
        : Interfaces.IAction<Twitch_CreateClip, Models.Action>
    {
        public override string Description   => $"Create clip, and put the edit URL in {CP_SDK.ChatPlexSDK.ProductName}Plus_TwitchClips.txt";
        public override string UIPlaceHolder => $"<b><i>Create clip, and put the edit URL in {CP_SDK.ChatPlexSDK.ProductName}Plus_TwitchClips.txt</i></b>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            if (l_TwitchService != null)
            {
                var l_HelixAPI = (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI;
                l_HelixAPI.CreateClip((p_Status, p_Result) =>
                {
                    if (p_Status != CP_SDK.Chat.Services.Twitch.TwitchHelixResult.OK)
                        return;

                    try { System.IO.File.AppendAllLines($"{CP_SDK.ChatPlexSDK.ProductName}Plus_TwitchClips.txt", new List<string>() { p_Result?.edit_url ?? "invalid" }); }
                    catch { }
                });
            }

            yield return null;
        }
    }
}
