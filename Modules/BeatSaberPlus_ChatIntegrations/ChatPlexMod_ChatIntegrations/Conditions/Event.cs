using CP_SDK.XUI;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Conditions
{
    internal class EventRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterConditionType("Event_AlwaysFail",    () => new Event_AlwaysFail());
            ChatIntegrations.RegisterConditionType("Event_Disabled",      () => new Event_Disabled());
            ChatIntegrations.RegisterConditionType("Event_Enabled",       () => new Event_Enabled());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_AlwaysFail
        : Interfaces.ICondition<Event_AlwaysFail, Models.Condition>
    {
        public override string Description      => "Always fail the event";
        public override string UIPlaceHolder    => "<b><i>Make the event to always fail</i></b>";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(Models.EventContext p_Context)
        {
            return false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_Disabled
        : Interfaces.ICondition<Event_Disabled, Models.Conditions.Event_Disabled>
    {
        private XUIDropdown m_Event = null;

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Ensure that an event is disabled";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_NameToGUID.Clear();

            var l_Events    = ChatIntegrations.Instance.GetEventsByType(null);
            var l_Choices   = new List<string>() { "<i>None</i>" };
            var l_Selected  = "<i>None</i>";
            l_Events.Sort((x, y) => (x.GetTypeName() + x.GenericModel.Name).CompareTo(y.GetTypeName() + y.GenericModel.Name));

            foreach (var l_EventBase in l_Events)
            {
                var l_Line = BuildLineString(l_EventBase);
                l_Choices.Add(l_Line);
                m_NameToGUID.Add(l_Line, l_EventBase.GenericModel.GUID);

                if (Model.EventGUID != "" && l_EventBase.GenericModel.GUID == Model.EventGUID)
                    l_Selected = l_Line;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Event",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Event)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            if (m_Event.Element.GetValue() == "<i>None</i>")
                Model.EventGUID = "";

            if (m_NameToGUID.TryGetValue(m_Event.Element.GetValue(), out var l_SelectedGUID))
                Model.EventGUID = l_SelectedGUID;
            else
            {
                Model.EventGUID = "";
                m_Event.SetValue("<i>None</i>", false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            l_Text += "<color=yellow>[" + p_Event.GetTypeName() + "] </color> ";
            l_Text += p_Event.GenericModel.Name;
            l_Text += "</b>";

            return l_Text;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_Enabled
        : Interfaces.ICondition<Event_Enabled, Models.Conditions.Event_Enabled>
    {
        private XUIDropdown m_Event = null;

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Ensure that an event is enabled";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_NameToGUID.Clear();

            var l_Events    = ChatIntegrations.Instance.GetEventsByType(null);
            var l_Choices   = new List<string>() { "<i>None</i>" };
            var l_Selected  = "<i>None</i>";
            l_Events.Sort((x, y) => (x.GetTypeName() + x.GenericModel.Name).CompareTo(y.GetTypeName() + y.GenericModel.Name));

            foreach (var l_EventBase in l_Events)
            {
                var l_Line = BuildLineString(l_EventBase);
                l_Choices.Add(l_Line);
                m_NameToGUID.Add(l_Line, l_EventBase.GenericModel.GUID);

                if (Model.EventGUID != "" && l_EventBase.GenericModel.GUID == Model.EventGUID)
                    l_Selected = l_Line;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Event",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Event)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            if (m_Event.Element.GetValue() == "<i>None</i>")
                Model.EventGUID = "";

            if (m_NameToGUID.TryGetValue(m_Event.Element.GetValue(), out var l_SelectedGUID))
                Model.EventGUID = l_SelectedGUID;
            else
            {
                Model.EventGUID = "";
                m_Event.SetValue("<i>None</i>", false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            l_Text += "<color=yellow>[" + p_Event.GetTypeName() + "] </color> ";
            l_Text += p_Event.GenericModel.Name;
            l_Text += "</b>";

            return l_Text;
        }
    }
}
