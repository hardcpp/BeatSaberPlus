using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    public class TwitchChannelPointEvent : IChatChannelPointEvent
    {
        public string   RewardID        { get; internal set; }

        public string   TransactionID   { get; internal set; }

        public string   Title           { get; internal set; }

        public string   Prompt          { get; internal set; }

        public string   UserInput       { get; internal set; }

        public int      Cost            { get; internal set; }

        public string   Image           { get; internal set; }

        public string   BackgroundColor { get; internal set; }
    }
}