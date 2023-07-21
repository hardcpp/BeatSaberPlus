using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Flow layout group
    /// </summary>
    public abstract class CFLayout : LayoutGroup
    {
        public abstract RectTransform RTransform { get; }
        public abstract LayoutElement LElement   { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public enum EAxis
        {
            Horizontal,
            Vertical
        }

        public abstract EAxis   StartAxis               { get; set; }
        public abstract bool    ChildForceExpandWidth   { get; set; }
        public abstract bool    ChildForceExpandHeight  { get; set; }
        public abstract bool    ExpandHorizontalSpacing { get; set; }
        public abstract float   SpacingX                { get; set; }
        public abstract float   SpacingY                { get; set; }
        public abstract bool    InvertOrder             { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public virtual CFLayout SetWidth(float p_Width)
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
        public virtual CFLayout SetHeight(float p_Height)
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
        public virtual CFLayout SetMinWidth(float p_Width)
        {
            LElement.minWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public virtual CFLayout SetMinHeight(float p_Height)
        {
            LElement.minHeight = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set child alignment
        /// </summary>
        /// <param name="p_ChildAlign">New alignment</param>
        /// <returns></returns>
        public virtual CFLayout SetChildAlign(TextAnchor p_ChildAlign)
        {
            childAlignment = p_ChildAlign;
            return this;
        }
        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public virtual CFLayout SetSpacing(Vector2 p_Spacing)
        {
            SpacingX = p_Spacing.x;
            SpacingY = p_Spacing.y;
            return this;
        }
    }
}
