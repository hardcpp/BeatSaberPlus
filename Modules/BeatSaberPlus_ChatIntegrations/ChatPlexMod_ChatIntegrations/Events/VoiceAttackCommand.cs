using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// VoiceAttack command event
    /// </summary>
    public class VoiceAttackCommand : IEvent<VoiceAttackCommand, Models.Events.VoiceAttackCommand>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public VoiceAttackCommand()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.String,  "CommandGUID"),
                (EValueType.String,  "CommandName")
            }.AsReadOnly();

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
                    XUIText.Make("This event will get trigerred when you trigger the VoiceAttack command you did configured")
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
        private void UpdateUI() => m_CurrentCommandText.Element.SetText("Current command: " + Model.CommandName);
        /// <summary>
        /// Rebind button pressed
        /// </summary>
        private void OnRebindButton()
        {
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted += VoiceAttack_OnCommandExecuted;
            View.ShowLoadingModal("Please trigger any VoiceAttack command", true, () =>
            {
                ChatIntegrations.Instance.OnVoiceAttackCommandExecuted -= VoiceAttack_OnCommandExecuted;
            });
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

            View.CloseLoadingModal();

            UpdateUI();
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
            if (p_Context.Type != ETriggerType.VoiceAttackCommand || p_Context.VoiceAttackCommandGUID == null || p_Context.VoiceAttackCommandName == null)
                return false;

            return p_Context.VoiceAttackCommandGUID == Model.CommandGUID;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(EValueType.String, "CommandGUID", p_Context.VoiceAttackCommandGUID);
            p_Context.AddValue(EValueType.String, "CommandName", p_Context.VoiceAttackCommandName);
        }
    }
}
