using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models
{
    public class ChatResourceData : IChatResourceData
    {
        public string                   Uri         { get; set; }
        public Animation.EAnimationType Animation   { get; set; }
        public EChatResourceCategory    Category    { get; set; }
        public string                   Type        { get; set; }
    }
}
