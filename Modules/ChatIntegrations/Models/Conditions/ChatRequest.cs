using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class ChatRequest_QueueDuration : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsGreaterThan = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Duration = 10 * 60;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;
    }

    public class ChatRequest_QueueSize : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsGreaterThan = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 10;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;
    }

    public class ChatRequest_QueueStatus : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsOpen = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessageOnFail = true;
    }
}
