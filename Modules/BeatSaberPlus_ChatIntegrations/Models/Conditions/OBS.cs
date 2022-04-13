using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class OBS_IsInScene : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }
    public class OBS_IsNotInScene : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }
}
