using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Conditions
{
    public class Bits_Amount : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsGreaterThan = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 10;
    }
}
