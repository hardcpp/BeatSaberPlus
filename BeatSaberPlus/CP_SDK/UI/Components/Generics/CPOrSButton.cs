using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Primary or Secondary button component
    /// </summary>
    public abstract class CPOrSButton : MonoBehaviour
    {
        public abstract RectTransform       RTransform          { get; }
        public abstract ContentSizeFitter   CSizeFitter         { get; }
        public abstract LayoutGroup         LayoutGroupC        { get; }
        public abstract LayoutElement       LElement            { get; }
        public abstract Button              ButtonC             { get; }
        public abstract Image               BackgroundImageC    { get; }
        public abstract Image               IconImageC          { get; }
        public abstract CText               TextC               { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public abstract CPOrSButton OnClick(Action p_Functor, bool p_Add = true);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get text
        /// </summary>
        /// <returns></returns>
        public string GetText()
        {
            return TextC.GetText();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public virtual CPOrSButton SetWidth(float p_Width)
        {
            RTransform.sizeDelta = new Vector2(p_Width, RTransform.sizeDelta.y);
            LElement.preferredWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public virtual CPOrSButton SetHeight(float p_Height)
        {
            RTransform.sizeDelta = new Vector2(RTransform.sizeDelta.x, p_Height);
            LElement.preferredHeight = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set background color
        /// </summary>
        /// <param name="p_Color">New background color</param>
        /// <returns></returns>
        public virtual CPOrSButton SetBackgroundColor(Color p_Color)
        {
            BackgroundImageC.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set background sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public virtual CPOrSButton SetBackgroundSprite(Sprite p_Sprite)
        {
            BackgroundImageC.sprite = p_Sprite;
            BackgroundImageC.gameObject.SetActive(p_Sprite);
            return this;
        }
        /// <summary>
        /// Set font size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public CPOrSButton SetFontSize(float p_Size)
        {
            TextC.SetFontSize(p_Size);
            return this;
        }
        /// <summary>
        /// Set theme color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public virtual CPOrSButton SetColor(Color p_Color)
        {
            BackgroundImageC.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set button icon sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public virtual CPOrSButton SetIconSprite(Sprite p_Sprite)
        {
            IconImageC.sprite = p_Sprite;
            IconImageC.gameObject.SetActive(p_Sprite);
            return this;
        }
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public virtual CPOrSButton SetInteractable(bool p_Interactable)
        {
            ButtonC.interactable = p_Interactable;
            return this;
        }
        /// <summary>
        /// Set overflow mode
        /// </summary>
        /// <param name="p_OverflowMode">New overflow mdoe</param>
        /// <returns></returns>
        public CPOrSButton SetOverflowMode(TextOverflowModes p_OverflowMode)
        {
            TextC.SetOverflowMode(p_OverflowMode);
            return this;
        }
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Text">New text</param>
        /// <returns></returns>
        public CPOrSButton SetText(string p_Text)
        {
            TextC.SetText(p_Text);
            return this;
        }
        /// <summary>
        /// Set tooltip
        /// </summary>
        /// <param name="p_Tooltip">New tooltip</param>
        /// <returns></returns>
        public abstract CPOrSButton SetTooltip(string p_Tooltip);
    }
}
