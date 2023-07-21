using CP_SDK.XUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Actions
{
    public class Camera2_SwitchToDefaultScene
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<Camera2_SwitchToDefaultScene, ChatPlexMod_ChatIntegrations.Models.Action>
    {
        public override string  Description             => "Switch to default camera2 scene";
        public override string  UIPlaceHolder           => "Switch to default camera2 scene";
        public override bool    UIPlaceHolderTestButton => true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected override void OnUIPlaceholderTestButton()
        {
            if (!ModPresence.Camera2)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }

            Camera2.SDK.Scenes.ShowNormalScene();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }

            Camera2.SDK.Scenes.ShowNormalScene();

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Camera2_SwitchToScene
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<Camera2_SwitchToScene, Models.Actions.Camera2_SwitchToScene>
    {
        private XUIDropdown m_Scene = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Change active Camera2 scene";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Choices           = new List<string>() { "<i>None</i>" };
            var l_SelectedChoice    = "<i>None</i>";

            if (ModPresence.Camera2)
            {
                l_Choices = Camera2.SDK.Scenes.customScenes.Select(x => x.Key).ToList<string>();

                if (l_Choices.Count == 0)
                    l_Choices.Add("<i>None</i>");
            }
            else if (!ModPresence.Camera2)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] != Model.SceneName)
                    continue;

                l_SelectedChoice = l_Choices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Scene",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_SelectedChoice)
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Scene)
                ),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.SceneName = m_Scene.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (!ModPresence.Camera2)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }

            if (Camera2.SDK.Scenes.customScenes.ContainsKey(Model.SceneName))
                Camera2.SDK.Scenes.SwitchToCustomScene(Model.SceneName);
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_SwitchToScene Scene:{Model.SceneName} not found!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }

            if (Camera2.SDK.Scenes.customScenes.ContainsKey(Model.SceneName))
                Camera2.SDK.Scenes.SwitchToCustomScene(Model.SceneName);
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_SwitchToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Camera2_ToggleCamera
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<Camera2_ToggleCamera, Models.Actions.Camera2_ToggleCamera>
    {
        private XUIDropdown m_Camera     = null;
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Toggle Camera2 camera visibility";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Choices           = new List<string>() { "<i>None</i>" };
            var l_SelectedChoice    = "<i>None</i>";
            if (ModPresence.Camera2)
                l_Choices = Camera2.SDK.Cameras.available.ToList<string>();
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] != Model.CameraName)
                    continue;

                l_SelectedChoice = l_Choices[l_I];
                break;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Camera",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_SelectedChoice)
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Camera)
                ),

                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(ChatPlexMod_ChatIntegrations.Enums.Toggle.S).SetValue(ChatPlexMod_ChatIntegrations.Enums.Toggle.ToStr(Model.ChangeType)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                ),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.CameraName = m_Camera.Element.GetValue();
            Model.ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.ToEnum(m_ChangeType.Element.GetValue());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (!ModPresence.Camera2)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }

            if (Camera2.SDK.Cameras.available.Contains(Model.CameraName))
            {
                switch (Model.ChangeType)
                {
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, !Camera2.SDK.Cameras.active.Contains(Model.CameraName));
                        break;
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, true);
                        break;
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, false);
                        break;
                }
            }
            else
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_ToggleCamera Camera:{Model.CameraName} not found!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }

            if (Camera2.SDK.Cameras.available.Contains(Model.CameraName))
            {
                switch (Model.ChangeType)
                {
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, !Camera2.SDK.Cameras.active.Contains(Model.CameraName));
                        break;
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Enable:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, true);
                        break;
                    case ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Disable:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, false);
                        break;
                }
            }
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_ToggleCamera Camera:{Model.CameraName} not found!");
            }

            yield return null;
        }
    }
}
