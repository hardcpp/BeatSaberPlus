using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Conditions
{
    public class GamePlay_LevelEndType : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Pass = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Fail = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Quit = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Restart = true;
    }

    public class GamePlay_PlayingMap : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Solo = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Multi = true;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Replay = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int BeatmapType = 0;
    }
}
