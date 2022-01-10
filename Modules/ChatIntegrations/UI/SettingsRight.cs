using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.ChatIntegrations.UI
{
    /// <summary>
    /// Settings right event list view
    /// </summary>
    internal class SettingsRight : SDK.UI.ResourceViewController<SettingsRight>
    {
        /// <summary>
        /// Event line per page
        /// </summary>
        private static int EVENT_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sub view enum
        /// </summary>
        private enum SubView
        {
            Main,
            AddEvent,
            ImportEvent,
            TemplateEvent
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("FilterFrame")]
        private GameObject m_FilterFrame = null;
        [UIComponent("FilterFrame_DropDown")]
        private DropDownListSetting m_FilterFrame_DropDown;
        [UIValue("FilterFrame_DropDownOptions")]
        private List<object> m_FilterFrame_DropDownOptions = new List<object>() { "All" };

        [UIObject("EventListFrame_Background")]
        private GameObject m_EventListFrame = null;
        [UIObject("EventListFrame_Background")]
        private GameObject m_EventListFrame_Background = null;
        [UIObject("EventsList")]
        private GameObject m_EventsListView = null;
        private SDK.UI.DataSource.SimpleTextList m_EventsList = null;

        [UIComponent("EventsUpButton")]
        private Button m_EventsUpButton = null;
        [UIComponent("EventsDownButton")]
        private Button m_EventsDownButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("EventListButtonsFrame")]
        private GameObject m_EventListButtonsFrame = null;
        [UIObject("EventListButtonsFrame2")]
        private GameObject m_EventListButtonsFrame2 = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("AddEventFrame")]
        private GameObject m_AddEventFrame = null;
        [UIObject("AddEventFrame_Background")]
        private GameObject m_AddEventFrame_Background = null;
        [UIComponent("AddEventFrame_DropDown")]
        private DropDownListSetting m_AddEventFrame_DropDown;
        [UIValue("AddEventFrame_DropDownOptions")]
        private List<object> m_AddEventFrame_DropDownOptions = new List<object>() { "Loading...", };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("ImportEventFrame")]
        private GameObject m_ImportEventFrame = null;
        [UIObject("ImportEventFrame_Background")]
        private GameObject m_ImportEventFrame_Background = null;
        [UIComponent("ImportEventFrame_DropDown")]
        private DropDownListSetting m_ImportEventFrame_DropDown;
        [UIValue("ImportEventFrame_DropDownOptions")]
        private List<object> m_ImportEventFrame_DropDownOptions = new List<object>() { "Loading...", };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("TemplateEventFrame")]
        private GameObject m_TemplateEventFrame = null;
        [UIObject("TemplateEventFrame_Background")]
        private GameObject m_TemplateEventFrame_Background = null;
        [UIComponent("TemplateEventFrame_DropDown")]
        private DropDownListSetting m_TemplateEventFrame_DropDown;
        [UIValue("TemplateEventFrame_DropDownOptions")]
        private List<object> m_TemplateEventFrame_DropDownOptions = new List<object>() { "Loading...", };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("NewEventNameKeyboard")]
        private ModalKeyboard m_NewEventNameKeyboard = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("RenameKeyboard")]
        private ModalKeyboard m_RenameKeyboard = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current filter
        /// </summary>
        private string m_CurrentFilter = null;
        /// <summary>
        /// Filtered list
        /// </summary>
        private List<Interfaces.IEventBase> m_FilteredList = new List<Interfaces.IEventBase>();
        /// <summary>
        /// Current event list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Selected index
        /// </summary>
        private int m_SelectedIndex = -1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Update background color
            SDK.UI.Backgroundable.SetOpacity(m_EventListFrame_Background, 0.5f);
            SDK.UI.Backgroundable.SetOpacity(m_AddEventFrame_Background, 0.75f);
            SDK.UI.Backgroundable.SetOpacity(m_ImportEventFrame_Background, 0.75f);
            SDK.UI.Backgroundable.SetOpacity(m_TemplateEventFrame_Background, 0.75f);
            SDK.UI.ModalView.SetOpacity(m_NewEventNameKeyboard.modalView, 0.75f);
            SDK.UI.ModalView.SetOpacity(m_RenameKeyboard.modalView, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnFilterChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            /// Setup filters
            SDK.UI.DropDownListSetting.Setup(m_FilterFrame_DropDown, l_Event, true, 1f);

            /// Scale down up & down button
            m_EventsUpButton.transform.localScale = Vector3.one * 0.5f;
            m_EventsDownButton.transform.localScale = Vector3.one * 0.5f;

            /// Prepare event list
            var l_BSMLTableView = m_EventsListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_EventsListView.GetComponentInChildren<CustomListTableData>());
            m_EventsList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_EventsList.TableViewInstance  = l_BSMLTableView;
            m_EventsList.CellSizeValue      = 4.8f;
            l_BSMLTableView.didSelectCellWithIdxEvent += OnEventSelected;
            l_BSMLTableView.SetDataSource(m_EventsList, false);

            /// Bind events
            m_EventsUpButton.onClick.AddListener(OnPageUpPressed);
            m_EventsDownButton.onClick.AddListener(OnPageDownPressed);

            /// Remove new event type selector label
            SDK.UI.DropDownListSetting.Setup(m_AddEventFrame_DropDown, null, true, 0.95f);

            /// Remove import event type selector label
            SDK.UI.DropDownListSetting.Setup(m_ImportEventFrame_DropDown, null, true, 0.95f);

            /// Remove template event type selector label
            SDK.UI.DropDownListSetting.Setup(m_TemplateEventFrame_DropDown, null, true, 0.95f);

            OnFilterChanged(null);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            var l_Filters   = new List<object>() { "All" };
            var l_Types     = new List<object>();

            foreach (var l_CurrentType in ChatIntegrations.RegisteredEventTypes)
            {
                l_Filters.Add(l_CurrentType.GetType().Name);
                l_Types.Add(l_CurrentType.GetType().Name);
            }

            m_FilterFrame_DropDownOptions = l_Filters;
            m_FilterFrame_DropDown.values = l_Filters;
            m_FilterFrame_DropDown.UpdateChoices();

            m_AddEventFrame_DropDownOptions = l_Types;
            m_AddEventFrame_DropDown.values = l_Types;
            m_AddEventFrame_DropDown.UpdateChoices();

            SwitchSubView(SubView.Main);

            RebuildList(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On filter changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnFilterChanged(object p_Value)
        {
            m_CurrentFilter = (string)m_FilterFrame_DropDown.Value;

            m_FilteredList.Clear();
            foreach (var l_Event in ChatIntegrations.Instance.Events)
            {
                if ((m_CurrentFilter == null || m_CurrentFilter == "All") || m_CurrentFilter == l_Event.GetTypeNameShort())
                    m_FilteredList.Add(l_Event);
            }

            m_CurrentPage = 1;

            RebuildList(null);
        }
        /// <summary>
        /// Go to previous event page
        /// </summary>
        private void OnPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Rebuild list
            RebuildList(null);
        }
        /// <summary>
        /// Rebuild list
        /// </summary>
        /// <param name="p_EventToSelect">Event to auto select</param>
        private void RebuildList(Interfaces.IEventBase p_EventToSelect)
        {
            if (!UICreated)
                return;

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(m_FilteredList.Count) / (float)(EVENT_PER_PAGE)));

            if (p_EventToSelect != null)
            {
                var l_Index = m_FilteredList.IndexOf(p_EventToSelect);
                if (l_Index != -1)
                    m_CurrentPage = (l_Index / EVENT_PER_PAGE) + 1;
                else
                    OnEventSelected(null, -1);
            }

            /// Update overflow
            m_CurrentPage = Math.Max(1, Math.Min(m_CurrentPage, l_PageCount));

            /// Update UI
            m_EventsUpButton.interactable   = m_CurrentPage > 1;
            m_EventsDownButton.interactable = m_CurrentPage < l_PageCount;

            /// Clear old entries
            m_EventsList.TableViewInstance.ClearSelection();
            m_EventsList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_CurrentPage - 1) * EVENT_PER_PAGE;
                l_I < m_FilteredList.Count && l_I < (m_CurrentPage * EVENT_PER_PAGE);
                ++l_I)
            {
                var l_Event = m_FilteredList[l_I];

                m_EventsList.Data.Add(BuildLineString(l_Event));

                if (l_Event == p_EventToSelect)
                    l_RelIndexToFocus = m_EventsList.Data.Count - 1;
            }

            /// Refresh
            m_EventsList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_FilteredList.Count == 0)
                OnEventSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_EventsList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// On event selected
        /// </summary>
        /// <param name="p_TableView">TableView instance</param>
        /// <param name="p_RelIndex">Relative index</param>
        private void OnEventSelected(HMUI.TableView p_TableView, int p_RelIndex)
        {
            int l_EventIndex = ((m_CurrentPage - 1) * EVENT_PER_PAGE) + p_RelIndex;

            if (p_RelIndex < 0 || l_EventIndex >= m_FilteredList.Count)
            {
                m_SelectedIndex = -1;
                Settings.Instance.SelectEvent(null);
                return;
            }

            m_SelectedIndex = l_EventIndex;

            Settings.Instance.SelectEvent(m_FilteredList[m_SelectedIndex]);
        }
        /// <summary>
        /// Go to next event page
        /// </summary>
        private void OnPageDownPressed()
        {
            /// Increment current page
            m_CurrentPage++;

            /// Rebuild list
            RebuildList(null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New event button
        /// </summary>
        [UIAction("click-new-btn-pressed")]
        private void OnNewButton()
        {
            SwitchSubView(SubView.AddEvent);
        }
        /// <summary>
        /// New event cancel button
        /// </summary>
        [UIAction("click-cancel-add-event-btn-pressed")]
        private void OnAddEventCancelButton()
        {
            SwitchSubView(SubView.Main);
        }
        /// <summary>
        /// New event create button
        /// </summary>
        [UIAction("click-add-event-btn-pressed")]
        private void OnAddEventCreateButton()
        {
            SwitchSubView(SubView.Main);
            ShowModal("OpenNewEventNameModal");
        }
        /// <summary>
        /// On new event name keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("NewEventNameKeyboardPressed")]
        internal void NewEventNameKeyboardPressed(string p_Text)
        {
            var l_TypeName      = (string)m_AddEventFrame_DropDown.Value;
            var l_EventName     = p_Text;
            var l_MatchingType  = ChatIntegrations.RegisteredEventTypes.Where(x => x.GetType().Name == l_TypeName).FirstOrDefault();

            if (l_MatchingType != null)
            {
                /// Create instance
                var l_NewEvent = Activator.CreateInstance(l_MatchingType.GetType()) as Interfaces.IEventBase;
                l_NewEvent.GenericModel.Name = l_EventName;

                ChatIntegrations.Instance.AddEvent(l_NewEvent);

                if (m_FilterFrame_DropDown.Value != null && (string)m_FilterFrame_DropDown.Value != "All" && (string)m_FilterFrame_DropDown.Value != l_NewEvent.GetTypeNameShort())
                    m_FilterFrame_DropDown.Value = (object)l_NewEvent.GetTypeNameShort();

                OnFilterChanged(null);
                RebuildList(l_NewEvent);
            }
        }
        /// <summary>
        /// Rename event button
        /// </summary>
        [UIAction("click-rename-btn-pressed")]
        private void OnRenameButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event = m_FilteredList[m_SelectedIndex];
            m_RenameKeyboard.SetText(l_Event.GenericModel.Name);
            ShowModal("OpenRenameModal");
        }
        /// <summary>
        /// On rename keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("RenameKeyboardPressed")]
        internal void RenameKeyboardPressed(string p_Text)
        {
            var l_Event = m_FilteredList[m_SelectedIndex];
            l_Event.GenericModel.Name = string.IsNullOrEmpty(p_Text) ? "No name..." : p_Text;

            OnFilterChanged(null);
            RebuildList(l_Event);
        }
        /// <summary>
        /// Delete event button
        /// </summary>
        [UIAction("click-delete-btn-pressed")]
        private void OnDeleteButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event = m_FilteredList[m_SelectedIndex];
            ShowConfirmationModal($"<color=red>Do you want to delete event</color>\n\"{l_Event.GenericModel.Name}\"?", () =>
            {
                OnEventSelected(null, -1);
                ChatIntegrations.Instance.DeleteEvent(l_Event);
                OnFilterChanged(null);
                RebuildList(null);
            });
        }
        /// <summary>
        /// Toggle enable/disable on a event
        /// </summary>
        [UIAction("click-toggle-btn-pressed")]
        private void OnToggleButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event = m_FilteredList[m_SelectedIndex];
            if (l_Event.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable event\n\"{l_Event.GenericModel.Name}\"?", () =>
                {
                    ChatIntegrations.Instance.ToggleEvent(l_Event);
                    OnFilterChanged(null);
                    RebuildList(l_Event);
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable event\n\"{l_Event.GenericModel.Name}\"?", () =>
                {
                    ChatIntegrations.Instance.ToggleEvent(l_Event);
                    OnFilterChanged(null);
                    RebuildList(l_Event);
                });
            }
        }
        /// <summary>
        /// Export an event
        /// </summary>
        [UIAction("click-export-btn-pressed")]
        private void OnExportButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event         = m_FilteredList[m_SelectedIndex];
            var l_Serialized    = l_Event.Serialize();

            var l_EventName = l_Event.GenericModel.Name;
            if (l_EventName.Length > 20)
                l_EventName = l_EventName.Substring(0, 20);

            var l_FileName = SDK.Misc.Time.UnixTimeNow() + "_" + l_Event.GetTypeNameShort() + "_" + l_EventName + ".bspci";
            l_FileName = string.Concat(l_FileName.Split(System.IO.Path.GetInvalidFileNameChars()));

            System.IO.File.WriteAllText(ChatIntegrations.s_EXPORT_PATH + l_FileName, l_Serialized.ToString(Newtonsoft.Json.Formatting.Indented), System.Text.Encoding.Unicode);

            ShowMessageModal("Event exported in\n" + ChatIntegrations.s_EXPORT_PATH);
        }
        /// <summary>
        /// Import an event
        /// </summary>
        [UIAction("click-import-btn-pressed")]
        private void OnImportButton()
        {
            SwitchSubView(SubView.ImportEvent);

            var l_Files = new List<object>();
            foreach (var l_File in System.IO.Directory.GetFiles(ChatIntegrations.s_IMPORT_PATH, "*.bspci"))
                l_Files.Add(System.IO.Path.GetFileNameWithoutExtension(l_File));

            m_ImportEventFrame_DropDownOptions = l_Files;
            m_ImportEventFrame_DropDown.values = l_Files;
            m_ImportEventFrame_DropDown.UpdateChoices();
        }
        /// <summary>
        /// Import event cancel button
        /// </summary>
        [UIAction("click-cancel-import-event-btn-pressed")]
        private void OnImportEventCancelButton()
        {
            SwitchSubView(SubView.Main);
        }
        /// <summary>
        /// Import event button
        /// </summary>
        [UIAction("click-import-event-btn-pressed")]
        private void OnImportEventButton()
        {
            var l_FileName = ChatIntegrations.s_IMPORT_PATH + (string)m_ImportEventFrame_DropDown.Value + ".bspci";

            if (System.IO.File.Exists(l_FileName))
            {
                var l_Raw = System.IO.File.ReadAllText(l_FileName, System.Text.Encoding.Unicode);

                try
                {
                    var l_JObject  = JObject.Parse(l_Raw);
                    var l_NewEvent = ChatIntegrations.Instance.AddEventFromSerialized(l_JObject, true, false, out var l_Error);

                    if (l_NewEvent != null)
                    {
                        SwitchSubView(SubView.Main);

                        if (m_FilterFrame_DropDown.Value != null && (string)m_FilterFrame_DropDown.Value != "All" && (string)m_FilterFrame_DropDown.Value != l_NewEvent.GetTypeNameShort())
                            m_FilterFrame_DropDown.Value = (object)l_NewEvent.GetTypeNameShort();

                        OnFilterChanged(null);
                        RebuildList(l_NewEvent);
                    }
                    else
                        ShowMessageModal(l_Error);
                }
                catch
                {
                    ShowMessageModal("Invalid file!");
                }
            }
            else
                ShowMessageModal("File not found!");
        }
        /// <summary>
        /// Clone an event
        /// </summary>
        [UIAction("click-clone-btn-pressed")]
        private void OnCloneButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event         = m_FilteredList[m_SelectedIndex];
            var l_Serialized    = l_Event.Serialize();
            var l_NewEvent      = ChatIntegrations.Instance.AddEventFromSerialized(l_Serialized, false, true, out var _);

            if (l_NewEvent == null)
                ShowMessageModal("Clone failed, check BeatSaberPlus logs!");
            else
            {
                OnFilterChanged(null);
                RebuildList(l_NewEvent);
            }
        }
        /// <summary>
        /// Template event
        /// </summary>
        [UIAction("click-templates-btn-pressed")]
        private void OnTemplatesButton()
        {
            SwitchSubView(SubView.TemplateEvent);

            var l_Templates = new List<object>();
            foreach (var l_Template in GetTemplates())
                l_Templates.Add(l_Template);

            m_TemplateEventFrame_DropDownOptions = l_Templates;
            m_TemplateEventFrame_DropDown.values = l_Templates;
            m_TemplateEventFrame_DropDown.UpdateChoices();
        }
        /// <summary>
        /// Template event cancel button
        /// </summary>
        [UIAction("click-cancel-template-event-btn-pressed")]
        private void OnTemplateEventCancelButton()
        {
            SwitchSubView(SubView.Main);
        }
        /// <summary>
        /// Template event create button
        /// </summary>
        [UIAction("click-create-template-event-btn-pressed")]
        private void OnCreateTemplateEventButton()
        {
            var l_Template  = (string)m_TemplateEventFrame_DropDown.Value;
            var l_NewEvent  = CreateFromTemplate(l_Template);

            if (l_NewEvent == null)
                ShowMessageModal("Clone failed, check BeatSaberPlus logs!");
            else
            {
                SwitchSubView(SubView.Main);
                ChatIntegrations.Instance.AddEvent(l_NewEvent);
                OnFilterChanged(null);
                RebuildList(l_NewEvent);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get template lists
        /// </summary>
        /// <returns></returns>
        private List<string> GetTemplates()
        {
            return new List<string>()
            {
                "ChatPointReward : 5 Squats",
                "ChatPointReward : Countdown + Emote bomb",
                "ChatBits : Thanks message + emote bomb",
                "ChatSubscription : Thanks message + emote bomb",
                "ChatFollow : Thanks message + emote bomb",
                "ChatCommand : Discord command",
                "ChatCommand : 250% lights for 10 seconds with cooldown"
            }.OrderBy(x => x).ToList();
        }
        /// <summary>
        /// Create from template
        /// </summary>
        /// <param name="p_Template">Template name</param>
        /// <returns></returns>
        private Interfaces.IEventBase CreateFromTemplate(string p_Template)
        {
            if (p_Template == "ChatPointReward : 5 Squats")
            {
                var l_Event = new Events.ChatPointsReward();
                l_Event.Model.Cooldown  = 60;
                l_Event.Model.Cost      = 100;
                l_Event.Model.Name      = "5 Squats (Template)";
                l_Event.Model.Title     = "5 Squats (Template)";

                l_Event.AddCondition(new Conditions.GamePlay_PlayingMap() { Event = l_Event, IsEnabled = true });

                var l_SquatAction = new Actions.GamePlay_SpawnSquatWalls() { Event = l_Event, IsEnabled = true };
                l_SquatAction.Model.Count       = 5;
                l_SquatAction.Model.Interval    = 5;
                l_Event.AddAction(l_SquatAction);

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "5 squats from $SenderName, let's gooo!";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatPointReward : Countdown + Emote bomb")
            {
                var l_Event = new Events.ChatPointsReward();
                l_Event.Model.Cooldown  = 30;
                l_Event.Model.Cost      = 100;
                l_Event.Model.Name      = "Countdown + Emote bomb (Template)";
                l_Event.Model.Title     = "Emote bomb (Template)";

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Explosion in...";
                l_Event.AddAction(l_MessageAction);
                l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "3...";
                l_Event.AddAction(l_MessageAction);

                var l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                l_DelayAction.Model.Delay = 1;
                l_DelayAction.Model.PreventNextActionFailure = false;
                l_Event.AddAction(l_DelayAction);

                l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "2...";
                l_Event.AddAction(l_MessageAction);

                l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                l_DelayAction.Model.Delay = 1;
                l_DelayAction.Model.PreventNextActionFailure = false;
                l_Event.AddAction(l_DelayAction);

                l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "1...";
                l_Event.AddAction(l_MessageAction);

                var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                l_EmoteBombAction.Model.EmoteKindCount = 25;
                l_EmoteBombAction.Model.CountPerEmote = 40;
                l_Event.AddAction(l_EmoteBombAction);

                l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                l_DelayAction.Model.Delay = 1;
                l_DelayAction.Model.PreventNextActionFailure = false;
                l_Event.AddAction(l_DelayAction);

                l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "BOOM!";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatBits : Thanks message + emote bomb")
            {
                var l_Event = new Events.ChatBits();
                l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                var l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = true;
                l_CooldownCondition.Model.NotifyUser    = false;
                l_CooldownCondition.Model.CooldownTime  = 20;
                l_Event.Conditions.Add(l_CooldownCondition);

                var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                l_EmoteBombAction.Model.EmoteKindCount  = 10;
                l_EmoteBombAction.Model.CountPerEmote   = 10;
                l_Event.AddAction(l_EmoteBombAction);

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Thanks $UserName for the $Bits bits!";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatSubscription : Thanks message + emote bomb")
            {
                var l_Event = new Events.ChatSubscription();
                l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                l_EmoteBombAction.Model.EmoteKindCount  = 10;
                l_EmoteBombAction.Model.CountPerEmote   = 10;
                l_Event.AddAction(l_EmoteBombAction);

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Thanks $UserName for the $MonthCount of $SubPlan!";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatFollow : Thanks message + emote bomb")
            {
                var l_Event = new Events.ChatFollow();
                l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                var l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = true;
                l_CooldownCondition.Model.NotifyUser    = false;
                l_CooldownCondition.Model.CooldownTime  = 20 * 60;
                l_Event.Conditions.Add(l_CooldownCondition);

                var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                l_EmoteBombAction.Model.EmoteKindCount  = 5;
                l_EmoteBombAction.Model.CountPerEmote   = 5;
                l_Event.AddAction(l_EmoteBombAction);

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Thanks $UserName for the follow!";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatCommand : Discord command")
            {
                var l_Event = new Events.ChatCommand();
                l_Event.Model.Name      = "Discord command (Template)";
                l_Event.Model.Command   = "!discord";

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "@$UserName join my amazing discord at https://discord.gg/K4X94Ea";
                l_Event.AddAction(l_MessageAction);

                return l_Event;
            }
            else if (p_Template == "ChatCommand : 250% lights for 10 seconds with cooldown")
            {
                var l_Event = new Events.ChatCommand();
                l_Event.Model.Name      = "10 seconds of 250% lights with cooldown (Template)";
                l_Event.Model.Command   = "!lights";

                l_Event.AddCondition(new Conditions.GamePlay_PlayingMap() { Event = l_Event, IsEnabled = true });

                var l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = true;
                l_CooldownCondition.Model.NotifyUser    = true;
                l_CooldownCondition.Model.CooldownTime  = 60;
                l_Event.Conditions.Add(l_CooldownCondition);

                l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                l_CooldownCondition.Model.PerUser       = false;
                l_CooldownCondition.Model.NotifyUser    = true;
                l_CooldownCondition.Model.CooldownTime  = 20;
                l_Event.Conditions.Add(l_CooldownCondition);

                var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                l_MessageAction.Model.BaseValue = "Lights go brrrrr";
                l_Event.AddAction(l_MessageAction);

                var l_LightAction = new Actions.GamePlay_ChangeLightIntensity() { Event = l_Event, IsEnabled = true };
                l_LightAction.Model.UserValue       = 2.5f;
                l_LightAction.Model.SendChatMessage = false;
                l_LightAction.Model.ValueType       = 1;
                l_Event.AddAction(l_LightAction);

                var l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                l_DelayAction.Model.Delay                       = 10;
                l_DelayAction.Model.PreventNextActionFailure    = true;
                l_Event.AddAction(l_DelayAction);

                l_LightAction = new Actions.GamePlay_ChangeLightIntensity() { Event = l_Event, IsEnabled = true };
                l_LightAction.Model.ValueType       = 3;
                l_LightAction.Model.SendChatMessage = false;
                l_Event.AddAction(l_LightAction);

                return l_Event;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Switch to sub view
        /// </summary>
        /// <param name="p_SubView">New subview</param>
        private void SwitchSubView(SubView p_SubView)
        {
            m_FilterFrame.SetActive(p_SubView == SubView.Main);
            m_EventListFrame.SetActive(p_SubView == SubView.Main);
            m_EventListButtonsFrame.SetActive(p_SubView == SubView.Main);
            m_EventListButtonsFrame2.SetActive(p_SubView == SubView.Main);
            m_EventsUpButton.gameObject.SetActive(p_SubView == SubView.Main);
            m_EventsDownButton.gameObject.SetActive(p_SubView == SubView.Main);
            m_AddEventFrame.SetActive(p_SubView == SubView.AddEvent);
            m_ImportEventFrame.SetActive(p_SubView == SubView.ImportEvent);
            m_TemplateEventFrame.SetActive(p_SubView == SubView.TemplateEvent);
        }
        /// <summary>
        /// Ensure that an event is selected
        /// </summary>
        /// <returns></returns>
        private bool EnsureEventSelected()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an event first!");
                return false;
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build pick ban line
        /// </summary>
        /// <param name="p_IsBan">Is a ban</param>
        /// <param name="p_PlayerName">Player name</param>
        /// <param name="p_MapName">Name of the map</param>
        /// <returns>Built pick ban line</returns>
        private (string, string) BuildLineString(Interfaces.IEventBase p_Event)
        {
            /// Result line
            string l_Text = "";

            /// Fake line height
            l_Text += "<line-height=1%><align=\"left\">";

            if (!p_Event.IsEnabled)
                l_Text += "<alpha=#70><s>";
            else
                l_Text += "<b>";

            /// Left part
            l_Text += (p_Event.IsEnabled ? "<color=yellow>" : "") + "[" + p_Event.GetTypeNameShort() + "] " + (p_Event.IsEnabled ? "</color>" : "");
            l_Text += " ";
            l_Text += p_Event.GenericModel.Name;

            if (!p_Event.IsEnabled)
                l_Text += "</s> (Disabled)";
            else
                l_Text += "</b>";

            /// Line break
            l_Text += "\n";
            /// Restore line height
            l_Text += "<line-height=100%>";

            /// Right part
            l_Text += "<align=\"right\">";
            l_Text += "Used " + p_Event.GenericModel.UsageCount + " time(s)";

            return (l_Text, null);
        }
    }
}
