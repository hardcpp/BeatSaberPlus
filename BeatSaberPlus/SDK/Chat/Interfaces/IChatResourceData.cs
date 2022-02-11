namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public enum EChatResourceCategory
    {
        Emote,
        Cheermote,
        Badge,
    }

    public interface IChatResourceData
    {
        string Uri { get; }
        Animation.AnimationType Animation { get; }
        EChatResourceCategory Category { get; }
        string Type { get; }
    }
}
