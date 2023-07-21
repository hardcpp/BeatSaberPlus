using CP_SDK.Chat.Interfaces;

namespace ChatPlexMod_Chat.UI.Data
{
    /// <summary>
    /// Chat user list item
    /// </summary>
    internal class ChatUserListItem : CP_SDK.UI.Data.IListItem
    {
        public IChatService Service;
        public IChatUser    User;
        public string       Text;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_User">Chat user</param>
        public ChatUserListItem(IChatService p_Service, IChatUser p_User)
        {
            Service = p_Service;
            User    = p_User;

            Text = "<align=\"left\">[" + Service.DisplayName + "] ";
                    if (User.IsModerator || User.IsBroadcaster)    Text += "🗡 <color=yellow>";
            else if (User.IsVip)                                Text += "💎 <color=red>";
            else if (User.IsSubscriber)                         Text += "👑 <color=#008eff>";
            Text += User.DisplayName;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow()
        {
            if (!(Cell is CP_SDK.UI.Data.TextListCell l_TextListCell))
                return;

            l_TextListCell.Text.SetText(Text);
        }
        /// <summary>
        /// On hide
        /// </summary>
        public override void OnHide() { }
    }
}
