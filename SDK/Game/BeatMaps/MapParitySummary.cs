using Newtonsoft.Json;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public class MapParitySummary
    {
        [JsonProperty] public int errors = 0;
        [JsonProperty] public int warns = 0;
        [JsonProperty] public int resets = 0;
    }
}
