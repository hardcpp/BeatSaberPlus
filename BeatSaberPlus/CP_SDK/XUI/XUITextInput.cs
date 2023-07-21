using System;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CTextInput XUI Element
    /// </summary>
    public class XUITextInput
        : IXUIElement, IXUIElementReady<XUITextInput, UI.Components.CTextInput>, IXUIBindable<XUITextInput>
    {
        private UI.Components.CTextInput m_Element = null;

        private event Action<UI.Components.CTextInput> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform               RTransform  => Element?.RTransform;
        public          UI.Components.CTextInput    Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUITextInput(string p_Name, string p_PlaceHolder)
            : base(p_Name)
        {
            SetPlaceHolder(p_PlaceHolder);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_PlaceHolder">Placeholder text</param>
        public static XUITextInput Make(string p_PlaceHolder)
            => new XUITextInput("XUITextInput", p_PlaceHolder);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_PlaceHolder">Placeholder text</param>
        public static XUITextInput Make(string p_Name, string p_PlaceHolder)
            => new XUITextInput(p_Name, p_PlaceHolder);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.TextInputFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUITextInput.BuildUI] Error OnReady:");
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
        public XUITextInput OnReady(Action<UI.Components.CTextInput> p_Functor)
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
        public XUITextInput Bind(ref XUITextInput p_Target)
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
        public XUITextInput OnValueChanged(Action<string> p_Functor, bool p_Add = true) => OnReady(x => x.OnValueChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUITextInput SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set button interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUITextInput SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set is password
        /// </summary>
        /// <param name="p_IsPassword">Is password?</param>
        /// <returns></returns>
        public XUITextInput SetIsPassword(bool p_IsPassword) => OnReady(x => x.SetIsPassword(p_IsPassword));
        /// <summary>
        /// Set place holder
        /// </summary>
        /// <param name="p_PlaceHolder">New place holder</param>
        /// <returns></returns>
        public XUITextInput SetPlaceHolder(string p_PlaceHolder) => OnReady(x => x.SetPlaceHolder(p_PlaceHolder));
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUITextInput SetValue(string p_Value, bool p_Notify = true) => OnReady(x => x.SetValue(p_Value, p_Notify));
    }
}
