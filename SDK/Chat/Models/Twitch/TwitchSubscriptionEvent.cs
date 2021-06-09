using BeatSaberPlus.SDK.Chat.Interfaces;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    class TwitchSubscriptionEvent : IChatSubscriptionEvent
    {
        public string DisplayName { get; internal set; }

        public string SubPlan { get; internal set; }

        public bool IsGift { get; internal set; }

        public string RecipientDisplayName { get; internal set; }

        public int PurchasedMonthCount { get; internal set; }
    }
}
