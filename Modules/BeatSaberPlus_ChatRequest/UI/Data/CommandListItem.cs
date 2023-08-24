using System;

using EPermissions = BeatSaberPlus_ChatRequest.CRConfig._Commands.EPermission;

namespace BeatSaberPlus_ChatRequest.UI.Data
{
    /// <summary>
    /// Chat user list item
    /// </summary>
    internal class CommandListItem : CP_SDK.UI.Data.IListItem
    {
        public string               CommandName;
        public Func<EPermissions>   GetPermissions;
        public Action<EPermissions> SetPermissions;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_CommandName"></param>
        /// <param name="p_GetPermissions"></param>
        /// <param name="p_SetPermissions"></param>
        public CommandListItem(string p_CommandName, Func<EPermissions> p_GetPermissions, Action<EPermissions> p_SetPermissions)
        {
            CommandName = p_CommandName;
            GetPermissions = p_GetPermissions;
            SetPermissions = p_SetPermissions;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow()
        {
            if (!(Cell is CommandListCell l_CommandListCell))
                return;

            l_CommandListCell.Text.SetText(CommandName);
            l_CommandListCell.UpdateFrom(GetPermissions());
        }
        /// <summary>
        /// On hide
        /// </summary>
        public override void OnHide() { }
    }
}
