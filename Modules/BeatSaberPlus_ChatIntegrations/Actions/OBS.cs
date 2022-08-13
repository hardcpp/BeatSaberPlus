using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

using OBSService = CP_SDK.OBS.Service;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class OBSBuilder
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
                new OBS_RenameLastRecord(),
                new OBS_StartRecording(),
                new OBS_StartStreaming(),
                new OBS_SetRecordFilenameFormat(),
                new OBS_StopRecording(),
                new OBS_StopStreaming(),
                new OBS_SwitchPreviewToScene(),
                new OBS_SwitchToScene(),
                new OBS_ToggleStudioMode(),
                new OBS_ToggleSource(),
                new OBS_ToggleSourceAudio(),
                new OBS_Transition()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_RenameLastRecord : Interfaces.IAction<OBS_RenameLastRecord, Models.Actions.OBS_RenameLastRecord>
    {
        public override string Description => "Rename last record file";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("CurrentMessageText")]
        private HMUI.TextPageScrollView m_CurrentMessageText = null;

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
            m_CurrentMessageText.SetText(Model.Format);
        }

        [UIAction("click-set-game-btn-pressed")]
        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.IValueType.String || x.Item1 == Interfaces.IValueType.Integer || x.Item1 == Interfaces.IValueType.Floating).ToArray();
            var l_Keys = new List<(string, System.Action)>();

            l_Keys.Add(("$OriginalName", () => UI.Settings.Instance.UIInputKeyboardAppend("$OriginalName")));
            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => UI.Settings.Instance.UIInputKeyboardAppend("$" + l_Var.Item2)));

            UI.Settings.Instance.UIShowInputKeyboard(Model.Format, (p_Result) =>
            {
                Model.Format = p_Result;

                /// Update UI
                UpdateUI();

            }, l_Keys);
        }
        [UIAction("click-set-chat-btn-pressed")]
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.IValueType.String || x.Item1 == Interfaces.IValueType.Integer || x.Item1 == Interfaces.IValueType.Floating).ToList();
            var l_Message   = "Please input a message in chat with your streaming account.\nProvided values:\n";

            l_Variables.Insert(0, (Interfaces.IValueType.String, "$OriginalName"));
            l_Message += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

            m_ChatInputModal_Text.text = l_Message;

            m_ParserParams.EmitEvent("ShowChatInputModal");
        }
        private void Instance_OnBroadcasterChatMessage(CP_SDK.Chat.Interfaces.IChatMessage p_Message)
        {
            Model.Format = p_Message.Message;
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
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            var l_ExistingFile = OBSService.LastRecordedFileName;
            if (string.IsNullOrEmpty(l_ExistingFile) || !File.Exists(l_ExistingFile))
            {
                p_Context.HasActionFailed = true;
                yield break;
            }

            var l_Path      = Path.GetDirectoryName(l_ExistingFile);
            var l_Result    = Model.Format;
            var l_Variables = p_Context.GetValues(Interfaces.IValueType.String, Interfaces.IValueType.Integer, Interfaces.IValueType.Floating);
            l_Variables.Add((Interfaces.IValueType.String, "OriginalName"));

            for (int l_I = 0; l_I < l_Variables.Count; ++l_I)
            {
                var l_Var           = l_Variables[l_I];
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.IValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.IValueType.String && l_Var.Item2 == "OriginalName")
                    l_ReplaceValue = !string.IsNullOrEmpty(l_ExistingFile) ? Path.GetFileNameWithoutExtension(l_ExistingFile) : "";
                else if (l_Var.Item1 == Interfaces.IValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.IValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.IValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = string.Join("_", l_StringVal.Split(Path.GetInvalidFileNameChars()));

                l_Result = l_Result.Replace(l_Key, l_ReplaceValue);
            }

            var l_NewFile = Path.Combine(l_Path, l_Result + Path.GetExtension(l_ExistingFile));

            Task.Run(async () =>
            {
                /// Wait for OBS to finish IO
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);

                try
                {
                    if (File.Exists(l_NewFile))
                    {
                        l_NewFile = Path.Combine(l_Path, l_Result + CP_SDK.Misc.Time.UnixTimeNow() + Path.GetExtension(l_ExistingFile));
                        File.Move(l_ExistingFile, l_NewFile);
                    }
                    else
                        File.Move(l_ExistingFile, l_NewFile);
                }
                catch (Exception)
                {

                }
            }).ConfigureAwait(false);

            yield return null;
        }
    }

    public class OBS_StartRecording : Interfaces.IAction<OBS_StartRecording, Models.Action>
    {
        public override string Description => "Start recording";

        public OBS_StartRecording() { UIPlaceHolder = "Start recording"; UIPlaceHolderTestButton = true; }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            OBSService.StartRecording();

            yield return null;
        }
        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StartRecording();
        }
    }

    public class OBS_StartStreaming : Interfaces.IAction<OBS_StartStreaming, Models.Action>
    {
        public override string Description => "Start streaming";

        public OBS_StartStreaming() { UIPlaceHolder = "Start streaming"; UIPlaceHolderTestButton = true; }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            OBSService.StartStreaming();

            yield return null;
        }
        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StartStreaming();
        }
    }

    public class OBS_SetRecordFilenameFormat : Interfaces.IAction<OBS_SetRecordFilenameFormat, Models.Actions.OBS_SetRecordFilenameFormat>
    {
        public override string Description => "Set record filename format";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("CurrentMessageText")]
        private HMUI.TextPageScrollView m_CurrentMessageText = null;

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
            m_CurrentMessageText.SetText(Model.Format);
        }

        [UIAction("click-set-game-btn-pressed")]
        private void OnSetFromGameButton()
        {
            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.IValueType.String || x.Item1 == Interfaces.IValueType.Integer || x.Item1 == Interfaces.IValueType.Floating).ToArray();
            var l_Keys = new List<(string, System.Action)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => UI.Settings.Instance.UIInputKeyboardAppend("$" + l_Var.Item2)));

            UI.Settings.Instance.UIShowInputKeyboard(Model.Format, (p_Result) =>
            {
                Model.Format = p_Result;

                /// Update UI
                UpdateUI();

            }, l_Keys);
        }
        [UIAction("click-set-chat-btn-pressed")]
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Variables = Event.ProvidedValues.Where(x => x.Item1 == Interfaces.IValueType.String || x.Item1 == Interfaces.IValueType.Integer || x.Item1 == Interfaces.IValueType.Floating).ToArray();
            var l_Message   = "Please input a message in chat with your streaming account.\nProvided values:\n";
            l_Message      += string.Join(", ", l_Variables.Select(x => "$" + x.Item2).ToArray());

            m_ChatInputModal_Text.text = l_Message;

            m_ParserParams.EmitEvent("ShowChatInputModal");
        }
        private void Instance_OnBroadcasterChatMessage(CP_SDK.Chat.Interfaces.IChatMessage p_Message)
        {
            Model.Format = p_Message.Message;
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
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            var l_Result    = Model.Format;
            var l_Variables = p_Context.GetValues(Interfaces.IValueType.String, Interfaces.IValueType.Integer, Interfaces.IValueType.Floating);

            for (int l_I = 0; l_I < l_Variables.Count; ++l_I)
            {
                var l_Var           = l_Variables[l_I];
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.IValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.IValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.IValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.IValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = string.Join("_", l_StringVal.Split(System.IO.Path.GetInvalidFileNameChars()));

                l_Result = l_Result.Replace(l_Key, l_ReplaceValue);
            }

            OBSService.SetRecordFilenameFormat(l_Result);

            yield return null;
        }
    }

    public class OBS_StopRecording : Interfaces.IAction<OBS_StopRecording, Models.Action>
    {
        public override string Description => "Stop recording";

        public OBS_StopRecording() { UIPlaceHolder = "Stop recording"; UIPlaceHolderTestButton = true; }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            OBSService.StopRecording();

            yield return null;
        }
        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StopRecording();
        }
    }

    public class OBS_StopStreaming : Interfaces.IAction<OBS_StopStreaming, Models.Action>
    {
        public override string Description => "Stop streaming";

        public OBS_StopStreaming() { UIPlaceHolder = "Stop streaming"; UIPlaceHolderTestButton = true; }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            OBSService.StopStreaming();

            yield return null;
        }
        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StopStreaming();
        }
    }

    public class OBS_SwitchPreviewToScene : Interfaces.IAction<OBS_SwitchPreviewToScene, Models.Actions.OBS_SwitchPreviewToScene>
    {
        public override string Description => "Change active OBS scene";

#pragma warning disable CS0414
        [UIComponent("Scene_DropDown")]
        protected DropDownListSetting m_Scene_DropDown = null;
        [UIValue("Scene_DropDownOptions")]
        private List<object> m_Scene_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Scene_DropDown,  l_Event, true);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Scene_DropDown.UpdateChoices();
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.SceneName = m_Scene_DropDown.Value as string;
        }

        [UIAction("click-activescene-btn-pressed")]
        private void OnActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene_DropDown.Value = OBSService.ActiveScene?.name;
        }
        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                l_Scene.SetAsPreview();
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchPreviewToScene Scene:{Model.SceneName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                l_Scene.SetAsPreview();
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchPreviewToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    public class OBS_SwitchToScene : Interfaces.IAction<OBS_SwitchToScene, Models.Actions.OBS_SwitchToScene>
    {
        public override string Description => "Change active OBS scene";

#pragma warning disable CS0414
        [UIComponent("Scene_DropDown")]
        protected DropDownListSetting m_Scene_DropDown = null;
        [UIValue("Scene_DropDownOptions")]
        private List<object> m_Scene_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Scene_DropDown,  l_Event, true);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Scene_DropDown.UpdateChoices();
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.SceneName = m_Scene_DropDown.Value as string;
        }

        [UIAction("click-activescene-btn-pressed")]
        private void OnActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene_DropDown.Value = OBSService.ActiveScene?.name;
        }
        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                l_Scene.SwitchTo();
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchToScene Scene:{Model.SceneName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                l_Scene.SwitchTo();
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    public class OBS_ToggleStudioMode : Interfaces.IAction<OBS_ToggleStudioMode, Models.Actions.OBS_ToggleStudioMode>
    {
        public override string Description => "Enable or disable studio mode";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList, l_Event, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ToggleType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }
        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            switch (Model.ToggleType)
            {
                case 0:
                    if (OBSService.IsInStudioMode)
                        OBSService.DisableStudioMode();
                    else
                        OBSService.EnableStudioMode();
                    break;
                case 1:
                    OBSService.EnableStudioMode();
                    break;
                case 2:
                    OBSService.DisableStudioMode();
                    break;
            }
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            switch (Model.ToggleType)
            {
                case 0:
                    if (OBSService.IsInStudioMode)
                        OBSService.DisableStudioMode();
                    else
                        OBSService.EnableStudioMode();
                    break;
                case 1:
                    OBSService.EnableStudioMode();
                    break;
                case 2:
                    OBSService.DisableStudioMode();
                    break;
            }

            yield return null;
        }
    }

    public class OBS_ToggleSource : Interfaces.IAction<OBS_ToggleSource, Models.Actions.OBS_ToggleSource>
    {
        public override string Description => "Toggle source visibility";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;

        [UIComponent("Scene_DropDown")]
        protected DropDownListSetting m_Scene_DropDown = null;
        [UIValue("Scene_DropDownOptions")]
        private List<object> m_Scene_DropDownOptions = new List<object>() { "Loading...", };

        [UIComponent("Source_DropDown")]
        protected DropDownListSetting m_Source_DropDown = null;
        [UIValue("Source_DropDownOptions")]
        private List<object> m_Source_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event     = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));
            var l_EventSrc  = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChangedSrc), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(         m_TypeList,         l_Event,    true);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Scene_DropDown,   l_Event,    true);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Source_DropDown,  l_EventSrc, true);

            RebuildSceneList();
            RebuildSourceList();
        }

        private void OnSettingChanged(object p_Value)
        {
            var l_SceneChanged = Model.SceneName != m_Scene_DropDown.Value as string;

            Model.ToggleType    = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.SceneName     = m_Scene_DropDown.Value as string;

            if (l_SceneChanged)
                RebuildSourceList();
        }
        private void OnSettingChangedSrc(object p_Value)
        {
            Model.SourceName = m_Source_DropDown.Value as string;
        }

        private void RebuildSceneList()
        {
            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Scene_DropDown.UpdateChoices();
        }
        private void RebuildSourceList()
        {
            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                {
                    if (l_Scene.sources.Count != 0)
                    {
                        for (int l_I = 0;l_I < l_Scene.sources.Count; ++l_I)
                        {
                            var l_Source = l_Scene.sources[l_I];
                            l_Choices.Add(l_Source.name);

                            for (int l_Y = 0; l_Y < l_Source.groupChildren.Count; ++l_Y)
                                l_Choices.Add(l_Source.groupChildren[l_Y].name);
                        }
                    }
                }
                else
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SourceName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Source_DropDownOptions = l_Choices;
            m_Source_DropDown.values = l_Choices;
            m_Source_DropDown.Value  = l_Choices[l_ChoiceIndex];
            m_Source_DropDown.UpdateChoices();
        }

        [UIAction("click-activescene-btn-pressed")]
        private void OnActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene_DropDown.Value = OBSService.ActiveScene?.name;
            RebuildSourceList();
        }
        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetVisible(Model.ToggleType == 0 ? !l_Source.render : (Model.ToggleType == 1 ? true : false));
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetVisible(Model.ToggleType == 0 ? !l_Source.render : (Model.ToggleType == 1 ? true : false));
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    public class OBS_ToggleSourceAudio : Interfaces.IAction<OBS_ToggleSourceAudio, Models.Actions.OBS_ToggleSource>
    {
        public override string Description => "Toggle source audio";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;

        [UIComponent("Scene_DropDown")]
        protected DropDownListSetting m_Scene_DropDown = null;
        [UIValue("Scene_DropDownOptions")]
        private List<object> m_Scene_DropDownOptions = new List<object>() { "Loading...", };

        [UIComponent("Source_DropDown")]
        protected DropDownListSetting m_Source_DropDown = null;
        [UIValue("Source_DropDownOptions")]
        private List<object> m_Source_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event     = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));
            var l_EventSrc  = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChangedSrc), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(         m_TypeList,         l_Event,    true);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Scene_DropDown,   l_Event,    true);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Source_DropDown,  l_EventSrc, true);

            RebuildSceneList();
            RebuildSourceList();
        }

        private void OnSettingChanged(object p_Value)
        {
            var l_SceneChanged = Model.SceneName != m_Scene_DropDown.Value as string;

            Model.ToggleType    = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.SceneName     = m_Scene_DropDown.Value as string;

            if (l_SceneChanged)
                RebuildSourceList();
        }
        private void OnSettingChangedSrc(object p_Value)
        {
            Model.SourceName = m_Source_DropDown.Value as string;
        }

        private void RebuildSceneList()
        {
            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Scene_DropDown.UpdateChoices();
        }
        private void RebuildSourceList()
        {
            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                {
                    if (l_Scene.sources.Count != 0)
                    {
                        for (int l_I = 0;l_I < l_Scene.sources.Count; ++l_I)
                        {
                            var l_Source = l_Scene.sources[l_I];
                            l_Choices.Add(l_Source.name);

                            for (int l_Y = 0; l_Y < l_Source.groupChildren.Count; ++l_Y)
                                l_Choices.Add(l_Source.groupChildren[l_Y].name);
                        }
                    }
                }
                else
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SourceName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Source_DropDownOptions = l_Choices;
            m_Source_DropDown.values = l_Choices;
            m_Source_DropDown.Value  = l_Choices[l_ChoiceIndex];
            m_Source_DropDown.UpdateChoices();
        }

        [UIAction("click-activescene-btn-pressed")]
        private void OnActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene_DropDown.Value = OBSService.ActiveScene?.name;
            RebuildSourceList();
        }
        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetMuted(Model.ToggleType == 0 ? false : true);
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetMuted(Model.ToggleType == 0 ? false : true);
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    public class OBS_Transition : Interfaces.IAction<OBS_Transition, Models.Actions.OBS_Transition>
    {
        public override string Description => "Transition between preview to active";

#pragma warning disable CS0414

        [UIComponent("OverrideDuration")]
        private ToggleSetting m_OverrideDuration = null;
        [UIComponent("Duration")]
        private SliderSetting m_Duration = null;
        [UIComponent("OverrideTransition")]
        private ToggleSetting m_OverrideTransition = null;
        [UIComponent("Transition_DropDown")]
        protected DropDownListSetting m_Transition_DropDown = null;
        [UIValue("Transition_DropDown")]
        private List<object> m_Transition_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event     = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(       m_OverrideDuration,     l_Event,        Model.OverrideDuration,     true);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(       m_Duration,             l_Event, null,  Model.Duration,             true, true, new Vector2(0.08f, 0.10f), new Vector2(0.93f, 0.90f));
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(       m_OverrideTransition,   l_Event,        Model.OverrideTransition,   true);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Transition_DropDown,  l_Event,                                    true);

            RebuildTransitionList();

            m_Duration.interactable             = Model.OverrideDuration;
            m_Transition_DropDown.interactable  = Model.OverrideTransition;
        }

        private void OnSettingChanged(object p_Value)
        {
            Model.OverrideDuration      = m_OverrideDuration.Value;
            Model.Duration              = (int)m_Duration.Value;
            Model.OverrideTransition    = m_OverrideTransition.Value;
            Model.Transition            = m_Transition_DropDown.Value as string;

            m_Duration.interactable             = Model.OverrideDuration;
            m_Transition_DropDown.interactable  = Model.OverrideTransition;
        }

        private void RebuildTransitionList()
        {
            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Transitions.ToList<object>());
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.Transition)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Transition_DropDownOptions = l_Choices;
            m_Transition_DropDown.values = l_Choices;
            m_Transition_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Transition_DropDown.UpdateChoices();
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (Model.OverrideDuration && Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(Model.Duration, Model.Transition);
            else if (Model.OverrideDuration)
                OBSService.PreviewTransitionToScene(Model.Duration);
            else if (Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(-1, Model.Transition);
            else
                OBSService.PreviewTransitionToScene();
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                yield break;
            }


            if (Model.OverrideDuration && Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(Model.Duration, Model.Transition);
            else if (Model.OverrideDuration)
                OBSService.PreviewTransitionToScene(Model.Duration);
            else if (Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(-1, Model.Transition);
            else
                OBSService.PreviewTransitionToScene();

            yield return null;
        }
    }
}
