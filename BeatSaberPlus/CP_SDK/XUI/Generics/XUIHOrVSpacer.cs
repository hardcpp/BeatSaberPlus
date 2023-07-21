using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// IHOrVLayout abstract XUI Element
    /// </summary>
    /// <typeparam name="t_Base">Base type</typeparam>
    /// <typeparam name="t_Component">Component type</typeparam>
    public abstract class _XUIHOrVSpacer<t_Base, t_Component>
        : IXUIElement, IXUIElementReady<t_Base, t_Component>, IXUIBindable<t_Base>
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

        protected _XUIHOrVSpacer(string p_Name, float p_Spacing) : base(p_Name) => SetSpacing(p_Spacing);

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

            try { m_OnReady?.Invoke(m_Element as t_Component); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][_XUIHOrVSpacer<{typeof(t_Component).Name}>.BuildUI] Error OnReady:");
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
        /// Set spacing
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public t_Base SetSpacing(float p_Spacing)
        {
            return OnReady((x) =>
            {
                if (m_Element is UI.Components.CHLayout) { x.SetMinWidth(p_Spacing);  x.SetWidth(p_Spacing);  }
                else                                     { x.SetMinHeight(p_Spacing); x.SetHeight(p_Spacing); }
            });
        }
    }
}
