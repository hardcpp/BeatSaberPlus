using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// Chat command event
    /// </summary>
    public class ChatCommand : IEvent<ChatCommand, Models.Events.ChatCommand>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatCommand()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.Emotes,  "MessageEmotes"),
                (EValueType.Integer, "MessageNumber"),
                (EValueType.String,  "MessageContent"),
                (EValueType.String,  "UserName")
            }.AsReadOnly();

            RegisterCustomCondition("User_Permissions", () => new Conditions.User_Permissions(), true);

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

        private XUIText m_CurrentCommandText = null;

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
                    XUIText.Make(   "This event will be triggered when someone uses the command which you've configured\n" +
                                    "You can change the command by clicking the rebind button bellow")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                )
                .SetBackground(true),

                XUIText.Make("Current command : ")
                    .SetAlign(TMPro.TextAlignmentOptions.Midline)
                    .Bind(ref m_CurrentCommandText),

                XUIPrimaryButton.Make("Rebind", OnRebindButton)
            };

            BuildUIAuto(p_Parent);

            UpdateUI();
        }
        /// <summary>
        /// Update UI component values
        /// </summary>
        private void UpdateUI() => m_CurrentCommandText.Element.SetText("Current command: " + Model.Command);
        /// <summary>
        /// Rebind button pressed
        /// </summary>
        private void OnRebindButton()
        {
            UI.SettingsMainView.Instance.ShowKeyboardModal(Model.Command, (p_Result) =>
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
            if (p_Context.Type != ETriggerType.ChatMessage || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.Message == null)
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

            var l_Emotes    = p_Context.Message.Emotes;
            var l_Number    = (Int64?)null;
            var l_Content   = null as string;

            if (l_FirstSpaceIndex != -1)
            {
                var l_Remaining = p_Context.Message.Message.Substring(l_FirstSpaceIndex + 1);

                if (Int64.TryParse(Regex.Match(l_Remaining, @"-?\d+").Value, out var l_NumberVal))
                    l_Number = (Int64?)l_NumberVal;

                l_Content = l_Remaining;
            }

            p_Context.AddValue(EValueType.String,   "UserName",         p_Context.User.DisplayName);
            p_Context.AddValue(EValueType.Emotes,   "MessageEmotes",    l_Emotes);
            p_Context.AddValue(EValueType.Integer,  "MessageNumber",    l_Number);
            p_Context.AddValue(EValueType.String,   "MessageContent",   l_Content);
        }
    }
}
