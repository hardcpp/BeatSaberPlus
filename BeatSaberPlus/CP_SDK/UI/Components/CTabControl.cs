using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Tab control widget
    /// </summary>
    public abstract class CTabControl : MonoBehaviour
    {
        public abstract RectTransform       RTransform  { get; }
        public abstract LayoutElement       LElement    { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active tab changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CTabControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get active tab
        /// </summary>
        /// <returns></returns>
        public abstract int GetActiveTab();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set active tab
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CTabControl SetActiveTab(int p_Index, bool p_Notify = true);
        /// <summary>
        /// Set tabs
        /// </summary>
        /// <param name="p_Tabs">Tabs</param>
        /// <returns></returns>
        public abstract CTabControl SetTabs(params (string, RectTransform)[] p_Tabs);
    }
}
