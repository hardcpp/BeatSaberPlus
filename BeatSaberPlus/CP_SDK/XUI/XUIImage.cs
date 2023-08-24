using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CImage XUI Element
    /// </summary>
    public class XUIImage
        : IXUIElement, IXUIElementReady<XUIImage, UI.Components.CImage>, IXUIBindable<XUIImage>
    {
        private UI.Components.CImage m_Element = null;

        private event Action<UI.Components.CImage> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform        RTransform  => Element?.RTransform;
        public          UI.Components.CImage Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIImage(string p_Name, Sprite p_Sprite)
            : base(p_Name)
        {
            if (p_Sprite)
                SetSprite(p_Sprite);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Sprite">Sprite</param>
        public static XUIImage Make(Sprite p_Sprite = null)
            => new XUIImage("XUIImage", p_Sprite);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Sprite">Sprite</param>
        public static XUIImage Make(string p_Name, Sprite p_Sprite = null)
            => new XUIImage(p_Name, p_Sprite);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.ImageFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIImage.BuildUI] Error OnReady:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On ready, append callback functor
        /// </summary>
        /// <param name="p_Functor">Functor to add</param>
        /// <returns></returns>
        public XUIImage OnReady(Action<UI.Components.CImage> p_Functor)
        {
            if (m_Element)    p_Functor?.Invoke(m_Element);
            else m_OnReady += p_Functor;
            return this;
        }
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        public XUIImage Bind(ref XUIImage p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIImage SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIImage SetWidth(float p_Width) => OnReady(x => x.SetWidth(p_Width));
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIImage SetHeight(float p_Height) => OnReady(x => x.SetHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set min width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIImage SetMinWidth(float p_Width) => OnReady(x => x.SetMinWidth(p_Width));
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIImage SetMinHeight(float p_Height) => OnReady(x => x.SetMinHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public XUIImage SetColor(Color p_Color) => OnReady(x => x.SetColor(p_Color));
        /// <summary>
        /// Set enhanced image
        /// </summary>
        /// <param name="p_EnhancedImage">New enhanced image</param>
        /// <returns></returns>
        public XUIImage SetEnhancedImage(Unity.EnhancedImage p_EnhancedImage) => OnReady(x => x.SetEnhancedImage(p_EnhancedImage));
        /// <summary>
        /// Set pixels per unit multiplier
        /// </summary>
        /// <param name="p_Multiplier">New multiplier</param>
        /// <returns></returns>
        public XUIImage SetPixelsPerUnitMultiplier(float p_Multiplier) => OnReady(x => x.SetPixelsPerUnitMultiplier(p_Multiplier));
        /// <summary>
        /// Set sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public XUIImage SetSprite(Sprite p_Sprite) => OnReady(x => x.SetSprite(p_Sprite));
        /// <summary>
        /// Set type
        /// </summary>
        /// <param name="p_Type">New type</param>
        /// <returns></returns>
        public XUIImage SetType(Image.Type p_Type) => OnReady(x => x.SetType(p_Type));
    }
}
