using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class SongChartVisualizer_ToggleVisibility : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ToggleType = 0;
    }
}