using CP_SDK.Unity.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Horizontal or vertical layout base component
    /// </summary>
    public abstract class CHOrVLayout : MonoBehaviour
    {
        private Image m_Background = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public abstract RectTransform                   RTransform      { get; }
        public abstract ContentSizeFitter               CSizeFitter     { get; }
        public abstract LayoutElement                   LElement        { get; }
        public abstract HorizontalOrVerticalLayoutGroup HOrVLayoutGroup { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get background fill amount
        /// </summary>
        /// <returns></returns>
        public float GetBackgroundFillAmount()
            => m_Background ? m_Background.fillAmount : 0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set background state
        /// </summary>
        /// <param name="p_Enabled">Is enabled?</param>
        /// <param name="p_Color">Optional color, default to black</param>
        /// <param name="p_RaycastTarget">Should raycast target</param>
        /// <returns></returns>
        public CHOrVLayout SetBackground(bool p_Enabled, Color? p_Color = null, bool p_RaycastTarget = false)
        {
            if (p_Enabled)
            {
                if (!m_Background)
                {
                    m_Background = gameObject.AddComponent(UISystem.Override_UnityComponent_Image) as Image;
                    m_Background.material                = UISystem.Override_GetUIMaterial();
                    m_Background.raycastTarget           = p_RaycastTarget;
                }

                m_Background.pixelsPerUnitMultiplier = 1;
                m_Background.type                    = Image.Type.Sliced;
                m_Background.sprite                  = UISystem.GetUIRoundBGSprite();
                m_Background.color                   = p_Color.HasValue ? p_Color.Value : UISystem.DefaultBGColor;
            }
            else if (m_Background)
            {
                GameObject.Destroy(m_Background);
                m_Background = null;
            }

            return this;
        }
        /// <summary>
        /// Set background color
        /// </summary>
        /// <param name="p_Color">New background color</param>
        /// <returns></returns>
        public virtual CHOrVLayout SetBackgroundColor(Color p_Color)
        {
            if (!m_Background) return this;
            m_Background.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set background fill method
        /// </summary>
        /// <param name="p_FillMethod">Fill method</param>
        /// <returns></returns>
        public virtual CHOrVLayout SetBackgroundFillMethod(Image.FillMethod p_FillMethod)
        {
            if (!m_Background) return this;
            m_Background.fillMethod = p_FillMethod;
            return this;
        }
        /// <summary>
        /// Set background fill amount
        /// </summary>
        /// <param name="p_FillAmount">Fill amount</param>
        /// <returns></returns>
        public virtual CHOrVLayout SetBackgroundFillAmount(float p_FillAmount)
        {
            if (!m_Background) return this;
            m_Background.fillAmount = p_FillAmount;
            return this;
        }
        /// <summary>
        /// Set background sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <param name="p_Type">Image type</param>
        /// <returns></returns>
        public virtual CHOrVLayout SetBackgroundSprite(Sprite p_Sprite, Image.Type p_Type = Image.Type.Simple)
        {
            if (!m_Background) return this;
            m_Background.type   = p_Type;
            m_Background.sprite = p_Sprite;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public CHOrVLayout SetWidth(float p_Width)
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
        public CHOrVLayout SetHeight(float p_Height)
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
        public CHOrVLayout SetMinWidth(float p_Width)
        {
            LElement.minWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public CHOrVLayout SetMinHeight(float p_Height)
        {
            LElement.minHeight = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Padding">New padding</param>
        /// <returns></returns>
        public CHOrVLayout SetPadding(RectOffset p_Padding)
        {
            HOrVLayoutGroup.padding = p_Padding;
            return this;
        }
        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Top">Top padding</param>
        /// <param name="p_Right">Right padding</param>
        /// <param name="p_Bottom">Bottom padding</param>
        /// <param name="p_Left">Left padding</param>
        /// <returns></returns>
        public CHOrVLayout SetPadding(int p_Top, int p_Right, int p_Bottom, int p_Left)
        {
            SetPadding(new RectOffset(p_Left, p_Right, p_Top, p_Bottom));
            return this;
        }
        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Padding">New padding</param>
        /// <returns></returns>
        public CHOrVLayout SetPadding(int p_Padding)
        {
            HOrVLayoutGroup.padding = new RectOffset(p_Padding, p_Padding, p_Padding, p_Padding);
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public CHOrVLayout SetSpacing(float p_Spacing)
        {
            HOrVLayoutGroup.spacing = p_Spacing;
            return this;
        }
    }
}
