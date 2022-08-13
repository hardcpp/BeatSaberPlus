using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Conditions
{
    internal class EventBuilder
    {
        internal static List<Interfaces.IConditionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            var l_Result = new List<Interfaces.IConditionBase>()
            {
                new Event_AlwaysFail(),
                new Event_Disabled(),
                new Event_Enabled(),
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

    public class Event_AlwaysFail : Interfaces.ICondition<Event_AlwaysFail, Models.Condition>
    {
        public override string Description => "Always fail the event";

        public Event_AlwaysFail() => UIPlaceHolder = "<b><i>Make the event to always fail</i></b>";

        public override bool Eval(Models.EventContext p_Context)
        {
            return false;
        }
    }

    public class Event_Disabled : Interfaces.ICondition<Event_Disabled, Models.Conditions.Event_Disabled>
    {
        public override string Description => "Ensure that an event is disabled";

#pragma warning disable CS0414
        [UIComponent("Event_DropDown")]     protected DropDownListSetting   m_Event_DropDown = null;
        [UIValue("Event_DropDownOptions")]  private List<object>            m_Event_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Event_DropDown, l_Event, true);

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

                if (Model.EventGUID != "" && l_EventBase.GenericModel.GUID == Model.EventGUID)
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
                Model.EventGUID = "";

            if (m_NameToGUID.TryGetValue((string)m_Event_DropDown.Value, out var l_SelectedGUID))
                Model.EventGUID = l_SelectedGUID;
            else
            {
                Model.EventGUID = "";
                m_Event_DropDown.Value = m_Event_DropDownOptions[0];
            }
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            var l_SelectedEvent = string.IsNullOrEmpty(Model.EventGUID) ? null : ChatIntegrations.Instance.GetEventByGUID(Model.EventGUID);
            if (l_SelectedEvent != null)
                return !l_SelectedEvent.IsEnabled;

            return false;
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
    public class Event_Enabled : Interfaces.ICondition<Event_Enabled, Models.Conditions.Event_Enabled>
    {
        public override string Description => "Ensure that an event is enabled";

#pragma warning disable CS0414
        [UIComponent("Event_DropDown")]     protected DropDownListSetting   m_Event_DropDown = null;
        [UIValue("Event_DropDownOptions")]  private List<object>            m_Event_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Event_DropDown, l_Event, true);

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

                if (Model.EventGUID != "" && l_EventBase.GenericModel.GUID == Model.EventGUID)
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
                Model.EventGUID = "";

            if (m_NameToGUID.TryGetValue((string)m_Event_DropDown.Value, out var l_SelectedGUID))
                Model.EventGUID = l_SelectedGUID;
            else
            {
                Model.EventGUID = "";
                m_Event_DropDown.Value = m_Event_DropDownOptions[0];
            }
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            var l_SelectedEvent = string.IsNullOrEmpty(Model.EventGUID) ? null : ChatIntegrations.Instance.GetEventByGUID(Model.EventGUID);
            if (l_SelectedEvent != null)
                return l_SelectedEvent.IsEnabled;

            return false;
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
