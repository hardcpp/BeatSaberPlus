using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// Chat bits event
    /// </summary>
    public class ChatBits : IEvent<ChatBits, Models.Events.ChatBits>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatBits()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.Integer, "Bits"),
                (EValueType.String,  "UserName")
            }.AsReadOnly();

            RegisterCustomCondition("Bits_Amount", () => new Conditions.Bits_Amount(), true);

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
                    XUIText.Make("This event will be triggered whenever someone spend bits your channel!")
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
            if (p_Context.Type != ETriggerType.ChatBits || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.BitsEvent == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(EValueType.Integer, "Bits",      (Int64?)p_Context.BitsEvent.Value);
            p_Context.AddValue(EValueType.String,  "UserName",  p_Context.User.DisplayName);
        }
    }
}
