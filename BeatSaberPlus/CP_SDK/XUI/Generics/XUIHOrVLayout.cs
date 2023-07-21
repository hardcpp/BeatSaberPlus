using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CHOrVLayout abstract XUI Element
    /// </summary>
    /// <typeparam name="t_Base">Base type</typeparam>
    /// <typeparam name="t_Component">Component type</typeparam>
    public abstract class _XUIHOrVLayout<t_Base, t_Component>
        : IXUIElementWithChilds<t_Base>, IXUIElementReady<t_Base, t_Component>, IXUIBindable<t_Base>
        where t_Component : UI.Components.CHOrVLayout
        where t_Base      : IXUIElement
    {
        private UI.Components.CHOrVLayout m_Element = null;

        private event Action<t_Component> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform   RTransform  => Element?.RTransform;
        public          t_Component     Element     => m_Element as t_Component;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected _XUIHOrVLayout(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = typeof(t_Component) == typeof(UI.Components.CHLayout)
                        ?
                            UI.UISystem.HLayoutFactory.Create(m_InitialName, p_Parent) as UI.Components.CHOrVLayout
                        :
                            UI.UISystem.VLayoutFactory.Create(m_InitialName, p_Parent) as UI.Components.CHOrVLayout;

            m_Element.SetSpacing(2.0f);
            m_Element.SetPadding(new RectOffset(2, 2, 2, 2));

            BuildUIChilds(m_Element.transform);

            try { m_OnReady?.Invoke(m_Element as t_Component); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][_XUIHOrVLayout<{typeof(t_Component).Name}>.BuildUI] Error OnReady:");
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
            if (m_Element)    p_Functor?.Invoke(m_Element as t_Component);
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
        /// Set background state
        /// </summary>
        /// <param name="p_Enabled">Is enabled?</param>
        /// <param name="p_Color">Optional color, default to black</param>
        /// <param name="p_RaycastTarget">Should raycast target</param>
        /// <returns></returns>
        public t_Base SetBackground(bool p_Enabled, Color? p_Color = null, bool p_RaycastTarget = false) => OnReady((x) => x.SetBackground(p_Enabled, p_Color, p_RaycastTarget));
        /// <summary>
        /// Set background color
        /// </summary>
        /// <param name="p_Color">New background color</param>
        /// <returns></returns>
        public t_Base SetBackgroundColor(Color p_Color) => OnReady((x) => x.SetBackgroundColor(p_Color));

        /// <summary>
        /// Set background fill method
        /// </summary>
        /// <param name="p_FillMethod">Fill method</param>
        /// <returns></returns>
        public t_Base SetBackgroundFillMethod(Image.FillMethod p_FillMethod) => OnReady(x => x.SetBackgroundFillMethod(p_FillMethod));
        /// <summary>
        /// Set background fill amount
        /// </summary>
        /// <param name="p_FillAmount">Fill amount</param>
        /// <returns></returns>
        public t_Base SetBackgroundFillAmount(float p_FillAmount) => OnReady(x => x.SetBackgroundFillAmount(p_FillAmount));
        /// <summary>
        /// Set background sprite
        /// </summary>
        /// <param name="p_Sprite">New sprite</param>
        /// <param name="p_Type">Image type</param>
        /// <returns></returns>
        public t_Base SetBackgroundSprite(Sprite p_Sprite, Image.Type p_Type = Image.Type.Simple) => OnReady((x) => x.SetBackgroundSprite(p_Sprite, p_Type));

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
        /// Set min width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public t_Base SetMinWidth(float p_Width) => OnReady((x) => x.SetMinWidth(p_Width));
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public t_Base SetMinHeight(float p_Height) => OnReady((x) => x.SetMinHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Padding">New padding</param>
        /// <returns></returns>
        public t_Base SetPadding(RectOffset p_Padding) => OnReady((x) => x.SetPadding(p_Padding));
        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Top">Top padding</param>
        /// <param name="p_Right">Right padding</param>
        /// <param name="p_Bottom">Bottom padding</param>
        /// <param name="p_Left">Left padding</param>
        /// <returns></returns>
        public t_Base SetPadding(int p_Top, int p_Right, int p_Bottom, int p_Left) => OnReady((x) => x.SetPadding(p_Top, p_Right, p_Bottom, p_Left));
        /// <summary>
        /// Set padding
        /// </summary>
        /// <param name="p_Padding">New padding</param>
        /// <returns></returns>
        public t_Base SetPadding(int p_Padding) => OnReady((x) => x.SetPadding(p_Padding));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public t_Base SetSpacing(float p_Spacing) => OnReady((x) => x.SetSpacing(p_Spacing));
    }
}
