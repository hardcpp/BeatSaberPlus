using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatIntegrations.UI
{
    /// <summary>
    /// Chat integrations main settings view
    /// </summary>
    internal partial class Settings
    {
        private static int s_ADD_CONDITION_FRAME_CATEGORY_PER_PAGE = 8;
        private static int s_ADD_CONDITION_FRAME_TYPE_PER_PAGE = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIObject("AddConditionFrame")]
        private GameObject m_AddConditionFrame = null;

        [UIObject("AddConditionFrame_LeftBackground")]
        private GameObject m_AddConditionFrame_LeftBackground = null;
        [UIComponent("AddConditionFrame_CategoryUpButton")]
        private Button m_AddConditionFrame_CategoryUpButton = null;
        [UIObject("AddConditionFrame_CategoryList")]
        private GameObject m_AddConditionFrame_CategoryListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_AddConditionFrame_CategoryList = null;
        [UIComponent("AddConditionFrame_CategoryDownButton")]
        private Button m_AddConditionFrame_CategoryDownButton = null;

        [UIObject("AddConditionFrame_RightBackground")]
        private GameObject m_AddConditionFrame_RightBackground = null;
        [UIComponent("AddConditionFrame_TypeUpButton")]
        private Button m_AddConditionFrame_TypeUpButton = null;
        [UIObject("AddConditionFrame_TypeList")]
        private GameObject m_AddConditionFrame_TypeListView = null;
        private BeatSaberPlus.SDK.UI.DataSource.SimpleTextList m_AddConditionFrame_TypeList = null;
        [UIComponent("AddConditionFrame_TypeDownButton")]
        private Button m_AddConditionFrame_TypeDownButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private int m_AddConditionFrame_CategoryListPage = 1;
        private int m_AddConditionFrame_SelectedCategory = -1;

        private int m_AddConditionFrame_TypeListPage = 1;
        private int m_AddConditionFrame_SelectedType = -1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void SetupAddConditionFrame()
        {
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_AddConditionFrame_LeftBackground,   0.50f);
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_AddConditionFrame_RightBackground,  0.50f);

            m_AddConditionFrame_CategoryUpButton.transform.localScale      = Vector3.one * 0.6f;
            m_AddConditionFrame_CategoryDownButton.transform.localScale    = Vector3.one * 0.6f;

            /// Setup add condition category list
            if (m_AddConditionFrame_CategoryListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_AddConditionFrame_CategoryListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_AddConditionFrame_CategoryListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_AddConditionFrame_CategoryListView.GetComponentInChildren<CustomListTableData>());
                m_AddConditionFrame_CategoryList                   = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_AddConditionFrame_CategoryList.TableViewInstance = l_BSMLTableView;
                m_AddConditionFrame_CategoryList.CellSizeValue     = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent         += AddConditionFrame_CategorySelected;
                l_BSMLTableView.SetDataSource(m_AddConditionFrame_CategoryList, false);

                /// Bind events
                m_AddConditionFrame_CategoryUpButton.onClick.AddListener(AddConditionFrame_CategoryPageUpPressed);
                m_AddConditionFrame_CategoryDownButton.onClick.AddListener(AddConditionFrame_CategoryPageDownPressed);
            }

            m_AddConditionFrame_TypeUpButton.transform.localScale      = Vector3.one * 0.6f;
            m_AddConditionFrame_TypeDownButton.transform.localScale    = Vector3.one * 0.6f;

            /// Setup add condition type list
            if (m_AddConditionFrame_TypeListView.GetComponent<LayoutElement>())
            {
                var l_LayoutElement = m_AddConditionFrame_TypeListView.GetComponent<LayoutElement>();
                l_LayoutElement.preferredWidth  = 45;
                l_LayoutElement.preferredHeight = 40;

                var l_BSMLTableView = m_AddConditionFrame_TypeListView.GetComponentInChildren<BSMLTableView>();
                l_BSMLTableView.SetDataSource(null, false);
                GameObject.DestroyImmediate(m_AddConditionFrame_TypeListView.GetComponentInChildren<CustomListTableData>());
                m_AddConditionFrame_TypeList                    = l_BSMLTableView.gameObject.AddComponent<BeatSaberPlus.SDK.UI.DataSource.SimpleTextList>();
                m_AddConditionFrame_TypeList.TableViewInstance  = l_BSMLTableView;
                m_AddConditionFrame_TypeList.CellSizeValue      = 5f;
                l_BSMLTableView.didSelectCellWithIdxEvent   += AddConditionFrame_TypeSelected;
                l_BSMLTableView.SetDataSource(m_AddConditionFrame_TypeList, false);

                /// Bind events
                m_AddConditionFrame_TypeUpButton.onClick.AddListener(AddConditionFrame_TypePageUpPressed);
                m_AddConditionFrame_TypeDownButton.onClick.AddListener(AddConditionFrame_TypePageDownPressed);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New condition button
        /// </summary>
        [UIAction("AddConditionFrame_Show")]
        private void AddConditionFrame_Show()
        {
            AddConditionFrame_CategoryRebuildList();

            if (m_AddConditionFrame_SelectedCategory < 0 || m_AddConditionFrame_SelectedCategory > AddConditionFrame_GetConditionCategories().Count)
                AddConditionFrame_CategorySelected(null, 0);

            AddConditionFrame_TypeRebuildList();

            m_EventFrame.SetActive(false);
            m_AddConditionFrame.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous page
        /// </summary>
        private void AddConditionFrame_CategoryPageUpPressed()
        {
            /// Underflow check
            if (m_AddConditionFrame_CategoryListPage < 2)
                return;

            /// Decrement current page
            m_AddConditionFrame_CategoryListPage--;

            /// Rebuild list
            AddConditionFrame_CategoryRebuildList();
        }
        /// <summary>
        /// Rebuilt list
        /// </summary>
        private void AddConditionFrame_CategoryRebuildList()
        {
            if (!UICreated)
                return;

            var l_Candidates = AddConditionFrame_GetConditionCategories();

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(l_Candidates.Count) / (float)(s_ADD_CONDITION_FRAME_CATEGORY_PER_PAGE)));

            /// Update overflow
            m_AddConditionFrame_CategoryListPage = Math.Max(1, Math.Min(m_AddConditionFrame_CategoryListPage, l_PageCount));

            /// Update UI
            m_AddConditionFrame_CategoryUpButton.interactable    = m_AddConditionFrame_CategoryListPage > 1;
            m_AddConditionFrame_CategoryDownButton.interactable  = m_AddConditionFrame_CategoryListPage < l_PageCount;

            /// Clear old entries
            m_AddConditionFrame_CategoryList.TableViewInstance.ClearSelection();
            m_AddConditionFrame_CategoryList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_AddConditionFrame_CategoryListPage - 1) * s_ADD_CONDITION_FRAME_CATEGORY_PER_PAGE;
                     l_I < l_Candidates.Count && l_I < (m_AddConditionFrame_CategoryListPage * s_ADD_CONDITION_FRAME_CATEGORY_PER_PAGE);
                    ++l_I)
            {
                m_AddConditionFrame_CategoryList.Data.Add(("<align=\"left\"><color=yellow> " + (l_I + 1) + "</color> - " + l_Candidates[l_I], null));
            }

            /// Refresh
            m_AddConditionFrame_CategoryList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Conditions.Count == 0)
                AddConditionFrame_CategorySelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_AddConditionFrame_CategoryList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an element is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void AddConditionFrame_CategorySelected(TableView p_List, int p_RelIndex)
        {
            int l_ConditionIndex = ((m_AddConditionFrame_CategoryListPage - 1) * s_ADD_CONDITION_FRAME_CATEGORY_PER_PAGE) + p_RelIndex;

            var l_Candidates = AddConditionFrame_GetConditionCategories();
            if (p_RelIndex < 0 || l_ConditionIndex >= l_Candidates.Count)
            {
                m_AddConditionFrame_SelectedCategory = -1;
                return;
            }

            m_AddConditionFrame_SelectedCategory = l_ConditionIndex;
            AddConditionFrame_TypeRebuildList();
        }
        /// <summary>
        /// Go to next page
        /// </summary>
        private void AddConditionFrame_CategoryPageDownPressed()
        {
            /// Increment current page
            m_AddConditionFrame_CategoryListPage++;

            /// Rebuild list
            AddConditionFrame_CategoryRebuildList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous page
        /// </summary>
        private void AddConditionFrame_TypePageUpPressed()
        {
            /// Underflow check
            if (m_AddConditionFrame_TypeListPage < 2)
                return;

            /// Decrement current page
            m_AddConditionFrame_TypeListPage--;

            /// Rebuild list
            AddConditionFrame_TypeRebuildList();
        }
        /// <summary>
        /// Rebuilt list
        /// </summary>
        private void AddConditionFrame_TypeRebuildList()
        {
            if (!UICreated)
                return;

            var l_Candidates = AddConditionFrame_GetConditionTypes(m_AddConditionFrame_SelectedCategory);

            /// Update page count
            var l_PageCount = Math.Max(1, Mathf.CeilToInt((float)(l_Candidates.Count) / (float)(s_ADD_CONDITION_FRAME_TYPE_PER_PAGE)));

            /// Update overflow
            m_AddConditionFrame_TypeListPage = Math.Max(1, Math.Min(m_AddConditionFrame_TypeListPage, l_PageCount));

            /// Update UI
            m_AddConditionFrame_TypeUpButton.interactable    = m_AddConditionFrame_TypeListPage > 1;
            m_AddConditionFrame_TypeDownButton.interactable  = m_AddConditionFrame_TypeListPage < l_PageCount;

            /// Clear old entries
            m_AddConditionFrame_TypeList.TableViewInstance.ClearSelection();
            m_AddConditionFrame_TypeList.Data.Clear();

            int l_RelIndexToFocus = -1;
            for (int l_I = (m_AddConditionFrame_TypeListPage - 1) * s_ADD_CONDITION_FRAME_TYPE_PER_PAGE;
                     l_I < l_Candidates.Count && l_I < (m_AddConditionFrame_TypeListPage * s_ADD_CONDITION_FRAME_TYPE_PER_PAGE);
                    ++l_I)
            {
                m_AddConditionFrame_TypeList.Data.Add(("<align=\"left\"><color=yellow> " + (l_I + 1) + "</color> - " + l_Candidates[l_I].Item2, null));
            }

            /// Refresh
            m_AddConditionFrame_TypeList.TableViewInstance.ReloadData();

            /// Update focus
            if (m_CurrentEvent.Conditions.Count == 0)
                AddConditionFrame_TypeSelected(null, -1);
            else if (l_RelIndexToFocus != -1)
                m_AddConditionFrame_TypeList.TableViewInstance.SelectCellWithIdx(l_RelIndexToFocus, true);
        }
        /// <summary>
        /// When an element is selected
        /// </summary>
        /// <param name="p_List">List instance</param>
        /// <param name="p_RelIndex">Selected index</param>
        private void AddConditionFrame_TypeSelected(TableView p_List, int p_RelIndex)
        {
            int l_ConditionIndex = ((m_AddConditionFrame_TypeListPage - 1) * s_ADD_CONDITION_FRAME_TYPE_PER_PAGE) + p_RelIndex;

            var l_Candidates = AddConditionFrame_GetConditionTypes(m_AddConditionFrame_SelectedCategory);
            if (p_RelIndex < 0 || l_ConditionIndex >= l_Candidates.Count)
            {
                m_AddConditionFrame_SelectedType = -1;
                return;
            }

            m_AddConditionFrame_SelectedType = l_ConditionIndex;
        }
        /// <summary>
        /// Go to next page
        /// </summary>
        private void AddConditionFrame_TypePageDownPressed()
        {
            /// Increment current page
            m_AddConditionFrame_TypeListPage++;

            /// Rebuild list
            AddConditionFrame_TypeRebuildList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel add condition button pressed
        /// </summary>
        [UIAction("AddConditionFrame_OnCancelAddConditionButton")]
        private void AddConditionFrame_OnCancelAddConditionButton()
        {
            m_AddConditionFrame.SetActive(false);
            m_EventFrame.SetActive(true);
        }
        /// <summary>
        /// On add condition button pressed
        /// </summary>
        [UIAction("AddConditionFrame_OnAddConditionButton")]
        private void AddConditionFrame_OnAddConditionButton()
        {
            if (m_AddConditionFrame_SelectedCategory == -1 || m_AddConditionFrame_SelectedType == -1)
            {
                ShowMessageModal("Please select a condition!");
                return;
            }

            var l_Candidates    = AddConditionFrame_GetConditionTypes(m_AddConditionFrame_SelectedCategory);
            var l_NewCondition  = Activator.CreateInstance(l_Candidates[m_AddConditionFrame_SelectedType].Item1.GetType()) as Interfaces.IConditionBase;
            l_NewCondition.Event = m_CurrentEvent;
            l_NewCondition.IsEnabled = true;

            m_CurrentEvent.AddCondition(l_NewCondition);

            RebuildConditionList(l_NewCondition);

            m_AddConditionFrame.SetActive(false);
            m_EventFrame.SetActive(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get condition categories
        /// </summary>
        /// <returns></returns>
        private List<string> AddConditionFrame_GetConditionCategories()
        {
            List<string> l_Result = new List<string>();

            foreach (var l_Current in m_CurrentEvent.AvailableConditions.Select(x => x.GetTypeNameShort()))
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
        /// Get conditions types for specific category
        /// </summary>
        /// <param name="p_CategoryIndex"></param>
        /// <returns></returns>
        private List<(Interfaces.IConditionBase, string)> AddConditionFrame_GetConditionTypes(int p_CategoryIndex)
        {
            var l_Result = new List<(Interfaces.IConditionBase, string)>();
            var l_Categories = AddConditionFrame_GetConditionCategories();

            if (p_CategoryIndex == -1 || p_CategoryIndex > l_Categories.Count)
                return l_Result;

            foreach (var l_Current in m_CurrentEvent.AvailableConditions)
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
