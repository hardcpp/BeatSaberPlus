using Newtonsoft.Json;
using System;

namespace BeatSaberPlus_SongOverlay.Models
{
    [Serializable]
    public class Score
    {
        [JsonProperty] internal uint score = 0;
        [JsonProperty] internal float accuracy = 0;
        [JsonProperty] internal uint combo = 0;
        [JsonProperty] internal uint missCount = 0;
        [JsonProperty] internal float currentHealth = 0;
    }
}
