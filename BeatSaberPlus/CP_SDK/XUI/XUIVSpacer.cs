namespace CP_SDK.XUI
{
    /// <summary>
    /// CVLayout XUI Element
    /// </summary>
    public class XUIVSpacer
        : _XUIHOrVSpacer<XUIVSpacer, UI.Components.CVLayout>
    {
        protected XUIVSpacer(string p_Name, float p_Spacing) : base(p_Name, p_Spacing) { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Spacing">Spacing</param>
        public static XUIVSpacer Make(float p_Spacing)
            => new XUIVSpacer("XUIVSpacer", p_Spacing);
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Name">Element name</param>
        /// <param name="p_Spacing">Spacing</param>
        public static XUIVSpacer Make(string p_Name, float p_Spacing)
            => new XUIVSpacer(p_Name, p_Spacing);
    }
}
