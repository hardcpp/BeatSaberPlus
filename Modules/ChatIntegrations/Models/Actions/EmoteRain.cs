using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class EmoteRain_CustomRain : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 20;
    }
    public class EmoteRain_EmoteBombRain : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint EmoteKindCount = 20;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint CountPerEmote = 20;
    }
}
