using CP_SDK.Animation;

namespace CP_SDK.Chat.Interfaces
{
    public interface IChatEmote
    {
        string          Id          { get; }
        string          Name        { get; }
        string          Uri         { get; }
        int             StartIndex  { get; }
        int             EndIndex    { get; }
        EAnimationType  Animation   { get; }
    }
}
