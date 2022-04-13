using BeatSaberPlus.SDK.Chat.Interfaces;
using System;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    public class TwitchMessage : IChatMessage, ICloneable
    {
        public string Id { get; internal set; }
        public string Message { get; internal set; }
        public bool IsSystemMessage { get; internal set; }
        public bool IsActionMessage { get; internal set; }
        public bool IsHighlighted { get; internal set; }
        public bool IsPing { get; internal set; }
        public IChatUser Sender { get; internal set; }
        public IChatChannel Channel { get; internal set; }
        public IChatEmote[] Emotes { get; internal set; }
        public string TargetUserId { get; internal set; }
        public string TargetMsgId { get; internal set; }
        public string Type { get; internal set; }
        public int Bits { get; internal set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
