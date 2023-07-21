using System;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CPrimaryButton XUI Element
    /// </summary>
    public class XUIPrimaryButton
        : _XUIPOrSButton<XUIPrimaryButton, UI.Components.CPrimaryButton>
    {
        protected XUIPrimaryButton(string p_Name, string p_Label, Action p_OnClick = null)
            : base(p_Name, p_Label, p_OnClick) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Label">Button text</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUIPrimaryButton Make(string p_Label, Action p_OnClick = null)
            => new XUIPrimaryButton("XUIPrimaryButton", p_Label, p_OnClick);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Label">Button text</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUIPrimaryButton Make(string p_Name, string p_Label, Action p_OnClick = null)
            => new XUIPrimaryButton(p_Name, p_Label, p_OnClick);
    }
}
