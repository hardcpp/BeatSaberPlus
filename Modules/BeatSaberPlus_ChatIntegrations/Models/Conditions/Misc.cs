using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class Misc_Cooldown : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PerUser = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint CooldownTime = 10;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool NotifyUser = true;
    }
}
