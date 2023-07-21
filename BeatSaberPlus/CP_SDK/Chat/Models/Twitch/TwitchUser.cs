using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    /// <summary>
    /// Twitch user model
    /// </summary>
    public class TwitchUser : IChatUser
    {
        public string       Id              { get; internal set; }
        public string       UserName        { get; internal set; }
        public string       DisplayName     { get; internal set; }
        public string       PaintedName     { get; internal set; }
        public string       Color           { get; internal set; }
        public bool         IsModerator     { get; internal set; }
        public bool         IsBroadcaster   { get; internal set; }
        public bool         IsSubscriber    { get; internal set; }
        public bool         IsTurbo         { get; internal set; }
        public bool         IsVip           { get; internal set; }
        public IChatBadge[] Badges          { get; internal set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal int    _BadgesCache    = 0;
        internal bool   _FancyNameReady = false;
        internal bool   _HadFollowed    = false;
    }
}
