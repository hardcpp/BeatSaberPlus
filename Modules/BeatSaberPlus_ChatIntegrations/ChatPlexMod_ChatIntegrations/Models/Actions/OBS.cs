using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ChatPlexMod_ChatIntegrations.Models.Actions
{
    public class OBS_RenameLastRecord : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Format = "$OriginalName (Completed!)";
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SetRecordFilenameFormat : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Format = "%CCYY-%MM-%DD %hh-%mm-%ss";
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SwitchPreviewToScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_SwitchToScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleStudioMode : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.Toggle.E ChangeType = Enums.Toggle.E.Toggle;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ChangeType")
            || !p_Serialized.ContainsKey("ToggleType"))
                return;

            ChangeType = Enums.Toggle.ToEnum(p_Serialized["ToggleType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleSource : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.Toggle.E ChangeType = Enums.Toggle.E.Toggle;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SourceName = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ChangeType")
            || !p_Serialized.ContainsKey("ToggleType"))
                return;

            ChangeType = Enums.Toggle.ToEnum(p_Serialized["ToggleType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_ToggleSourceAudio : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.Toggle.E ChangeType = Enums.Toggle.E.Toggle;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SourceName = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("ChangeType")
            || !p_Serialized.ContainsKey("ToggleType"))
                return;

            ChangeType = Enums.Toggle.ToEnum(p_Serialized["ToggleType"].Value<int>());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class OBS_Transition : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool OverrideDuration = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int Duration = 300;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool OverrideTransition = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Transition = "Fade";
    }
}
