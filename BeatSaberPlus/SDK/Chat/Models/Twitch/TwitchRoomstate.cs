namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    /// <summary>
    /// Twitch room state model
    /// </summary>
    public struct TwitchRoomstate
    {
        public string BroadcasterLang { get; internal set; }
        public string RoomId { get; internal set; }
        public bool EmoteOnly { get; internal set; }
        public bool FollowersOnly { get; internal set; }
        public bool SubscribersOnly { get; internal set; }
        public bool R9K { get; internal set; }
        public int SlowModeInterval { get; internal set; }
        public int MinFollowTime { get; internal set; }
    }
}
