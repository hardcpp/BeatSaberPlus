using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class Event_ExecuteDummy : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Continue = true;
    }

    public class Event_Toggle : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ChangeType = 0;
    }
}
