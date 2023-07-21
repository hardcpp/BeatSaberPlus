using CP_SDK.Chat.Interfaces;

namespace CP_SDK.Chat.Models.Twitch
{
    /// <summary>
    /// Twitch subscription model
    /// </summary>
    public class TwitchSubscriptionEvent : IChatSubscriptionEvent
    {
        public string   DisplayName             { get; internal set; }
        public string   SubPlan                 { get; internal set; }
        public bool     IsGift                  { get; internal set; }
        public string   RecipientDisplayName    { get; internal set; }
        public int      PurchasedMonthCount     { get; internal set; }
    }
}
