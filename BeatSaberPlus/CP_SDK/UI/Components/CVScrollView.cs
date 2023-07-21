using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Vertical scroll view component
    /// </summary>
    public abstract class CVScrollView : MonoBehaviour
    {
        /// <summary>
        /// Scroll type enum
        /// </summary>
        public enum EScrollType
        {
            PageSize,
            FixedCellSize
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public abstract RectTransform   RTransform      { get; }
        public abstract LayoutElement   LElement        { get; }
        public abstract RectTransform   Container       { get; }

        public EScrollType              ScrollType              = EScrollType.PageSize;
        public float                    FixedCellSize           = 10f;
        public float                    PageStepNormalizedSize  = 0.7f;

        public abstract float           Position        { get; }
        public abstract float           ViewPortWidth   { get; }
        public abstract float           ScrollableSize  { get; }
        public abstract float           ScrollPageSize  { get; }
        public abstract float           ContentSize     { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On scroll changed
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CVScrollView OnScrollChanged(Action<float> p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update content size
        /// </summary>
        public abstract CVScrollView UpdateContentSize();
        /// <summary>
        /// Set content size
        /// </summary>
        /// <param name="p_ContentSize">New content size</param>
        public abstract CVScrollView SetContentSize(float p_ContentSize);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Scroll to position
        /// </summary>
        /// <param name="p_TargetPosition">New target position</param>
        /// <param name="p_Animated">Is animated?</param>
        public abstract CVScrollView ScrollTo(float p_TargetPosition, bool p_Animated);
        /// <summary>
        /// Scroll to end
        /// </summary>
        /// <param name="p_Animated">Is animated?</param>
        public abstract CVScrollView ScrollToEnd(bool p_Animated);
        /// <summary>
        /// Refresh scroll buttons
        /// </summary>
        public abstract CVScrollView RefreshScrollButtons();
    }
}
