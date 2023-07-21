namespace CP_SDK.UI.Data
{
    /// <summary>
    /// Text list item
    /// </summary>
    public class TextListItem : IListItem
    {
        public string                       Text;
        public string                       Tooltip;
        public TMPro.TextAlignmentOptions   Align;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Text">Value</param>
        public TextListItem(string p_Text = "Default...", string p_Tooltip = null, TMPro.TextAlignmentOptions p_Align = TMPro.TextAlignmentOptions.CaplineLeft)
        {
            Text    = p_Text;
            Tooltip = p_Tooltip;
            Align   = p_Align;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow()
        {
            if (!(Cell is TextListCell l_TextListCell))
                return;

            l_TextListCell.Text.SetText(Text).SetAlign(Align);
            l_TextListCell.Tooltip = Tooltip;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On select
        /// </summary>
        public override void OnSelect()
        {

        }
        /// <summary>
        /// On Unselect
        /// </summary>
        public override void OnUnselect()
        {

        }
    }
}
