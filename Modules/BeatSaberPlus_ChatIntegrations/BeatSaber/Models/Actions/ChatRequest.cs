using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Models.Actions
{
    public class ChatRequest_AddToQueue : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.ValueSource.E ValueSource = Enums.ValueSource.E.User;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string TitlePrefix = "[ChatIntegrations]";
        [JsonProperty]
        public bool AsModerator = false;
        [JsonProperty]
        public bool AddToTop = false;
        [JsonProperty]
        public bool SendChatMessage = true;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_ToggleQueue : ChatPlexMod_ChatIntegrations.Models.Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.EChatRequestQueueToggle.E ChangeType = Enums.EChatRequestQueueToggle.E.Toggle;
    }
}