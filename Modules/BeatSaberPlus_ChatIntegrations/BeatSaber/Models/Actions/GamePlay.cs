using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Actions
{
    public class GamePlay_ChangeBombColor : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Color = "#CCCCCC";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            switch (p_Serialized["ValueType"].Value<int>())
            {
                case 0: ValueSource = Enums.ValueSource.E.Config; break;
                case 1: ValueSource = Enums.ValueSource.E.User;   break;
                case 2: ValueSource = Enums.ValueSource.E.Event;  break;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeBombScale : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.4f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 1.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            ValueSource = Enums.ValueSource.ToEnum(p_Serialized["ValueType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeDebris : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool Debris = false;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeLightIntensity : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            ValueSource = Enums.ValueSource.ToEnum(p_Serialized["ValueType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeMusicVolume : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            ValueSource = Enums.ValueSource.ToEnum(p_Serialized["ValueType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeNoteColors : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Left = "#FF0000";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Right = "#0000FF";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            switch (p_Serialized["ValueType"].Value<int>())
            {
                case 0: ValueSource = Enums.ValueSource.E.Config; break;
                case 1: ValueSource = Enums.ValueSource.E.User;   break;
                case 2: ValueSource = Enums.ValueSource.E.Event;  break;
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ChangeNoteScale : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.Random;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float UserValue = 0.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Min = 0.4f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Max = 1.5f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SendChatMessage = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ValueSource")
            || !p_Serialized.ContainsKey("ValueType"))
                return;

            ValueSource = Enums.ValueSource.ToEnum(p_Serialized["ValueType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_Pause : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool HideUI = false;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_SpawnSquatWalls : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Interval = 2f;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Count = 10;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_SpawnBombPatterns : ChatPlexMod_ChatIntegrations.Models.Action
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

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class GamePlay_ToggleHUD : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public ChatPlexMod_ChatIntegrations.Enums.Toggle.E ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.E.Toggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ChangeType")
            || !p_Serialized.ContainsKey("ToggleType"))
                return;

            ChangeType = ChatPlexMod_ChatIntegrations.Enums.Toggle.ToEnum(p_Serialized["ToggleType"].Value<int>());
        }
    }
}
