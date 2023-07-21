using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CFLayout XUI Element
    /// </summary>
    public class XUIFLayout
        : IXUIElementWithChilds<XUIFLayout>, IXUIElementReady<XUIFLayout, UI.Components.CFLayout>, IXUIBindable<XUIFLayout>
    {
        private UI.Components.CFLayout m_Element = null;

        private event Action<UI.Components.CFLayout> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => Element?.RTransform;
        public          UI.Components.CFLayout  Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIFLayout(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIFLayout Make(params IXUIElement[] p_Childs)
            => new XUIFLayout("XUIFLayout", p_Childs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIFLayout Make(string p_Name, params IXUIElement[] p_Childs)
            => new XUIFLayout(p_Name, p_Childs);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element =  UI.UISystem.FLayoutFactory.Create(m_InitialName, p_Parent);

            BuildUIChilds(m_Element.transform);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIFLayout.BuildUI] Error OnReady:");
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
        public XUIFLayout OnReady(Action<UI.Components.CFLayout> p_Functor)
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
        public XUIFLayout Bind(ref XUIFLayout p_Target)
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
        public XUIFLayout SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIFLayout SetWidth(float p_Width) => OnReady((x) => x.SetWidth(p_Width));
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIFLayout SetHeight(float p_Height) => OnReady((x) => x.SetHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set min width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIFLayout SetMinWidth(float p_Width) => OnReady((x) => x.SetMinWidth(p_Width));
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIFLayout SetMinHeight(float p_Height) => OnReady((x) => x.SetMinHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set child alignment
        /// </summary>
        /// <param name="p_ChildAlign">New alignment</param>
        /// <returns></returns>
        public XUIFLayout SetChildAlign(TextAnchor p_ChildAlign) => OnReady(x => x.SetChildAlign(p_ChildAlign));
        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public XUIFLayout SetSpacing(Vector2 p_Spacing) => OnReady(x => x.SetSpacing(p_Spacing));
    }
}
