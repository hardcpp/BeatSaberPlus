using Newtonsoft.Json;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public class MapStats
    {
        [JsonProperty] public int plays = 0;
        [JsonProperty] public int downloads = 0;
        [JsonProperty] public int upvotes = 0;
        [JsonProperty] public int downvotes = 0;
        [JsonProperty] public float score = 0;
    }
}
