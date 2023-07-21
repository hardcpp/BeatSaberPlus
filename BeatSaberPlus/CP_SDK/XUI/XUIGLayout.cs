using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CGLayout XUI Element
    /// </summary>
    public class XUIGLayout
        : IXUIElementWithChilds<XUIGLayout>, IXUIElementReady<XUIGLayout, UI.Components.CGLayout>, IXUIBindable<XUIGLayout>
    {
        private UI.Components.CGLayout m_Element = null;

        private event Action<UI.Components.CGLayout> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => Element?.RTransform;
        public          UI.Components.CGLayout  Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIGLayout(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIGLayout Make(params IXUIElement[] p_Childs)
            => new XUIGLayout("XUIGLayout", p_Childs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIGLayout Make(string p_Name, params IXUIElement[] p_Childs)
            => new XUIGLayout(p_Name, p_Childs);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element =  UI.UISystem.GLayoutFactory.Create(m_InitialName, p_Parent);

            BuildUIChilds(m_Element.transform);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIGLayout.BuildUI] Error OnReady:");
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
        public XUIGLayout OnReady(Action<UI.Components.CGLayout> p_Functor)
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
        public XUIGLayout Bind(ref XUIGLayout p_Target)
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
        public XUIGLayout SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set preferred width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIGLayout SetWidth(float p_Width) => OnReady((x) => x.SetWidth(p_Width));
        /// <summary>
        /// Set preferred height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIGLayout SetHeight(float p_Height) => OnReady((x) => x.SetHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set min width
        /// </summary>
        /// <param name="p_Width">Width</param>
        /// <returns></returns>
        public XUIGLayout SetMinWidth(float p_Width) => OnReady((x) => x.SetMinWidth(p_Width));
        /// <summary>
        /// Set min height
        /// </summary>
        /// <param name="p_Height">Height</param>
        /// <returns></returns>
        public XUIGLayout SetMinHeight(float p_Height) => OnReady((x) => x.SetMinHeight(p_Height));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set cell size
        /// </summary>
        /// <param name="p_CellSize">New size</param>
        /// <returns></returns>
        public XUIGLayout SetCellSize(Vector2 p_CellSize) => OnReady(x => x.SetCellSize(p_CellSize));
        /// <summary>
        /// Set child alignment
        /// </summary>
        /// <param name="p_ChildAlign">New alignment</param>
        /// <returns></returns>
        public XUIGLayout SetChildAlign(TextAnchor p_ChildAlign) => OnReady(x => x.SetChildAlign(p_ChildAlign));
        /// <summary>
        /// Set layout constraint
        /// </summary>
        /// <param name="p_Constraint">New value</param>
        /// <returns></returns>
        public XUIGLayout SetConstraint(GridLayoutGroup.Constraint p_Constraint) => OnReady(x => x.SetConstraint(p_Constraint));
        /// <summary>
        /// Set layout constraint count
        /// </summary>
        /// <param name="p_ConstraintCount">New value</param>
        /// <returns></returns>
        public XUIGLayout SetConstraintCount(int p_ConstraintCount) => OnReady(x => x.SetConstraintCount(p_ConstraintCount));
        /// <summary>
        /// Set spacing between elements
        /// </summary>
        /// <param name="p_Spacing">New spacing</param>
        /// <returns></returns>
        public XUIGLayout SetSpacing(Vector2 p_Spacing) => OnReady(x => x.SetSpacing(p_Spacing));
    }
}
