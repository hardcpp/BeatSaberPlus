using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatIntegrations.UI
{
    /// <summary>
    /// Chat integrations main settings view
    /// </summary>
    internal partial class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
        /// <summary>
        /// Maximum item on a list page
        /// </summary>
        private static int s_CONDITION_ACTION_PER_PAGE = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("MessageFrame")]
        private GameObject m_MessageFrame = null;
        [UIObject("MessageFrame_Background")]
        private GameObject m_MessageFrame_Background = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("EventFrame")]
        private GameObject m_EventFrame = null;

        [UIObject("EventFrame_TabSelector")]
        private GameObject m_EventFrame_TabSelector;
        private TextSegmentedControl m_EventFrame_TabSelectorControl = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("EventFrame_TriggerTab")]
        private GameObject m_EventFrame_TriggerTab = null;
        [UIComponent("EventFrame_TriggerTab_Title")]
        private TextMeshProUGUI m_EventFrame_TriggerTab_Title = null;
        [UIObject("EventFrame_TriggerTab_EventContent")]
        private GameObject m_EventFrame_TriggerTab_EventContent = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("EventFrame_ConditionsTab")]
        private GameObject m_EventFrame_ConditionsTab = null;

        [UIComponent("EventFrame_ConditionsTab_UpButton")]
        private Button m_EventFrame_ConditionsTab_UpButton = null;
        [UIObject("EventFrame_ConditionsTab_List")]
        private GameObject m_EventFrame_ConditionsTab_ListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_EventFrame_ConditionsTab_List = null;
        [UIComponent("EventFrame_ConditionsTab_DownButton")]
        private Button m_EventFrame_ConditionsTab_DownButton = null;

        [UIObject("EventFrame_ConditionsTab_ConditionContent")]
        private GameObject m_EventFrame_ConditionsTab_ConditionContent = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("EventFrame_ActionsTab")]
        private GameObject m_EventFrame_ActionsTab = null;

        [UIComponent("EventFrame_ActionsTab_UpButton")]
        private Button m_EventFrame_ActionsTab_UpButton = null;
        [UIObject("EventFrame_ActionsTab_List")]
        private GameObject m_EventFrame_ActionsTab_ListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_EventFrame_ActionsTab_List = null;
        [UIComponent("EventFrame_ActionsTab_DownButton")]
        private Button m_EventFrame_ActionsTab_DownButton = null;

        [UIObject("EventFrame_ActionsTab_ActionContent")]
        private GameObject m_EventFrame_ActionsTab_ActionContent = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("InputKeyboard")]
        private ModalKeyboard m_InputKeyboard = null;
        [UIValue("InputKeyboardValue")]
        private string m_InputKeyboardValue = "";
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Interfaces.IEventBase m_CurrentEvent = null;
        private int m_ConditionListPage = 1;
        private int m_SelectedCondition = -1;
        private int m_ActionListPage = 1;
        private int m_SelectedAction = -1;

        private Dictionary<string, System.Type> m_ConditionAddingMatches = new Dictionary<string, System.Type>();

        /// <summary>
        /// Keyboard original key count
        /// </summary>
        private int m_InputKeyboardInitialKeyCount = -1;
        /// <summary>
        /// Input keyboard callback
        /// </summary>
        private Action<string> m_InputKeyboardCallback = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Update opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_MessageFrame_Background,                     0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EventFrame_TriggerTab,                       0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EventFrame_ConditionsTab,                    0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EventFrame_ConditionsTab_ConditionContent,   0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EventFrame_ActionsTab,                       0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_EventFrame_ActionsTab_ActionContent,         0.50f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_InputKeyboard.modalView,                          0.75f);

            /// Scale down up & down button
            m_EventFrame_ConditionsTab_UpButton.transform.localScale    = Vector3.one * 0.6f;
            m_EventFrame_ConditionsTab_DownButton.transform.localScale  = Vector3.one * 0.6f;
            m_EventFrame_ActionsTab_UpButton.transform.localScale       = Vector3.one * 0.6f;
            m_EventFrame_ActionsTab_DownButton.transform.localScale     = Vector3.one * 0.6f;

            /// Setup condition list
            if (m_EventFrame_ConditionsTab_ListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_EventFrame_ConditionsTab_ListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_EventFrame_ConditionsTab_ListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_EventFrame_ConditionsTab_ListView.GetComponentInChildren<CustomListTableData>());
                m_EventFrame_ConditionsTab_List = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_EventFrame_ConditionsTab_List.TableViewInstance = l_BSMLTableView;
                m_EventFrame_ConditionsTab_List.CellSizeValue = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent += OnConditionSelected;
                l_BSMLTableView.SetDataSource(m_EventFrame_ConditionsTab_List, false);

                /// Bind events
                m_EventFrame_ConditionsTab_UpButton.onClick.AddListener(OnConditionPageUpPressed);
                m_EventFrame_ConditionsTab_DownButton.onClick.AddListener(OnConditionPageDownPressed);
            }

            /// Setup action list
            if (m_EventFrame_ActionsTab_ListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_EventFrame_ActionsTab_ListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_EventFrame_ActionsTab_ListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_EventFrame_ActionsTab_ListView.GetComponentInChildren<CustomListTableData>());
                m_EventFrame_ActionsTab_List = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_EventFrame_ActionsTab_List.TableViewInstance = l_BSMLTableView;
                m_EventFrame_ActionsTab_List.CellSizeValue = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent += OnActionSelected;
                l_BSMLTableView.SetDataSource(m_EventFrame_ActionsTab_List, false);

                /// Bind events
                m_EventFrame_ActionsTab_UpButton.onClick.AddListener(OnActionPageUpPressed);
                m_EventFrame_ActionsTab_DownButton.onClick.AddListener(OnActionPageDownPressed);
            }

            SetupAddConditionFrame();
            SetupAddActionFrame();

            /// Create type selector
            m_EventFrame_TabSelectorControl = BeatSaberPlus.SDK.UI.TextSegmentedControl.Create(m_EventFrame_TabSelector.transform as RectTransform, false);
            m_EventFrame_TabSelectorControl.SetTexts(new string[] { "Trigger", "Conditions", "Actions" });
            m_EventFrame_TabSelectorControl.ReloadData();
            m_EventFrame_TabSelectorControl.didSelectCellEvent += OnTabSelected;

            /// Select a null event to hide everything
            SelectEvent(null);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {

        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage = null;
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted = null;
            ChatIntegrations.Instance.SaveDatabase();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Select event to edit
        /// </summary>
        /// <param name="p_Event"></param>
        internal void SelectEvent(Interfaces.IEventBase p_Event)
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage = null;
            ChatIntegrations.Instance.OnVoiceAttackCommandExecuted = null;

            m_CurrentEvent = p_Event;

            /// Clean up trigger specific UI
            if (m_EventFrame_TriggerTab_EventContent.transform.childCount != 0)
                GameObject.DestroyImmediate(m_EventFrame_TriggerTab_EventContent.transform.GetChild(0).gameObject);

            OnConditionSelected(null, -1);
            OnActionSelected(null, -1);

            /// Hide everything if no event selection
            if (p_Event == null)
            {
                m_EventFrame.SetActive(false);
                m_AddConditionFrame.SetActive(false);
                m_AddActionFrame.SetActive(false);
                m_MessageFrame.SetActive(true);
                return;
            }

            ////////////////////////////////////////////////////////////////////////////
            /// Trigger tab
            m_EventFrame_TriggerTab_Title.SetText("<u><b><color=yellow>" + p_Event.GetTypeNameShort() + " | " + p_Event.GenericModel.Name + "</b></u>");
            p_Event.BuildUI(m_EventFrame_TriggerTab_EventContent.transform);
            ////////////////////////////////////////////////////////////////////////////
            /// Conditions tab
            m_ConditionListPage = 1;
            m_SelectedCondition = -1;
            RebuildConditionList(m_CurrentEvent.Conditions.FirstOrDefault());
            ////////////////////////////////////////////////////////////////////////////
            /// Actions
            m_ActionListPage = 1;
            m_SelectedAction = -1;
            RebuildActionList(m_CurrentEvent.Actions.FirstOrDefault());
            ////////////////////////////////////////////////////////////////////////////

            /// Update UI
            m_MessageFrame.SetActive(false);
            m_AddConditionFrame.SetActive(false);
            m_AddActionFrame.SetActive(false);
            m_EventFrame.SetActive(true);

            /// Force first tab to be active
            m_EventFrame_TabSelectorControl.SelectCellWithNumber(0);
            OnTabSelected(null, 0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When a tab is selected
        /// </summary>
        /// <param name="p_SegmentControl">Tab control instance</param>
        /// <param name="p_TabIndex">Tab index</param>
        private void OnTabSelected(SegmentedControl p_SegmentControl, int p_TabIndex)
        {
            m_EventFrame_TriggerTab.SetActive(p_TabIndex == 0);
            m_EventFrame_ConditionsTab.SetActive(p_TabIndex == 1);
            m_EventFrame_ActionsTab.SetActive(p_TabIndex == 2);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous condition page
        /// </summary>
        private void OnConditionPageUpPressed()
        {
            /// Underflow check
            if (m_ConditionListPage < 2)
                return;

            /// Decrement current page
            m_ConditionListPage--;

            /// Rebuild list
            RebuildConditionList(null);
        }
        /// <summary>
        /// Rebuilt condition list
        /// </summary>
        /// <param name="p_KeepFocus">To focus</param>
        private void RebuildConditionList(Interfaces.IConditionBase p_ConditionToFocus)
        {
            if (!UICreated)
                return;

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(m_CurrentEvent.Conditions.Count) / (float)(s_CONDITION_ACTION_PER_PAGE)));

            if (p_ConditionToFocus != null)
            {
                var l_Index = m_CurrentEvent.Conditions.IndexOf(p_ConditionToFocus);
                if (l_Index != -1)
                    m_ConditionListPage = (l_Index / s_CONDITION_ACTION_PER_PAGE) + 1;
                else
                    OnConditionSelected(null, -1);
            }

            /// Update overflow
            m_ConditionListPage = Math.Max(1, Math.Min(m_ConditionListPage, l_PageCount));

            /// Update UI
            m_EventFrame_ConditionsTab_UpButton.interactable   = m_ConditionListPage > 1;
            m_EventFrame_ConditionsTab_DownButton.interactable = m_ConditionListPage < l_PageCount;

            /// Clear old entries
            m_EventFrame_ConditionsTab_List.TableViewInstance.ClearSelection();
            m_EventFrame_ConditionsTab_List.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_ConditionListPage - 1) * s_CONDITION_ACTION_PER_PAGE;
                l_I < m_CurrentEvent.Conditions.Count && l_I < (m_ConditionListPage * s_CONDITION_ACTION_PER_PAGE);
                ++l_I)
            {
                var l_Condition     = m_CurrentEvent.Conditions[l_I];
                var l_Name          = FancyShortTypeName(l_Condition.GetTypeNameShort());
                var l_Description   = l_Condition.Description;

                m_EventFrame_ConditionsTab_List.Data.Add(("<align=\"left\">" + (l_Condition.IsEnabled ? "" : "<alpha=#70><s>") + l_Name, l_Description));

                if (l_Condition == p_ConditionToFocus)
                    l_RelIndexToFocus = m_EventFrame_ConditionsTab_List.Data.Count - 1;
            }

            /// Refresh
            m_EventFrame_ConditionsTab_List.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Conditions.Count == 0)
                OnConditionSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_EventFrame_ConditionsTab_List.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an condition is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void OnConditionSelected(TableView p_List, int p_RelIndex)
        {
            /// Clean up condition specific UI
            if (m_EventFrame_ConditionsTab_ConditionContent.transform.childCount != 0)
                GameObject.DestroyImmediate(m_EventFrame_ConditionsTab_ConditionContent.transform.GetChild(0).gameObject);

            int l_ConditionIndex = ((m_ConditionListPage - 1) * s_CONDITION_ACTION_PER_PAGE) + p_RelIndex;

            if (p_RelIndex < 0 || l_ConditionIndex >= m_CurrentEvent.Conditions.Count)
            {
                m_SelectedCondition = -1;
                return;
            }

            m_SelectedCondition = l_ConditionIndex;

            var l_Condition = m_CurrentEvent.Conditions[l_ConditionIndex];
            l_Condition.BuildUI(m_EventFrame_ConditionsTab_ConditionContent.transform);
        }
        /// <summary>
        /// Go to next condition page
        /// </summary>
        private void OnConditionPageDownPressed()
        {
            /// Increment current page
            m_ConditionListPage++;

            /// Rebuild list
            RebuildConditionList(null);
        }
        /// <summary>
        /// Move condition down
        /// </summary>
        [UIAction("click-condition-movedown-btn-pressed")]
        private void OnConditionMoveDownPressed()
        {
            if (m_SelectedCondition == -1)
                return;

            var l_Condition = m_CurrentEvent.Conditions[m_SelectedCondition];
            m_SelectedCondition = m_CurrentEvent.MoveCondition(l_Condition, false);

            RebuildConditionList(l_Condition);
        }
        /// <summary>
        /// Move condition up
        /// </summary>
        [UIAction("click-condition-moveup-btn-pressed")]
        private void OnConditionMoveUpPressed()
        {
            if (m_SelectedCondition == -1)
                return;

            var l_Condition = m_CurrentEvent.Conditions[m_SelectedCondition];
            m_SelectedCondition = m_CurrentEvent.MoveCondition(l_Condition, true);

            RebuildConditionList(l_Condition);
        }
        /// <summary>
        /// Toggle condition button
        /// </summary>
        [UIAction("click-condition-toggle-btn-pressed")]
        private void OnConditionToggleButton()
        {
            if (m_SelectedCondition == -1)
            {
                ShowMessageModal("Please select an condition first!");
                return;
            }

            var l_Condition = m_CurrentEvent.Conditions[m_SelectedCondition];
            if (l_Condition.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable condition\n\"{l_Condition.GetTypeNameShort()}\"?", () =>
                {
                    l_Condition.IsEnabled = false;
                    RebuildConditionList(null);
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable condition\n\"{l_Condition.GetTypeNameShort()}\"?", () =>
                {
                    l_Condition.IsEnabled = true;
                    RebuildConditionList(null);
                });
            }
        }
        /// <summary>
        /// On delete condition button
        /// </summary>
        [UIAction("click-condition-delete-btn-pressed")]
        private void OnConditionDeleteButton()
        {
            if (m_SelectedCondition == -1)
            {
                ShowMessageModal("Please select an condition first!");
                return;
            }

            var l_Condition = m_CurrentEvent.Conditions[m_SelectedCondition];
            ShowConfirmationModal($"<color=red>Do you want to delete condition</color>\n\"{l_Condition.GetTypeNameShort()}\"?", () =>
            {
                OnConditionSelected(null, -1);
                m_CurrentEvent.DeleteCondition(l_Condition);
                RebuildConditionList(null);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous action page
        /// </summary>
        private void OnActionPageUpPressed()
        {
            /// Underflow check
            if (m_ActionListPage < 2)
                return;

            /// Decrement current page
            m_ActionListPage--;

            /// Rebuild list
            RebuildActionList(null);
        }
        /// <summary>
        /// Rebuilt action list
        /// </summary>
        /// <param name="p_KeepFocus">Should keep actual focus</param>
        private void RebuildActionList(Interfaces.IActionBase p_ActionToFocus)
        {
            if (!UICreated)
                return;

            /// Update page count
            var l_PageCount  = Math.Max(1, Mathf.CeilToInt((float)(m_CurrentEvent.Actions.Count) / (float)(s_CONDITION_ACTION_PER_PAGE)));

            if (p_ActionToFocus != null)
            {
                var l_Index = m_CurrentEvent.Actions.IndexOf(p_ActionToFocus);
                if (l_Index != -1)
                    m_ActionListPage = (l_Index / s_CONDITION_ACTION_PER_PAGE) + 1;
                else
                    OnActionSelected(null, -1);
            }

            /// Update overflow
            m_ActionListPage = Math.Max(1, Math.Min(m_ActionListPage, l_PageCount));

            /// Update UI
            m_EventFrame_ActionsTab_UpButton.interactable   = m_ActionListPage > 1;
            m_EventFrame_ActionsTab_DownButton.interactable = m_ActionListPage < l_PageCount;

            /// Clear old entries
            m_EventFrame_ActionsTab_List.TableViewInstance.ClearSelection();
            m_EventFrame_ActionsTab_List.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_ActionListPage - 1) * s_CONDITION_ACTION_PER_PAGE;
                l_I < m_CurrentEvent.Actions.Count && l_I < (m_ActionListPage * s_CONDITION_ACTION_PER_PAGE);
                ++l_I)
            {
                var l_Action        = m_CurrentEvent.Actions[l_I];
                var l_Name          = FancyShortTypeName(l_Action.GetTypeNameShort());
                var l_Description   = l_Action.Description;

                m_EventFrame_ActionsTab_List.Data.Add(("<align=\"left\">" + (l_Action.IsEnabled ? "" : "<alpha=#70><s>") + l_Name, l_Description));

                if (l_Action == p_ActionToFocus)
                    l_RelIndexToFocus = m_EventFrame_ActionsTab_List.Data.Count - 1;
            }

            /// Refresh
            m_EventFrame_ActionsTab_List.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Actions.Count == 0)
                OnActionSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_EventFrame_ActionsTab_List.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an action is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void OnActionSelected(TableView p_List, int p_RelIndex)
        {
            /// Clean up action specific UI
            if (m_EventFrame_ActionsTab_ActionContent.transform.childCount != 0)
                GameObject.DestroyImmediate(m_EventFrame_ActionsTab_ActionContent.transform.GetChild(0).gameObject);

            int l_ActionIndex = ((m_ActionListPage - 1) * s_CONDITION_ACTION_PER_PAGE) + p_RelIndex;

            if (p_RelIndex < 0 || p_RelIndex >= m_CurrentEvent.Actions.Count)
            {
                m_SelectedAction = -1;
                return;
            }

            m_SelectedAction = l_ActionIndex;

            var l_Action = m_CurrentEvent.Actions[l_ActionIndex];
            l_Action.BuildUI(m_EventFrame_ActionsTab_ActionContent.transform);
        }
        /// <summary>
        /// Go to next action page
        /// </summary>
        private void OnActionPageDownPressed()
        {
            /// Increment current page
            m_ActionListPage++;

            /// Rebuild list
            RebuildActionList(null);
        }
        /// <summary>
        /// Move action down
        /// </summary>
        [UIAction("click-action-movedown-btn-pressed")]
        private void OnActionMoveDownPressed()
        {
            if (m_SelectedAction == -1)
                return;

            var l_Action        = m_CurrentEvent.Actions[m_SelectedAction];
            m_SelectedAction    = m_CurrentEvent.MoveAction(l_Action, false);

            RebuildActionList(l_Action);
        }
        /// <summary>
        /// Move action up
        /// </summary>
        [UIAction("click-action-moveup-btn-pressed")]
        private void OnActionMoveUpPressed()
        {
            if (m_SelectedAction == -1)
                return;

            var l_Action        = m_CurrentEvent.Actions[m_SelectedAction];
            m_SelectedAction    = m_CurrentEvent.MoveAction(l_Action, true);

            RebuildActionList(l_Action);
        }
        /// <summary>
        /// Toggle action button
        /// </summary>
        [UIAction("click-action-toggle-btn-pressed")]
        private void OnActionToggleButton()
        {
            if (m_SelectedAction == -1)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_Action = m_CurrentEvent.Actions[m_SelectedAction];
            if (l_Action.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable action\n\"{l_Action.GetTypeNameShort()}\"?", () =>
                {
                    l_Action.IsEnabled = false;
                    RebuildActionList(l_Action);
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable action\n\"{l_Action.GetTypeNameShort()}\"?", () =>
                {
                    l_Action.IsEnabled = true;
                    RebuildActionList(l_Action);
                });
            }
        }
        /// <summary>
        /// On delete action button
        /// </summary>
        [UIAction("click-action-delete-btn-pressed")]
        private void OnActionDeleteButton()
        {
            if (m_SelectedAction == -1)
            {
                ShowMessageModal("Please select an action first!");
                return;
            }

            var l_Action = m_CurrentEvent.Actions[m_SelectedAction];
            ShowConfirmationModal($"<color=red>Do you want to delete action</color>\n\"{l_Action.GetTypeNameShort()}\"?", () =>
            {
                OnActionSelected(null, -1);
                m_CurrentEvent.DeleteAction(l_Action);
                RebuildActionList(null);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show input keyboard
        /// </summary>
        /// <param name="p_Value">Start value</param>
        /// <param name="p_Callback">On enter callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public void UIShowInputKeyboard(string p_Value, Action<string> p_Callback, List<(string, Action)> p_CustomKeys = null)
        {
            m_InputKeyboardValue = p_Value;

            /// Clear old keys
            if (m_InputKeyboardInitialKeyCount == -1)
                m_InputKeyboardInitialKeyCount = m_InputKeyboard.keyboard.keys.Count;

            while (m_InputKeyboard.keyboard.keys.Count > m_InputKeyboardInitialKeyCount)
            {
                var l_Key = m_InputKeyboard.keyboard.keys.ElementAt(m_InputKeyboard.keyboard.keys.Count - 1);
                m_InputKeyboard.keyboard.Clear(l_Key);
                m_InputKeyboard.keyboard.keys.RemoveAt(m_InputKeyboard.keyboard.keys.Count - 1);

                GameObject.Destroy(l_Key.mybutton.gameObject);
            }

            /// Add custom keys
            if (p_CustomKeys != null && p_CustomKeys.Count != 0)
            {
                var l_FirstButton   = m_InputKeyboard.keyboard.BaseButton.GetComponentInChildren<TextMeshProUGUI>();
                var l_Color         = new Color(0.92f, 0.64f, 0);
                var l_ButtonY       = 11f;
                var l_Margin        = 1f;
                var l_TotalLeft     = -35.0f;

                var l_I = 0;
                foreach (var l_Var in p_CustomKeys)
                {
                    var l_Position  = new Vector2(l_TotalLeft, l_ButtonY);
                    var l_Width     = l_FirstButton.GetPreferredValues(l_Var.Item1).x * 2.0f;
                    var l_Key       = new KEYBOARD.KEY(m_InputKeyboard.keyboard, l_Position, l_Var.Item1, l_Width, 10f, l_Color);

                    l_TotalLeft     += ((l_Width / 2.0f) + l_Margin);
                    l_Key.keyaction += (_) => l_Var.Item2.Invoke();

                    m_InputKeyboard.keyboard.keys.Add(l_Key);
                    ++l_I;
                }
            }

            /// Show keyboard
            m_InputKeyboardCallback = p_Callback;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardText.enableAutoSizing = true;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.enableAutoSizing = true;
            m_InputKeyboard.modalView.Show(true);
        }
        /// <summary>
        /// Append value to current keyboard input
        /// </summary>
        /// <param name="p_Value">Value to append</param>
        public void UIInputKeyboardAppend(string p_Value)
        {
            m_InputKeyboard.keyboard.KeyboardText.text += p_Value;
        }
        /// <summary>
        /// On input keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("InputKeyboardEnterPressed")]
        private void InputKeyboardEnterPressed(string p_Text)
        {
            m_InputKeyboardCallback?.Invoke(p_Text);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show loading modal
        /// </summary>
        public void UIShowLoading()
        {
            ShowLoadingModal();
        }
        /// <summary>
        /// Hide loading modal
        /// </summary>
        public void UIHideLoading()
        {
            HideLoadingModal();
        }
        /// <summary>
        /// Set pending message
        /// </summary>
        /// <param name="p_Message">Message</param>
        public void UISetPendingMessage(string p_Message)
        {
            SetMessageModal_PendingMessage(p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private string FancyShortTypeName(string p_Input)
        {
            return p_Input.Replace("_", "::");
        }
    }
}
