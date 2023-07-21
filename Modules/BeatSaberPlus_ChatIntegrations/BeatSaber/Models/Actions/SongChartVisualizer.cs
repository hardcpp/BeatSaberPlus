using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Actions
{
    public class SongChartVisualizer_ToggleVisibility : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public ChatPlexMod_ChatIntegrations.Enums.Toggle.E ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ChangeType")
            || !p_Serialized.ContainsKey("ToggleType"))
                return;

            ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.ToEnum(p_Serialized["ToggleType"].Value<int>());
        }
    }
}