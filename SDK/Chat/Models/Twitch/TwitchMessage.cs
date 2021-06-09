using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections.ObjectModel;

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
        public ReadOnlyDictionary<string, string> Metadata { get; internal set; }
        /// <summary>
        /// The IRC message type for this TwitchMessage
        /// </summary>
        public string Type { get; internal set; }
        /// <summary>
        /// The number of bits in this message, if any.
        /// </summary>
        public int Bits { get; internal set; }

        public TwitchMessage() { }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
