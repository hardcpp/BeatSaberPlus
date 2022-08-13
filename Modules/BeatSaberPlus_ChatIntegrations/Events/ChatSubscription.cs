using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// Chat subscription event
    /// </summary>
    public class ChatSubscription : IEvent<ChatSubscription, Models.Events.ChatSubscription>
    {
        /// <summary>
        /// Provided values list
        /// </summary>
        public override IReadOnlyList<(IValueType, string)> ProvidedValues { get; protected set; }
        /// <summary>
        /// Available conditions list
        /// </summary>
        public override IReadOnlyList<IConditionBase> AvailableConditions { get; protected set; }
        /// <summary>
        /// Available actions list
        /// </summary>
        public override IReadOnlyList<IActionBase> AvailableActions { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatSubscription()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.String,     "UserName"),
                (IValueType.String,     "SubPlan"),
                (IValueType.String,     "RecipientName"),
                (IValueType.Integer,    "MonthCount"),
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.ChatRequest_QueueDuration(),
                new Conditions.ChatRequest_QueueSize(),
                new Conditions.ChatRequest_QueueStatus(),
                new Conditions.GamePlay_InMenu(),
                new Conditions.GamePlay_PlayingMap(),
                new Conditions.Misc_Cooldown(),

                new Conditions.Subscription_IsGift(),
                new Conditions.Subscription_PurchasedMonthCount(),
                new Conditions.Subscription_PlanType()
            }
            .Union(BeatSaberPlus_ChatIntegrations.Conditions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Conditions.OBSBuilder.BuildFor(this))
            .Union(GetInstanciatedCustomConditionList())
            .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<IActionBase>()
            {

            }
            .Union(BeatSaberPlus_ChatIntegrations.Actions.Camera2Builder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.ChatBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EmoteRainBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.GamePlayBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.MiscBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.NoteTweakerBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.OBSBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.TwitchBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.SongChartVisualizerBuilder.BuildFor(this))
            .Union(GetInstanciatedCustomActionList())
            .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIObject("InfoBackground")]
        private GameObject m_InfoBackground = null;
#pragma warning restore CS0414

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed bool CanBeExecuted(Models.EventContext p_Context)
        {
            /// Ensure that we have all data
            if (p_Context.Type != TriggerType.ChatSubscription || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.SubscriptionEvent == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(IValueType.String,    "UserName",        p_Context.User.DisplayName);
            p_Context.AddValue(IValueType.String,    "SubPlan",         p_Context.SubscriptionEvent.SubPlan);
            p_Context.AddValue(IValueType.String,    "RecipientName",   p_Context.SubscriptionEvent.RecipientDisplayName ?? "");
            p_Context.AddValue(IValueType.Integer,   "MonthCount",      (Int64?)p_Context.SubscriptionEvent.PurchasedMonthCount);
        }
    }
}
