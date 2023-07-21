namespace ChatPlexMod_ChatIntegrations.UI.Data
{
    /// <summary>
    /// Action list item
    /// </summary>
    internal class ActionListItem : CP_SDK.UI.Data.IListItem
    {
        public Interfaces.IActionBase Action;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Action">Action</param>
        public ActionListItem(Interfaces.IActionBase p_Action)
        {
            Action = p_Action;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow() => Refresh();
        /// <summary>
        /// On hide
        /// </summary>
        public override void OnHide() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh
        /// </summary>
        public void Refresh()
        {
            if (Cell == null || !(Cell is CP_SDK.UI.Data.TextListCell l_TextListCell))
                return;

            var l_Text = "<align=\"left\">" + (Action.IsEnabled ? "<color=yellow>" + Action.GetTypeName().Replace("_", "::</color><b>") : "<alpha=#70>" + Action.GetTypeName().Replace("_", "::"));
            l_TextListCell.Text.SetText(l_Text);
        }
    }
}
