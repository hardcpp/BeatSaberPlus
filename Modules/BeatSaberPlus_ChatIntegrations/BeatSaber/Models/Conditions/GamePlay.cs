using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Conditions
{
    public class GamePlay_LevelEndType : ChatPlexMod_ChatIntegrations.Models.Condition
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

    public class GamePlay_PlayingMap : ChatPlexMod_ChatIntegrations.Models.Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public Enums.LevelType.E LevelType = Enums.LevelType.E.Solo;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public Enums.BeatmapModType.E BeatmapModType = Enums.BeatmapModType.E.All;
    }
}
