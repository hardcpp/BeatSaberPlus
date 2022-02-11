using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class GamePlay_ChangeDebris : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Debris = false;
    }

    public class GamePlay_ChangeLightIntensity : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ValueType = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;
    }

    public class GamePlay_ChangeMusicVolume : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ValueType = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;
    }

    public class GamePlay_ChangeNoteColors : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ValueType = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Left = "#FF0000";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Right = "#0000FF";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;
    }

    public class GamePlay_SpawnSquatWalls : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Interval = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Count = 10;
    }

    public class GamePlay_SpawnBombPatterns : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Interval = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Count = 1;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte L0 = 0b00000111;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte L1 = 0b00000111;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte L2 = 0b00000111;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public byte L3 = 0b00000111;
    }

    public class GamePlay_ToggleLights : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ChangeType = 0;
    }
}
