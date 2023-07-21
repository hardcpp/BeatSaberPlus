using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// Dummy event
    /// </summary>
    public class Dummy : IEvent<Dummy, Models.Event>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public Dummy()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {

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

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("Dummy event that can get triggered by other events!")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                ).SetBackground(true),

                XUIPrimaryButton.Make("Execute", OnExecutePressed)
            };

            BuildUIAuto(p_Parent);
        }
        /// <summary>
        /// Execute button pressed
        /// </summary>
        private void OnExecutePressed()
        {
            ChatIntegrations.Instance.ExecuteEvent(this, new Models.EventContext() { Type = ETriggerType.Dummy });
            UI.SettingsMainView.Instance.ShowMessageModal("Ok!");
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
            if (p_Context.Type != ETriggerType.Dummy)
                return false;

            return true;
        }
    }
}
