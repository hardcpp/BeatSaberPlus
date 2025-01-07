using CP_SDK.Chat.Interfaces;
using CP_SDK.OBS.Models;
using CP_SDK.XUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

using OBSService = CP_SDK.OBS.Service;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class OBSRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("OBS_RenameLastRecord",           () => new OBS_RenameLastRecord());
            ChatIntegrations.RegisterActionType("OBS_StartRecording",             () => new OBS_StartRecording());
            ChatIntegrations.RegisterActionType("OBS_StartStreaming",             () => new OBS_StartStreaming());
            ChatIntegrations.RegisterActionType("OBS_SetRecordFilenameFormat",    () => new OBS_SetRecordFilenameFormat());
            ChatIntegrations.RegisterActionType("OBS_StopRecording",              () => new OBS_StopRecording());
            ChatIntegrations.RegisterActionType("OBS_StopStreaming",              () => new OBS_StopStreaming());
            ChatIntegrations.RegisterActionType("OBS_SwitchPreviewToScene",       () => new OBS_SwitchPreviewToScene());
            ChatIntegrations.RegisterActionType("OBS_SwitchToScene",              () => new OBS_SwitchToScene());
            ChatIntegrations.RegisterActionType("OBS_ToggleStudioMode",           () => new OBS_ToggleStudioMode());
            ChatIntegrations.RegisterActionType("OBS_ToggleSource",               () => new OBS_ToggleSource());
            ChatIntegrations.RegisterActionType("OBS_ToggleSourceAudio",          () => new OBS_ToggleSourceAudio());
            ChatIntegrations.RegisterActionType("OBS_Transition",                 () => new OBS_Transition());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_RenameLastRecord
        : Interfaces.IAction<OBS_RenameLastRecord, Models.Actions.OBS_RenameLastRecord>
    {
        private XUIText m_Format = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Rename last record file";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("Marker name")
                        .SetColor(Color.yellow),

                    XUIText.Make(Model.Format)
                        .Bind(ref m_Format)
                )
                .SetBackground(true),

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
            var l_Keys = new List<(string, System.Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.Format, (p_Result) =>
            {
                Model.Format = p_Result;
                m_Format.SetText(Model.Format);
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
            Model.Format = p_Message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            View.CloseLoadingModal();

            m_Format.SetText(Model.Format);
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
            var l_Variables = p_Context.GetValues(Interfaces.EValueType.String, Interfaces.EValueType.Integer, Interfaces.EValueType.Floating);
            l_Variables.Add((Interfaces.EValueType.String, "OriginalName"));

            for (int l_I = 0; l_I < l_Variables.Count; ++l_I)
            {
                var l_Var           = l_Variables[l_I];
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.EValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.EValueType.String && l_Var.Item2 == "OriginalName")
                    l_ReplaceValue = !string.IsNullOrEmpty(l_ExistingFile) ? Path.GetFileNameWithoutExtension(l_ExistingFile) : "";
                else if (l_Var.Item1 == Interfaces.EValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
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

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_StartRecording
        : Interfaces.IAction<OBS_StartRecording, Models.Action>
    {
        public override string  Description             => "Start recording";
        public override string  UIPlaceHolder           => "Start recording";
        public override bool    UIPlaceHolderTestButton => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StartRecording();
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

            OBSService.StartRecording();

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_StartStreaming
        : Interfaces.IAction<OBS_StartStreaming, Models.Action>
    {
        public override string  Description             => "Start streaming";
        public override string  UIPlaceHolder           => "Start streaming";
        public override bool    UIPlaceHolderTestButton => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StartStream();
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

            OBSService.StartStream();

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SetRecordFilenameFormat
        : Interfaces.IAction<OBS_SetRecordFilenameFormat, Models.Actions.OBS_SetRecordFilenameFormat>
    {
        private XUIText m_MarkerName = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Set record filename format";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("Marker name")
                        .SetColor(Color.yellow),

                    XUIText.Make(Model.Format)
                        .Bind(ref m_MarkerName)
                )
                .SetBackground(true),

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
            var l_Keys = new List<(string, System.Action, string)>();

            foreach (var l_Var in l_Variables)
                l_Keys.Add(("$" + l_Var.Item2, () => View.KeyboardModal_Append("$" + l_Var.Item2), null));

            View.ShowKeyboardModal(Model.Format, (p_Result) =>
            {
                Model.Format = p_Result;
                m_MarkerName.SetText(Model.Format);
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
            Model.Format = p_Message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            View.CloseLoadingModal();

            m_MarkerName.SetText(Model.Format);
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
            var l_Variables = p_Context.GetValues(Interfaces.EValueType.String, Interfaces.EValueType.Integer, Interfaces.EValueType.Floating);

            for (int l_I = 0; l_I < l_Variables.Count; ++l_I)
            {
                var l_Var           = l_Variables[l_I];
                var l_Key           = "$" + l_Var.Item2;
                var l_ReplaceValue  = l_Var.Item1 == Interfaces.EValueType.String ? "" : "0";

                if (l_Var.Item1 == Interfaces.EValueType.Integer && p_Context.GetIntegerValue(l_Var.Item2, out var l_IntegerVal))
                    l_ReplaceValue = l_IntegerVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.Floating && p_Context.GetFloatingValue(l_Var.Item2, out var l_FloatVal))
                    l_ReplaceValue = l_FloatVal.Value.ToString();
                else if (l_Var.Item1 == Interfaces.EValueType.String && p_Context.GetStringValue(l_Var.Item2, out var l_StringVal))
                    l_ReplaceValue = string.Join("_", l_StringVal.Split(System.IO.Path.GetInvalidFileNameChars()));

                l_Result = l_Result.Replace(l_Key, l_ReplaceValue);
            }

            OBSService.SetProfileParameter_Output_FilenameFormatting(l_Result);

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_StopRecording
        : Interfaces.IAction<OBS_StopRecording, Models.Action>
    {
        public override string  Description             => "Stop recording";
        public override string  UIPlaceHolder           => "Stop recording";
        public override bool    UIPlaceHolderTestButton => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StopRecording();
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

            OBSService.StopRecording();

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_StopStreaming
        : Interfaces.IAction<OBS_StopStreaming, Models.Action>
    {
        public override string  Description             => "Stop streaming";
        public override string  UIPlaceHolder           => "Stop streaming";
        public override bool    UIPlaceHolderTestButton => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnUIPlaceholderTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            OBSService.StopStream();
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

            OBSService.StopStream();

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SwitchPreviewToScene
        : Interfaces.IAction<OBS_SwitchPreviewToScene, Models.Actions.OBS_SwitchPreviewToScene>
    {
        private XUIDropdown m_Scene = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change active preview scene in OBS";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Selected = "";
            var l_Choices  = new List<string>() { "<i>None</i>" };

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Values.Select(x => x.sceneName));
            else
            {
                XUIElements = new IXUIElement[]
                {
                    XUIText.Make("OBS is not connected!")
                        .SetColor(Color.red)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                };

                BuildUIAuto(p_Parent);
                return;
            }

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] != Model.SceneName)
                    continue;

                l_Selected = l_Choices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                XUIText.Make(Description),
                XUIDropdown.Make().SetOptions(l_Choices).SetValue(l_Selected)
                    .OnValueChanged((_, __) => OnSettingChanged()).Bind(ref m_Scene),

                XUISecondaryButton.Make("Select active scene", OnSelectActiveSceneButton),
                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            Model.SceneName = m_Scene.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
            => m_Scene.SetValue(OBSService.ActiveProgramScene?.sceneName);
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                l_Scene.SetCurrentPreview();
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchPreviewToScene Scene:{Model.SceneName} not found!");
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

            if (OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                l_Scene.SetCurrentPreview();
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchPreviewToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SwitchToScene
        : Interfaces.IAction<OBS_SwitchToScene, Models.Actions.OBS_SwitchToScene>
    {
        private XUIDropdown m_Scene = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change active scene in OBS";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Selected = "";
            var l_Choices  = new List<string>() { "<i>None</i>" };

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Values.Select(x => x.sceneName));
            else
            {
                XUIElements = new IXUIElement[]
                {
                    XUIText.Make("OBS is not connected!")
                        .SetColor(Color.red)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                };

                BuildUIAuto(p_Parent);
                return;
            }

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] != Model.SceneName)
                    continue;

                l_Selected = l_Choices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                XUIText.Make(Description),
                XUIDropdown.Make().SetOptions(l_Choices).SetValue(l_Selected)
                    .OnValueChanged((_, __) => OnSettingChanged()).Bind(ref m_Scene),

                XUISecondaryButton.Make("Select active scene", OnSelectActiveSceneButton),
                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            Model.SceneName = m_Scene.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
            => m_Scene.SetValue(OBSService.ActiveProgramScene?.sceneName);
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                l_Scene.SetCurrentProgram();
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchToScene Scene:{Model.SceneName} not found!");
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

            if (OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                l_Scene.SetCurrentProgram();
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_SwitchToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleStudioMode : Interfaces.IAction<OBS_ToggleStudioMode, Models.Actions.OBS_ToggleStudioMode>
    {
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Enable or disable studio mode";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Change type"),
                XUIDropdown.Make().SetOptions(Enums.Toggle.S).SetValue(Enums.Toggle.ToStr(Model.ChangeType))
                    .OnValueChanged((_, __) => OnSettingChanged()).Bind(ref m_ChangeType),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.ChangeType = Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            switch (Model.ChangeType)
            {
                case Enums.Toggle.E.Toggle:
                    if (OBSService.IsInStudioMode)
                        OBSService.DisableStudioMode();
                    else
                        OBSService.EnableStudioMode();
                    break;
                case Enums.Toggle.E.Enable:
                    OBSService.EnableStudioMode();
                    break;
                case Enums.Toggle.E.Disable:
                    OBSService.DisableStudioMode();
                    break;
            }
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

            switch (Model.ChangeType)
            {
                case Enums.Toggle.E.Toggle:
                    if (OBSService.IsInStudioMode)
                        OBSService.DisableStudioMode();
                    else
                        OBSService.EnableStudioMode();
                    break;
                case Enums.Toggle.E.Enable:
                    OBSService.EnableStudioMode();
                    break;
                case Enums.Toggle.E.Disable:
                    OBSService.DisableStudioMode();
                    break;
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleSource
        : Interfaces.IAction<OBS_ToggleSource, Models.Actions.OBS_ToggleSource>
    {
        private XUIDropdown m_ChangeType    = null;
        private XUIDropdown m_Scene         = null;
        private XUIDropdown m_Source        = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<string>    m_SourceChoicesText = null;
        private List<SceneItem> m_SourceChoicesItem = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Toggle source visibility";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                XUIElements = new IXUIElement[]
                {
                    XUIText.Make("OBS is not connected!")
                        .SetColor(Color.red)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                };

                BuildUIAuto(p_Parent);
                return;
            }

            var l_SceneChoices = new List<string>() { "<i>None</i>" };
            var l_SelectedScene = "<i>None</i>";

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_SceneChoices.AddRange(OBSService.Scenes.Values.Select(x => x.sceneName));

            for (int l_I = 0; l_I < l_SceneChoices.Count; ++l_I)
            {
                if (l_SceneChoices[l_I] != Model.SceneName)
                    continue;

                l_SelectedScene = l_SceneChoices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Change type").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .SetOptions(Enums.Toggle.S).SetValue(Enums.Toggle.ToStr(Model.ChangeType)).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_ChangeType),

                XUIText.Make("Scene").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .SetOptions(l_SceneChoices).SetValue(l_SelectedScene).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_Scene),

                XUIText.Make("Source").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .OnValueChanged((_, __) => OnSettingChangedSrc())
                    .Bind(ref m_Source),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Select active scene",    OnSelectActiveSceneButton),
                    XUIPrimaryButton.Make("Test",                   OnTestButton)
                )
            };

            BuildUIAuto(p_Parent);

            UpdateSourceSelection();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            var l_SceneChanged = Model.SceneName != m_Scene.Element.GetValue();

            Model.ChangeType    = Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());
            Model.SceneName     = m_Scene.Element.GetValue();

            if (l_SceneChanged)
                UpdateSourceSelection();
        }
        private void OnSettingChangedSrc()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            Model.SourceName = m_Source.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void RebuildSourceList()
        {
            var l_SourceChoicesText     = new List<string>() { "<i>None</i>" };
            var l_SourceChoicesItem     = new List<SceneItem>() { null };

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                {
                    if (Model.SceneName != "<i>None</i>")
                        CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");

                    return;
                }

                for (int l_I = 0; l_I < l_Scene.sceneItems.Count; ++l_I)
                {
                    var l_Source = l_Scene.sceneItems[l_I];
                    l_SourceChoicesText.Add(l_Source.sourceName);
                    l_SourceChoicesItem.Add(l_Source);

                    if (l_Source.SubItems?.Count > 0)
                    {
                        for (int l_Y = 0; l_Y < l_Source.SubItems.Count; ++l_Y)
                        {
                            var l_SubSource = l_Source.SubItems[l_Y];
                            l_SourceChoicesText.Add($"⟼{l_SubSource.sourceName}");
                            l_SourceChoicesItem.Add(l_SubSource);
                        }
                    }
                }
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            m_SourceChoicesText = l_SourceChoicesText;
            m_SourceChoicesItem = l_SourceChoicesItem;
        }
        private void UpdateSourceSelection()
        {
            if (m_SourceChoicesItem == null)
                RebuildSourceList();

            var l_SelectedSource = "<i>None</i>";
            for (int l_I = 0; l_I < m_SourceChoicesText.Count; ++l_I)
            {
                if (m_SourceChoicesText[l_I] != Model.SourceName)
                    continue;

                l_SelectedSource = m_SourceChoicesText[l_I];
                break;
            }

            m_Source.SetOptions(m_SourceChoicesText).SetValue(l_SelectedSource);
        }
        private SceneItem FindSource()
        {
            if (m_SourceChoicesItem == null)
                RebuildSourceList();

            var l_SceneItem = null as SceneItem;
            for (int l_I = 0; l_I < m_SourceChoicesText.Count; ++l_I)
            {
                if (m_SourceChoicesText[l_I] != Model.SourceName)
                    continue;

                l_SceneItem = m_SourceChoicesItem[l_I];
                break;
            }

            return l_SceneItem;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveProgramScene?.sceneName;
            m_Scene.SetValue(OBSService.ActiveProgramScene?.sceneName);
        }
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
            {
                if (Model.SceneName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");

                return;
            }

            var l_Source = FindSource();
            if (l_Source != null)
                l_Source.SetEnabled(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.sceneItemEnabled : (Model.ChangeType == Enums.Toggle.E.Enable));
            else if (Model.SourceName != "<i>None</i>")
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Source:{Model.SourceName} not found!");
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

            if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
            {
                p_Context.HasActionFailed = true;

                if (Model.SceneName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");

                yield return null;
            }

            var l_Source = FindSource();
            if (l_Source != null)
                l_Source.SetEnabled(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.sceneItemEnabled : (Model.ChangeType == Enums.Toggle.E.Enable));
            else
            {
                p_Context.HasActionFailed = true;

                if (Model.SourceName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Source:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleSourceAudio
        : Interfaces.IAction<OBS_ToggleSourceAudio, Models.Actions.OBS_ToggleSourceAudio>
    {
        private XUIDropdown m_ChangeType    = null;
        private XUIDropdown m_Scene         = null;
        private XUIDropdown m_Source        = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<string>    m_SourceChoicesText = null;
        private List<SceneItem> m_SourceChoicesItem = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Toggle source audio";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                XUIElements = new IXUIElement[]
                {
                    XUIText.Make("OBS is not connected!")
                        .SetColor(Color.red)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                };

                BuildUIAuto(p_Parent);
                return;
            }

            var l_SceneChoices = new List<string>() { "<i>None</i>" };
            var l_SelectedScene = "<i>None</i>";

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_SceneChoices.AddRange(OBSService.Scenes.Values.Select(x => x.sceneName));

            for (int l_I = 0; l_I < l_SceneChoices.Count; ++l_I)
            {
                if (l_SceneChoices[l_I] != Model.SceneName)
                    continue;

                l_SelectedScene = l_SceneChoices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Change type").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .SetOptions(Enums.Toggle.S).SetValue(Enums.Toggle.ToStr(Model.ChangeType)).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_ChangeType),

                XUIText.Make("Scene").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .SetOptions(l_SceneChoices).SetValue(l_SelectedScene).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_Scene),

                XUIText.Make("Source").SetColor(Color.yellow),
                XUIDropdown.Make()
                    .OnValueChanged((_, __) => OnSettingChangedSrc())
                    .Bind(ref m_Source),

                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Select active scene",    OnSelectActiveSceneButton),
                    XUIPrimaryButton.Make("Test",                   OnTestButton)
                )
            };

            BuildUIAuto(p_Parent);

            UpdateSourceSelection();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            var l_SceneChanged = Model.SceneName != m_Scene.Element.GetValue();

            Model.ChangeType    = Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());
            Model.SceneName     = m_Scene.Element.GetValue();

            if (l_SceneChanged)
                UpdateSourceSelection();
        }
        private void OnSettingChangedSrc()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            Model.SourceName = m_Source.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void RebuildSourceList()
        {
            var l_SourceChoicesText = new List<string>() { "<i>None</i>" };
            var l_SourceChoicesItem = new List<SceneItem>() { null };

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
                {
                    if (Model.SceneName != "<i>None</i>")
                        CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");

                    return;
                }

                for (int l_I = 0; l_I < l_Scene.sceneItems.Count; ++l_I)
                {
                    var l_Source = l_Scene.sceneItems[l_I];
                    l_SourceChoicesText.Add(l_Source.sourceName);
                    l_SourceChoicesItem.Add(l_Source);

                    if (l_Source.SubItems?.Count > 0)
                    {
                        for (int l_Y = 0; l_Y < l_Source.SubItems.Count; ++l_Y)
                        {
                            var l_SubSource = l_Source.SubItems[l_Y];
                            l_SourceChoicesText.Add($"⟼{l_SubSource.sourceName}");
                            l_SourceChoicesItem.Add(l_SubSource);
                        }
                    }
                }
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            m_SourceChoicesText = l_SourceChoicesText;
            m_SourceChoicesItem = l_SourceChoicesItem;
        }
        private void UpdateSourceSelection()
        {
            if (m_SourceChoicesItem == null)
                RebuildSourceList();

            var l_SelectedSource = "<i>None</i>";
            for (int l_I = 0; l_I < m_SourceChoicesText.Count; ++l_I)
            {
                if (m_SourceChoicesText[l_I] != Model.SourceName)
                    continue;

                l_SelectedSource = m_SourceChoicesText[l_I];
                break;
            }

            m_Source.SetOptions(m_SourceChoicesText).SetValue(l_SelectedSource);
        }
        private SceneItem FindSource()
        {
            if (m_SourceChoicesItem == null)
                RebuildSourceList();

            var l_SceneItem = null as SceneItem;
            for (int l_I = 0; l_I < m_SourceChoicesText.Count; ++l_I)
            {
                if (m_SourceChoicesText[l_I] != Model.SourceName)
                    continue;

                l_SceneItem = m_SourceChoicesItem[l_I];
                break;
            }

            return l_SceneItem;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveProgramScene?.sceneName;
            m_Scene.SetValue(OBSService.ActiveProgramScene?.sceneName);
        }
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
            {
                if (Model.SceneName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");

                return;
            }

            var l_Source = FindSource();
            if (l_Source != null)
            {
                if (Model.ChangeType == Enums.Toggle.E.Toggle)
                    l_Source.ToggleMute();
                else
                    l_Source.SetMuted(Model.ChangeType == Enums.Toggle.E.Enable);
            }
            else if (Model.SourceName != "<i>None</i>")
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Source:{Model.SourceName} not found!");
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

            if (!OBSService.TryGetSceneByName(Model.SceneName, out var l_Scene))
            {
                p_Context.HasActionFailed = true;

                if (Model.SceneName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");

                yield return null;
            }

            var l_Source = FindSource();
            if (l_Source != null)
            {
                if (Model.ChangeType == Enums.Toggle.E.Toggle)
                    l_Source.ToggleMute();
                else
                    l_Source.SetMuted(Model.ChangeType == Enums.Toggle.E.Enable);
            }
            else
            {
                p_Context.HasActionFailed = true;

                if (Model.SourceName != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Source:{Model.SourceName} not found!");
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_Transition
        : Interfaces.IAction<OBS_Transition, Models.Actions.OBS_Transition>
    {
        private XUIToggle   m_OverrideDuration      = null;
        private XUISlider   m_Duration              = null;
        private XUIToggle   m_OverrideTransition    = null;
        private XUIDropdown m_Transition            = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Transition between preview to active";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_TransitionChoices     = new List<string>() { "<i>None</i>" };
            var l_SelectedTransition    = "<i>None</i>";

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_TransitionChoices.AddRange(OBSService.Transitions.Values.Select(x => x.transitionName));
            else
            {
                XUIElements = new IXUIElement[]
                {
                    XUIText.Make("OBS is not connected!")
                        .SetColor(Color.red)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                };

                BuildUIAuto(p_Parent);
                return;
            }

            for (int l_I = 0; l_I < l_TransitionChoices.Count; ++l_I)
            {
                if (l_TransitionChoices[l_I] != Model.Transition)
                    continue;

                l_SelectedTransition = l_TransitionChoices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                XUIText.Make("Override duration"),
                XUIToggle.Make()
                    .SetValue(Model.OverrideDuration).OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_OverrideDuration),

                XUIText.Make("Duration"),
                XUISlider.Make()
                    .SetMinValue(0.0f).SetMaxValue(10000.0f).SetIncrements(1.0f).SetInteger(true)
                    .SetValue(Model.Duration).OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_Duration),

                XUIText.Make("Override transition"),
                XUIToggle.Make()
                    .SetValue(Model.OverrideTransition).OnValueChanged((_) => OnSettingChanged())
                    .Bind(ref m_OverrideTransition),

                XUIText.Make("Transition"),
                XUIDropdown.Make()
                    .SetOptions(l_TransitionChoices).SetValue(l_SelectedTransition).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_Transition),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
            OnSettingChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            /// Do not saved if OBS is not connected
            if (OBSService.Status != OBSService.EStatus.Connected)
                return;

            Model.OverrideDuration      = m_OverrideDuration.Element.GetValue();
            Model.Duration              = (int)m_Duration.Element.GetValue();
            Model.OverrideTransition    = m_OverrideTransition.Element.GetValue();
            Model.Transition            = m_Transition.Element.GetValue();

            m_Duration.SetInteractable(Model.OverrideDuration);
            m_Transition.SetInteractable(Model.OverrideTransition);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            var l_Transition = null as CP_SDK.OBS.Models.Transition;
            if (Model.OverrideTransition && !OBSService.TryGetTransitionByName(Model.Transition, out l_Transition))
            {
                if (Model.Transition != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_Transition Transition:{Model.Transition} not found!");

                return;
            }

            if (Model.OverrideDuration && Model.OverrideTransition)
                OBSService.CustomStudioModeTransition(Model.Duration, l_Transition);
            else if (Model.OverrideDuration)
                OBSService.CustomStudioModeTransition(Model.Duration);
            else if (Model.OverrideTransition)
                OBSService.CustomStudioModeTransition(-1, l_Transition);
            else
                OBSService.CustomStudioModeTransition();
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

            var l_Transition = null as CP_SDK.OBS.Models.Transition;
            if (Model.OverrideTransition && !OBSService.TryGetTransitionByName(Model.Transition, out l_Transition))
            {
                p_Context.HasActionFailed = true;

                if (Model.Transition != "<i>None</i>")
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_Transition Transition:{Model.Transition} not found!");

                yield return null;
            }

            if (Model.OverrideDuration && Model.OverrideTransition)
                OBSService.CustomStudioModeTransition(Model.Duration, l_Transition);
            else if (Model.OverrideDuration)
                OBSService.CustomStudioModeTransition(Model.Duration);
            else if (Model.OverrideTransition)
                OBSService.CustomStudioModeTransition(-1, l_Transition);
            else
                OBSService.CustomStudioModeTransition();

            yield return null;
        }
    }
}
