using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class Camera2Builder
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
                new Camera2_SwitchToDefaultScene(),
                new Camera2_SwitchToScene(),
                new Camera2_ToggleCamera(),
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Camera2_SwitchToDefaultScene : Interfaces.IAction<Camera2_SwitchToDefaultScene, Models.Action>
    {
        public override string Description => "Switch to default camera2 scene";

        public Camera2_SwitchToDefaultScene() { UIPlaceHolder = "Switch to default camera2 scene"; UIPlaceHolderTestButton = true; }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }

            Camera2.SDK.Scenes.ShowNormalScene();

            yield return null;
        }
        protected override void OnUIPlaceholderTestButton()
        {
            if (!ModPresence.Camera2)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }

            Camera2.SDK.Scenes.ShowNormalScene();
        }
    }

    public class Camera2_SwitchToScene : Interfaces.IAction<Camera2_SwitchToScene, Models.Actions.Camera2_SwitchToScene>
    {
        public override string Description => "Change active Camera2 scene";

#pragma warning disable CS0414
        [UIComponent("Scene_DropDown")]
        protected DropDownListSetting m_Scene_DropDown = null;
        [UIValue("Scene_DropDownOptions")]
        private List<object> m_Scene_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Scene_DropDown,  l_Event, true);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (ModPresence.Camera2 && ModPresence.Camera2Fixed)
            {
                l_Choices = Camera2.SDK.Scenes.customScenes.Select(x => x.Key).ToList<object>();

                if (l_Choices.Count == 0)
                    l_Choices.Add("<i>None</i>");
            }
            else if (!ModPresence.Camera2)
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
            else if (!ModPresence.Camera2Fixed)
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 is not updated!");

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

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.SceneName = m_Scene_DropDown.Value as string;
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (!ModPresence.Camera2)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }
            if (!ModPresence.Camera2Fixed)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 is not updated!");
                return;
            }

            if (Camera2.SDK.Scenes.customScenes.ContainsKey(Model.SceneName))
                Camera2.SDK.Scenes.SwitchToCustomScene(Model.SceneName);
            else
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_SwitchToScene Scene:{Model.SceneName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }
            if (!ModPresence.Camera2Fixed)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 is not updated!");
                yield break;
            }

            if (Camera2.SDK.Scenes.customScenes.ContainsKey(Model.SceneName))
                Camera2.SDK.Scenes.SwitchToCustomScene(Model.SceneName);
            else
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_SwitchToScene Scene:{Model.SceneName} not found!");
            }

            yield return null;
        }
    }

    public class Camera2_ToggleCamera : Interfaces.IAction<Camera2_ToggleCamera, Models.Actions.Camera2_ToggleCamera>
    {
        public override string Description => "Toggle Camera2 camera visibility";

#pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;

        [UIComponent("Camera_DropDown")]
        protected DropDownListSetting m_Camera_DropDown = null;
        [UIValue("Camera_DropDownOptions")]
        private List<object> m_Camera_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(         m_TypeList,         l_Event, false);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup( m_Camera_DropDown,  l_Event, true);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (ModPresence.Camera2)
                l_Choices = Camera2.SDK.Cameras.available.ToList<object>();
            else
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.CameraName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Camera_DropDownOptions = l_Choices;
            m_Camera_DropDown.values = l_Choices;
            m_Camera_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Camera_DropDown.UpdateChoices();

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ToggleType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
            Model.CameraName = m_Camera_DropDown.Value as string;
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (!ModPresence.Camera2)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                return;
            }

            if (Camera2.SDK.Cameras.available.Contains(Model.CameraName))
            {
                switch (Model.ToggleType)
                {
                    case 0:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, !Camera2.SDK.Cameras.active.Contains(Model.CameraName));
                        break;
                    case 1:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, true);
                        break;
                    case 2:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, false);
                        break;
                }
            }
            else
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_ToggleCamera Camera:{Model.CameraName} not found!");
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModPresence.Camera2)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, Camera2 mod is missing!");
                yield break;
            }

            if (Camera2.SDK.Cameras.available.Contains(Model.CameraName))
            {
                switch (Model.ToggleType)
                {
                    case 0:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, !Camera2.SDK.Cameras.active.Contains(Model.CameraName));
                        break;
                    case 1:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, true);
                        break;
                    case 2:
                        Camera2.SDK.Cameras.SetCameraActive(Model.CameraName, false);
                        break;
                }
            }
            else
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:Camera2_ToggleCamera Camera:{Model.CameraName} not found!");
            }

            yield return null;
        }
    }
}
