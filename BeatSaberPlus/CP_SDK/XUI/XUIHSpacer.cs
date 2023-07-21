namespace CP_SDK.XUI
{
    /// <summary>
    /// CHLayout XUI Element
    /// </summary>
    public class XUIHSpacer
        : _XUIHOrVSpacer<XUIHSpacer, UI.Components.CHLayout>
    {
        protected XUIHSpacer(string p_Name, float p_Spacing) : base(p_Name, p_Spacing) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Spacing">Spacing</param>
        public static XUIHSpacer Make(float p_Spacing)
            => new XUIHSpacer("XUIHSpacer", p_Spacing);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Spacing">Spacing</param>
        public static XUIHSpacer Make(string p_Name, float p_Spacing)
            => new XUIHSpacer(p_Name, p_Spacing);
    }
}
