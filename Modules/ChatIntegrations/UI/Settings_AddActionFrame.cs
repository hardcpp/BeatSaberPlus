using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.ChatIntegrations.UI
{
    /// <summary>
    /// Chat integrations main settings view
    /// </summary>
    internal partial class Settings
    {
        private static int s_ADD_ACTION_FRAME_CATEGORY_PER_PAGE = 8;
        private static int s_ADD_ACTION_FRAME_TYPE_PER_PAGE = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("AddActionFrame")]
        private GameObject m_AddActionFrame = null;

        [UIObject("AddActionFrame_LeftBackground")]
        private GameObject m_AddActionFrame_LeftBackground = null;
        [UIComponent("AddActionFrame_CategoryUpButton")]
        private Button m_AddActionFrame_CategoryUpButton = null;
        [UIObject("AddActionFrame_CategoryList")]
        private GameObject m_AddActionFrame_CategoryListView = null;
        private SDK.UI.DataSource.SimpleTextList m_AddActionFrame_CategoryList = null;
        [UIComponent("AddActionFrame_CategoryDownButton")]
        private Button m_AddActionFrame_CategoryDownButton = null;

        [UIObject("AddActionFrame_RightBackground")]
        private GameObject m_AddActionFrame_RightBackground = null;
        [UIComponent("AddActionFrame_TypeUpButton")]
        private Button m_AddActionFrame_TypeUpButton = null;
        [UIObject("AddActionFrame_TypeList")]
        private GameObject m_AddActionFrame_TypeListView = null;
        private SDK.UI.DataSource.SimpleTextList m_AddActionFrame_TypeList = null;
        [UIComponent("AddActionFrame_TypeDownButton")]
        private Button m_AddActionFrame_TypeDownButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private int m_AddActionFrame_CategoryListPage = 1;
        private int m_AddActionFrame_SelectedCategory = -1;

        private int m_AddActionFrame_TypeListPage = 1;
        private int m_AddActionFrame_SelectedType = -1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void SetupAddActionFrame()
        {
            SDK.UI.Backgroundable.SetOpacity(m_AddActionFrame_LeftBackground,   0.50f);
            SDK.UI.Backgroundable.SetOpacity(m_AddActionFrame_RightBackground,  0.50f);

            m_AddActionFrame_CategoryUpButton.transform.localScale      = Vector3.one * 0.6f;
            m_AddActionFrame_CategoryDownButton.transform.localScale    = Vector3.one * 0.6f;

            /// Setup add action category list
            if (m_AddActionFrame_CategoryListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_AddActionFrame_CategoryListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_AddActionFrame_CategoryListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_AddActionFrame_CategoryListView.GetComponentInChildren<CustomListTableData>());
                m_AddActionFrame_CategoryList                   = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
                m_AddActionFrame_CategoryList.TableViewInstance = l_BSMLTableView;
                m_AddActionFrame_CategoryList.CellSizeValue     = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent       += AddActionFrame_CategorySelected;
                l_BSMLTableView.SetDataSource(m_AddActionFrame_CategoryList, false);

                /// Bind events
                m_AddActionFrame_CategoryUpButton.onClick.AddListener(AddActionFrame_CategoryPageUpPressed);
                m_AddActionFrame_CategoryDownButton.onClick.AddListener(AddActionFrame_CategoryPageDownPressed);
            }

            m_AddActionFrame_TypeUpButton.transform.localScale      = Vector3.one * 0.6f;
            m_AddActionFrame_TypeDownButton.transform.localScale    = Vector3.one * 0.6f;

            /// Setup add action type list
            if (m_AddActionFrame_TypeListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_AddActionFrame_TypeListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_AddActionFrame_TypeListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_AddActionFrame_TypeListView.GetComponentInChildren<CustomListTableData>());
                m_AddActionFrame_TypeList                    = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
                m_AddActionFrame_TypeList.TableViewInstance  = l_BSMLTableView;
                m_AddActionFrame_TypeList.CellSizeValue      = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent   += AddActionFrame_TypeSelected;
                l_BSMLTableView.SetDataSource(m_AddActionFrame_TypeList, false);

                /// Bind events
                m_AddActionFrame_TypeUpButton.onClick.AddListener(AddActionFrame_TypePageUpPressed);
                m_AddActionFrame_TypeDownButton.onClick.AddListener(AddActionFrame_TypePageDownPressed);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New action button
        /// </summary>
        [UIAction("AddActionFrame_Show")]
        private void AddActionFrame_Show()
        {
            AddActionFrame_CategoryRebuildList();

            if (m_AddActionFrame_SelectedCategory < 0 || m_AddActionFrame_SelectedCategory > AddActionFrame_GetActionCategories().Count)
                AddActionFrame_CategorySelected(null, 0);

            AddActionFrame_TypeRebuildList();

            m_EventFrame.SetActive(false);
            m_AddActionFrame.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous page
        /// </summary>
        private void AddActionFrame_CategoryPageUpPressed()
        {
            /// Underflow check
            if (m_AddActionFrame_CategoryListPage < 2)
                return;

            /// Decrement current page
            m_AddActionFrame_CategoryListPage--;

            /// Rebuild list
            AddActionFrame_CategoryRebuildList();
        }
        /// <summary>
        /// Rebuilt list
        /// </summary>
        private void AddActionFrame_CategoryRebuildList()
        {
            if (!UICreated)
                return;

            var l_Candidates = AddActionFrame_GetActionCategories();

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(l_Candidates.Count) / (float)(s_ADD_ACTION_FRAME_CATEGORY_PER_PAGE)));

            /// Update overflow
            m_AddActionFrame_CategoryListPage = Math.Max(1, Math.Min(m_AddActionFrame_CategoryListPage, l_PageCount));

            /// Update UI
            m_AddActionFrame_CategoryUpButton.interactable    = m_AddActionFrame_CategoryListPage > 1;
            m_AddActionFrame_CategoryDownButton.interactable  = m_AddActionFrame_CategoryListPage < l_PageCount;

            /// Clear old entries
            m_AddActionFrame_CategoryList.TableViewInstance.ClearSelection();
            m_AddActionFrame_CategoryList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_AddActionFrame_CategoryListPage - 1) * s_ADD_ACTION_FRAME_CATEGORY_PER_PAGE;
                     l_I < l_Candidates.Count && l_I < (m_AddActionFrame_CategoryListPage * s_ADD_ACTION_FRAME_CATEGORY_PER_PAGE);
                    ++l_I)
            {
                m_AddActionFrame_CategoryList.Data.Add(("<align=\"left\"><color=yellow> " + (l_I + 1) + "</color> - " + l_Candidates[l_I], null));
            }

            /// Refresh
            m_AddActionFrame_CategoryList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Conditions.Count == 0)
                AddActionFrame_CategorySelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_AddActionFrame_CategoryList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an element is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void AddActionFrame_CategorySelected(TableView p_List, int p_RelIndex)
        {
            int l_ConditionIndex = ((m_AddActionFrame_CategoryListPage - 1) * s_ADD_ACTION_FRAME_CATEGORY_PER_PAGE) + p_RelIndex;

            var l_Candidates = AddActionFrame_GetActionCategories();
            if (p_RelIndex < 0 || l_ConditionIndex >= l_Candidates.Count)
            {
                m_AddActionFrame_SelectedCategory = -1;
                return;
            }

            m_AddActionFrame_SelectedCategory = l_ConditionIndex;
            AddActionFrame_TypeRebuildList();
        }
        /// <summary>
        /// Go to next page
        /// </summary>
        private void AddActionFrame_CategoryPageDownPressed()
        {
            /// Increment current page
            m_AddActionFrame_CategoryListPage++;

            /// Rebuild list
            AddActionFrame_CategoryRebuildList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous page
        /// </summary>
        private void AddActionFrame_TypePageUpPressed()
        {
            /// Underflow check
            if (m_AddActionFrame_TypeListPage < 2)
                return;

            /// Decrement current page
            m_AddActionFrame_TypeListPage--;

            /// Rebuild list
            AddActionFrame_TypeRebuildList();
        }
        /// <summary>
        /// Rebuilt list
        /// </summary>
        private void AddActionFrame_TypeRebuildList()
        {
            if (!UICreated)
                return;

            var l_Candidates = AddActionFrame_GetActionTypes(m_AddActionFrame_SelectedCategory);

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(l_Candidates.Count) / (float)(s_ADD_ACTION_FRAME_TYPE_PER_PAGE)));

            /// Update overflow
            m_AddActionFrame_TypeListPage = Math.Max(1, Math.Min(m_AddActionFrame_TypeListPage, l_PageCount));

            /// Update UI
            m_AddActionFrame_TypeUpButton.interactable    = m_AddActionFrame_TypeListPage > 1;
            m_AddActionFrame_TypeDownButton.interactable  = m_AddActionFrame_TypeListPage < l_PageCount;

            /// Clear old entries
            m_AddActionFrame_TypeList.TableViewInstance.ClearSelection();
            m_AddActionFrame_TypeList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_AddActionFrame_TypeListPage - 1) * s_ADD_ACTION_FRAME_TYPE_PER_PAGE;
                     l_I < l_Candidates.Count && l_I < (m_AddActionFrame_TypeListPage * s_ADD_ACTION_FRAME_TYPE_PER_PAGE);
                    ++l_I)
            {
                m_AddActionFrame_TypeList.Data.Add(("<align=\"left\"><color=yellow> " + (l_I + 1) + "</color> - " + l_Candidates[l_I].Item2, null));
            }

            /// Refresh
            m_AddActionFrame_TypeList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Conditions.Count == 0)
                AddActionFrame_TypeSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_AddActionFrame_TypeList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an element is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void AddActionFrame_TypeSelected(TableView p_List, int p_RelIndex)
        {
            int l_ConditionIndex = ((m_AddActionFrame_TypeListPage - 1) * s_ADD_ACTION_FRAME_TYPE_PER_PAGE) + p_RelIndex;

            var l_Candidates = AddActionFrame_GetActionTypes(m_AddActionFrame_SelectedCategory);
            if (p_RelIndex < 0 || l_ConditionIndex >= l_Candidates.Count)
            {
                m_AddActionFrame_SelectedType = -1;
                return;
            }

            m_AddActionFrame_SelectedType = l_ConditionIndex;
        }
        /// <summary>
        /// Go to next page
        /// </summary>
        private void AddActionFrame_TypePageDownPressed()
        {
            /// Increment current page
            m_AddActionFrame_TypeListPage++;

            /// Rebuild list
            AddActionFrame_TypeRebuildList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel add action button pressed
        /// </summary>
        [UIAction("AddActionFrame_OnCancelAddActionButton")]
        private void AddActionFrame_OnCancelAddActionButton()
        {
            m_AddActionFrame.SetActive(false);
            m_EventFrame.SetActive(true);
        }
        /// <summary>
        /// On add action button pressed
        /// </summary>
        [UIAction("AddActionFrame_OnAddActionButton")]
        private void AddActionFrame_OnAddActionButton()
        {
            if (m_AddActionFrame_SelectedCategory == -1 || m_AddActionFrame_SelectedType == -1)
            {
                ShowMessageModal("Please select an action!");
                return;
            }

            var l_Candidates = AddActionFrame_GetActionTypes(m_AddActionFrame_SelectedCategory);
            var l_NewAction  = Activator.CreateInstance(l_Candidates[m_AddActionFrame_SelectedType].Item1.GetType()) as Interfaces.IActionBase;
            l_NewAction.Event = m_CurrentEvent;
            l_NewAction.IsEnabled = true;

            m_CurrentEvent.AddAction(l_NewAction);

            RebuildActionList(l_NewAction);

            m_AddActionFrame.SetActive(false);
            m_EventFrame.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get action categories
        /// </summary>
        /// <returns></returns>
        private List<string> AddActionFrame_GetActionCategories()
        {
            List<string> l_Result = new List<string>();

            foreach (var l_Current in m_CurrentEvent.AvailableActions.Select(x => x.GetTypeNameShort()))
            {
                if (l_Current.Contains("_"))
                    l_Result.Add(l_Current.Substring(0, l_Current.IndexOf("_")));
                else
                    l_Result.Add("Others");
            }

            l_Result = l_Result.Distinct().OrderBy(x => x).ToList();
            return l_Result;
        }
        /// <summary>
        /// Get actions types for specific category
        /// </summary>
        /// <param name="p_CategoryIndex"></param>
        /// <returns></returns>
        private List<(Interfaces.IActionBase, string)> AddActionFrame_GetActionTypes(int p_CategoryIndex)
        {
            var l_Result = new List<(Interfaces.IActionBase, string)>();
            var l_Categories = AddActionFrame_GetActionCategories();

            if (p_CategoryIndex == -1 || p_CategoryIndex > l_Categories.Count)
                return l_Result;

            foreach (var l_Current in m_CurrentEvent.AvailableActions)
            {
                var l_ShortName = l_Current.GetTypeNameShort();
                if (l_ShortName.Contains("_") && l_ShortName.Substring(0, l_ShortName.IndexOf("_")) == l_Categories[p_CategoryIndex])
                {
                    l_Result.Add((l_Current, "<alpha=#80>" + l_ShortName.Substring(0, l_ShortName.IndexOf("_"))
                                           + "<alpha=#FF>::<b>" + l_ShortName.Substring(l_ShortName.IndexOf("_") + 1) + "</b>"));
                }
                else if (!l_ShortName.Contains("_") && l_Categories[p_CategoryIndex] == "Others")
                    l_Result.Add((l_Current, "<b>" + l_ShortName + "</b>"));
            }

            l_Result = l_Result.Distinct().OrderBy(x => x.Item2).ToList();
            return l_Result;
        }
    }
}
