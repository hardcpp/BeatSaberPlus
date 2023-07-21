using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CIconButton XUI Element
    /// </summary>
    public class XUIIconButton
        : IXUIElement, IXUIElementReady<XUIIconButton, UI.Components.CIconButton>, IXUIBindable<XUIIconButton>
    {
        private UI.Components.CIconButton m_Element = null;

        private event Action<UI.Components.CIconButton> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform               RTransform  => Element?.RTransform;
        public          UI.Components.CIconButton   Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIIconButton(string p_Name, Sprite p_Sprite, Action p_OnClick = null)
            : base(p_Name)
        {
            OnClick(p_OnClick);
            SetSprite(p_Sprite);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_OnClick">On click callback</param>
        public static XUIIconButton Make(Action p_OnClick = null)
            => new XUIIconButton("XUIIconButton", null, p_OnClick);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Sprite">Sprite</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUIIconButton Make(Sprite p_Sprite, Action p_OnClick = null)
            => new XUIIconButton("XUIIconButton", p_Sprite, p_OnClick);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Sprite">Sprite</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUIIconButton Make(string p_Name, Sprite p_Sprite, Action p_OnClick = null)
            => new XUIIconButton(p_Name, p_Sprite, p_OnClick);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.IconButtonFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIIconButton.BuildUI] Error OnReady:");
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
        public XUIIconButton OnReady(Action<UI.Components.CIconButton> p_Functor)
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
        public XUIIconButton Bind(ref XUIIconButton p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUIIconButton OnClick(Action p_Functor, bool p_Add = true) => OnReady((x) => x.OnClick(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIIconButton SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIIconButton SetWidth(float p_Width) => OnReady((x) => x.SetWidth(p_Width));
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIIconButton SetHeight(float p_Height) => OnReady((x) => x.SetHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set sprite color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public XUIIconButton SetColor(Color p_Color) => OnReady((x) => x.SetColor(p_Color));
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUIIconButton SetInteractable(bool p_Interactable) => OnReady((x) => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set button sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public XUIIconButton SetSprite(Sprite p_Sprite) => OnReady((x) => x.SetSprite(p_Sprite));
    }
}
