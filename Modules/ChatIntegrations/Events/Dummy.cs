using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// Dummy event
    /// </summary>
    class Dummy : IEvent<Dummy, Models.Event>
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
        public Dummy()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {

            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.ChatRequest_QueueDuration(),
                new Conditions.ChatRequest_QueueSize(),
                new Conditions.ChatRequest_QueueStatus(),
                new Conditions.Event_AlwaysFail(),
                new Conditions.Misc_Cooldown(),
                new Conditions.GamePlay_PlayingMap()
            }
            .Union(GetInstanciatedCustomConditionList())
            .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<IActionBase>()
            {

            }
            .Union(BeatSaberPlus_ChatIntegrations.Actions.ChatBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EmoteRainBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.EventBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.GamePlayBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.MiscBuilder.BuildFor(this))
            .Union(BeatSaberPlus_ChatIntegrations.Actions.TwitchBuilder.BuildFor(this))
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
            if (p_Context.Type != TriggerType.Dummy)
                return false;

            return true;
        }
    }
}
