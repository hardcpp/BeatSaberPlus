using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// Chat subscription event
    /// </summary>
    public class ChatSubscription : IEvent<ChatSubscription, Models.Events.ChatSubscription>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatSubscription()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.String,     "UserName"),
                (EValueType.String,     "SubPlan"),
                (EValueType.String,     "RecipientName"),
                (EValueType.Integer,    "MonthCount"),
            }.AsReadOnly();

            RegisterCustomCondition("Subscription_IsGift",              () => new Conditions.Subscription_IsGift(),                 true);
            RegisterCustomCondition("Subscription_PurchasedMonthCount", () => new Conditions.Subscription_PurchasedMonthCount(),    true);
            RegisterCustomCondition("Subscription_PlanType",            () => new Conditions.Subscription_PlanType(),               true);

            /// Build possible list
            AvailableConditions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalConditionsTypes)
                .Union(GetCustomConditionTypes())
                .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalActionsTypes)
                .Union(GetCustomActionTypes())
                .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("This event will be triggered whenever someone subscribe or subgift to your channel!")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                ).SetBackground(true)
            };

            BuildUIAuto(p_Parent);
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
            if (p_Context.Type != ETriggerType.ChatSubscription || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.SubscriptionEvent == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(EValueType.String,    "UserName",        p_Context.User.DisplayName);
            p_Context.AddValue(EValueType.String,    "SubPlan",         p_Context.SubscriptionEvent.SubPlan);
            p_Context.AddValue(EValueType.String,    "RecipientName",   p_Context.SubscriptionEvent.RecipientDisplayName ?? "");
            p_Context.AddValue(EValueType.Integer,   "MonthCount",      (Int64?)p_Context.SubscriptionEvent.PurchasedMonthCount);
        }
    }
}
