using System;
using TMPro;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CPOrSButton abstract XUI Element
    /// </summary>
    /// <typeparam name="t_Base">Base type</typeparam>
    /// <typeparam name="t_Component">Component type</typeparam>
    public abstract class _XUIPOrSButton<t_Base, t_Component>
        : IXUIElementWithChilds<t_Base>, IXUIElementReady<t_Base, t_Component>, IXUIBindable<t_Base>
        where t_Component   : UI.Components.CPOrSButton
        where t_Base        : IXUIElement
    {
        private UI.Components.CPOrSButton m_Element = null;

        private event Action<t_Component> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform  => Element?.RTransform;
        public          t_Component     Element     => m_Element as t_Component;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected _XUIPOrSButton(string p_Name, string p_Label, Action p_OnClick)
            : base(p_Name)
        {
            if (p_OnClick != null)
                OnClick(p_OnClick);
            SetText(p_Label);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element =  typeof(t_Component) == typeof(UI.Components.CPrimaryButton)
                        ?
                            UI.UISystem.PrimaryButtonFactory.Create(m_InitialName, p_Parent) as UI.Components.CPOrSButton
                        :
                            UI.UISystem.SecondaryButtonFactory.Create(m_InitialName, p_Parent) as UI.Components.CPOrSButton;

            try { m_OnReady?.Invoke(m_Element as t_Component); }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][_XUIPOrSButton<{typeof(t_Component).Name}>.BuildUI] Error OnReady:");
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
        public t_Base OnReady(Action<t_Component> p_Functor)
        {
            if (m_Element)     p_Functor?.Invoke(m_Element as t_Component);
            else m_OnReady += p_Functor;
            return this as t_Base;
        }
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        public t_Base Bind(ref t_Base p_Target)
        {
            p_Target = this as t_Base;
            return this as t_Base;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public t_Base SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public t_Base SetWidth(float p_Width) => OnReady((x) => x.SetWidth(p_Width));
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public t_Base SetHeight(float p_Height) => OnReady((x) => x.SetHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On click event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public t_Base OnClick(Action p_Functor, bool p_Add = true) => OnReady((x) => x.OnClick(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set button background sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public t_Base SetBackgroundSprite(Sprite p_Sprite) => OnReady(x => x.SetBackgroundSprite(p_Sprite));
        /// <summary>
        /// Set font size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public t_Base SetFontSize(float p_Size) => OnReady((x) => x.SetFontSize(p_Size));
        /// <summary>
        /// Set button icon sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <returns></returns>
        public t_Base SetIconSprite(Sprite p_Sprite) => OnReady(x => x.SetIconSprite(p_Sprite));
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public t_Base SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set overflow mode
        /// </summary>
        /// <param name="p_OverflowMode">New overflow mdoe</param>
        /// <returns></returns>
        public t_Base SetOverflowMode(TextOverflowModes p_OverflowMode) => OnReady((x) => x.SetOverflowMode(p_OverflowMode));
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Text">New text</param>
        /// <returns></returns>
        public t_Base SetText(string p_Text) => OnReady((x) => x.SetText(p_Text));
        /// <summary>
        /// Set tooltip
        /// </summary>
        /// <param name="p_Tooltip">New tooltip</param>
        /// <returns></returns>
        public t_Base SetTooltip(string p_Tooltip) => OnReady((x) => x.SetTooltip(p_Tooltip));
    }
}
