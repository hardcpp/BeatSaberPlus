namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchRoomstate
    {
        public string BroadcasterLang { get; internal set; }
        public string RoomId { get; internal set; }
        public bool EmoteOnly { get; internal set; }
        public bool FollowersOnly { get; internal set; }
        public bool SubscribersOnly { get; internal set; }
        public bool R9K { get; internal set; }

        /// <summary>
        /// The number of seconds a chatter without moderator privileges must wait between sending messages
        /// </summary>
        public int SlowModeInterval { get; internal set; }

        /// <summary>
        /// If FollowersOnly is true, this specifies the number of minutes a user must be following before they can chat.
        /// </summary>
        public int MinFollowTime { get; internal set; }
    }
}
