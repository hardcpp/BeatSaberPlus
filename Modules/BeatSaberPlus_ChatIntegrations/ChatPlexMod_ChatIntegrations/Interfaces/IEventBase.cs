using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Interfaces
{
    /// <summary>
    /// IEvent generic class
    /// </summary>
    public abstract class IEventBase : IUIConfigurable
    {
        public abstract Models.Event                        GenericModel        { get; }
        public abstract bool                                IsEnabled           { get; set; }
        public abstract IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public abstract IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public abstract IReadOnlyList<string>               AvailableActions    { get; protected set; }
        public          List<IConditionBase>                Conditions          { get; protected set; } = new List<IConditionBase>();
        public          List<IActionBase>                   OnSuccessActions    { get; protected set; } = new List<IActionBase>();
        public          List<IActionBase>                   OnFailActions       { get; protected set; } = new List<IActionBase>();

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
                        "[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Handle] Event {0} provided {1} values, {2} excepted, event discarded!",
                        GetTypeName(), p_Context.VariableCount, ProvidedValues.Count));

                    OnEventFailed(p_Context);

                    return false;
                }

                var l_ConditionCount = Conditions.Count;
                for (var l_I = 0; l_I < l_ConditionCount; ++l_I)
                {
                    var l_Condition = Conditions[l_I];
                    if (l_Condition.IsEnabled && !l_Condition.Eval(p_Context))
                    {
                        CP_SDK.Unity.MTCoroutineStarter.EnqueueFromThread(DoOnFailActions(p_Context));
                        OnEventFailed(p_Context);
                        return false;
                    }
                }

                CP_SDK.Unity.MTCoroutineStarter.EnqueueFromThread(DoActions(p_Context));
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance?.Error("[ChatPlexMod_ChatIntegrations.Interfaces][IEvent.Handle] Error:");
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
        public void AddCondition(IConditionBase p_Condition)
        {
            Conditions.Add(p_Condition);
        }
        /// <summary>
        /// Move condition
        /// </summary>
        /// <param name="p_Condition">Condition to move</param>
        public int MoveCondition(IConditionBase p_Condition, bool p_Up)
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
        public void DeleteCondition(IConditionBase p_Condition)
        {
            Conditions.Remove(p_Condition);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an action to the event
        /// </summary>
        /// <param name="p_Action">Action to add</param>
        public void AddOnSuccessAction(IActionBase p_Action)
        {
            OnSuccessActions.Add(p_Action);
        }
        /// <summary>
        /// Move action
        /// </summary>
        /// <param name="p_Action">Action to move</param>
        public int MoveOnSuccessAction(IActionBase p_Action, bool p_Up)
        {
            var l_Index = OnSuccessActions.IndexOf(p_Action);

            if (l_Index == -1 || (l_Index == 0 && p_Up) || (l_Index == (OnSuccessActions.Count - 1) && !p_Up))
                return -1;

            OnSuccessActions.Remove(p_Action);
            OnSuccessActions.Insert(l_Index + (p_Up ? -1 : 1), p_Action);

            return OnSuccessActions.IndexOf(p_Action);
        }
        /// <summary>
        /// Delete an action from the event
        /// </summary>
        /// <param name="p_Action">Action to delete</param>
        public void DeleteOnSuccessAction(IActionBase p_Action)
        {
            OnSuccessActions.Remove(p_Action);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an on fail action to the event
        /// </summary>
        /// <param name="p_OnFailAction">Action to add</param>
        public void AddOnFailAction(IActionBase p_OnFailAction)
        {
            OnFailActions.Add(p_OnFailAction);
        }
        /// <summary>
        /// Move an on fail action
        /// </summary>
        /// <param name="p_OnFailAction">Action to move</param>
        public int MoveOnFailAction(IActionBase p_OnFailAction, bool p_Up)
        {
            var l_Index = OnFailActions.IndexOf(p_OnFailAction);

            if (l_Index == -1 || (l_Index == 0 && p_Up) || (l_Index == (OnFailActions.Count - 1) && !p_Up))
                return -1;

            OnFailActions.Remove(p_OnFailAction);
            OnFailActions.Insert(l_Index + (p_Up ? -1 : 1), p_OnFailAction);

            return OnFailActions.IndexOf(p_OnFailAction);
        }
        /// <summary>
        /// Delete an on fail action from the event
        /// </summary>
        /// <param name="p_OnFailAction">Action to delete</param>
        public void DeleteOnFailAction(IActionBase p_OnFailAction)
        {
            OnFailActions.Remove(p_OnFailAction);
        }

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

            GenericModel.CreationDate   = CP_SDK.Misc.Time.UnixTimeNow();
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
        /// Do actions
        /// </summary>
        /// <param name="p_Context">Event context</param>
        /// <returns></returns>
        private IEnumerator DoActions(Models.EventContext p_Context)
        {
            var l_OnSuccessActionCount = OnSuccessActions.Count;
            for (int l_I = 0; l_I < l_OnSuccessActionCount; ++l_I)
            {
                var l_Action = OnSuccessActions[l_I];
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
        /// <summary>
        /// Do on fail actions
        /// </summary>
        /// <param name="p_Context">Event context</param>
        /// <returns></returns>
        private IEnumerator DoOnFailActions(Models.EventContext p_Context)
        {
            var l_OnFailActionCount = OnFailActions.Count;
            for (int l_I = 0; l_I < l_OnFailActionCount; ++l_I)
            {
                var l_OnFailAction = OnFailActions[l_I];
                if (!l_OnFailAction.IsEnabled)
                    continue;

                yield return l_OnFailAction.Eval(p_Context);

                if (!p_Context.PreventNextActionFailure && p_Context.HasActionFailed)
                    break;
            }

            yield return null;
        }
    }
}
