using System;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents
{
    /// <summary>
    /// Default CFloatingPanel component
    /// </summary>
    internal class DefaultCFloatingPanel : Components.CFloatingPanel
    {
        protected       RectTransform                       m_RTransform;
        private event   Action<Components.CFloatingPanel>   m_OnRelease;
        private event   Action<Components.CFloatingPanel>   m_OnGrab;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform RTransform => m_RTransform;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        public virtual void Init()
        {
            if (m_RTransform)
                return;

            gameObject.layer = UISystem.UILayer;

            m_RTransform = transform as RectTransform;
            m_RTransform.localPosition   = Vector3.zero;
            m_RTransform.localRotation   = Quaternion.identity;
            m_RTransform.localScale      = new Vector3(0.02f, 0.02f, 0.02f);

            var l_Canvas = gameObject.AddComponent<Canvas>();
            l_Canvas.additionalShaderChannels   = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            l_Canvas.sortingOrder               = 3;

            var l_CanvasScaler = gameObject.AddComponent<CanvasScaler>();
            l_CanvasScaler.dynamicPixelsPerUnit     = 3.44f;
            l_CanvasScaler.referencePixelsPerUnit   = 10f;

            SetBackground(true);
            SetSize(new Vector2(20f, 20f));

            try { UISystem.OnScreenCreated?.Invoke(this); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCFloatingPanel.Init] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When grabbed
        /// </summary>
        internal void FireOnGrab()
        {
            try { m_OnGrab?.Invoke(this); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCFloatingPanel.OnHandleGrab] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            m_OnGrab?.Invoke(this);
        }
        /// <summary>
        /// When released
        /// </summary>
        internal void FireOnRelease()
        {
            try { m_OnRelease?.Invoke(this); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.DefaultComponents][DefaultCFloatingPanel.OnHandleReleased] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On grab event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CFloatingPanel OnGrab(Action<Components.CFloatingPanel> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnGrab += p_Functor;
            else        m_OnGrab -= p_Functor;

            return this;
        }
        /// <summary>
        /// On release event
        /// </summary>
        /// <param name="p_Functor">Functor to add/remove</param>
        /// <param name="p_Add">Should add</param>
        /// <returns></returns>
        public override Components.CFloatingPanel OnRelease(Action<Components.CFloatingPanel> p_Functor, bool p_Add = true)
        {
            if (p_Add)  m_OnRelease += p_Functor;
            else        m_OnRelease -= p_Functor;

            return this;
        }
    }
}
