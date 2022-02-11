using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class User_Permissions : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Subscriber = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool VIP = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Moderator = true;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool NotifyWhenNoPermission = true;
    }
}
