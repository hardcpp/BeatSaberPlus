using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CVScrollView XUI Element
    /// </summary>
    public class XUIVScrollView
        : IXUIElementWithChilds<XUIVScrollView>, IXUIElementReady<XUIVScrollView, UI.Components.CVScrollView>, IXUIBindable<XUIVScrollView>
    {
        private UI.Components.CVScrollView m_Element = null;

        private event Action<UI.Components.CVScrollView> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform               RTransform  => Element?.RTransform;
        public          UI.Components.CVScrollView  Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIVScrollView(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIVScrollView Make(params IXUIElement[] p_Childs)
            => new XUIVScrollView("XUIVScrollView", p_Childs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIVScrollView Make(string p_Name, params IXUIElement[] p_Childs)
            => new XUIVScrollView(p_Name, p_Childs);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.VScrollViewFactory.Create(m_InitialName, p_Parent);

            BuildUIChilds(m_Element.Container);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIVScrollView.BuildUI] Error OnReady:");
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
        public XUIVScrollView OnReady(Action<UI.Components.CVScrollView> p_Functor)
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
        public XUIVScrollView Bind(ref XUIVScrollView p_Target)
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
        public XUIVScrollView SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
    }
}
