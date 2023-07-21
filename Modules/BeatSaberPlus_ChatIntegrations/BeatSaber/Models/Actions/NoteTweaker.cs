using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Actions
{
    public class NoteTweaker_SwitchProfile : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Profile = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Temporary = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("Profile")
            || !p_Serialized.ContainsKey("BaseValue"))
                return;

            Profile = p_Serialized["BaseValue"].Value<string>();
        }
    }
}