using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Actions
{
    public class Chat_SendMessage : Action
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool AddTTSPefix = true;

        public Chat_SendMessage()
        {
            BaseValue = "Test content";
        }
    }
}
