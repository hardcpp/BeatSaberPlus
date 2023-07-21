using CP_SDK.Chat.Interfaces;
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

            OBSService.StartStreaming();
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

            OBSService.StartStreaming();

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

            OBSService.SetRecordFilenameFormat(l_Result);

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

            OBSService.StopStreaming();
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

            OBSService.StopStreaming();

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
                l_Choices.AddRange(OBSService.Scenes.Keys);
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
                XUIText.Make("Dummy event to execute"),
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
            => m_Scene.SetValue(OBSService.ActiveScene?.name);
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
                l_Choices.AddRange(OBSService.Scenes.Keys);
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
                XUIText.Make("Dummy event to execute"),
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
            => m_Scene.SetValue(OBSService.ActiveScene?.name);
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
                l_SceneChoices.AddRange(OBSService.Scenes.Keys.ToList<string>());

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

            RebuildSourceList();
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
                RebuildSourceList();
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
            var l_SourceChoices     = new List<string>() { "<i>None</i>" };
            var l_SelectedSource    = "<i>None</i>";

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (!OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                {
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
                    return;
                }

                for (int l_I = 0;l_I < l_Scene.sources.Count; ++l_I)
                {
                    var l_Source = l_Scene.sources[l_I];
                    l_SourceChoices.Add(l_Source.name);

                    for (int l_Y = 0; l_Y < l_Source.groupChildren.Count; ++l_Y)
                        l_SourceChoices.Add(l_Source.groupChildren[l_Y].name);
                }
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_SourceChoices.Count; ++l_I)
            {
                if (l_SourceChoices[l_I] != Model.SourceName)
                    continue;

                l_SelectedSource = l_SourceChoices[l_I];
                break;
            }

            m_Source.SetOptions(l_SourceChoices).SetValue(l_SelectedSource);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene.SetValue(OBSService.ActiveScene?.name);
        }
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetVisible(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.render : (Model.ChangeType == Enums.Toggle.E.Enable ? true : false));
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
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

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetVisible(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.render : (Model.ChangeType == Enums.Toggle.E.Enable ? true : false));
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSource Scene:{Model.SceneName} not found!");
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
                l_SceneChoices.AddRange(OBSService.Scenes.Keys.ToList<string>());

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

            RebuildSourceList();
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
                RebuildSourceList();
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
            var l_SourceChoices     = new List<string>() { "<i>None</i>" };
            var l_SelectedSource    = "<i>None</i>";

            if (OBSService.Status == OBSService.EStatus.Connected)
            {
                if (!OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene))
                {
                    CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");
                    return;
                }

                for (int l_I = 0;l_I < l_Scene.sources.Count; ++l_I)
                {
                    var l_Source = l_Scene.sources[l_I];
                    l_SourceChoices.Add(l_Source.name);

                    for (int l_Y = 0; l_Y < l_Source.groupChildren.Count; ++l_Y)
                        l_SourceChoices.Add(l_Source.groupChildren[l_Y].name);
                }
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_SourceChoices.Count; ++l_I)
            {
                if (l_SourceChoices[l_I] != Model.SourceName)
                    continue;

                l_SelectedSource = l_SourceChoices[l_I];
                break;
            }

            m_Source.SetOptions(l_SourceChoices).SetValue(l_SelectedSource);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSelectActiveSceneButton()
        {
            Model.SceneName = OBSService.ActiveScene?.name;
            m_Scene.SetValue(OBSService.ActiveScene?.name);
        }
        private void OnTestButton()
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, not connected to OBS!");
                return;
            }

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetMuted(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.render : (Model.ChangeType == Enums.Toggle.E.Enable ? true : false));
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");
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

            CP_SDK.OBS.Models.Source l_Source = null;
            if (OBSService.Scenes.TryGetValue(Model.SceneName, out var l_Scene) && (l_Source = l_Scene.GetSourceByName(Model.SourceName)) != null)
                l_Source.SetMuted(Model.ChangeType == Enums.Toggle.E.Toggle ? !l_Source.render : (Model.ChangeType == Enums.Toggle.E.Enable ? true : false));
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:OBS_ToggleSourceAudio Scene:{Model.SceneName} not found!");
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
                l_TransitionChoices.AddRange(OBSService.Transitions.ToList<string>());
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

            if (Model.OverrideDuration && Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(Model.Duration, Model.Transition);
            else if (Model.OverrideDuration)
                OBSService.PreviewTransitionToScene(Model.Duration);
            else if (Model.OverrideTransition)
                OBSService.PreviewTransitionToScene(-1, Model.Transition);
            else
                OBSService.PreviewTransitionToScene();
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
