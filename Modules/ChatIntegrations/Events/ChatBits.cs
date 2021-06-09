using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlus.Modules.ChatIntegrations.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Events
{
    /// <summary>
    /// Chat bits event
    /// </summary>
    public class ChatBits : IEvent<ChatBits, Models.Events.ChatBits>
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
        public ChatBits()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.Integer, "Bits"),
                (IValueType.String,  "UserName")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.Bits_Amount(),
                new Conditions.Event_AlwaysFail(),
                new Conditions.GamePlay_InMenu(),
                new Conditions.GamePlay_PlayingMap(),
                new Conditions.Misc_Cooldown()
            }
            .Union(GetInstanciatedCustomConditionList())
            .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<IActionBase>()
            {

            }
            .Union(Modules.ChatIntegrations.Actions.ChatBuilder.BuildFor(this))
            .Union(Modules.ChatIntegrations.Actions.EmoteRainBuilder.BuildFor(this))
            .Union(Modules.ChatIntegrations.Actions.EventBuilder.BuildFor(this))
            .Union(Modules.ChatIntegrations.Actions.GamePlayBuilder.BuildFor(this))
            .Union(Modules.ChatIntegrations.Actions.MiscBuilder.BuildFor(this))
            .Union(Modules.ChatIntegrations.Actions.TwitchBuilder.BuildFor(this))
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
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);
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
            if (p_Context.Type != TriggerType.ChatBits || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.BitsEvent == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(IValueType.Integer, "Bits",      (Int64?)p_Context.BitsEvent.Value);
            p_Context.AddValue(IValueType.String,  "UserName",  p_Context.User.DisplayName);
        }
    }
}
