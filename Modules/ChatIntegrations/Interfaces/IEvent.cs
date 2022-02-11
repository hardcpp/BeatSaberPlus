using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Interfaces
{
    /// <summary>
    /// IEvent generic class
    /// </summary>
    public abstract class IEventBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Generic model
        /// </summary>
        public abstract Models.Event GenericModel { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        public abstract bool IsEnabled { get; set; }
        /// <summary>
        /// Provided values list
        /// </summary>
        public abstract IReadOnlyList<(IValueType, string)> ProvidedValues { get; protected set; }
        /// <summary>
        /// Available conditions list
        /// </summary>
        public abstract IReadOnlyList<IConditionBase> AvailableConditions { get; protected set; }
        /// <summary>
        /// Condition list
        /// </summary>
        public abstract List<IConditionBase> Conditions { get; protected set; }
        /// <summary>
        /// Available actions list
        /// </summary>
        public abstract IReadOnlyList<IActionBase> AvailableActions { get; protected set; }
        /// <summary>
        /// Action list
        /// </summary>
        public abstract List<IActionBase> Actions { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public bool Handle(Models.EventContext p_Context)
        {
            try
            {
                if (!CanBeExecuted(p_Context))
                    return false;

                BuildProvidedValues(p_Context);

                if (p_Context.VariableCount != ProvidedValues.Count)
                {
                    Logger.Instance?.Error(string.Format(
                        "[Modules.ChatIntegrations.Interfaces][IEvent.Handle] Event {0} provided {1} values, {2} excepted, event discarded!",
                        GetTypeName(), p_Context.VariableCount, ProvidedValues.Count));

                    OnEventFailed(p_Context);

                    return false;
                }

                foreach (var l_Condition in Conditions)
                {
                    if (l_Condition.IsEnabled && !l_Condition.Eval(p_Context))
                    {
                        OnEventFailed(p_Context);
                        return false;
                    }
                }

                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() => SharedCoroutineStarter.instance.StartCoroutine(DoActions(p_Context)));
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error("[Modules.ChatIntegrations.Interfaces][IEvent.Handle] Error:");
                Logger.Instance?.Error(l_Exception);
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeName();
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeNameShort();
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public abstract JObject Serialize();
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        /// <param name="p_Error">Error output</param>
        public abstract bool Unserialize(JObject p_Serialized, out string p_Error);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an condition to the event
        /// </summary>
        /// <param name="p_Condition">Condition to add</param>
        public abstract void AddCondition(IConditionBase p_Condition);
        /// <summary>
        /// Move condition
        /// </summary>
        /// <param name="p_Condition">Condition to move</param>
        public abstract int MoveCondition(IConditionBase p_Condition, bool p_Up);
        /// <summary>
        /// Delete an condition from the event
        /// </summary>
        /// <param name="p_Condition">Condition to delete</param>
        public abstract void DeleteCondition(IConditionBase p_Condition);
        /// <summary>
        /// Add an action to the event
        /// </summary>
        /// <param name="p_Action">Action to add</param>
        public abstract void AddAction(IActionBase p_Action);
        /// <summary>
        /// Move action
        /// </summary>
        /// <param name="p_Action">Action to move</param>
        public abstract int MoveAction(IActionBase p_Action, bool p_Up);
        /// <summary>
        /// Delete an action from the event
        /// </summary>
        /// <param name="p_Action">Action to delete</param>
        public abstract void DeleteAction(IActionBase p_Action);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public abstract void BuildUI(Transform p_Parent);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On import or clone
        /// </summary>
        /// <param name="p_IsImport">Is an import</param>
        /// <param name="p_IsClone">Is a clone</param>
        public virtual void OnImportOrClone(bool p_IsImport, bool p_IsClone)
        {
            if (p_IsImport)
                GenericModel.Name += " (Import)";
            if (p_IsClone)
                GenericModel.Name += " (Clone)";

            GenericModel.CreationDate   = BeatSaberPlus.SDK.Misc.Time.UnixTimeNow();
            GenericModel.LastUsageDate  = 0;
            GenericModel.UsageCount     = 0;
        }
        /// <summary>
        /// When the event is enabled
        /// </summary>
        public virtual void OnEnable() { }
        /// <summary>
        /// When the event is successful
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public virtual void OnSuccess(Models.EventContext p_Context) { }
        /// <summary>
        /// When the event failed
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public virtual void OnEventFailed(Models.EventContext p_Context) { }
        /// <summary>
        /// When the event is disabled
        /// </summary>
        public virtual void OnDisable() { }
        /// <summary>
        /// When the event is deleted
        /// </summary>
        public virtual void OnDelete() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected abstract bool CanBeExecuted(Models.EventContext p_Context);
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected virtual void BuildProvidedValues(Models.EventContext p_Context)
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notify property changed
        /// </summary>
        /// <param name="p_PropertyName">Property name</param>
        protected void NotifyPropertyChanged([CallerMemberName] string p_PropertyName = "")
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p_PropertyName));
            }
            catch (Exception l_Exception)
            {
                Logger.Instance?.Error($"[Modules.ChatIntegrations][IEvent.NotifyPropertyChanged] Error Invoking PropertyChanged: {l_Exception.Message}");
                Logger.Instance?.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Do actions
        /// </summary>
        /// <param name="p_Context">Event context</param>
        /// <returns></returns>
        private IEnumerator DoActions(Models.EventContext p_Context)
        {
            foreach (var l_Action in Actions)
            {
                if (!l_Action.IsEnabled)
                    continue;

                yield return l_Action.Eval(p_Context);

                if (!p_Context.PreventNextActionFailure && p_Context.HasActionFailed)
                {
                    OnEventFailed(p_Context);
                    break;
                }
            }

            if (!p_Context.PreventNextActionFailure && !p_Context.HasActionFailed)
                OnSuccess(p_Context);

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// IEvent generic class
    /// </summary>
    public abstract class IEvent<T, M> : IEventBase
        where T : IEvent<T, M>, new()
        where M : Models.Event, new()
    {
        /// <summary>
        /// Generic model
        /// </summary>
        public override sealed Models.Event GenericModel { get => Model; }
        /// <summary>
        /// Is enabled
        /// </summary>
        public override sealed bool IsEnabled { get => Model.Enabled; set { Model.Enabled = value; } }
        /// <summary>
        /// Condition list
        /// </summary>
        public override sealed List<IConditionBase> Conditions { get; protected set; } = new List<IConditionBase>();
        /// <summary>
        /// Action list
        /// </summary>
        public override sealed List<IActionBase> Actions { get; protected set; } = new List<IActionBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Model
        /// </summary>
        public M Model { get; protected set; } = new M() { GUID = Guid.NewGuid().ToString(), Enabled = true, CreationDate = BeatSaberPlus.SDK.Misc.Time.UnixTimeNow() };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Custom available conditions
        /// </summary>
        private static List<IConditionBase> m_CustomAvailableConditions = new List<IConditionBase>();
        /// <summary>
        /// Custom available actions
        /// </summary>
        private static List<IActionBase> m_CustomAvailableActions = new List<IActionBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public override sealed string GetTypeName()
        {
            return string.Join(".", typeof(T).Namespace, typeof(T).Name);
        }
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public override sealed string GetTypeNameShort()
        {
            return typeof(T).Name;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register custom condition
        /// </summary>
        /// <typeparam name="T">Condition to register</typeparam>
        public static void RegisterCustomCondition<TCondition>() where TCondition : IConditionBase, new()
            => m_CustomAvailableConditions.Add(new TCondition());
        /// <summary>
        /// Register custom action
        /// </summary>
        /// <typeparam name="T">Action to register</typeparam>
        public static void RegisterCustomAction<TAction>() where TAction : IActionBase, new()
            => m_CustomAvailableActions.Add(new TAction());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected List<IConditionBase> GetInstanciatedCustomConditionList()
            => m_CustomAvailableConditions.Select(x => Activator.CreateInstance(x.GetType()) as IConditionBase).ToList();
        protected List<IActionBase> GetInstanciatedCustomActionList()
            => m_CustomAvailableActions.Select(x => Activator.CreateInstance(x.GetType()) as IActionBase).ToList();

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
                ["Type"]        = GetTypeName(),
                ["Event"]       = JObject.FromObject(Model),
                ["Conditions"]  = JArray.FromObject(Conditions.Select(x => x.Serialize()).ToArray()),
                ["Actions"]     = JArray.FromObject(Actions.Select(x => x.Serialize()).ToArray())
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

            if (p_Serialized["Event"]["Type"].Value<string>().StartsWith("BeatSaberPlus.Modules.ChatIntegrations."))
                p_Serialized["Event"]["Type"] = p_Serialized["Event"]["Type"].Value<string>().Replace("BeatSaberPlus.Modules.ChatIntegrations.", "BeatSaberPlus_ChatIntegrations.");

            if (p_Serialized["Event"]["Type"].Value<string>() != GetTypeName())
            {
                p_Error = "Invalid event format for type " + GetTypeName();
                return false;
            }

            Model = p_Serialized["Event"].ToObject<M>();

            if (p_Serialized.ContainsKey("Conditions") && p_Serialized["Conditions"].Type == JTokenType.Array)
            {
                foreach (JObject l_SerializedCondition in (p_Serialized["Conditions"] as JArray))
                {
                    if (!l_SerializedCondition.ContainsKey(nameof(Models.Condition.Type)))
                        continue;

                    var l_ConditionType = l_SerializedCondition[nameof(Models.Condition.Type)].Value<string>();

                    if (l_ConditionType.StartsWith("BeatSaberPlus.Modules.ChatIntegrations."))
                    {
                        l_ConditionType = l_ConditionType.Replace("BeatSaberPlus.Modules.ChatIntegrations.", "BeatSaberPlus_ChatIntegrations.");
                        l_SerializedCondition[nameof(Models.Condition.Type)] = l_ConditionType;
                    }

                    var l_MatchingType  = AvailableConditions.Where(x => x.GetTypeName() == l_ConditionType).FirstOrDefault();

                    if (l_MatchingType == null)
                    {
                        /// Todo backup this condition to avoid loss
                        Logger.Instance?.Error($"[Modules.ChatIntegrations.Interfaces][IEvent.Unserialize] Missing condition type \"{l_ConditionType}\"");
                        continue;
                    }

                    /// Create instance
                    var l_NewCondition = Activator.CreateInstance(l_MatchingType.GetType()) as Interfaces.IConditionBase;
                    l_NewCondition.Event = this;

                    /// Unserialize condition
                    if (!l_NewCondition.Unserialize(l_SerializedCondition))
                    {
                        /// Todo backup this condition to avoid loss
                        Logger.Instance?.Error($"[Modules.ChatIntegrations.Interfaces][IEvent.Unserialize] Failed to unserialize condition\n\"{l_ConditionType.ToString()}\"");
                        continue;
                    }

                    Conditions.Add(l_NewCondition);
                }
            }

            if (p_Serialized.ContainsKey("Actions") && p_Serialized["Actions"].Type == JTokenType.Array)
            {
                foreach (JObject l_SerializedAction in (p_Serialized["Actions"] as JArray))
                {
                    if (!l_SerializedAction.ContainsKey(nameof(Models.Condition.Type)))
                        continue;

                    var l_ActionType = l_SerializedAction[nameof(Models.Action.Type)].Value<string>();

                    if (l_ActionType.StartsWith("BeatSaberPlus.Modules.ChatIntegrations."))
                    {
                        l_ActionType = l_ActionType.Replace("BeatSaberPlus.Modules.ChatIntegrations.", "BeatSaberPlus_ChatIntegrations.");
                        l_SerializedAction[nameof(Models.Action.Type)] = l_ActionType;
                    }

                    var l_MatchingType  = AvailableActions.Where(x => x.GetTypeName() == l_ActionType).FirstOrDefault();

                    if (l_MatchingType == null)
                    {
                        /// Todo backup this action to avoid loss
                        Logger.Instance?.Error($"[Modules.ChatIntegrations.Interfaces][IEvent.Unserialize] Missing action type \"{l_ActionType}\"");
                        continue;
                    }

                    /// Create instance
                    var l_NewAction = Activator.CreateInstance(l_MatchingType.GetType()) as Interfaces.IActionBase;
                    l_NewAction.Event = this;

                    /// Unserialize action
                    if (!l_NewAction.Unserialize(l_SerializedAction))
                    {
                        /// Todo backup this event to avoid loss
                        Logger.Instance?.Error($"[Modules.ChatIntegrations.Interfaces][IEvent.Unserialize] Failed to unserialize action\n\"{l_ActionType.ToString()}\"");
                        continue;
                    }

                    Actions.Add(l_NewAction);
                }
            }

            p_Error = "";

            if (string.IsNullOrEmpty(Model.GUID))
                Model.GUID = Guid.NewGuid().ToString();

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an condition to the event
        /// </summary>
        /// <param name="p_Condition">Condition to add</param>
        public override sealed void AddCondition(IConditionBase p_Condition)
        {
            Conditions.Add(p_Condition);
        }
        /// <summary>
        /// Move condition
        /// </summary>
        /// <param name="p_Condition">Condition to move</param>
        public override sealed int MoveCondition(IConditionBase p_Condition, bool p_Up)
        {
            var l_Index = Conditions.IndexOf(p_Condition);

            if (l_Index == -1 || (l_Index == 0 && p_Up) || (l_Index == (Conditions.Count - 1) && !p_Up))
                return -1;

            Conditions.Remove(p_Condition);
            Conditions.Insert(l_Index + (p_Up ? -1 : 1), p_Condition);

            return Conditions.IndexOf(p_Condition);
        }
        /// <summary>
        /// Delete an condition from the event
        /// </summary>
        /// <param name="p_Condition">Condition to delete</param>
        public override sealed void DeleteCondition(IConditionBase p_Condition)
        {
            Conditions.Remove(p_Condition);
        }
        /// <summary>
        /// Add an action to the event
        /// </summary>
        /// <param name="p_Action">Action to add</param>
        public override sealed void AddAction(IActionBase p_Action)
        {
            Actions.Add(p_Action);
        }
        /// <summary>
        /// Move action
        /// </summary>
        /// <param name="p_Action">Action to move</param>
        public override sealed int MoveAction(IActionBase p_Action, bool p_Up)
        {
            var l_Index = Actions.IndexOf(p_Action);

            if (l_Index == -1 || (l_Index == 0 && p_Up) || (l_Index == (Actions.Count - 1) && !p_Up))
                return -1;

            Actions.Remove(p_Action);
            Actions.Insert(l_Index + (p_Up ? -1 : 1), p_Action);

            return Actions.IndexOf(p_Action);
        }
        /// <summary>
        /// Delete an action from the event
        /// </summary>
        /// <param name="p_Action">Action to delete</param>
        public override sealed void DeleteAction(IActionBase p_Action)
        {
            Actions.Remove(p_Action);
        }
    }
}
