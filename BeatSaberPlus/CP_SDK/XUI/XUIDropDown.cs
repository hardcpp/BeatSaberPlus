using System;
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CDropdown XUI Element
    /// </summary>
    public class XUIDropdown
        : IXUIElement, IXUIElementReady<XUIDropdown, UI.Components.CDropdown>, IXUIBindable<XUIDropdown>
    {
        private UI.Components.CDropdown m_Element = null;

        private event Action<UI.Components.CDropdown> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform           RTransform  => Element?.RTransform;
        public          UI.Components.CDropdown Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIDropdown(string p_Name, List<string> p_Options = null)
            : base(p_Name)
        {
            SetOptions(p_Options);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Options">Options</param>
        public static XUIDropdown Make(List<string> p_Options = null)
            => new XUIDropdown("XUIDropdown", p_Options);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Options">Options</param>
        public static XUIDropdown Make(string p_Name, List<string> p_Options = null)
            => new XUIDropdown(p_Name, p_Options);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.DropdownFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIDropdown.BuildUI] Error OnReady:");
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
        public XUIDropdown OnReady(Action<UI.Components.CDropdown> p_Functor)
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
        public XUIDropdown Bind(ref XUIDropdown p_Target)
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
        public XUIDropdown OnValueChanged(Action<int, string> p_Functor, bool p_Add = true)
        {
            OnReady((x) => x.OnValueChanged(p_Functor, p_Add));
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUIDropdown SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set interactable state
        /// </summary>
        /// <param name="p_Interactable">New state</param>
        /// <returns></returns>
        public XUIDropdown SetInteractable(bool p_Interactable) => OnReady(x => x.SetInteractable(p_Interactable));
        /// <summary>
        /// Set available options
        /// </summary>
        /// <param name="p_Options">New options list</param>
        /// <returns></returns>
        public XUIDropdown SetOptions(List<string> p_Options) => OnReady((x) => x.SetOptions(p_Options));
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">Select option</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUIDropdown SetValue(string p_Value, bool p_Notify = true) => OnReady((x) => x.SetValue(p_Value, p_Notify));
    }
}
