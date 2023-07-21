using Newtonsoft.Json;

namespace ChatPlexMod_ChatIntegrations.Models.Conditions
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
