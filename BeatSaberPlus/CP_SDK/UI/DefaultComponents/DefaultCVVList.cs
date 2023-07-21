using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CVVList component
    /// </summary>
    public class DefaultCVVList : Components.CVVList
    {
        private RectTransform       m_RTransform;
        private DefaultCVScrollView m_ScrollView;

        private Data.IListCell                  m_ListCellTemplate  = null;
        private Pool.ObjectPool<Data.IListCell> m_ListCellPool      = null;
        private List<Data.IListCell>            m_VisibleListCells  = new List<Data.IListCell>(10);
        private List<Data.IListItem>            m_ListItems         = new List<Data.IListItem>(100);
        private Data.IListItem                  m_SelectedListItem  = null;
        private bool                            m_Dirty             = false;

        private event Action<Data.IListItem> m_OnListItemSelected;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform      => m_RTransform;
        public override LayoutElement   LElement        => m_ScrollView?.LElement;
        public override float           ScrollPosition  => m_ScrollView?.Position ?? 0.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            m_RTransform = transform as RectTransform;

            m_ScrollView = gameObject.AddComponent<DefaultCVScrollView>();
            m_ScrollView.Init();
            m_ScrollView.ScrollType = Components.CVScrollView.EScrollType.FixedCellSize;
            m_ScrollView.OnScrollChanged((_) => UpdateForCurrentScroll());

            GameObject.DestroyImmediate(m_ScrollView.Container.GetComponent<VerticalLayoutGroup>());
            GameObject.DestroyImmediate(m_ScrollView.Container.parent.GetComponent<VerticalLayoutGroup>());

            m_ScrollView.Container.anchorMin = new Vector2(0.0f, 1.0f);
            m_ScrollView.Container.anchorMax = new Vector2(1.0f, 1.0f);
            m_ScrollView.Container.pivot     = new Vector2(0.5f, 1.0f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On list item selected event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CVXList OnListItemSelected(Action<Data.IListItem> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnListItemSelected += p_Functor;
            else        m_OnListItemSelected -= p_Functor;

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get selected item
        /// </summary>
        /// <returns></returns>
        public override Data.IListItem GetSelectedItem()
            => m_SelectedListItem;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Scroll to position
        /// </summary>
        /// <param name="p_TargetPosition">New target position</param>
        /// <param name="p_Animated">Is animated?</param>
        public override Components.CVXList ScrollTo(float p_TargetPosition, bool p_Animated)
        {
            m_ScrollView?.ScrollTo(p_TargetPosition, p_Animated);
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set list cell prefab
        /// </summary>
        /// <param name="p_Prefab">New prefab</param>
        public override Components.CVXList SetListCellPrefab(Data.IListCell p_Prefab)
        {
            ClearVisibles(true);

            if (m_ListCellPool != null)
            {
                m_ListCellPool.Clear();
                m_ListCellPool = null;
            }

            GameObject.DontDestroyOnLoad(p_Prefab.gameObject);
            p_Prefab.gameObject.SetActive(false);

            m_ListCellTemplate = p_Prefab;

            m_ListCellPool = new Pool.ObjectPool<Data.IListCell>(
                createFunc:      ( ) => {
                    var x = m_ListCellTemplate.Create(m_ScrollView.Container);
                    x.RTransform.anchorMin = new Vector2(0.0f, 1.0f);
                    x.RTransform.anchorMax = new Vector2(1.0f, 1.0f);
                    x.RTransform.pivot     = new Vector2(0.5f, 1.0f);
                    x.RTransform.sizeDelta = new Vector2(0.0f, x.GetCellHeight());

                    return x;
                },
                actionOnGet:     (x) => x.gameObject.SetActive(true),
                actionOnRelease: (x) => x.gameObject.SetActive(false),
                actionOnDestroy: (x) => GameObject.Destroy(x.gameObject),
                collectionCheck: true
            );

            m_ScrollView.FixedCellSize = m_ListCellTemplate.GetCellHeight();

            UpdateForCurrentScroll();

            return this;
        }
        /// <summary>
        /// Set list items
        /// </summary>
        /// <param name="p_ListItems">New items</param>
        /// <returns></returns>
        public override Components.CVXList SetListItems(List<Data.IListItem> p_ListItems)
        {
            m_ListItems.Clear();
            if (p_ListItems != null && p_ListItems.Count > 0)
                m_ListItems.AddRange(p_ListItems);

            m_ScrollView.Container.sizeDelta = new Vector2(0.0f, m_ListItems.Count * m_ListCellTemplate.GetCellHeight());
            m_ScrollView.SetContentSize(m_ScrollView.Container.sizeDelta.y);
            m_ScrollView.ScrollTo(0.0f, false);

            ClearVisibles(true);
            SetSelectedListItem(null);
            UpdateForCurrentScroll();

            return this;
        }
        /// <summary>
        /// Set selected list item
        /// </summary>
        /// <param name="p_ListItem">Selected list item</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public override Components.CVXList SetSelectedListItem(Data.IListItem p_ListItem, bool p_Notify = true)
        {
            var l_NewSelectListItem = p_ListItem;
            if (!m_ListItems.Contains(l_NewSelectListItem))
                l_NewSelectListItem = null;

            try { m_SelectedListItem?.OnUnselect(); }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.SetSelectedListItem] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }

            m_SelectedListItem = l_NewSelectListItem;

            if (m_SelectedListItem != null)
            {
                var l_PackIndex         = m_ListItems.IndexOf(l_NewSelectListItem);
                var l_TargetHeight      = l_PackIndex * m_ListCellTemplate.GetCellHeight();
                var l_CenteredHeight    = l_TargetHeight - ((GetListCellPerPage() / 2) * m_ListCellTemplate.GetCellHeight());
                var l_MaxHeight         = Mathf.Max(0, m_ScrollView.ContentSize - m_ScrollView.ScrollPageSize);

                l_CenteredHeight = Mathf.Max(0, l_CenteredHeight);
                if (l_CenteredHeight > l_MaxHeight)
                    l_CenteredHeight = l_MaxHeight;

                if (m_ScrollView.ContentSize < m_ScrollView.ScrollPageSize)
                    l_CenteredHeight = 0.0f;

                m_ScrollView.ScrollTo(l_CenteredHeight, true);
                UpdateForCurrentScroll();

                try { m_SelectedListItem?.OnSelect(); }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.SetSelectedListItem] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }

            if (p_Notify)
            {
                try { m_OnListItemSelected?.Invoke(m_SelectedListItem); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.SetSelectedListItem] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }
            }

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a list item
        /// </summary>
        /// <param name="p_ListItem">Item to add</param>
        /// <returns></returns>
        public override Components.CVXList AddListItem(Data.IListItem p_ListItem)
        {
            m_ListItems.Add(p_ListItem);

            m_ScrollView.Container.sizeDelta = new Vector2(0.0f, m_ListItems.Count * m_ListCellTemplate.GetCellHeight());
            m_ScrollView.SetContentSize(m_ScrollView.Container.sizeDelta.y);

            ClearVisibles(false);
            UpdateForCurrentScroll();

            return this;
        }
        /// <summary>
        /// Sort list items by a functor
        /// </summary>
        /// <param name="p_Functor"></param>
        public override Components.CVXList SortListItems(Func<Data.IListItem, Data.IListItem, int> p_Functor)
        {
            m_ListItems.Sort((x, y) => p_Functor(x, y));

            ClearVisibles(false);
            UpdateForCurrentScroll();

            return this;
        }
        /// <summary>
        /// Remove a list item
        /// </summary>
        /// <param name="p_ListItem">Item to remove</param>
        /// <returns></returns>
        public override Components.CVXList RemoveListItem(Data.IListItem p_ListItem)
        {
            m_ListItems.Remove(p_ListItem);

            m_ScrollView.Container.sizeDelta = new Vector2(0.0f, m_ListItems.Count * m_ListCellTemplate.GetCellHeight());
            m_ScrollView.SetContentSize(m_ScrollView.Container.sizeDelta.y);

            ClearVisibles(false);
            UpdateForCurrentScroll();

            if (m_SelectedListItem == p_ListItem)
                SetSelectedListItem(null);

            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On list cell clicked
        /// </summary>
        /// <param name="p_ListCell">Clicked list cell</param>
        public override void OnListCellClicked(Data.IListCell p_ListCell)
        {
            if (!p_ListCell || p_ListCell.OwnerList != this || !m_VisibleListCells.Contains(p_ListCell))
                return;

            SetSelectedListItem(p_ListCell.ListItem);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (!m_Dirty)
                return;

            UpdateForCurrentScroll();
            m_Dirty = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear
        /// </summary>
        private void ClearVisibles(bool p_ScrollToTop)
        {
            for (int l_I = 0; l_I < m_VisibleListCells.Count; ++l_I)
            {
                var l_ExistingCell = m_VisibleListCells[l_I];

                try { l_ExistingCell.ListItem.OnHide(); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.ClearVisibles] OnHide Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                l_ExistingCell.ListItem.SetCell(null);
                l_ExistingCell.Bind(null, -1, null);
                m_ListCellPool.Release(l_ExistingCell);
            }

            m_VisibleListCells.Clear();

            if (p_ScrollToTop)
                m_ScrollView.ScrollTo(0.0f, false);
        }
        /// <summary>
        /// Get list cell per page
        /// </summary>
        /// <returns></returns>
        private float GetListCellPerPage()
            => m_ScrollView.ScrollPageSize / m_ListCellTemplate.GetCellHeight();
        /// <summary>
        /// Update for current scroll
        /// </summary>
        private void UpdateForCurrentScroll()
        {
            if (!m_ListCellTemplate)
                return;

            var l_ScrollPosY    = Mathf.Clamp(m_ScrollView.Position, 0f, m_ScrollView.ContentSize);
            var l_StartItem     = Mathf.Max(0, Mathf.FloorToInt(l_ScrollPosY / m_ListCellTemplate.GetCellHeight()));
            var l_EndItem       = Mathf.Min(Mathf.CeilToInt(l_StartItem + GetListCellPerPage()), m_ListItems.Count);

            for (int l_I = 0; l_I < m_VisibleListCells.Count; ++l_I)
            {
                var l_ExistingCell = m_VisibleListCells[l_I];
                if (l_ExistingCell.Index < l_StartItem || l_ExistingCell.Index >= l_EndItem)
                {
                    try { l_ExistingCell.ListItem.OnHide(); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.UpdateForCurrentScroll] OnHide Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }

                    l_ExistingCell.ListItem.SetCell(null);
                    l_ExistingCell.Bind(null, -1, null);
                    m_ListCellPool.Release(l_ExistingCell);
                    m_VisibleListCells.RemoveAt(l_I);
                    l_I--;
                }
            }

            var l_Width = m_ScrollView.ViewPortWidth;
            for (int l_I = l_StartItem; l_I < l_EndItem; ++l_I)
            {
                if (m_VisibleListCells.Any(x => x.Index == l_I))
                    continue;

                var l_NewCell = m_ListCellPool.Get();
                l_NewCell.RTransform.anchoredPosition = new Vector2(0, -(l_I * m_ListCellTemplate.GetCellHeight()));
                l_NewCell.Bind(this, l_I, m_ListItems[l_I]);
                l_NewCell.ListItem.SetCell(l_NewCell);

                try { l_NewCell.ListItem.OnShow(); }
                catch (System.Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCVVList.UpdateForCurrentScroll] OnShow Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                m_VisibleListCells.Add(l_NewCell);
            }

            for (int l_I = 0; l_I < m_VisibleListCells.Count; ++l_I)
            {
                var l_Object = m_VisibleListCells[l_I];
                l_Object.SetState(l_Object.ListItem == m_SelectedListItem);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On layout changed
        /// </summary>
        private void OnRectTransformDimensionsChange()
        {
            m_Dirty = true;
            SetSelectedListItem(m_SelectedListItem, false);
        }
    }
}
