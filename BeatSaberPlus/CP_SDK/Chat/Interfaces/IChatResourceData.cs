using CP_SDK.Animation;

namespace CP_SDK.Chat.Interfaces
{
    public enum EChatResourceCategory
    {
        Emote,
        Cheermote,
        Badge,
    }

    public interface IChatResourceData
    {
        string                      Uri         { get; }
        EAnimationType              Animation   { get; }
        EChatResourceCategory       Category    { get; }
        string                      Type        { get; }
    }
}
