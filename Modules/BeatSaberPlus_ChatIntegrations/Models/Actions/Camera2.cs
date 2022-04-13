using Newtonsoft.Json;

namespace BeatSaberPlus_ChatIntegrations.Models.Actions
{
    public class Camera2_SwitchToScene : Action
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string SceneName = "";
    }

    public class Camera2_ToggleCamera : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public int ToggleType = 0;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CameraName = "";
    }
}
