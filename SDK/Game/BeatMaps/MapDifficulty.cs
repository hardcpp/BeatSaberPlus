using Newtonsoft.Json;

namespace BeatSaberPlus.SDK.Game.BeatMaps
{
    public class MapDifficulty
    {
        [JsonProperty] public float njs = 0;
        [JsonProperty] public float offset = 0;
        [JsonProperty] public int notes = 0;
        [JsonProperty] public int bombs = 0;
        [JsonProperty] public int obstacles = 0;
        [JsonProperty] public float nps = 0;
        [JsonProperty] public float length = 0;
        [JsonProperty] public string characteristic = "";
        [JsonProperty] public string difficulty = "";
        [JsonProperty] public int events = 0;
        [JsonProperty] public bool chroma = false;
        [JsonProperty] public bool me = false;
        [JsonProperty] public bool ne = false;
        [JsonProperty] public bool cinema = false;
        [JsonProperty] public float seconds = 0;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] public MapParitySummary paritySummary = null;
        [JsonProperty] public float stars = 0;
    }
}
