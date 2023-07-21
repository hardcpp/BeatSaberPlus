using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Components
{
    /// <summary>
    /// Image component
    /// </summary>
    public abstract class CImage : MonoBehaviour
    {
        public abstract RectTransform   RTransform  { get; }
        public abstract LayoutElement   LElement    { get; }
        public abstract Image           ImageC      { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public CImage SetWidth(float p_Width)
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
        public CImage SetHeight(float p_Height)
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
        public CImage SetMinWidth(float p_Width)
        {
            LElement.minWidth = p_Width;
            return this;
        }
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public CImage SetMinHeight(float p_Height)
        {
            LElement.minHeight = p_Height;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public virtual CImage SetColor(Color p_Color)
        {
            ImageC.color = p_Color;
            return this;
        }
        /// <summary>
        /// Set enhanced image
        /// </summary>
        /// <param name="p_EnhancedImage">New enhanced image</param>
        /// <returns></returns>
        public virtual CImage SetEnhancedImage(Unity.EnhancedImage p_EnhancedImage)
        {
            var l_Updater = GetComponent<Animation.AnimationStateUpdater>();
            if (!l_Updater)
                l_Updater = gameObject.AddComponent<Animation.AnimationStateUpdater>();

            if (l_Updater && p_EnhancedImage == null)
                GameObject.Destroy(l_Updater);
            else
            {
                l_Updater.TargetImage               = ImageC;
                l_Updater.ControllerDataInstance    = p_EnhancedImage.AnimControllerData;
            }

            return this;
        }
        /// <summary>
        /// Set pixels per unit multiplier
        /// </summary>
        /// <param name="p_Multiplier">New multiplier</param>
        /// <returns></returns>
        public virtual CImage SetPixelsPerUnitMultiplier(float p_Multiplier)
        {
            ImageC.pixelsPerUnitMultiplier = p_Multiplier;
            return this;
        }
        /// <summary>
        /// Set sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public virtual CImage SetSprite(Sprite p_Sprite)
        {
            ImageC.sprite = p_Sprite;
            return this;
        }
        /// <summary>
        /// Set type
        /// </summary>
        /// <param name="p_Type">New type</param>
        /// <returns></returns>
        public virtual CImage SetType(Image.Type p_Type)
        {
            ImageC.type = p_Type;
            return this;
        }
    }
}
