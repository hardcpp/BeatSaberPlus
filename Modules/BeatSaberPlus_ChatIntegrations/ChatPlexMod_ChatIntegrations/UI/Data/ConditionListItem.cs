namespace ChatPlexMod_ChatIntegrations.UI.Data
{
    /// <summary>
    /// Condition list item
    /// </summary>
    internal class ConditionListItem : CP_SDK.UI.Data.IListItem
    {
        public Interfaces.IConditionBase Condition;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Condition">Action</param>
        public ConditionListItem(Interfaces.IConditionBase p_Condition)
        {
            Condition = p_Condition;
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

            var l_Text = "<align=\"left\">" + (Condition.IsEnabled ? "<color=yellow>" + Condition.GetTypeName().Replace("_", "::</color><b>") : "<alpha=#70>" + Condition.GetTypeName().Replace("_", "::"));
            l_TextListCell.Text.SetText(l_Text);
        }
    }
}
