using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Conditions
{
    public class ChatRequest_QueueDuration : ChatPlexMod_ChatIntegrations.Models.Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public ChatPlexMod_ChatIntegrations.Enums.Comparison.E Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.GreaterOrEqual;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Duration = 10 * 60;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("Comparison")
            || !p_Serialized.ContainsKey("IsGreaterThan"))
                return;

            if (p_Serialized["IsGreaterThan"].Value<bool>())
                Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Greater;
            else
                Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Less;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_QueueSize : ChatPlexMod_ChatIntegrations.Models.Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public ChatPlexMod_ChatIntegrations.Enums.Comparison.E Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.GreaterOrEqual;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 10;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("Comparison")
            || !p_Serialized.ContainsKey("IsGreaterThan"))
                return;

            if (p_Serialized["IsGreaterThan"].Value<bool>())
                Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Greater;
            else
                Comparison = ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Less;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_QueueStatus : ChatPlexMod_ChatIntegrations.Models.Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.QueueStatus.E Status = Enums.QueueStatus.E.Open;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("Status")
            || !p_Serialized.ContainsKey("IsOpen"))
                return;

            if (p_Serialized["IsOpen"].Value<bool>())
                Status = Enums.QueueStatus.E.Open;
            else
                Status = Enums.QueueStatus.E.Closed;
        }
    }
}
