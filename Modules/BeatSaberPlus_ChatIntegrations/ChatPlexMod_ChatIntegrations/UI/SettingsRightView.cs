using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.UI
{
    /// <summary>
    /// Settings right view controller
    /// </summary>
    internal sealed class SettingsRightView : CP_SDK.UI.ViewController<SettingsRightView>
    {
        private XUIDropdown         m_Filter = null;
        private XUIVVList           m_Events = null;

        private Modals.EventCreateModal    m_EventCreateModal      = null;
        private Modals.EventImportModal    m_EventImportModal      = null;
        private Modals.EventTemplateModal  m_EventTemplateModal    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<Data.EventListItem> m_FilteredList      = new List<Data.EventListItem>();
        private Data.EventListItem       m_SelectedListItem  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayout(
                XUIHLayout.Make(
                    XUIText.Make("Filter"),
                    XUIDropdown.Make()
                        .SetOptions(new List<string>() { "All" }.Union(ChatIntegrations.RegisteredEventTypes).ToList())
                        .OnValueChanged((_, x) => OnFilterChanged(x))
                        .Bind(ref m_Filter)
                )
                .OnReady(x => x.CSizeFitter.enabled = false)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true),

                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<CP_SDK.UI.Data.TextListCell>.Get())
                        .OnListItemSelected(OnEventSelected)
                        .Bind(ref m_Events)
                )
                .SetHeight(50)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                XUIHLayout.Make("ExpandedButtonsLine",
                    XUIPrimaryButton.Make("New")        .OnClick(OnNewButton),
                    XUIPrimaryButton.Make("Rename")     .OnClick(OnRenameButton),
                    XUIPrimaryButton.Make("Toggle")     .OnClick(OnToggleButton),
                    XUIPrimaryButton.Make("Delete")     .OnClick(OnDeleteButton)
                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .ForEachDirect<XUIPrimaryButton>(y => y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained)),

                XUIHLayout.Make("ExpandedButtonsLine",
                    XUISecondaryButton.Make("Export")   .OnClick(OnExportButton),
                    XUISecondaryButton.Make("Import")   .OnClick(OnImportButton),
                    XUISecondaryButton.Make("Clone")    .OnClick(OnCloneButton),
                    XUISecondaryButton.Make("Templates").OnClick(OnTemplatesButton),
                    XUISecondaryButton.Make("Convert")  .OnClick(OnConvertButton)
                )
                .SetPadding(0)
                .OnReady(x => x.CSizeFitter.enabled = false)
                .ForEachDirect<XUISecondaryButton>(y => y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained))
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            m_EventCreateModal   = CreateModal<Modals.EventCreateModal>();
            m_EventImportModal   = CreateModal<Modals.EventImportModal>();
            m_EventTemplateModal = CreateModal<Modals.EventTemplateModal>();

            m_Filter.SetValue("All");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On filter changed
        /// </summary>
        /// <param name="p_Value">New value</param>
        private void OnFilterChanged(string p_Value)
        {
            m_FilteredList.Clear();
            var l_Events        = ChatIntegrations.Instance.Events;
            var l_EventCount    = l_Events.Count;
            for (var l_I = 0; l_I < l_EventCount; ++l_I)
            {
                var l_Event = l_Events[l_I];
                if ((p_Value == null || p_Value == "All") || l_Event.GetTypeName() == p_Value)
                    m_FilteredList.Add(new Data.EventListItem(l_Event));
            }
            m_FilteredList.Sort((x, y) => (x.Event.GetTypeName() + x.Event.GenericModel.Name).CompareTo((y.Event.GetTypeName() + y.Event.GenericModel.Name)));

            m_Events.SetListItems(m_FilteredList);
        }
        /// <summary>
        /// On event selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnEventSelected(CP_SDK.UI.Data.IListItem p_SelectedItem)
        {
            m_SelectedListItem = p_SelectedItem as Data.EventListItem;
            if (m_SelectedListItem == null)
            {
                SettingsMainView.Instance?.SelectEvent(null);
                return;
            }

            SettingsMainView.Instance?.SelectEvent(m_SelectedListItem.Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New event button
        /// </summary>
        private void OnNewButton()
        {
            ShowModal(m_EventCreateModal);
            m_EventCreateModal.Init((p_CreatedEvent) =>
            {
                if (p_CreatedEvent == null)
                    return;

                if (m_Filter.Element.GetValue() == "All" || m_Filter.Element.GetValue() == p_CreatedEvent.GetTypeName())
                {
                    var l_NewItem = new Data.EventListItem(p_CreatedEvent);
                    m_FilteredList.Add(l_NewItem);
                    m_Events.AddListItem(l_NewItem);
                    m_Events.SetSelectedListItem(l_NewItem);
                }
                else
                    m_Filter.SetValue(p_CreatedEvent.GetTypeName());
            });
        }
        /// <summary>
        /// Rename event button
        /// </summary>
        private void OnRenameButton()
        {
            if (!EnsureEventSelected())
                return;

            ShowKeyboardModal(m_SelectedListItem.Event.GenericModel.Name, (x) =>
            {
                m_SelectedListItem.Event.GenericModel.Name = x;
                m_SelectedListItem.Refresh();
            });
        }
        /// <summary>
        /// Toggle enable/disable on a event
        /// </summary>
        private void OnToggleButton()
        {
            if (!EnsureEventSelected())
                return;

            if (m_SelectedListItem.Event.IsEnabled)
            {
                ShowConfirmationModal($"Do you want to disable event\n\"{m_SelectedListItem.Event.GenericModel.Name}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    ChatIntegrations.Instance.ToggleEvent(m_SelectedListItem.Event);
                    m_SelectedListItem.Refresh();
                });
            }
            else
            {
                ShowConfirmationModal($"Do you want to enable event\n\"{m_SelectedListItem.Event.GenericModel.Name}\"?", (p_Confirm) =>
                {
                    if (!p_Confirm)
                        return;

                    ChatIntegrations.Instance.ToggleEvent(m_SelectedListItem.Event);
                    m_SelectedListItem.Refresh();
                });
            }
        }
        /// <summary>
        /// Delete event button
        /// </summary>
        private void OnDeleteButton()
        {
            if (!EnsureEventSelected())
                return;

            ShowConfirmationModal($"<color=red>Do you want to delete event</color>\n\"{m_SelectedListItem.Event.GenericModel.Name}\"?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                ChatIntegrations.Instance.DeleteEvent(m_SelectedListItem.Event);

                m_FilteredList.Remove(m_SelectedListItem);
                m_Events.RemoveListItem(m_SelectedListItem);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Export an event
        /// </summary>
        private void OnExportButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event         = m_SelectedListItem.Event;
            var l_Serialized    = l_Event.Serialize();

            var l_EventName = l_Event.GenericModel.Name;
            if (l_EventName.Length > 20)
                l_EventName = l_EventName.Substring(0, 20);

            var l_FileName = CP_SDK.Misc.Time.UnixTimeNow() + "_" + l_Event.GetTypeName() + "_" + l_EventName + ".bspci";
            l_FileName = string.Concat(l_FileName.Split(System.IO.Path.GetInvalidFileNameChars()));

            System.IO.File.WriteAllText(ChatIntegrations.s_EXPORT_PATH + l_FileName, l_Serialized.ToString(Newtonsoft.Json.Formatting.Indented), System.Text.Encoding.Unicode);

            ShowMessageModal("Event exported in\n" + ChatIntegrations.s_EXPORT_PATH);
        }
        /// <summary>
        /// Import an event
        /// </summary>
        private void OnImportButton()
        {
            ShowModal(m_EventImportModal);
            m_EventImportModal.Init((System.Action<Interfaces.IEventBase>)((p_ImportedEvent) =>
            {
                if (p_ImportedEvent == null)
                    return;

                if (m_Filter.Element.GetValue() == "All" || m_Filter.Element.GetValue() == p_ImportedEvent.GetTypeName())
                {
                    var l_NewItem = new Data.EventListItem(p_ImportedEvent);
                    m_FilteredList.Add(l_NewItem);
                    m_Events.AddListItem(l_NewItem);
                    m_Events.SetSelectedListItem(l_NewItem);
                }
                else
                    m_Filter.SetValue(p_ImportedEvent.GetTypeName());
            }));
        }
        /// <summary>
        /// Clone an event
        /// </summary>
        private void OnCloneButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event         = m_SelectedListItem.Event;
            var l_Serialized    = l_Event.Serialize();
            var l_NewEvent      = ChatIntegrations.Instance.AddEventFromSerialized(l_Serialized, false, true, out var _);

            if (l_NewEvent == null)
                ShowMessageModal("Clone failed, check logs!");
            else
            {
                var l_NewItem = new Data.EventListItem(l_NewEvent);
                m_FilteredList.Add(l_NewItem);
                m_Events.AddListItem(l_NewItem);
                m_Events.SetSelectedListItem(l_NewItem);
            }
        }
        /// <summary>
        /// Template event
        /// </summary>
        private void OnTemplatesButton()
        {
            ShowModal(m_EventTemplateModal);
            m_EventTemplateModal.Init((p_CreatedEvent) =>
            {
                if (p_CreatedEvent == null)
                    return;

                if (m_Filter.Element.GetValue() == "All" || m_Filter.Element.GetValue() == p_CreatedEvent.GetTypeName())
                {
                    var l_NewItem = new Data.EventListItem(p_CreatedEvent);
                    m_FilteredList.Add(l_NewItem);
                    m_Events.AddListItem(l_NewItem);
                    m_Events.SetSelectedListItem(l_NewItem);
                }
                else
                    m_Filter.SetValue(p_CreatedEvent.GetTypeName());
            });
        }
        /// <summary>
        /// Convert an event
        /// </summary>
        private void OnConvertButton()
        {
            if (!EnsureEventSelected())
                return;

            var l_Event = m_SelectedListItem.Event;
            if (l_Event is Events.Dummy)
            {
                ShowMessageModal("This event is already a dummy event!");
                return;
            }

            ShowConfirmationModal($"<color=yellow>Do you want to convert event</color>\n\"{l_Event.GenericModel.Name}\" <color=yellow>to Dummy?", (p_Confirm) =>
            {
                if (!p_Confirm)
                    return;

                var l_Serialized = l_Event.Serialize();
                l_Serialized["Type"] = string.Join(".", typeof(Events.Dummy).Namespace, typeof(Events.Dummy).Name);
                l_Serialized["Event"]["Type"]  = string.Join(".", typeof(Events.Dummy).Namespace, typeof(Events.Dummy).Name);
                l_Serialized["Event"]["Name"] += " (Converted)";

                var l_NewEvent = ChatIntegrations.Instance.AddEventFromSerialized(l_Serialized, false, true, out var _);
                if (l_NewEvent == null)
                    ShowMessageModal("Clone failed, check logs!");
                else
                {
                    if (m_Filter.Element.GetValue() == "All" || m_Filter.Element.GetValue() == l_NewEvent.GetTypeName())
                    {
                        var l_NewItem = new Data.EventListItem(l_NewEvent);
                        m_FilteredList.Add(l_NewItem);
                        m_Events.AddListItem(l_NewItem);
                        m_Events.SetSelectedListItem(l_NewItem);
                    }
                    else
                        m_Filter.SetValue(l_NewEvent.GetTypeName());
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ensure that an event is selected
        /// </summary>
        /// <returns></returns>
        private bool EnsureEventSelected()
        {
            if (m_SelectedListItem == null)
            {
                ShowMessageModal("Please select an event first!");
                return false;
            }

            return true;
        }
    }
}
