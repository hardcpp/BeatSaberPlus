using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// VoiceAttack command event
    /// </summary>
    public class VoiceAttackCommand : IEvent<VoiceAttackCommand, Models.Events.VoiceAttackCommand>
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
        public VoiceAttackCommand()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.String,  "CommandGUID"),
                (IValueType.String,  "CommandName")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
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
        [UIComponent("CurrentCommandText")]
        private TextMeshProUGUI m_CurrentCommandText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("RebindModal")]
        protected HMUI.ModalView m_RebindModal = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser params instance
        /// </summary>
        private BSMLParserParams m_ParserParams;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoBackground, 0.5f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_RebindModal, 0.5f);

            /// Update UI
            UpdateUI();
        }
        /// <summary>
        /// Update UI component values
        /// </summary>
        private void UpdateUI()
        {
            m_CurrentCommandText.SetText("<u>Current command :</u> " + Model.CommandName);
        }
        /// <summary>
        /// Rebind button pressed
        /// </summary>
        [UIAction("click-rebind-btn-pressed")]
        private void OnRebindButton()
        {
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted += VoiceAttack_OnCommandExecuted;

            m_ParserParams.EmitEvent("ShowRebindModal");
        }
        /// <summary>
        /// On VoiceAttack command executed
        /// </summary>
        /// <param name="p_GUID">Command GUID</param>
        /// <param name="p_Name">Command Name</param>
        private void VoiceAttack_OnCommandExecuted(string p_GUID, string p_Name)
        {
            Model.CommandGUID = p_GUID;
            Model.CommandName = p_Name;

            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted -= VoiceAttack_OnCommandExecuted;

            m_ParserParams.EmitEvent("CloseRebindModal");

            UpdateUI();
        }
        /// <summary>
        /// Cancel rebind button pressed
        /// </summary>
        [UIAction("click-cancel-rebind-btn-pressed")]
        private void OnCancelSetFromChatButton()
        {
            m_ParserParams.EmitEvent("CloseRebindModal");
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted -= VoiceAttack_OnCommandExecuted;
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
            if (p_Context.Type != TriggerType.VoiceAttackCommand || p_Context.VoiceAttackCommandGUID == null || p_Context.VoiceAttackCommandName == null)
                return false;

            return p_Context.VoiceAttackCommandGUID == Model.CommandGUID;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(IValueType.String, "CommandGUID", p_Context.VoiceAttackCommandGUID);
            p_Context.AddValue(IValueType.String, "CommandName", p_Context.VoiceAttackCommandName);
        }
    }
}
