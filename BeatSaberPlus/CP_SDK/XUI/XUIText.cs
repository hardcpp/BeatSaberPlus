using System;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CText XUI Element
    /// </summary>
    public class XUIText
        : IXUIElement, IXUIElementReady<XUIText, UI.Components.CText>, IXUIBindable<XUIText>
    {
        private UI.Components.CText m_Element = null;

        private event Action<UI.Components.CText> m_OnReady;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override RectTransform       RTransform  => Element?.RTransform;
        public          UI.Components.CText Element     => m_Element;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected XUIText(string p_Name, string p_Text)
            : base(p_Name)
        {
            SetText(p_Text);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Text">Text</param>
        public static XUIText Make(string p_Text)
            => new XUIText("XUIText", p_Text);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Text">Text</param>
        public static XUIText Make(string p_Name, string p_Text)
            => new XUIText(p_Name, p_Text);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BuildUI for this element into p_Parent transform
        /// </summary>
        /// <param name="p_Parent">Transform to build UI into</param>
        public override void BuildUI(Transform p_Parent)
        {
            m_Element = UI.UISystem.TextFactory.Create(m_InitialName, p_Parent);

            try { m_OnReady?.Invoke(m_Element); m_OnReady = null; }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.XUI][XUIText.BuildUI] Error OnReady:");
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
        public XUIText OnReady(Action<UI.Components.CText> p_Functor)
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
        public XUIText Bind(ref XUIText p_Target)
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
        public XUIText SetActive(bool p_Active) => OnReady(x => x.gameObject.SetActive(p_Active));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set align
        /// </summary>
        /// <param name="p_Align">New align</param>
        /// <returns></returns>
        public XUIText SetAlign(TextAlignmentOptions p_Align) => OnReady((x) => x.SetAlign(p_Align));
        /// <summary>
        /// Set alpha
        /// </summary>
        /// <param name="p_Alpha">New alpha</param>
        /// <returns></returns>
        public XUIText SetAlpha(float p_Alpha) => OnReady((x) => x.SetAlpha(p_Alpha));
        /// <summary>
        /// Set color
        /// </summary>
        /// <param name="p_Color">New color</param>
        /// <returns></returns>
        public XUIText SetColor(Color p_Color) => OnReady((x) => x.SetColor(p_Color));
        /// <summary>
        /// Set font size
        /// </summary>
        /// <param name="p_Size">New size</param>
        /// <returns></returns>
        public XUIText SetFontSize(float p_Size) => OnReady((x) => x.SetFontSize(p_Size));
        /// <summary>
        /// Set margins
        /// </summary>
        /// <param name="p_Left">Left margin</param>
        /// <param name="p_Top">Top margin</param>
        /// <param name="p_Right">Right margin</param>
        /// <param name="p_Bottom">Bottom margin</param>
        /// <returns></returns>
        public XUIText SetMargins(float p_Left, float p_Top, float p_Right, float p_Bottom) => OnReady((x) => x.SetMargins(p_Left, p_Top, p_Right, p_Bottom));
        /// <summary>
        /// Set overflow mode
        /// </summary>
        /// <param name="p_OverflowMode">New overflow mdoe</param>
        /// <returns></returns>
        public XUIText SetOverflowMode(TextOverflowModes p_OverflowMode) => OnReady((x) => x.SetOverflowMode(p_OverflowMode));
        /// <summary>
        /// Set style
        /// </summary>
        /// <param name="p_Style">New style</param>
        /// <returns></returns>
        public XUIText SetStyle(FontStyles p_Style) => OnReady((x) => x.SetStyle(p_Style));
        /// <summary>
        /// Set button text
        /// </summary>
        /// <param name="p_Text">New text</param>
        /// <returns></returns>
        public XUIText SetText(string p_Text) => OnReady((x) => x.SetText(p_Text));
        /// <summary>
        /// Set wrapping
        /// </summary>
        /// <param name="p_Wrapping">New state</param>
        /// <returns></returns>
        public XUIText SetWrapping(bool p_Wrapping) => OnReady((x) => x.SetWrapping(p_Wrapping));
    }
}
