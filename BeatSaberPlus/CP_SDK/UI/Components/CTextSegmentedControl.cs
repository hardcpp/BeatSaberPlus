using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// CTextSegmentedControl component
    /// </summary>
    public abstract class CTextSegmentedControl : MonoBehaviour
    {
        public abstract RectTransform       RTransform  { get; }
        public abstract ContentSizeFitter   CSizeFitter { get; }
        public abstract LayoutElement       LElement    { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active text changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CTextSegmentedControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get active text
        /// </summary>
        /// <returns></returns>
        public abstract int GetActiveText();
        /// <summary>
        /// Get text count
        /// </summary>
        /// <returns></returns>
        public abstract int GetTextCount();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set active text
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public abstract CTextSegmentedControl SetActiveText(int p_Index, bool p_Notify = true);
        /// <summary>
        /// Set texts
        /// </summary>
        /// <param name="p_Texts">New texts</param>
        /// <returns></returns>
        public abstract CTextSegmentedControl SetTexts(params string[] p_Texts);
    }
}
