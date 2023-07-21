using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CFLayout component
    /// </summary>
    public class DefaultCFLayout : Components.CFLayout
    {
        private RectTransform       m_RTransform;
        private LayoutElement       m_LElement;
        private EAxis               m_StartAxis                 = EAxis.Horizontal;
        private bool                m_ChildForceExpandWidth     = false;
        private bool                m_ChildForceExpandHeight    = false;
        private bool                m_ExpandHorizontalSpacing   = false;
        private float               m_SpacingX                  = 0.5f;
        private float               m_SpacingY                  = 0.5f;
        private bool                m_InvertOrder               = false;
        private float               m_LayoutWidth               = 0.0f;
        private float               m_LayoutHeight              = 0.0f;
        private List<RectTransform> m_ItemList                  = new List<RectTransform>();

        private bool IsCenterAlign => childAlignment == TextAnchor.LowerCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.UpperCenter;
        private bool IsRightAlign  => childAlignment == TextAnchor.LowerRight  || childAlignment == TextAnchor.MiddleRight  || childAlignment == TextAnchor.UpperRight;
        private bool IsMiddleAlign => childAlignment == TextAnchor.MiddleLeft  || childAlignment == TextAnchor.MiddleRight  || childAlignment == TextAnchor.MiddleCenter;
        private bool IsLowerAlign  => childAlignment == TextAnchor.LowerLeft   || childAlignment == TextAnchor.LowerRight   || childAlignment == TextAnchor.LowerCenter;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform RTransform => m_RTransform;
        public override LayoutElement LElement   => m_LElement;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override EAxis   StartAxis               { get => m_StartAxis;               set => SetProperty(ref m_StartAxis,                 value); }
        public override bool    ChildForceExpandWidth   { get => m_ChildForceExpandWidth;   set => SetProperty(ref m_ChildForceExpandWidth,     value); }
        public override bool    ChildForceExpandHeight  { get => m_ChildForceExpandHeight;  set => SetProperty(ref m_ChildForceExpandHeight,    value); }
        public override bool    ExpandHorizontalSpacing { get => m_ExpandHorizontalSpacing; set => SetProperty(ref m_ExpandHorizontalSpacing,   value); }
        public override float   SpacingX                { get => m_SpacingX;                set => SetProperty(ref m_SpacingX,                  value); }
        public override float   SpacingY                { get => m_SpacingY;                set => SetProperty(ref m_SpacingY,                  value); }
        public override bool    InvertOrder             { get => m_InvertOrder;             set => SetProperty(ref m_InvertOrder,               value); }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            gameObject.layer = UISystem.UILayer;

            m_RTransform = transform as RectTransform;
            m_RTransform.anchorMin = new Vector2(0f, 0f);
            m_RTransform.anchorMax = new Vector2(1f, 1f);
            m_RTransform.sizeDelta = new Vector2(0f, 0f);

            childAlignment = TextAnchor.MiddleCenter;

            m_LElement = gameObject.AddComponent<LayoutElement>();
            m_LElement.flexibleWidth  = 1f;
            m_LElement.flexibleHeight = 1f;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calculate the layout input for horizontal axis
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            if (StartAxis == EAxis.Horizontal)
            {
                base.CalculateLayoutInputHorizontal();

                var l_MinWidth = GetGreatestMinimumChildWidth() + padding.left + padding.right;
                SetLayoutInputForAxis(l_MinWidth, -1, -1, 0);
            }
            else
                m_LayoutWidth = SetLayout(0, true);
        }
        /// <summary>
        /// Calculate the layout input for vertical axis
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            if (StartAxis == EAxis.Horizontal)
                m_LayoutHeight = SetLayout(1, true);
            else
            {
                base.CalculateLayoutInputHorizontal();

                var l_MinHeight = GetGreatestMinimumChildHeigth() + padding.bottom + padding.top;
                SetLayoutInputForAxis(l_MinHeight, -1, -1, 1);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set layout horizontal
        /// </summary>
        public override void SetLayoutHorizontal() => SetLayout(0, false);
        /// <summary>
        /// Set layout vertical
        /// </summary>
        public override void SetLayoutVertical() => SetLayout(1, false);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get greatest minimum child width
        /// </summary>
        /// <returns></returns>
        private float GetGreatestMinimumChildWidth()
        {
            var l_Max = 0f;
            for (var l_I = 0; l_I < rectChildren.Count; ++l_I)
                l_Max = Mathf.Max(LayoutUtility.GetMinWidth(rectChildren[l_I]), l_Max);

            return l_Max;
        }
        /// <summary>
        /// Get greatest minimum child height
        /// </summary>
        /// <returns></returns>
        private float GetGreatestMinimumChildHeigth()
        {
            var l_Max = 0f;
            for (var l_I = 0; l_I < rectChildren.Count; ++l_I)
                l_Max = Mathf.Max(LayoutUtility.GetMinHeight(rectChildren[l_I]), l_Max);

            return l_Max;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Main layout method
        /// </summary>
        /// <param name="p_Axis">0 for horizontal axis, 1 for vertical</param>
        /// <param name="p_LayoutInput">If true, sets the layout input for the axis. If false, sets child position for axis</param>
        public float SetLayout(int p_Axis, bool p_LayoutInput)
        {
            var l_GroupHeight   = rectTransform.rect.height;
            var l_GroupWidth    = rectTransform.rect.width;

            float l_SpacingBetweenBars      = 0;
            float l_SpacingBetweenElements  = 0;
            float l_Offset                  = 0;
            float l_CounterOffset           = 0;
            float l_GroupSize               = 0;
            float l_WorkingSize             = 0;

            if (StartAxis == EAxis.Horizontal)
            {
                l_GroupSize                 = l_GroupHeight;
                l_WorkingSize               = l_GroupWidth - padding.left - padding.right;
                l_Offset                    = IsLowerAlign ? (float)padding.bottom : (float)padding.top;
                l_CounterOffset             = IsLowerAlign ? (float)padding.top    : (float)padding.bottom;
                l_SpacingBetweenBars        = SpacingY;
                l_SpacingBetweenElements    = SpacingX;
            }
            else if (StartAxis == EAxis.Vertical)
            {
                l_GroupSize                 = l_GroupWidth;
                l_WorkingSize               = l_GroupHeight - padding.top - padding.bottom;
                l_Offset                    = IsRightAlign ? (float)padding.right : (float)padding.left;
                l_CounterOffset             = IsRightAlign ? (float)padding.left  : (float)padding.right;
                l_SpacingBetweenBars        = SpacingX;
                l_SpacingBetweenElements    = SpacingY;
            }

            var l_CurrentBarSize  = 0f;
            var l_CurrentBarSpace = 0f;

            for (var l_I = 0; l_I < rectChildren.Count; ++l_I)
            {
                var     l_Index             = l_I;
                var     l_Child             = rectChildren[l_Index];
                float   l_ChildSize         = 0;
                float   l_ChildOtherSize    = 0;

                if (StartAxis == EAxis.Horizontal)
                {
                    if (InvertOrder)
                        l_Index = IsLowerAlign ? rectChildren.Count - 1 - l_I : l_I;

                    l_Child             = rectChildren[l_Index];
                    l_ChildSize         = LayoutUtility.GetPreferredSize(l_Child, 0);
                    l_ChildSize         = Mathf.Min(l_ChildSize, l_WorkingSize);
                    l_ChildOtherSize    = LayoutUtility.GetPreferredSize(l_Child, 1);
                    l_ChildOtherSize    = Mathf.Min(l_ChildOtherSize, l_WorkingSize);
                }
                else if (StartAxis == EAxis.Vertical)
                {
                    if (InvertOrder)
                        l_Index = IsRightAlign ? rectChildren.Count - 1 - l_I : l_I;

                    l_Child             = rectChildren[l_Index];
                    l_ChildSize         = LayoutUtility.GetPreferredSize(l_Child, 1);
                    l_ChildSize         = Mathf.Min(l_ChildSize, l_WorkingSize);
                    l_ChildOtherSize    = LayoutUtility.GetPreferredSize(l_Child, 0);
                    l_ChildOtherSize    = Mathf.Min(l_ChildOtherSize, l_WorkingSize);
                }

                /// If adding this element would exceed the bounds of the container, go to a new bar after processing the current bar
                if (l_CurrentBarSize + l_ChildSize > l_WorkingSize)
                {
                    l_CurrentBarSize -= l_SpacingBetweenElements;
                    if (!p_LayoutInput) /// Process current bar elements positioning
                    {
                        if (StartAxis == EAxis.Horizontal)
                        {
                            float newOffset = CalculateRowVerticalOffset(l_GroupSize, l_Offset, l_CurrentBarSpace);
                            LayoutRow(l_CurrentBarSize, l_CurrentBarSpace, l_WorkingSize, padding.left, newOffset, p_Axis);
                        }
                        else if (StartAxis == EAxis.Vertical)
                        {
                            float newOffset = CalculateColHorizontalOffset(l_GroupSize, l_Offset, l_CurrentBarSpace);
                            LayoutCol(l_CurrentBarSpace, l_CurrentBarSize, l_WorkingSize, newOffset, padding.top, p_Axis);
                        }
                    }

                    m_ItemList.Clear();

                    /// Add the current bar space to total barSpace accumulator, and reset to 0 for the next row
                    l_Offset += l_CurrentBarSpace;
                    l_Offset += l_SpacingBetweenBars;

                    l_CurrentBarSpace = 0;
                    l_CurrentBarSize  = 0;
                }

                l_CurrentBarSize += l_ChildSize;
                m_ItemList.Add(l_Child);

                /// We need the largest element height to determine the starting position of the next line
                if (l_ChildOtherSize > l_CurrentBarSpace)   l_CurrentBarSpace = l_ChildOtherSize;
                /// Don't do this for the last one
                if (l_I < rectChildren.Count - 1)           l_CurrentBarSize += l_SpacingBetweenElements;
            }

            if (!p_LayoutInput) /// Layout the final bar
            {
                if (StartAxis == EAxis.Horizontal)
                {
                    float l_NewOffset = CalculateRowVerticalOffset(l_GroupHeight, l_Offset, l_CurrentBarSpace);
                    l_CurrentBarSize -= l_SpacingBetweenElements;
                    LayoutRow(l_CurrentBarSize, l_CurrentBarSpace, l_WorkingSize - (ChildForceExpandWidth ? 0 : l_SpacingBetweenElements), padding.left, l_NewOffset, p_Axis);
                }
                else if (StartAxis == EAxis.Vertical)
                {
                    float l_NewOffset = CalculateColHorizontalOffset(l_GroupWidth, l_Offset, l_CurrentBarSpace);
                    l_CurrentBarSize -= l_SpacingBetweenElements;
                    LayoutCol(l_CurrentBarSpace, l_CurrentBarSize, l_WorkingSize - (ChildForceExpandHeight ? 0 : l_SpacingBetweenElements), l_NewOffset, padding.top, p_Axis);
                }
            }

            m_ItemList.Clear();

            /// Add the last bar space to the barSpace accumulator
            l_Offset += l_CurrentBarSpace;
            l_Offset += l_CounterOffset;

            if (p_LayoutInput) SetLayoutInputForAxis(l_Offset, l_Offset, -1, p_Axis);

            return l_Offset;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calculate row vertical offset
        /// </summary>
        /// <param name="p_GroupHeight">Group height</param>
        /// <param name="p_YOffset">Y offset</param>
        /// <param name="p_CurrentRowHeight">Current row height</param>
        /// <returns></returns>
        private float CalculateRowVerticalOffset(float p_GroupHeight, float p_YOffset, float p_CurrentRowHeight)
        {
                 if (IsLowerAlign)  return p_GroupHeight - p_YOffset - p_CurrentRowHeight;
            else if (IsMiddleAlign) return p_GroupHeight * 0.5f - m_LayoutHeight * 0.5f + p_YOffset;

            return p_YOffset;
        }
        /// <summary>
        /// Calculate column horizontal offset
        /// </summary>
        /// <param name="p_GroupWidth">Group width</param>
        /// <param name="p_XOffset">X Offset</param>
        /// <param name="p_CurrentColWidth">Current column width</param>
        /// <returns></returns>
        private float CalculateColHorizontalOffset(float p_GroupWidth, float p_XOffset, float p_CurrentColWidth)
        {
                 if (IsRightAlign)  return p_GroupWidth - p_XOffset - p_CurrentColWidth;
            else if (IsCenterAlign) return p_GroupWidth * 0.5f - m_LayoutWidth * 0.5f + p_XOffset;

            return p_XOffset;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Layout row
        /// </summary>
        /// <param name="p_RowWidth">Row width</param>
        /// <param name="p_RowHeight">Row height</param>
        /// <param name="p_MaxWidth">Max width</param>
        /// <param name="p_XOffset">X offset</param>
        /// <param name="p_YOffset">Y offset</param>
        /// <param name="p_Axis">Axis</param>
        private void LayoutRow(float p_RowWidth, float p_RowHeight, float p_MaxWidth, float p_XOffset, float p_YOffset, int p_Axis)
        {
            var l_XPos = p_XOffset;

                 if (!ChildForceExpandWidth && IsCenterAlign) l_XPos += (p_MaxWidth - p_RowWidth) * 0.5f;
            else if (!ChildForceExpandWidth && IsRightAlign)  l_XPos += (p_MaxWidth - p_RowWidth);

            var l_ExtraWidth   = 0f;
            var l_ExtraSpacing = 0f;

                 if (ChildForceExpandWidth) l_ExtraWidth = (p_MaxWidth - p_RowWidth) / m_ItemList.Count;
            else if (ExpandHorizontalSpacing)
            {
                l_ExtraSpacing = (p_MaxWidth - p_RowWidth) / (m_ItemList.Count - 1);
                if (m_ItemList.Count > 1)
                {
                         if (IsCenterAlign) l_XPos -= l_ExtraSpacing * 0.5f * (m_ItemList.Count - 1);
                    else if (IsRightAlign)  l_XPos -= l_ExtraSpacing * (m_ItemList.Count - 1);
                }
            }

            for (var l_J = 0; l_J < m_ItemList.Count; ++l_J)
            {
                var l_Index           = IsLowerAlign ? m_ItemList.Count - 1 - l_J : l_J;
                var l_RowChild        = m_ItemList[l_Index];
                var l_RowChildWidth   = LayoutUtility.GetPreferredSize(l_RowChild, 0) + l_ExtraWidth;
                var l_RowChildHeight  = LayoutUtility.GetPreferredSize(l_RowChild, 1);

                if (ChildForceExpandHeight) l_RowChildHeight = p_RowHeight;

                l_RowChildWidth = Mathf.Min(l_RowChildWidth, p_MaxWidth);

                var l_YPos = p_YOffset;

                     if (IsMiddleAlign) l_YPos += (p_RowHeight - l_RowChildHeight) * 0.5f;
                else if (IsLowerAlign)  l_YPos += (p_RowHeight - l_RowChildHeight);

                if (ExpandHorizontalSpacing && l_J > 0)
                    l_XPos += l_ExtraSpacing;

                if (p_Axis == 0) SetChildAlongAxis(l_RowChild, 0, l_XPos, l_RowChildWidth);
                else             SetChildAlongAxis(l_RowChild, 1, l_YPos, l_RowChildHeight);

                /// Don't do horizontal spacing for the last one
                if (l_J < m_ItemList.Count - 1) l_XPos += l_RowChildWidth + SpacingX;
            }
        }
        /// <summary>
        /// Layout column
        /// </summary>
        /// <param name="p_ColWidth">Column width</param>
        /// <param name="p_ColHeight">Column height</param>
        /// <param name="p_MaxHeight">Max height</param>
        /// <param name="p_XOffset">X offset</param>
        /// <param name="p_YOffset">Y offset</param>
        /// <param name="p_Axis">Axis</param>
        private void LayoutCol(float p_ColWidth, float p_ColHeight, float p_MaxHeight, float p_XOffset, float p_YOffset, int p_Axis)
        {
            var l_YPos = p_YOffset;

                 if (!ChildForceExpandHeight && IsMiddleAlign) l_YPos += (p_MaxHeight - p_ColHeight) * 0.5f;
            else if (!ChildForceExpandHeight && IsLowerAlign)  l_YPos += (p_MaxHeight - p_ColHeight);

            var l_ExtraHeight  = 0f;
            var l_ExtraSpacing = 0f;

                 if (ChildForceExpandHeight) l_ExtraHeight = (p_MaxHeight - p_ColHeight) / m_ItemList.Count;
            else if (ExpandHorizontalSpacing)
            {
                l_ExtraSpacing = (p_MaxHeight - p_ColHeight) / (m_ItemList.Count - 1);
                if (m_ItemList.Count > 1)
                {
                         if (IsMiddleAlign) l_YPos -= l_ExtraSpacing * 0.5f * (m_ItemList.Count - 1);
                    else if (IsLowerAlign)  l_YPos -= l_ExtraSpacing * (m_ItemList.Count - 1);
                }
            }

            for (var l_J = 0; l_J < m_ItemList.Count; ++l_J)
            {
                var l_Index             = IsRightAlign ? m_ItemList.Count - 1 - l_J : l_J;
                var l_RowChild          = m_ItemList[l_Index];
                var l_RowChildWidth     = LayoutUtility.GetPreferredSize(l_RowChild, 0);
                var l_RowChildHeight    = LayoutUtility.GetPreferredSize(l_RowChild, 1) + l_ExtraHeight;

                if (ChildForceExpandWidth)
                    l_RowChildWidth = p_ColWidth;

                l_RowChildHeight = Mathf.Min(l_RowChildHeight, p_MaxHeight);

                var l_XPos = p_XOffset;

                     if (IsCenterAlign) l_XPos += (p_ColWidth - l_RowChildWidth) * 0.5f;
                else if (IsRightAlign)  l_XPos += (p_ColWidth - l_RowChildWidth);

                if (ExpandHorizontalSpacing && l_J > 0)
                    l_YPos += l_ExtraSpacing;

                if (p_Axis == 0) SetChildAlongAxis(l_RowChild, 0, l_XPos, l_RowChildWidth);
                else             SetChildAlongAxis(l_RowChild, 1, l_YPos, l_RowChildHeight);

                /// Don't do vertical spacing for the last one
                if (l_J < m_ItemList.Count - 1) l_YPos += l_RowChildHeight + SpacingY;
            }
        }
    }
}
