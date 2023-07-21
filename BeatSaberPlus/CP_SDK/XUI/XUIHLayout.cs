namespace CP_SDK.XUI
{
    /// <summary>
    /// CHLayout XUI Element
    /// </summary>
    public class XUIHLayout
        : _XUIHOrVLayout<XUIHLayout, UI.Components.CHLayout>
    {
        protected XUIHLayout(string p_Name, params IXUIElement[] p_Childs) : base(p_Name, p_Childs) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIHLayout Make(params IXUIElement[] p_Childs)
            => new XUIHLayout("XUIHLayout", p_Childs);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Childs">Child XUI Elements</param>
        public static XUIHLayout Make(string p_Name, params IXUIElement[] p_Childs)
            => new XUIHLayout(p_Name, p_Childs);
    }
}
