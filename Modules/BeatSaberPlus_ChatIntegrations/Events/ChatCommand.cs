using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberPlus_ChatIntegrations.Interfaces;
using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Events
{
    /// <summary>
    /// Chat command event
    /// </summary>
    public class ChatCommand : IEvent<ChatCommand, Models.Events.ChatCommand>
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
        public ChatCommand()
        {
            /// Build provided values list
            ProvidedValues = new List<(IValueType, string)>()
            {
                (IValueType.Emotes,  "MessageEmotes"),
                (IValueType.Integer, "MessageNumber"),
                (IValueType.String,  "MessageContent"),
                (IValueType.String,  "UserName")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<IConditionBase>()
            {
                new Conditions.ChatRequest_QueueDuration(),
                new Conditions.ChatRequest_QueueSize(),
                new Conditions.ChatRequest_QueueStatus(),
                new Conditions.Event_AlwaysFail(),
                new Conditions.GamePlay_InMenu(),
                new Conditions.GamePlay_PlayingMap(),
                new Conditions.Misc_Cooldown(),
                new Conditions.User_Permissions()
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
        [UIComponent("CurrentTriggerText")]
        private TextMeshProUGUI m_CurrentTriggerText = null;
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

            /// Update UI
            UpdateUI();
        }
        /// <summary>
        /// Update UI component values
        /// </summary>
        private void UpdateUI()
        {
            m_CurrentTriggerText.SetText("<u>Current command :</u> " + Model.Command);
        }
        /// <summary>
        /// Rebind button pressed
        /// </summary>
        [UIAction("click-rebind-btn-pressed")]
        private void OnRebindButton()
        {
            UI.Settings.Instance.UIShowInputKeyboard(Model.Command, (p_Result) =>
            {
                if (p_Result.Length > 0 && p_Result[0] != '!')
                    p_Result = "!" + p_Result;

                var l_FirstSpaceIndex = p_Result.IndexOf(' ');

                Model.Command = (l_FirstSpaceIndex != -1 ? p_Result.Substring(0, l_FirstSpaceIndex) : p_Result).ToLower();

                /// Update UI
                UpdateUI();
            });
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
            if (p_Context.Type != TriggerType.ChatMessage || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.Message == null)
                return false;

            /// Look for command sign
            if (p_Context.Message.Message.Length < 2 || p_Context.Message.Message[0] != '!')
                return false;

            var l_FirstSpaceIndex   = p_Context.Message.Message.IndexOf(' ');
            var l_Command           = (l_FirstSpaceIndex != -1 ? p_Context.Message.Message.Substring(0, l_FirstSpaceIndex) : p_Context.Message.Message).ToLower();

            return l_Command == Model.Command;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            var l_FirstSpaceIndex = p_Context.Message.Message.IndexOf(' ');

            var l_Emotes    = p_Context.Message.Emotes.ToList();
            var l_Number    = (Int64?)null;
            var l_Content   = null as string;

            if (l_FirstSpaceIndex != -1)
            {
                var l_Remaining = p_Context.Message.Message.Substring(l_FirstSpaceIndex + 1);

                if (Int64.TryParse(Regex.Match(l_Remaining, @"-?\d+").Value, out var l_NumberVal))
                    l_Number = (Int64?)l_NumberVal;

                l_Content = l_Remaining;
            }

            p_Context.AddValue(IValueType.String,   "UserName",         p_Context.User.DisplayName);
            p_Context.AddValue(IValueType.Emotes,   "MessageEmotes",    l_Emotes);
            p_Context.AddValue(IValueType.Integer,  "MessageNumber",    l_Number);
            p_Context.AddValue(IValueType.String,   "MessageContent",   l_Content);
        }
    }
}
