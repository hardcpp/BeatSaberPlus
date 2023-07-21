
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents.Subs
{
    /// <summary>
    /// Stack layout group
    /// </summary>
    public class SubStackLayoutGroup : LayoutGroup
    {
        private bool m_ChildForceExpandWidth = true;
        private bool m_ChildForceExpandHeight = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Child force expand width
        /// </summary>
        public bool ChildForceExpandWidth
        {
            get => m_ChildForceExpandWidth;
            set => SetProperty(ref m_ChildForceExpandWidth, value);
        }
        /// <summary>
        /// Child force expand height
        /// </summary>
        public bool ChildForceExpandHeight
        {
            get => m_ChildForceExpandHeight;
            set => SetProperty(ref m_ChildForceExpandHeight, value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calculate the layout input for horizontal axis
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            UpdateForAxis(0);
        }
        /// <summary>
        /// Calculate the layout input for vertical axis
        /// </summary>
        public override void CalculateLayoutInputVertical() => UpdateForAxis(1);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set layout horizontal
        /// </summary>
        public override void SetLayoutHorizontal() => SetChildrensAlongAxis(0);
        /// <summary>
        /// Set layout vertical
        /// </summary>
        public override void SetLayoutVertical() => SetChildrensAlongAxis(1);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the layout group for given axis
        /// </summary>
        /// <param name="p_Axis">0 Horizontal 1 vertical</param>
        private void UpdateForAxis(int p_Axis)
        {
            var l_Padding   = p_Axis == 0 ? padding.horizontal : padding.vertical;
            var l_AxisMin   = 0f;
            var l_AxisPref  = 0f;
            var l_AxisFlex  = 0f;

            for (var l_I = 0; l_I < rectChildren.Count; ++l_I)
            {
                var l_RectTransform = rectChildren[l_I];

                if (l_RectTransform.GetComponent<Image>())
                    continue;

                var l_MinSize       = LayoutUtility.GetMinSize(         l_RectTransform, p_Axis);
                var l_PreferredSize = LayoutUtility.GetPreferredSize(   l_RectTransform, p_Axis);
                var l_FlexibleSize  = LayoutUtility.GetFlexibleSize(    l_RectTransform, p_Axis);

                if (p_Axis == 0 ? ChildForceExpandWidth : ChildForceExpandHeight)
                    l_FlexibleSize = Mathf.Max(l_FlexibleSize, 1.0f);

                l_AxisMin   = Mathf.Max(l_MinSize + l_Padding, l_AxisMin);
                l_AxisPref  = Mathf.Max(l_PreferredSize + l_Padding, l_AxisPref);
                l_AxisFlex  = Mathf.Max(l_FlexibleSize, l_AxisFlex);
            }

            l_AxisPref = Mathf.Max(l_AxisMin, l_AxisPref);

            SetLayoutInputForAxis(l_AxisMin, l_AxisPref, l_AxisFlex, p_Axis);
        }
        /// <summary>
        /// Set childrens along axis for given axis
        /// </summary>
        /// <param name="p_Axis">0 Horizontal 1 vertical</param>
        private void SetChildrensAlongAxis(int p_Axis)
        {
            var l_AxisSize      = rectTransform.rect.size[p_Axis];
            var l_NewAxisSize   = l_AxisSize - (p_Axis == 0 ? padding.horizontal : padding.vertical);

            for (var l_I = 0; l_I < rectChildren.Count; ++l_I)
            {
                var l_RectTransform = rectChildren[l_I];
                var l_MinSize       = LayoutUtility.GetMinSize(         l_RectTransform, p_Axis);
                var l_PreferredSize = LayoutUtility.GetPreferredSize(   l_RectTransform, p_Axis);
                var l_FlexibleSize  = LayoutUtility.GetFlexibleSize(    l_RectTransform, p_Axis);

                if ((p_Axis == 0) ? ChildForceExpandWidth : ChildForceExpandHeight)
                    l_FlexibleSize = Mathf.Max(l_FlexibleSize, 1.0f);

                var l_Size      = Mathf.Clamp(l_NewAxisSize, l_MinSize, (l_FlexibleSize > 0.0f) ? l_AxisSize : l_PreferredSize);
                var l_Position  = GetStartOffset(p_Axis, l_Size);

                SetChildAlongAxis(l_RectTransform, p_Axis, l_Position, l_Size);
            }
        }
    }
}
