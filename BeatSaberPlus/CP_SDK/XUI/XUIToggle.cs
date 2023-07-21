using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CToggle XUI Element
    /// </summary>
    public class XUIToggle
        : IXUIElement, IXUIElementReady<XUIToggle, UI.Components.CToggle>, IXUIBindable<XUIToggle>
    {
        private UI.Components.CToggle m_Toggle = null;

        private event Action<UI.Components.CToggle> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => Element?.RTransform;
        public          UI.Components.CToggle   Element     => m_Toggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIToggle(string p_Name) : base(p_Name) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public static XUIToggle Make()
            => new XUIToggle("XUIToggle");
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        public static XUIToggle Make(string p_Name)
            => new XUIToggle(p_Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Toggle = UI.UISystem.ToggleFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Toggle); }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIToggle.BuildUI] Error OnReady:");
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
        public XUIToggle OnReady(Action<UI.Components.CToggle> p_Functor)
        {
            if (m_Toggle)     p_Functor?.Invoke(m_Toggle);
            else m_OnReady += p_Functor;
            return this;
        }
        /// <summary>
        /// On ready, bind
        /// </summary>
        /// <param name="p_Target">Bind target</param>
        /// <returns></returns>
        public XUIToggle Bind(ref XUIToggle p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On value changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUIToggle OnValueChanged(Action<bool> p_Functor, bool p_Add = true) => OnReady((x) => x.OnValueChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIToggle SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUIToggle SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Notify">Notify?</param>
        /// <returns></returns>
        public XUIToggle SetValue(bool p_Value, bool p_Notify = true) => OnReady((x) => x.SetValue(p_Value, p_Notify));
    }
}
