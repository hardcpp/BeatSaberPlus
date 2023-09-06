using Newtonsoft.Json;

namespace ChatPlexMod_ChatIntegrations.Models.Actions
{
    public class Misc_Delay : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Delay = 10;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint DelayMs = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PreventNextActionFailure = true;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_PlaySound : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float Volume = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float PitchMin = 1;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public float PitchMax = 1;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool KillOnSceneSwitch = false;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class WaitMenuScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PreventNextActionFailure = true;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class WaitPlayingScene : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool PreventNextActionFailure = true;
    }
}
