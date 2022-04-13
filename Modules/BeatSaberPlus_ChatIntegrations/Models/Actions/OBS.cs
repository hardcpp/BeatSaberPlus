using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class OBS_SetRecordFilenameFormat : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string Format = "%CCYY-%MM-%DD %hh-%mm-%ss";
    }

    public class OBS_SwitchPreviewToScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }

    public class OBS_SwitchToScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
    }

    public class OBS_ToggleStudioMode : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ToggleType = 0;
    }

    public class OBS_ToggleSource : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ToggleType = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SourceName = "";
    }

    public class OBS_ToggleSourceAudio : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ToggleType = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SceneName = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string SourceName = "";
    }

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
