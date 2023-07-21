using CP_SDK.Animation;
using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    public class TwitchCheermoteTier : IChatResourceData
    {
        public string                   Uri         { get; internal set; }
        public int                      MinBits     { get; internal set; }
        public string                   Color       { get; internal set; }
        public EAnimationType           Animation   { get; internal set; } = EAnimationType.GIF;
        public EChatResourceCategory    Category    { get; internal set; } = EChatResourceCategory.Cheermote;
        public string                   Type        { get; internal set; } = "TwitchCheermote";
    }
}
