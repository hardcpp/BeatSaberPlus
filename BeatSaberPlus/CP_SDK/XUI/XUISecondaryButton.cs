using System;

namespace CP_SDK.XUI
{
    /// <summary>
    /// CSecondaryButton XUI Element
    /// </summary>
    public class XUISecondaryButton
        : _XUIPOrSButton<XUISecondaryButton, UI.Components.CSecondaryButton>
    {
        protected XUISecondaryButton(string p_Name, string p_Label, Action p_OnClick = null)
            : base(p_Name, p_Label, p_OnClick) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Label">Button text</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUISecondaryButton Make(string p_Label, Action p_OnClick = null)
            => new XUISecondaryButton("XUISecondaryButton", p_Label, p_OnClick);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Label">Button text</param>
        /// <param name="p_OnClick">On click callback</param>
        public static XUISecondaryButton Make(string p_Name, string p_Label, Action p_OnClick = null)
            => new XUISecondaryButton(p_Name, p_Label, p_OnClick);
    }
}
