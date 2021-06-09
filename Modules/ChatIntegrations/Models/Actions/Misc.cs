using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Actions
{
    public class Misc_Delay : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Delay = 10;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PreventNextActionFailure = true;
    }

    public class Misc_PlaySound : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Volume = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Pitch = 1;
    }
}
