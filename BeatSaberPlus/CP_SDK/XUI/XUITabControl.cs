using System;
using System.Linq;
using UnityEngine;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CTabControl XUI Element
    /// </summary>
    public class XUITabControl
        : IXUIElement, IXUIElementReady<XUITabControl, UI.Components.CTabControl>, IXUIBindable<XUITabControl>
    {
        private UI.Components.CTabControl m_Element = null;

        private event Action<UI.Components.CTabControl> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform               RTransform  => Element?.RTransform;
        public          UI.Components.CTabControl   Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUITabControl(string p_Name, params (string, IXUIElement)[] p_Tabs)
            : base(p_Name)
        {
            SetTabs(p_Tabs);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Tabs">Tabs</param>
        public static XUITabControl Make(params (string, IXUIElement)[] p_Tabs)
            => new XUITabControl("XUITabControl", p_Tabs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Tabs">Tabs</param>
        public static XUITabControl Make(string p_Name, params (string, IXUIElement)[] p_Tabs)
            => new XUITabControl(p_Name, p_Tabs);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.TabControl.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUITabControl.BuildUI] Error OnReady:");
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
        public XUITabControl OnReady(Action<UI.Components.CTabControl> p_Functor)
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
        public XUITabControl Bind(ref XUITabControl p_Target)
        {
            p_Target = this;
            return this;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On active tab changed event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public XUITabControl OnActiveChanged(Action<int> p_Functor, bool p_Add = true) => OnReady(x => x.OnActiveChanged(p_Functor, p_Add));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set game object active state
        /// </summary>
        /// <param name="p_Active">New state</param>
        /// <returns></returns>
        public XUITabControl SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));
        /// <summary>
        /// Set active tab
        /// </summary>
        /// <param name="p_Index">New active index</param>
        /// <param name="p_Notify">Should notify?</param>
        /// <returns></returns>
        public XUITabControl SetActiveTab(int p_Index, bool p_Notify = true) => OnReady(x => x.SetActiveTab(p_Index, p_Notify));
        /// <summary>
        /// Set texts
        /// </summary>
        /// <param name="p_Tabs">New tabs</param>
        /// <returns></returns>
        public XUITabControl SetTabs(params (string, IXUIElement)[] p_Tabs)
        {
            OnReady(x => {
                for (var l_I = 0; l_I < p_Tabs.Length; ++l_I)
                {
                    if (p_Tabs[l_I].Item2.RTransform)
                        continue;

                    p_Tabs[l_I].Item2.BuildUI(null);
                }

                x.SetTabs(p_Tabs.Select(y => (y.Item1, y.Item2.RTransform)).ToArray());
            });

            return this;
        }
    }
}
