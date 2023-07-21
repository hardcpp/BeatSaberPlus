using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Generic virtual list interface
    /// </summary>
    public abstract class CVXList : MonoBehaviour
    {
        public abstract RectTransform   RTransform      { get; }
        public abstract LayoutElement   LElement        { get; }
        public abstract float           ScrollPosition  { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On list item selected event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CVXList OnListItemSelected(Action<Data.IListItem> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get selected item
        /// </summary>
        /// <returns></returns>
        public abstract Data.IListItem GetSelectedItem();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Scroll to position
        /// </summary>
        /// <param name="p_TargetPosition">New target position</param>
        /// <param name="p_Animated">Is animated?</param>
        public abstract CVXList ScrollTo(float p_TargetPosition, bool p_Animated);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set list cell prefab
        /// </summary>
        /// <param name="p_Prefab">New prefab</param>
        public abstract CVXList SetListCellPrefab(Data.IListCell p_Prefab);
        /// <summary>
        /// Set list items
        /// </summary>
        /// <param name="p_ListItems">New items</param>
        /// <returns></returns>
        public abstract CVXList SetListItems(List<Data.IListItem> p_ListItems);
        /// <summary>
        /// Set list items
        /// </summary>
        /// <param name="p_ListItems">New items</param>
        /// <returns></returns>
        public CVXList SetListItems<T>(List<T> p_ListItems)
            where T : Data.IListItem
        {
            var l_List = Pool.ListPool<Data.IListItem>.Get();
            try
            {
                l_List.Clear();
                l_List.Capacity = p_ListItems.Count;
                l_List.AddRange(p_ListItems);
                SetListItems(l_List);
            }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Components][CVXList.SetListItems] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            finally
            {
                Pool.ListPool<Data.IListItem>.Release(l_List);
            }

            return this;
        }
        /// <summary>
        /// Set selected list item
        /// </summary>
        /// <param name="p_ListItem">Selected list item</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CVXList SetSelectedListItem(Data.IListItem p_ListItem, bool p_Notify = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a list item
        /// </summary>
        /// <param name="p_ListItem">Item to add</param>
        /// <returns></returns>
        public abstract CVXList AddListItem(Data.IListItem p_ListItem);
        /// <summary>
        /// Sort list items by a functor
        /// </summary>
        /// <param name="p_Functor"></param>
        public abstract CVXList SortListItems(Func<Data.IListItem, Data.IListItem, int> p_Functor);
        /// <summary>
        /// Remove a list item
        /// </summary>
        /// <param name="p_ListItem">Item to remove</param>
        /// <returns></returns>
        public abstract CVXList RemoveListItem(Data.IListItem p_ListItem);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On list cell clicked
        /// </summary>
        /// <param name="p_ListCell">Clicked list cell</param>
        public abstract void OnListCellClicked(Data.IListCell p_ListCell);
    }
}
