using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Grid layout group
    /// </summary>
    public abstract class CGLayout : MonoBehaviour
    {
        public abstract RectTransform       RTransform      { get; }
        public abstract ContentSizeFitter   CSizeFitter     { get; }
        public abstract LayoutElement       LElement        { get; }
        public abstract GridLayoutGroup     GLayoutGroup    { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public virtual CGLayout SetWidth(float p_Width)
        {
            RTransform.sizeDelta    = new Vector2(p_Width, RTransform.sizeDelta.y);
            LElement.preferredWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public virtual CGLayout SetHeight(float p_Height)
        {
            RTransform.sizeDelta        = new Vector2(RTransform.sizeDelta.x, p_Height);
            LElement.preferredHeight    = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set min width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public virtual CGLayout SetMinWidth(float p_Width)
        {
            LElement.minWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public virtual CGLayout SetMinHeight(float p_Height)
        {
            LElement.minHeight = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set cell size
        /// </summary>
        /// <param name="p_CellSize">New size</param>
        /// <returns></returns>
        public virtual CGLayout SetCellSize(Vector2 p_CellSize)
        {
            GLayoutGroup.cellSize = p_CellSize;
            return this;
        }
        /// <summary>
        /// Set child alignment
        /// </summary>
        /// <param name="p_ChildAlign">New alignment</param>
        /// <returns></returns>
        public virtual CGLayout SetChildAlign(TextAnchor p_ChildAlign)
        {
            GLayoutGroup.childAlignment = p_ChildAlign;
            return this;
        }
        /// <summary>
        /// Set layout constraint
        /// </summary>
        /// <param name="p_Constraint">New value</param>
        /// <returns></returns>
        public virtual CGLayout SetConstraint(GridLayoutGroup.Constraint p_Constraint)
        {
            GLayoutGroup.constraint = p_Constraint;
            return this;
        }
        /// <summary>
        /// Set layout constraint count
        /// </summary>
        /// <param name="p_ConstraintCount">New value</param>
        /// <returns></returns>
        public virtual CGLayout SetConstraintCount(int p_ConstraintCount)
        {
            GLayoutGroup.constraintCount = p_ConstraintCount;
            return this;
        }
        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public virtual CGLayout SetSpacing(Vector2 p_Spacing)
        {
            GLayoutGroup.spacing = p_Spacing;
            return this;
        }
    }
}
