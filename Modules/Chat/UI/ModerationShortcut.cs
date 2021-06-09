using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.Chat.UI
{
    internal class ModerationShortcut : SDK.UI.ResourceViewController<ModerationShortcut>
    {
        /// <summary>
        /// Event line per page
        /// </summary>
        private static int EVENT_PER_PAGE = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIComponent("ShortcutUpButton")]
        private Button m_ShortcutUpButton = null;
        [UIObject("Shortcut_Background")]
        private GameObject m_Shortcut_Background = null;
        [UIObject("ShortcutList")]
        private GameObject m_ShortcutListView = null;
        private SDK.UI.DataSource.SimpleTextList m_ShortcutList = null;
        [UIComponent("ShortcutDownButton")]
        private Button m_ShortcutDownButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("NewKeyboard")]
        private ModalKeyboard m_NewKeyboard = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Filtered list
        /// </summary>
        private List<string> m_FilteredList = new List<string>();
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
            SDK.UI.Backgroundable.SetOpacity(m_Shortcut_Background, 0.5f);
            SDK.UI.ModalView.SetOpacity(m_NewKeyboard.modalView, 0.75f);

            /// Scale down up & down button
            m_ShortcutUpButton.transform.localScale   = Vector3.one * 0.5f;
            m_ShortcutDownButton.transform.localScale = Vector3.one * 0.5f;

            /// Prepare event list
            var l_BSMLTableView = m_ShortcutListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_ShortcutListView.GetComponentInChildren<CustomListTableData>());
            m_ShortcutList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_ShortcutList.TableViewInstance = l_BSMLTableView;
            m_ShortcutList.CellSizeValue = 4.8f;
            l_BSMLTableView.didSelectCellWithIdxEvent += OnShortcutSelected;
            l_BSMLTableView.SetDataSource(m_ShortcutList, false);

            /// Bind events
            m_ShortcutUpButton.onClick.AddListener(OnPageUpPressed);
            m_ShortcutDownButton.onClick.AddListener(OnPageDownPressed);

            /// Build the shortcut list
            m_FilteredList = Config.Chat.ModerationKeys.Split(new string[] { Config.Chat.s_ModerationKeyDefault_Split }, System.StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            /// Rebuild list
            m_CurrentPage = 1;
            RebuildList(null);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            Config.Chat.ModerationKeys = string.Join(Config.Chat.s_ModerationKeyDefault_Split, m_FilteredList);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous shortcut page
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
        /// <param name="p_ShortcutToFocus">Event to auto select</param>
        private void RebuildList(string p_ShortcutToFocus)
        {
            if (!UICreated)
                return;

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(m_FilteredList.Count) / (float)(EVENT_PER_PAGE)));

            if (p_ShortcutToFocus != null)
            {
                var l_Index = m_FilteredList.IndexOf(p_ShortcutToFocus);
                if (l_Index != -1)
                    m_CurrentPage = (l_Index / EVENT_PER_PAGE) + 1;
                else
                    OnShortcutSelected(null, -1);
            }

            /// Update overflow
            m_CurrentPage = Math.Max(1, Math.Min(m_CurrentPage, l_PageCount));

            /// Update UI
            m_ShortcutUpButton.interactable     = m_CurrentPage > 1;
            m_ShortcutDownButton.interactable   = m_CurrentPage < l_PageCount;

            /// Clear old entries
            m_ShortcutList.TableViewInstance.ClearSelection();
            m_ShortcutList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_CurrentPage - 1) * EVENT_PER_PAGE;
                l_I < m_FilteredList.Count && l_I < (m_CurrentPage * EVENT_PER_PAGE);
                ++l_I)
            {
                var l_ShortCut = m_FilteredList[l_I];

                m_ShortcutList.Data.Add((l_ShortCut, null));

                if (l_ShortCut == p_ShortcutToFocus)
                    l_RelIndexToFocus = m_ShortcutList.Data.Count - 1;
            }

            /// Refresh
            m_ShortcutList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_FilteredList.Count == 0)
                OnShortcutSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_ShortcutList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// On shortcut selected
        /// </summary>
        /// <param name="p_TableView">TableView instance</param>
        /// <param name="p_RelIndex">Relative index</param>
        private void OnShortcutSelected(HMUI.TableView p_TableView, int p_RelIndex)
        {
           int l_EventIndex = ((m_CurrentPage - 1) * EVENT_PER_PAGE) + p_RelIndex;

           if (p_RelIndex < 0 || l_EventIndex >= m_FilteredList.Count)
           {
               m_SelectedIndex = -1;
               return;
           }

           m_SelectedIndex = l_EventIndex;
        }
        /// <summary>
        /// Go to next shortcut page
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
        /// New shortcut button
        /// </summary>
        [UIAction("click-new-btn-pressed")]
        private void OnNewButton()
        {
            m_NewKeyboard.SetText("");
            ShowModal("OpenNewModal");
        }
        /// <summary>
        /// Delete shortcut button
        /// </summary>
        [UIAction("click-delete-btn-pressed")]
        private void OnDeleteButton()
        {
           if (!EnsureShortcutSelected())
               return;

           var l_Shortcut = m_FilteredList[m_SelectedIndex];

           ShowConfirmationModal($"<color=red>Do you want to delete shortcut</color>\n\"{l_Shortcut}\"?", () =>
           {
               OnShortcutSelected(null, -1);
               m_FilteredList.Remove(l_Shortcut);
               RebuildList(null);
           });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On rename keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("NewKeyboardPressed")]
        internal void NewKeyboardPressed(string p_Text)
        {
            m_FilteredList.Add(p_Text);
            RebuildList(p_Text);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ensure that an shortcut is selected
        /// </summary>
        /// <returns></returns>
        private bool EnsureShortcutSelected()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an shortcut first!");
                return false;
            }

            return true;
        }
    }
}
