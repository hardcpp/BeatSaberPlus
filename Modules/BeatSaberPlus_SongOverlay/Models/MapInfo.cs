using Newtonsoft.Json;
using System;

namespace BeatSaberPlus_SongOverlay.Models
{
    [Serializable]
    public class MapInfo
    {
        [JsonProperty] internal string level_id = "";
        [JsonProperty] internal string name = "N/A";
        [JsonProperty] internal string sub_name = "N/A";
        [JsonProperty] internal string artist = "N/A";
        [JsonProperty] internal string mapper = "N/A";
        [JsonProperty] internal string characteristic = "N/A";
        [JsonProperty] internal string difficulty = "N/A";
        [JsonProperty] internal uint duration = 0;
        [JsonProperty] internal float BPM = 0;
        [JsonProperty] internal float PP = 0;
        [JsonProperty] internal string BSRKey = "";
        [JsonProperty] internal string coverRaw = "";
    }
}
