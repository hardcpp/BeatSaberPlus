using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Events
{
    /// <summary>
    /// VoiceAttack command event model
    /// </summary>
    public class VoiceAttackCommand : Event
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string CommandGUID = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string CommandName = "";
    }
}
