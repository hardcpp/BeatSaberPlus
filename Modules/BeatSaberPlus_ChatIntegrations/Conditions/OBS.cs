using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

using OBSService = BeatSaberPlus.SDK.OBS.Service;

namespace BeatSaberPlus_ChatIntegrations.Conditions
{
    internal class OBSBuilder
    {
        internal static List<Interfaces.IConditionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            var l_Result = new List<Interfaces.IConditionBase>()
            {
                new OBS_IsConnected(),
                new OBS_IsNotConnected(),

                new OBS_IsStreaming(),
                new OBS_IsNotStreaming(),

                new OBS_IsRecording(),
                new OBS_IsNotRecording(),

                new OBS_IsInStudioMode(),
                new OBS_IsNotInStudioMode(),

                new OBS_IsInScene(),
                new OBS_IsNotInScene()
            };

            switch (p_Event)
            {
                default:
                    break;
            }

            return l_Result;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_IsConnected : Interfaces.ICondition<OBS_IsConnected, Models.Condition>
    {
        public override string Description => "Is OBS connected?";
        public OBS_IsConnected() => UIPlaceHolder = "<b><i>Ensure that OBS is connected</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected;
    }
    public class OBS_IsNotConnected : Interfaces.ICondition<OBS_IsNotConnected, Models.Condition>
    {
        public override string Description => "Is OBS not connected?";
        public OBS_IsNotConnected() => UIPlaceHolder = "<b><i>Ensure that OBS is not connected</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status != OBSService.EStatus.Connected;
    }

    public class OBS_IsStreaming : Interfaces.ICondition<OBS_IsStreaming, Models.Condition>
    {
        public override string Description => "Is OBS streaming?";
        public OBS_IsStreaming() => UIPlaceHolder = "<b><i>Ensure that OBS is streaming</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && OBSService.IsStreaming;
    }
    public class OBS_IsNotStreaming : Interfaces.ICondition<OBS_IsNotStreaming, Models.Condition>
    {
        public override string Description => "Is OBS not streaming?";
        public OBS_IsNotStreaming() => UIPlaceHolder = "<b><i>Ensure that OBS is not streaming</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && !OBSService.IsStreaming;
    }

    public class OBS_IsRecording : Interfaces.ICondition<OBS_IsRecording, Models.Condition>
    {
        public override string Description => "Is OBS recording?";
        public OBS_IsRecording() => UIPlaceHolder = "<b><i>Ensure that OBS is recording</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && OBSService.IsRecording;
    }
    public class OBS_IsNotRecording : Interfaces.ICondition<OBS_IsNotRecording, Models.Condition>
    {
        public override string Description => "Is OBS not recording?";
        public OBS_IsNotRecording() => UIPlaceHolder = "<b><i>Ensure that OBS is not recording</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && !OBSService.IsRecording;
    }

    public class OBS_IsInStudioMode : Interfaces.ICondition<OBS_IsInStudioMode, Models.Condition>
    {
        public override string Description => "Is OBS in studio mode?";
        public OBS_IsInStudioMode() => UIPlaceHolder = "<b><i>Ensure that OBS is in studio mode</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && OBSService.IsInStudioMode;
    }
    public class OBS_IsNotInStudioMode : Interfaces.ICondition<OBS_IsNotInStudioMode, Models.Condition>
    {
        public override string Description => "Is OBS not in studio mode?";
        public OBS_IsNotInStudioMode() => UIPlaceHolder = "<b><i>Ensure that OBS is not in studio mode</i></b>";
        public override bool Eval(Models.EventContext p_Context)
            => OBSService.Status == OBSService.EStatus.Connected && !OBSService.IsInStudioMode;
    }

    public class OBS_IsInScene : Interfaces.ICondition<OBS_IsInScene, Models.Conditions.OBS_IsInScene>
    {
        public override string Description => "Is OBS in scene";

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

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Condition failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value  = l_Choices[l_ChoiceIndex];
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
        public override bool Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Condition failed, not connected to OBS!");
                return false;
            }

            return OBSService.ActiveScene?.name == Model.SceneName;
        }
    }
    public class OBS_IsNotInScene : Interfaces.ICondition<OBS_IsNotInScene, Models.Conditions.OBS_IsNotInScene>
    {
        public override string Description => "Is OBS not in scene";

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

            if (OBSService.Status == OBSService.EStatus.Connected)
                l_Choices.AddRange(OBSService.Scenes.Keys.ToList<object>());
            else
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Condition failed, not connected to OBS!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.SceneName)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Scene_DropDownOptions = l_Choices;
            m_Scene_DropDown.values = l_Choices;
            m_Scene_DropDown.Value  = l_Choices[l_ChoiceIndex];
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

        public override bool Eval(Models.EventContext p_Context)
        {
            if (OBSService.Status != OBSService.EStatus.Connected)
            {
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Condition failed, not connected to OBS!");
                return false;
            }

            return !(OBSService.ActiveScene?.name == Model.SceneName);
        }
    }
}
