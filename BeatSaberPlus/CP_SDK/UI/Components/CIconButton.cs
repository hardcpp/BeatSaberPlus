using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Icon button component
    /// </summary>
    public abstract class CIconButton : MonoBehaviour
    {
        public abstract RectTransform   RTransform  { get; }
        public abstract LayoutElement   LElement    { get; }
        public abstract Button          ButtonC     { get; }
        public abstract Image           IconImageC  { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CIconButton OnClick(Action p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public virtual CIconButton SetWidth(float p_Width)
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
        public virtual CIconButton SetHeight(float p_Height)
        {
            RTransform.sizeDelta        = new Vector2(RTransform.sizeDelta.x, p_Height);
            LElement.preferredHeight    = p_Height;
            return this;
        }
        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set sprite color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public virtual CIconButton SetColor(Color p_Color)
        {
            IconImageC.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public virtual CIconButton SetInteractable(bool p_Interactable)
        {
            ButtonC.interactable = p_Interactable;
            return this;
        }
        /// <summary>
        /// Set button sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public virtual CIconButton SetSprite(Sprite p_Sprite)
        {
            IconImageC.sprite = p_Sprite;
            return this;
        }
    }
}
