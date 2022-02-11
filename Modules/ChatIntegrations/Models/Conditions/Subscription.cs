using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class Subscription_PlanType : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int PlanType = 0;
    }

    public class Subscription_PurchasedMonthCount : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 0;
    }
}
