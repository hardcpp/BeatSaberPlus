using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class Event_Enabled : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string EventGUID = "";
    }
    public class Event_Disabled : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string EventGUID = "";
    }
}
