using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CTextSegmentedControl XUI Element
    /// </summary>
    public class XUITextSegmentedControl
        : IXUIElement, IXUIElementReady<XUITextSegmentedControl, UI.Components.CTextSegmentedControl>, IXUIBindable<XUITextSegmentedControl>
    {
        private UI.Components.CTextSegmentedControl m_Element = null;

        private event Action<UI.Components.CTextSegmentedControl> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform                       RTransform  => Element?.RTransform;
        public          UI.Components.CTextSegmentedControl Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUITextSegmentedControl(string p_Name, params string[] p_Texts)
            : base(p_Name)
        {
            SetTexts(p_Texts);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Texts">Texts</param>
        public static XUITextSegmentedControl Make(params string[] p_Texts)
            => new XUITextSegmentedControl("XUITextSegmentedControl", p_Texts);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Texts">Texts</param>
        public static XUITextSegmentedControl Make(string p_Name, params string[] p_Texts)
            => new XUITextSegmentedControl(p_Name, p_Texts);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.TextSegmentedControlFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUITextSegmentedControl.BuildUI] Error OnReady:");
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
        public XUITextSegmentedControl OnReady(Action<UI.Components.CTextSegmentedControl> p_Functor)
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
        public XUITextSegmentedControl Bind(ref XUITextSegmentedControl p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active text changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUITextSegmentedControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true) => OnReady(x => x.OnActiveChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUITextSegmentedControl SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set active text
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUITextSegmentedControl SetActiveText(int p_Index, bool p_Notify = true) => OnReady(x => x.SetActiveText(p_Index, p_Notify));
        /// <summary>
        /// Set texts
        /// </summary>
        /// <param name="p_Texts">New texts</param>
        /// <returns></returns>
        public XUITextSegmentedControl SetTexts(params string[] p_Texts) => OnReady(x => x.SetTexts(p_Texts));
    }
}
