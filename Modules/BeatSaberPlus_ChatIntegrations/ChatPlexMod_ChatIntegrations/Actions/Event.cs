using ChatPlexMod_ChatIntegrations.Models;
using CP_SDK.XUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class EventRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("Event_ExecuteDummy", () => new Event_ExecuteDummy());
            ChatIntegrations.RegisterActionType("Event_Toggle",       () => new Event_Toggle());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_ExecuteDummy
        : Interfaces.IAction<Event_ExecuteDummy, Models.Actions.Event_ExecuteDummy>
    {
        private XUIDropdown m_Dropdown = null;
        private XUIToggle   m_Continue = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Execute a dummy event";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Events   = ChatIntegrations.Instance.GetEventsByType(typeof(Events.Dummy));
            var l_Choices  = new List<string>() { "<i>None</i>" };
            var l_Selected = "<i>None</i>";
            l_Events.Sort((x, y) => (x.GetTypeName() + x.GenericModel.Name).CompareTo(y.GetTypeName() + y.GenericModel.Name));

            foreach (var l_EventBase in l_Events)
            {
                if (l_EventBase == Event)
                    continue;

                l_Choices.Add(l_EventBase.GenericModel.Name);

                if (Model.BaseValue != "" && l_EventBase.GenericModel.GUID == Model.BaseValue)
                    l_Selected = l_EventBase.GenericModel.Name;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Dummy event to execute",
                    XUIDropdown.Make()
                    .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                    .Bind(ref m_Dropdown)
                ),

                Templates.SettingsHGroup("Continue execution if failed",
                    XUIToggle.Make()
                        .SetValue(Model.Continue).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Continue)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            if (m_Dropdown.Element.GetValue() == "<i>None</i>")
                Model.BaseValue = "";

            var l_SelectedEvent = ChatIntegrations.Instance.GetEventByName(m_Dropdown.Element.GetValue());
            if (l_SelectedEvent != null && l_SelectedEvent is Events.Dummy)
                Model.BaseValue = l_SelectedEvent.GenericModel.GUID;
            else
            {
                Model.BaseValue = "";
                m_Dropdown.SetValue("<i>None</i>", false);
            }

            Model.Continue = m_Continue.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(EventContext p_Context)
        {
            var l_SelectedEvent = ChatIntegrations.Instance.GetEventByGUID(Model.BaseValue);
            if (l_SelectedEvent != null && l_SelectedEvent is Events.Dummy)
            {
                if (Model.Continue)
                    ChatIntegrations.Instance.ExecuteEvent(l_SelectedEvent, new EventContext() { Type = Interfaces.ETriggerType.Dummy });
                else
                    p_Context.HasActionFailed = !ChatIntegrations.Instance.ExecuteEvent(l_SelectedEvent, new EventContext() { Type = Interfaces.ETriggerType.Dummy });
            }
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Event_Toggle
        : Interfaces.IAction<Event_Toggle, Models.Actions.Event_Toggle>
    {
        private XUIDropdown m_Event         = null;
        private XUIDropdown m_ChangeType    = null;

        private Dictionary<string, string> m_NameToGUID = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Toggle an event";

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

                if (Model.BaseValue != "" && l_EventBase.GenericModel.GUID == Model.BaseValue)
                    l_Selected = l_Line;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Dummy event to execute",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Event)
                ),

                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(Enums.Toggle.S).SetValue(Enums.Toggle.ToStr(Model.ChangeType)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            if (m_Event.Element.GetValue() == "<i>None</i>")
                Model.BaseValue = "";

            if (m_NameToGUID.TryGetValue(m_Event.Element.GetValue(), out var l_SelectedGUID))
                Model.BaseValue = l_SelectedGUID;
            else
            {
                Model.BaseValue = "";
                m_Event.SetValue("<i>None</i>", false);
            }

            Model.ChangeType = Enums.Toggle.ToInt(m_ChangeType.Element.GetValue());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(EventContext p_Context)
        {
            var l_SelectedEvent = string.IsNullOrEmpty(Model.BaseValue) ? null : ChatIntegrations.Instance.GetEventByGUID(Model.BaseValue);
            if (l_SelectedEvent != null)
            {
                if (Model.ChangeType == (int)Enums.Toggle.E.Toggle
                    || (Model.ChangeType == (int)Enums.Toggle.E.Enable && !l_SelectedEvent.IsEnabled)
                    || (Model.ChangeType == (int)Enums.Toggle.E.Disable && l_SelectedEvent.IsEnabled))
                    ChatIntegrations.Instance.ToggleEvent(l_SelectedEvent);
            }

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private string BuildLineString(Interfaces.IEventBase p_Event)
            => $"<b><color=yellow>[{p_Event.GetTypeName()}]</color> {p_Event.GenericModel.Name}</b>";
    }
}
