namespace CP_SDK.XUI
{
    /// <summary>
    /// CVLayout XUI Element
    /// </summary>
    public class XUIVLayout
        : _XUIHOrVLayout<XUIVLayout, UI.Components.CVLayout>
    {
        protected XUIVLayout(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIVLayout Make(params IXUIElement[] p_Childs)
            => new XUIVLayout("XUIVLayout", p_Childs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIVLayout Make(string p_Name, params IXUIElement[] p_Childs)
            => new XUIVLayout(p_Name, p_Childs);
    }
}
