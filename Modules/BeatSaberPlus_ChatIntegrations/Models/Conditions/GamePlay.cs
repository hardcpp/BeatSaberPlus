using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public enum ELevelType
        {
            Any,
            Solo,
            Multiplayer,
            SoloAndMultiplayer,
            Replay,
        }
        public enum EBeatmapModType
        {
            All,
            NonNoodle,
            Noodle,
            Chroma,
            NoodleOrChroma
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ELevelType LevelType = ELevelType.Solo;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public EBeatmapModType BeatmapModType = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("LevelType")
                || !p_Serialized.ContainsKey("Solo")
                || !p_Serialized.ContainsKey("Multi")
                || !p_Serialized.ContainsKey("Replay")
                || !p_Serialized.ContainsKey("BeatmapType"))
                return;

            if (p_Serialized["Solo"].Value<bool>() && p_Serialized["Multi"].Value<bool>() && p_Serialized["Replay"].Value<bool>())
                LevelType = ELevelType.Any;
            else if (p_Serialized["Solo"].Value<bool>() && p_Serialized["Multi"].Value<bool>() && !p_Serialized["Replay"].Value<bool>())
                LevelType = ELevelType.SoloAndMultiplayer;
            else if (!p_Serialized["Solo"].Value<bool>() && p_Serialized["Multi"].Value<bool>() && !p_Serialized["Replay"].Value<bool>())
                LevelType = ELevelType.Multiplayer;
            else if (!p_Serialized["Solo"].Value<bool>() && !p_Serialized["Multi"].Value<bool>() && p_Serialized["Replay"].Value<bool>())
                LevelType = ELevelType.Replay;
            else
                LevelType = ELevelType.Any;

            BeatmapModType = (EBeatmapModType)p_Serialized["BeatmapType"].Value<int>();
        }
    }
}
