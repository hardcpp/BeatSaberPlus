using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatPlexMod_ChatIntegrations.Interfaces
{
    /// <summary>
    /// IEvent generic class
    /// </summary>
    public abstract class IEvent<t_Event, t_Model> : IEventBase
        where t_Event : IEvent<t_Event, t_Model>, new()
        where t_Model : Models.Event, new()
    {
        public override sealed Models.Event GenericModel    => Model;
        public override sealed bool         IsEnabled       { get => Model.Enabled; set { Model.Enabled = value; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public t_Model Model { get; protected set; } = new t_Model() {
            GUID            = Guid.NewGuid().ToString(),
            Enabled         = true,
            CreationDate    = CP_SDK.Misc.Time.UnixTimeNow()
        };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static List<string> m_CustomAvailableConditions = new List<string>();
        private static List<string> m_CustomAvailableActions    = new List<string>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public override string GetTypeName() => typeof(t_Event).Name;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register custom condition
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Func">Create func</param>
        /// <param name="p_SilentFail">Should fail silently</param>
        public static void RegisterCustomCondition(string p_Name, Func<IConditionBase> p_Func, bool p_SilentFail = false)
        {
            if (m_CustomAvailableConditions.Contains(p_Name))
            {
                if (!p_SilentFail)
                    Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent<{typeof(t_Event).Name}, {typeof(t_Model).Name}>.RegisterCustomCondition] Type \"{p_Name}\" already registered");
                return;
            }

            m_CustomAvailableConditions.Add(p_Name);
            ChatIntegrations.RegisterConditionType(p_Name, p_Func, p_SilentFail, true);
        }
        /// <summary>
        /// Register custom action
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Func">Create func</param>
        /// <param name="p_SilentFail">Should fail silently</param>
        public static void RegisterCustomAction(string p_Name, Func<IActionBase> p_Func, bool p_SilentFail = false)
        {
            if (m_CustomAvailableActions.Contains(p_Name))
            {
                if (!p_SilentFail)
                    Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent<{typeof(t_Event).Name}, {typeof(t_Model).Name}>.RegisterCustomAction] Type \"{p_Name}\" already registered");
                return;
            }

            m_CustomAvailableActions.Add(p_Name);
            ChatIntegrations.RegisterActionType(p_Name, p_Func, p_SilentFail, true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected IReadOnlyList<string> GetCustomConditionTypes()
            => m_CustomAvailableConditions.AsReadOnly();
        protected IReadOnlyList<string> GetCustomActionTypes()
            => m_CustomAvailableActions.AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public override sealed JObject Serialize()
        {
            Model.Type = GetTypeName();

            return new JObject() {
                ["Type"]            = GetTypeName(),
                ["Event"]           = JObject.FromObject(Model),
                ["Conditions"]      = JArray.FromObject(Conditions.Select(x => x.Serialize()).ToArray()),
                ["Actions"]         = JArray.FromObject(OnSuccessActions.Select(x => x.Serialize()).ToArray()),
                ["OnFailActions"]   = JArray.FromObject(OnFailActions.Select(x => x.Serialize()).ToArray())
            };
        }
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        /// <param name="p_Error">Error output</param>
        public override sealed bool Unserialize(JObject p_Serialized, out string p_Error)
        {
            if (!p_Serialized.ContainsKey("Event") || !p_Serialized.ContainsKey("Conditions"))
            {
                p_Error = "Invalid event format";
                return false;
            }

            if (!(p_Serialized["Event"] as JObject).ContainsKey("Type"))
            {
                p_Error = "Invalid event format for type " + GetTypeName();
                return false;
            }

            p_Serialized["Event"]["Type"] = ChatIntegrations.GetPatchedTypeName(p_Serialized["Event"]["Type"].Value<string>());
            if (p_Serialized["Event"]["Type"].Value<string>() != GetTypeName())
            {
                p_Error = "Invalid event format for type " + GetTypeName();
                return false;
            }

            Model = p_Serialized["Event"].ToObject<t_Model>();

            if (p_Serialized.ContainsKey("Conditions") && p_Serialized["Conditions"].Type == JTokenType.Array)
            {
                var l_ConditionsJArray = p_Serialized["Conditions"] as JArray;
                foreach (JObject l_SerializedCondition in l_ConditionsJArray)
                {
                    if (!l_SerializedCondition.ContainsKey("Type"))
                        continue;

                    var l_ConditionType = ChatIntegrations.GetPatchedTypeName(l_SerializedCondition["Type"].Value<string>());
                    l_SerializedCondition["Type"] = l_ConditionType;

                    /// Create instance
                    var l_NewCondition = ChatIntegrations.CreateCondition(l_ConditionType);
                    if (l_NewCondition == null)
                    {
                        /// Todo backup this condition to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Missing condition type \"{l_ConditionType}\"");
                        continue;
                    }

                    l_NewCondition.Event = this;

                    /// Unserialize condition
                    if (!l_NewCondition.Unserialize(l_SerializedCondition))
                    {
                        /// Todo backup this condition to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Failed to unserialize condition\n\"{l_ConditionType.ToString()}\"");
                        continue;
                    }

                    AddCondition(l_NewCondition);
                }
            }

            if (p_Serialized.ContainsKey("Actions") && p_Serialized["Actions"].Type == JTokenType.Array)
            {
                var l_OnSuccessActionsJArray = p_Serialized["Actions"] as JArray;
                foreach (JObject l_SerializedOnSuccessAction in l_OnSuccessActionsJArray)
                {
                    if (!l_SerializedOnSuccessAction.ContainsKey("Type"))
                        continue;

                    var l_OnSuccessActionType = ChatIntegrations.GetPatchedTypeName(l_SerializedOnSuccessAction["Type"].Value<string>());
                    l_SerializedOnSuccessAction["Type"] = l_OnSuccessActionType;

                    /// Create instance
                    var l_NewOnSuccessAction = ChatIntegrations.CreateAction(l_OnSuccessActionType);
                    if (l_NewOnSuccessAction == null)
                    {
                        /// Todo backup this action to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Missing action type \"{l_OnSuccessActionType}\"");
                        continue;
                    }

                    l_NewOnSuccessAction.Event = this;

                    /// Unserialize action
                    if (!l_NewOnSuccessAction.Unserialize(l_SerializedOnSuccessAction))
                    {
                        /// Todo backup this event to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Failed to unserialize action\n\"{l_OnSuccessActionType.ToString()}\"");
                        continue;
                    }

                    AddOnSuccessAction(l_NewOnSuccessAction);
                }
            }

            if (p_Serialized.ContainsKey("OnFailActions") && p_Serialized["OnFailActions"].Type == JTokenType.Array)
            {
                var l_OnFailActionsJArray = p_Serialized["OnFailActions"] as JArray;
                foreach (JObject l_SerializedOnFailAction in l_OnFailActionsJArray)
                {
                    if (!l_SerializedOnFailAction.ContainsKey("Type"))
                        continue;

                    var l_OnFailActionType = ChatIntegrations.GetPatchedTypeName(l_SerializedOnFailAction["Type"].Value<string>());
                    l_SerializedOnFailAction["Type"] = l_OnFailActionType;

                    /// Create instance
                    var l_NewOnFailAction = ChatIntegrations.CreateAction(l_OnFailActionType);
                    if (l_NewOnFailAction == null)
                    {
                        /// Todo backup this action to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Missing action type \"{l_OnFailActionType}\"");
                        continue;
                    }

                    l_NewOnFailAction.Event = this;

                    /// Unserialize action
                    if (!l_NewOnFailAction.Unserialize(l_SerializedOnFailAction))
                    {
                        /// Todo backup this event to avoid loss
                        Logger.Instance?.Error($"[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Unserialize] Failed to unserialize action\n\"{l_OnFailActionType.ToString()}\"");
                        continue;
                    }

                    AddOnFailAction(l_NewOnFailAction);
                }
            }

            p_Error = "";

            if (string.IsNullOrEmpty(Model.GUID))
                Model.GUID = Guid.NewGuid().ToString();

            return true;
        }
    }
}
