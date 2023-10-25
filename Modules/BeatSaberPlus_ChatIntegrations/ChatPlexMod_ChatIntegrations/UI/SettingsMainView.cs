using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.UI
{
    /// <summary>
    /// Settings main view controller
    /// </summary>
    public partial class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUIVLayout m_EmptyFrame  = null;
        private XUIVLayout m_MainFrame   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITabControl           m_TabControl                    = null;

        private XUIVLayout              m_TriggerTab_Content            = null;

        private XUIVVList               m_ConditionsTab_List            = null;
        private XUIVLayout              m_ConditionsTab_Content         = null;

        private XUIVVList               m_OnSuccessActionsTab_List      = null;
        private XUIVLayout              m_OnSuccessActionsTab_Content   = null;

        private XUIVVList               m_OnFailActionsTab_List         = null;
        private XUIVLayout              m_OnFailActionsTab_Content      = null;

        private Modals.AddXModal        m_AddXModal                     = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Interfaces.IEventBase m_CurrentEvent = null;

        private List<Data.ConditionListItem>    m_ConditionsTab_Items               = new List<Data.ConditionListItem>();
        private List<Data.ActionListItem>       m_OnSuccessActionsTab_Items         = new List<Data.ActionListItem>();
        private List<Data.ActionListItem>       m_OnFailActionsTab_Items            = new List<Data.ActionListItem>();

        private Data.ConditionListItem          m_SelectedConditionListItem         = null;
        private Data.ActionListItem             m_SelectedOnSuccessActionListItem   = null;
        private Data.ActionListItem             m_SelectedOnFailActionListItem      = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat Integrations | Settings"),

                XUIText.Make("Please select an event to edit on right screen")
                    .SetFontSize(4.5f)
            )
            .SetBackground(true, null, true)
            .Bind(ref m_EmptyFrame)
            .BuildUI(transform);

            m_EmptyFrame.SetActive(false);

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat Integrations | Settings"),

                XUITabControl.Make(
                    ("Trigger",             BuildTriggerTab()),
                    ("Conditions",          BuildConditionsTab()),
                    ("On Success Actions",  BuildOnSuccessActionsTab()),
                    ("On Fail Actions",     BuildOnFailActionsTab())
                )
                .Bind(ref m_TabControl)
            )
            .SetBackground(true, null, true)
            .Bind(ref m_MainFrame)
            .BuildUI(transform);

            m_MainFrame.SetActive(false);

            m_AddXModal = CreateModal<Modals.AddXModal>();

            /// Select a null event to hide everything
            SelectEvent(null);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            var l_Instance = ChatIntegrations.Instance;
            if (l_Instance != null)
            {
                l_Instance.SaveDatabase();
                l_Instance.OnBroadcasterChatMessage     = null;
                l_Instance.OnVoiceAttackCommandExecuted = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build trigger tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildTriggerTab()
        {
            return XUIVLayout.Make(

            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            .Bind(ref m_TriggerTab_Content);
        }
        /// <summary>
        /// Build conditions tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildConditionsTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<CP_SDK.UI.Data.TextListCell>.Get())
                            .OnListItemSelected((x) => OnConditionSelected(x))
                            .Bind(ref m_ConditionsTab_List)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetHeight(45)
                    .SetBackground(true)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUISecondaryButton.Make("▼", OnConditionMoveDownPressed).SetWidth(22.0f),
                        XUISecondaryButton.Make("▲", OnConditionMoveUpPressed)  .SetWidth(22.0f)
                    )
                    .SetPadding(0),

                    XUIHLayout.Make(
                        XUIPrimaryButton  .Make("+",      OnConditionTabCreate)   .SetWidth(10.0f),
                        XUISecondaryButton.Make("Toggle", OnConditionToggleButton).SetWidth(22.0f),
                        XUISecondaryButton.Make("-",      OnConditionDeleteButton).SetWidth(10.0f)
                    )
                    .SetPadding(0)
                )
                .SetPadding(0)
                .SetWidth(50.0f)
                .SetBackground(true),

                XUIVLayout.Make(

                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .OnReady(x => x.VLayoutGroup.childForceExpandWidth = x.VLayoutGroup.childForceExpandHeight = true)
                .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                .Bind(ref m_ConditionsTab_Content)
            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true);
        }
        /// <summary>
        /// Build on success actions tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildOnSuccessActionsTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<CP_SDK.UI.Data.TextListCell>.Get())
                            .OnListItemSelected((x) => OnOnSuccessActionSelected(x))
                            .Bind(ref m_OnSuccessActionsTab_List)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetHeight(45)
                    .SetBackground(true)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUISecondaryButton.Make("▼", OnOnSuccessActionMoveDownPressed).SetWidth(22.0f),
                        XUISecondaryButton.Make("▲", OnOnSuccessActionMoveUpPressed)  .SetWidth(22.0f)
                    )
                    .SetPadding(0),

                    XUIHLayout.Make(
                        XUIPrimaryButton  .Make("+",      OnOnSuccessActionTabCreate)   .SetWidth(10.0f),
                        XUISecondaryButton.Make("Toggle", OnOnSuccessActionToggleButton).SetWidth(22.0f),
                        XUISecondaryButton.Make("-",      OnOnSuccessActionDeleteButton).SetWidth(10.0f)
                    )
                    .SetPadding(0)
                )
                .SetPadding(0)
                .SetWidth(50.0f)
                .SetBackground(true),

                XUIVLayout.Make(

                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .OnReady(x => x.VLayoutGroup.childForceExpandWidth = x.VLayoutGroup.childForceExpandHeight = true)
                .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                .Bind(ref m_OnSuccessActionsTab_Content)
            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true);
        }
        /// <summary>
        /// Build on fail actions tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildOnFailActionsTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<CP_SDK.UI.Data.TextListCell>.Get())
                            .OnListItemSelected((x) => OnOnFailActionSelected(x))
                            .Bind(ref m_OnFailActionsTab_List)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetHeight(45)
                    .SetBackground(true)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUISecondaryButton.Make("▼", OnOnFailActionMoveDownPressed).SetWidth(22.0f),
                        XUISecondaryButton.Make("▲", OnOnFailActionMoveUpPressed)  .SetWidth(22.0f)
                    )
                    .SetPadding(0),

                    XUIHLayout.Make(
                        XUIPrimaryButton  .Make("+",      OnOnFailActionTabCreate)   .SetWidth(10.0f),
                        XUISecondaryButton.Make("Toggle", OnOnFailActionToggleButton).SetWidth(22.0f),
                        XUISecondaryButton.Make("-",      OnOnFailActionDeleteButton).SetWidth(10.0f)
                    )
                    .SetPadding(0)
                )
                .SetPadding(0)
                .SetWidth(50.0f)
                .SetBackground(true),

                XUIVLayout.Make(

                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .OnReady(x => x.VLayoutGroup.childForceExpandWidth = x.VLayoutGroup.childForceExpandHeight = true)
                .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                .Bind(ref m_OnFailActionsTab_Content)
            )
            .SetSpacing(0).SetPadding(0)
            .OnReady(x => x.CSizeFitter.enabled = false)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Select event to edit
        /// </summary>
        /// <param name="p_Event"></param>
        internal void SelectEvent(Interfaces.IEventBase p_Event)
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage      = null;
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted  = null;

            CloseAllModals();

            m_CurrentEvent = p_Event;

            /// Clean up trigger specific UI
            GameObjectU.DestroyChilds(m_TriggerTab_Content.Element.gameObject);

            /// Hide everything if no event selection
            if (p_Event == null)
            {
                m_MainFrame.SetActive(false);
                m_EmptyFrame.SetActive(true);
                return;
            }

            p_Event.BuildUI(m_TriggerTab_Content.Element.RTransform);

            RebuildConditionList(m_CurrentEvent.Conditions.FirstOrDefault());
            RebuildOnSuccessActionList(m_CurrentEvent.OnSuccessActions.FirstOrDefault());
            RebuildOnFailActionList(m_CurrentEvent.OnFailActions.FirstOrDefault());

            /// Update UI
            m_EmptyFrame.SetActive(false);
            m_MainFrame.SetActive(true);

            /// Force first tab to be active
            m_TabControl.SetActiveTab(0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuilt condition list
        /// </summary>
        /// <param name="p_KeepFocus">To focus</param>
        private void RebuildConditionList(Interfaces.IConditionBase p_ConditionToFocus)
        {
            if (!UICreated)
                return;

            m_ConditionsTab_Items = m_CurrentEvent.Conditions.Select(x => new Data.ConditionListItem(x)).ToList();
            m_ConditionsTab_List.SetListItems(m_ConditionsTab_Items);
            m_ConditionsTab_List.SetSelectedListItem(m_ConditionsTab_Items.FirstOrDefault(x => x.Condition == p_ConditionToFocus) ?? m_ConditionsTab_Items.FirstOrDefault());
        }
        /// <summary>
        /// When a condition is selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnConditionSelected(CP_SDK.UI.Data.IListItem p_SelectedItem)
        {
            /// Clean up condition specific UI
            GameObjectU.DestroyChilds(m_ConditionsTab_Content.Element.gameObject);

            m_SelectedConditionListItem = p_SelectedItem as Data.ConditionListItem;
            if (m_SelectedConditionListItem == null)
                return;

            m_SelectedConditionListItem.Condition.BuildUI(m_ConditionsTab_Content.Element.RTransform);
        }
        /// <summary>
        /// Move condition down
        /// </summary>
        private void OnConditionMoveDownPressed()
        {
            if (m_SelectedConditionListItem == null)
                return;

            m_CurrentEvent.MoveCondition(m_SelectedConditionListItem.Condition, false);
            RebuildConditionList(m_SelectedConditionListItem.Condition);
        }
        /// <summary>
        /// Move condition up
        /// </summary>
        private void OnConditionMoveUpPressed()
        {
            if (m_SelectedConditionListItem == null)
                return;

            m_CurrentEvent.MoveCondition(m_SelectedConditionListItem.Condition, true);
            RebuildConditionList(m_SelectedConditionListItem.Condition);
        }
        /// <summary>
        /// On condition create button
        /// </summary>
        private void OnConditionTabCreate()
        {
            ShowModal(m_AddXModal);
            m_AddXModal.Init(m_CurrentEvent.AvailableConditions, (p_Type) =>
            {
                var l_Condition = ChatIntegrations.CreateCondition(p_Type);
                l_Condition.Event       = m_CurrentEvent;
                l_Condition.IsEnabled   = true;

                m_CurrentEvent.AddCondition(l_Condition);

                var l_ListItem = new Data.ConditionListItem(l_Condition);
                m_ConditionsTab_List.AddListItem(l_ListItem);
                m_ConditionsTab_List.SetSelectedListItem(l_ListItem);
            });
        }
        /// <summary>
        /// Toggle condition button
        /// </summary>
        private void OnConditionToggleButton()
        {
            if (m_SelectedConditionListItem == null)
            {
                ShowMessageModal("Please select a condition first!");
                return;
            }

            var l_Condition = m_SelectedConditionListItem.Condition;
            if (l_Condition.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable condition\n\"{l_Condition.GetTypeName()}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_Condition.IsEnabled = false;
                    m_SelectedConditionListItem.Refresh();
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable condition\n\"{l_Condition.GetTypeName()}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_Condition.IsEnabled = true;
                    m_SelectedConditionListItem.Refresh();
                });
            }
        }
        /// <summary>
        /// On delete condition button
        /// </summary>
        private void OnConditionDeleteButton()
        {
            if (m_SelectedConditionListItem == null)
            {
                ShowMessageModal("Please select a condition first!");
                return;
            }

            var l_Condition = m_SelectedConditionListItem.Condition;
            ShowConfirmationModal($"<color=red>Do you want to delete condition</color>\n\"{l_Condition.GetTypeName()}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_ConditionsTab_Items.Remove(m_SelectedConditionListItem);
                m_ConditionsTab_List.RemoveListItem(m_SelectedConditionListItem);
                l_Condition.Event.DeleteCondition(l_Condition);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuilt action list
        /// </summary>
        /// <param name="p_KeepFocus">Should keep actual focus</param>
        private void RebuildOnSuccessActionList(Interfaces.IActionBase p_ActionToFocus)
        {
            if (!UICreated)
                return;

            var l_Items = m_CurrentEvent.OnSuccessActions.Select(x => new Data.ActionListItem(x)).ToList();
            m_OnSuccessActionsTab_List.SetListItems(l_Items);
            m_OnSuccessActionsTab_List.SetSelectedListItem(l_Items.FirstOrDefault(x => x.Action == p_ActionToFocus) ?? l_Items.FirstOrDefault());
        }
        /// <summary>
        /// When an action is selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnOnSuccessActionSelected(CP_SDK.UI.Data.IListItem p_SelectedItem)
        {
            /// Clean up condition specific UI
            GameObjectU.DestroyChilds(m_OnSuccessActionsTab_Content.Element.gameObject);

            m_SelectedOnSuccessActionListItem = p_SelectedItem as Data.ActionListItem;
            if (m_SelectedOnSuccessActionListItem == null)
                return;

            m_SelectedOnSuccessActionListItem.Action.BuildUI(m_OnSuccessActionsTab_Content.Element.RTransform);
        }
        /// <summary>
        /// Move action down
        /// </summary>
        private void OnOnSuccessActionMoveDownPressed()
        {
            if (m_SelectedOnSuccessActionListItem == null)
                return;

            m_CurrentEvent.MoveOnSuccessAction(m_SelectedOnSuccessActionListItem.Action, false);
            RebuildOnSuccessActionList(m_SelectedOnSuccessActionListItem.Action);
        }
        /// <summary>
        /// Move action up
        /// </summary>
        private void OnOnSuccessActionMoveUpPressed()
        {
            if (m_SelectedOnSuccessActionListItem == null)
                return;

            m_CurrentEvent.MoveOnSuccessAction(m_SelectedOnSuccessActionListItem.Action, true);
            RebuildOnSuccessActionList(m_SelectedOnSuccessActionListItem.Action);
        }
        /// <summary>
        /// On create on success action button
        /// </summary>
        private void OnOnSuccessActionTabCreate()
        {
            ShowModal(m_AddXModal);
            m_AddXModal.Init(m_CurrentEvent.AvailableActions, (p_Type) =>
            {
                var l_Action = ChatIntegrations.CreateAction(p_Type);
                l_Action.Event       = m_CurrentEvent;
                l_Action.IsEnabled   = true;

                m_CurrentEvent.AddOnSuccessAction(l_Action);

                var l_ListItem = new Data.ActionListItem(l_Action);
                m_OnSuccessActionsTab_List.AddListItem(l_ListItem);
                m_OnSuccessActionsTab_List.SetSelectedListItem(l_ListItem);
            });
        }
        /// <summary>
        /// Toggle action button
        /// </summary>
        private void OnOnSuccessActionToggleButton()
        {
            if (m_SelectedOnSuccessActionListItem == null)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_OnSuccessAction = m_SelectedOnSuccessActionListItem.Action;
            if (l_OnSuccessAction.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable action\n\"{l_OnSuccessAction.GetTypeName()}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_OnSuccessAction.IsEnabled = false;
                    m_SelectedOnSuccessActionListItem.Refresh();
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable action\n\"{l_OnSuccessAction.GetTypeName()}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    l_OnSuccessAction.IsEnabled = true;
                    m_SelectedOnSuccessActionListItem.Refresh();
                });
            }
        }
        /// <summary>
        /// On delete action button
        /// </summary>
        private void OnOnSuccessActionDeleteButton()
        {
            if (m_SelectedOnSuccessActionListItem == null)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_OnSuccessAction = m_SelectedOnSuccessActionListItem.Action;
            ShowConfirmationModal($"<color=red>Do you want to delete action</color>\n\"{l_OnSuccessAction.GetTypeName()}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_OnSuccessActionsTab_Items.Remove(m_SelectedOnSuccessActionListItem);
                m_OnSuccessActionsTab_List.RemoveListItem(m_SelectedOnSuccessActionListItem);
                l_OnSuccessAction.Event.DeleteOnSuccessAction(l_OnSuccessAction);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Rebuilt action list
        /// </summary>
        /// <param name="p_KeepFocus">Should keep actual focus</param>
        private void RebuildOnFailActionList(Interfaces.IActionBase p_OnFailActionToFocus)
        {
            if (!UICreated)
                return;

            var l_Items = m_CurrentEvent.OnFailActions.Select(x => new Data.ActionListItem(x)).ToList();
            m_OnFailActionsTab_List.SetListItems(l_Items);
            m_OnFailActionsTab_List.SetSelectedListItem(l_Items.FirstOrDefault(x => x.Action == p_OnFailActionToFocus) ?? l_Items.FirstOrDefault());
        }
        /// <summary>
        /// When an on fail action is selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnOnFailActionSelected(CP_SDK.UI.Data.IListItem p_SelectedItem)
        {
            /// Clean up condition specific UI
            GameObjectU.DestroyChilds(m_OnFailActionsTab_Content.Element.gameObject);

            m_SelectedOnFailActionListItem = p_SelectedItem as Data.ActionListItem;
            if (m_SelectedOnFailActionListItem == null)
                return;

            m_SelectedOnFailActionListItem.Action.BuildUI(m_OnFailActionsTab_Content.Element.RTransform);
        }
        /// <summary>
        /// Move on fail action down
        /// </summary>
        private void OnOnFailActionMoveDownPressed()
        {
            if (m_SelectedOnFailActionListItem == null)
                return;

            m_CurrentEvent.MoveOnFailAction(m_SelectedOnFailActionListItem.Action, false);
            RebuildOnSuccessActionList(m_SelectedOnFailActionListItem.Action);
        }
        /// <summary>
        /// Move on fail action up
        /// </summary>
        private void OnOnFailActionMoveUpPressed()
        {
            if (m_SelectedOnFailActionListItem == null)
                return;

            m_CurrentEvent.MoveOnFailAction(m_SelectedOnFailActionListItem.Action, true);
            RebuildOnSuccessActionList(m_SelectedOnFailActionListItem.Action);
        }
        /// <summary>
        /// On create on fail action button
        /// </summary>
        private void OnOnFailActionTabCreate()
        {
            ShowModal(m_AddXModal);
            m_AddXModal.Init(m_CurrentEvent.AvailableActions, (p_Type) =>
            {
                var l_Action = ChatIntegrations.CreateAction(p_Type);
                l_Action.Event       = m_CurrentEvent;
                l_Action.IsEnabled   = true;

                m_CurrentEvent.AddOnFailAction(l_Action);

                var l_ListItem = new Data.ActionListItem(l_Action);
                m_OnFailActionsTab_List.AddListItem(l_ListItem);
                m_OnFailActionsTab_List.SetSelectedListItem(l_ListItem);
            });
        }
        /// <summary>
        /// Toggle on fail action button
        /// </summary>
        private void OnOnFailActionToggleButton()
        {
            if (m_SelectedOnFailActionListItem == null)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_OnFailAction = m_SelectedOnFailActionListItem.Action;
            if (l_OnFailAction.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable action\n\"{l_OnFailAction.GetTypeName()}\"?", (p_Confirm) =>
                {
                    l_OnFailAction.IsEnabled = false;
                    m_SelectedOnFailActionListItem.Refresh();
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable action\n\"{l_OnFailAction.GetTypeName()}\"?", (p_Confirm) =>
                {
                    l_OnFailAction.IsEnabled = true;
                    m_SelectedOnFailActionListItem.Refresh();
                });
            }
        }
        /// <summary>
        /// On delete on fail action button
        /// </summary>
        private void OnOnFailActionDeleteButton()
        {
            if (m_SelectedOnFailActionListItem == null)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_OnFailAction = m_SelectedOnFailActionListItem.Action;
            ShowConfirmationModal($"<color=red>Do you want to delete action</color>\n\"{l_OnFailAction.GetTypeName()}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                m_OnFailActionsTab_Items.Remove(m_SelectedOnFailActionListItem);
                m_OnFailActionsTab_List.RemoveListItem(m_SelectedOnFailActionListItem);
                l_OnFailAction.Event.DeleteOnFailAction(l_OnFailAction);
            });
        }
    }
}
