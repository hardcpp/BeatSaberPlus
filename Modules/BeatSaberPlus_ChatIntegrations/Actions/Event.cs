using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberPlus_ChatIntegrations.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class EventBuilder
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
                new Event_ExecuteDummy(),
                new Event_Toggle()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_ExecuteDummy : Interfaces.IAction<Event_ExecuteDummy, Models.Actions.Event_ExecuteDummy>
    {
        public override string Description => "Execute a dummy event";

#pragma warning disable CS0414
        [UIComponent("Event_DropDown")]
        protected DropDownListSetting m_Event_DropDown = null;
        [UIValue("Event_DropDownOptions")]
        private List<object> m_Event_DropDownOptions = new List<object>() { "Loading...", };
        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;
        [UIComponent("ContinueToggle")]
        private ToggleSetting m_ContinueToggle = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Event_DropDown, l_Event, true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ContinueToggle, l_Event, Model.Continue, false);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            var l_Events = ChatIntegrations.Instance.GetEventsByType(typeof(Events.Dummy));
            l_Events.Sort((x, y) => (x.GetTypeNameShort() + x.GenericModel.Name).CompareTo(y.GetTypeNameShort() + y.GenericModel.Name));
            foreach (var l_EventBase in l_Events)
            {
                if (l_EventBase == Event)
                    continue;

                l_Choices.Add(l_EventBase.GenericModel.Name);

                if (Model.BaseValue != "" && l_EventBase.GenericModel.GUID == Model.BaseValue)
                    l_ChoiceIndex = l_Choices.Count - 1;
            }

            m_Event_DropDownOptions = l_Choices;
            m_Event_DropDown.values = l_Choices;
            m_Event_DropDown.Value =  l_Choices[l_ChoiceIndex];
            m_Event_DropDown.UpdateChoices();
        }
        private void OnSettingChanged(object p_Value)
        {
            if ((string)p_Value == "<i>None</i>")
                Model.BaseValue = "";

            var l_SelectedEvent = ChatIntegrations.Instance.GetEventByName((string)m_Event_DropDown.Value);

            if (l_SelectedEvent != null && l_SelectedEvent is Events.Dummy)
                Model.BaseValue = l_SelectedEvent.GenericModel.GUID;
            else
            {
                Model.BaseValue = "";
                m_Event_DropDown.Value = m_Event_DropDownOptions[0];
            }
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            var l_SelectedEvent = ChatIntegrations.Instance.GetEventByGUID(Model.BaseValue);
            if (l_SelectedEvent != null && l_SelectedEvent is Events.Dummy)
            {
                if (Model.Continue)
                    ChatIntegrations.Instance.ExecuteEvent(l_SelectedEvent, new EventContext() { Type = Interfaces.TriggerType.Dummy });
                else
                    p_Context.HasActionFailed = !ChatIntegrations.Instance.ExecuteEvent(l_SelectedEvent, new EventContext() { Type = Interfaces.TriggerType.Dummy });
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class Event_Toggle : Interfaces.IAction<Event_Toggle, Models.Actions.Event_Toggle>
    {
        public override string Description => "Toggle an event";

#pragma warning disable CS0414
        [UIComponent("Event_DropDown")]     protected DropDownListSetting   m_Event_DropDown = null;
        [UIValue("Event_DropDownOptions")]  private List<object>            m_Event_DropDownOptions = new List<object>() { "Loading...", };
        [UIComponent("TypeList")]           private ListSetting             m_TypeList = null;
        [UIValue("TypeList_Choices")]       private List<object>            m_TypeListList_Choices = new List<object>() { "Toggle", "Enable", "Disable" };
        [UIValue("TypeList_Value")]         private string                  m_TypeList_Value;
#pragma warning restore CS0414

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ChangeType % m_TypeListList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Event_DropDown, l_Event, true);
            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList, l_Event, false);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            m_NameToGUID.Clear();

            var l_Events = ChatIntegrations.Instance.GetEventsByType(null);
            l_Events.Sort((x, y) => (x.GetTypeNameShort() + x.GenericModel.Name).CompareTo(y.GetTypeNameShort() + y.GenericModel.Name));
            foreach (var l_EventBase in l_Events)
            {
                var l_Line = BuildLineString(l_EventBase);
                l_Choices.Add(l_Line);
                m_NameToGUID.Add(l_Line, l_EventBase.GenericModel.GUID);

                if (Model.BaseValue != "" && l_EventBase.GenericModel.GUID == Model.BaseValue)
                    l_ChoiceIndex = l_Choices.Count - 1;
            }

            m_Event_DropDownOptions = l_Choices;
            m_Event_DropDown.values = l_Choices;
            m_Event_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Event_DropDown.UpdateChoices();
        }
        private void OnSettingChanged(object p_Value)
        {
            if ((string)p_Value == "<i>None</i>")
                Model.BaseValue = "";

            if (m_NameToGUID.TryGetValue((string)m_Event_DropDown.Value, out var l_SelectedGUID))
                Model.BaseValue = l_SelectedGUID;
            else
            {
                Model.BaseValue = "";
                m_Event_DropDown.Value = m_Event_DropDownOptions[0];
            }

            Model.ChangeType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            var l_SelectedEvent = string.IsNullOrEmpty(Model.BaseValue) ? null : ChatIntegrations.Instance.GetEventByGUID(Model.BaseValue);
            if (l_SelectedEvent != null)
            {
                if (Model.ChangeType == 0 || (Model.ChangeType == 1 && !l_SelectedEvent.IsEnabled) || (Model.ChangeType == 2 && l_SelectedEvent.IsEnabled))
                    ChatIntegrations.Instance.ToggleEvent(l_SelectedEvent);
            }

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build event line
        /// </summary>
        /// <param name="p_Event">Event to build for</param>
        private string BuildLineString(Interfaces.IEventBase p_Event)
        {
            /// Result line
            string l_Text = "";
            l_Text += "<align=\"left\"><b>";

            /// Left part
            l_Text += "<color=yellow>[" + p_Event.GetTypeNameShort() + "] </color> ";
            l_Text += p_Event.GenericModel.Name;
            l_Text += "</b>";

            return l_Text;
        }
    }
}
