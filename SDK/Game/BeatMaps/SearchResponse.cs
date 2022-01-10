using Newtonsoft.Json;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public class SearchResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapDetail[] docs = null;
    }
}
