using System;
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CVVList XUI Element
    /// </summary>
    public class XUIVVList
        : IXUIElement, IXUIElementReady<XUIVVList, UI.Components.CVVList>, IXUIBindable<XUIVVList>
    {
        private UI.Components.CVVList m_Element = null;

        private event Action<UI.Components.CVVList> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform RTransform => Element?.RTransform;
        public UI.Components.CVVList Element => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIVVList(string p_Name) : base(p_Name) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public static XUIVVList Make()
            => new XUIVVList("XUIVVList");
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        public static XUIVVList Make(string p_Name)
            => new XUIVVList(p_Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.VVListFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIVVList.BuildUI] Error OnReady:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ready, append callback functor
        /// </summary>
        /// <param name="p_Functor">Functor to add</param>
        /// <returns></returns>
        public XUIVVList OnReady(Action<UI.Components.CVVList> p_Functor)
        {
            if (m_Element)    p_Functor?.Invoke(m_Element);
            else m_OnReady += p_Functor;
            return this;
        }
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        public XUIVVList Bind(ref XUIVVList p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On list item selected event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUIVVList OnListItemSelected(Action<UI.Data.IListItem> p_Functor, bool p_Add = true) => OnReady(x => x.OnListItemSelected(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIVVList SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set list cell prefab
        /// </summary>
        /// <param name="p_Prefab">New prefab</param>
        public XUIVVList SetListCellPrefab(UI.Data.IListCell p_Prefab) => OnReady(x => x.SetListCellPrefab(p_Prefab));
        /// <summary>
        /// Set list items
        /// </summary>
        /// <param name="p_ListItems">New items</param>
        /// <returns></returns>
        public XUIVVList SetListItems(List<UI.Data.IListItem> p_ListItems) => OnReady(x => x.SetListItems(p_ListItems));
        /// <summary>
        /// Set list items
        /// </summary>
        /// <param name="p_ListItems">New items</param>
        /// <returns></returns>
        public XUIVVList SetListItems<T>(List<T> p_ListItems) where T : UI.Data.IListItem => OnReady(x => x.SetListItems<T>(p_ListItems));
        /// <summary>
        /// Set selected list item
        /// </summary>
        /// <param name="p_ListItem">Selected list item</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUIVVList SetSelectedListItem(UI.Data.IListItem p_ListItem, bool p_Notify = true) => OnReady(x => x.SetSelectedListItem(p_ListItem, p_Notify));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add a list item
        /// </summary>
        /// <param name="p_ListItem">Item to add</param>
        /// <returns></returns>
        public XUIVVList AddListItem(UI.Data.IListItem p_ListItem) => OnReady(x => x.AddListItem(p_ListItem));
        /// <summary>
        /// Sort list items by a functor
        /// </summary>
        /// <param name="p_Functor"></param>
        public XUIVVList SortListItems(Func<UI.Data.IListItem, UI.Data.IListItem, int> p_Functor) => OnReady(x => x.SortListItems(p_Functor));
        /// <summary>
        /// Remove a list item
        /// </summary>
        /// <param name="p_ListItem">Item to remove</param>
        /// <returns></returns>
        public XUIVVList RemoveListItem(UI.Data.IListItem p_ListItem) => OnReady(x => x.RemoveListItem(p_ListItem));
    }
}
