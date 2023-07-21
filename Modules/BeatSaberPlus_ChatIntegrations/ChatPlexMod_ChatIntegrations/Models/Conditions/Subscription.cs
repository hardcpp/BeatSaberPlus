using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ChatPlexMod_ChatIntegrations.Models.Conditions
{
    public class Subscription_PlanType : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.TwitchSubscribtionPlanType.E SubscribtionPlanType = Enums.TwitchSubscribtionPlanType.E.Tier1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("SubscribtionPlanType")
                || !p_Serialized.ContainsKey("PlanType"))
                return;

            SubscribtionPlanType = Enums.TwitchSubscribtionPlanType.ToEnum(p_Serialized["PlanType"].Value<int>());
        }
    }

    public class Subscription_PurchasedMonthCount : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.Comparison.E Comparison = Enums.Comparison.E.GreaterOrEqual;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 0;
    }
}
