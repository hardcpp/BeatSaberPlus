using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CColorInput XUI Element
    /// </summary>
    public class XUIColorInput
        : IXUIElement, IXUIElementReady<XUIColorInput, UI.Components.CColorInput>, IXUIBindable<XUIColorInput>
    {
        private UI.Components.CColorInput m_Element = null;

        private event Action<UI.Components.CColorInput> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform               RTransform  => Element?.RTransform;
        public          UI.Components.CColorInput   Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIColorInput(string p_Name)
            : base(p_Name)
        { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIColorInput Make()
            => new XUIColorInput("XUIColorInput");
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIColorInput Make(string p_Name)
            => new XUIColorInput(p_Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.ColorInputFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIColorInput.BuildUI] Error OnReady:");
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
        public XUIColorInput OnReady(Action<UI.Components.CColorInput> p_Functor)
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
        public XUIColorInput Bind(ref XUIColorInput p_Target)
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
        public XUIColorInput OnValueChanged(Action<Color> p_Functor, bool p_Add = true) => OnReady(x => x.OnValueChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIColorInput SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set alpha support
        /// </summary>
        /// <param name="p_Support">New state</param>
        /// <returns></returns>
        public XUIColorInput SetAlphaSupport(bool p_Support) => OnReady(x => x.SetAlphaSupport(p_Support));
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUIColorInput SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUIColorInput SetValue(Color p_Value, bool p_Notify = true) => OnReady(x => x.SetValue(p_Value, p_Notify));
    }
}
