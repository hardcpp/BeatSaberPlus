using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Events
{
    /// <summary>
    /// Chat command event model
    /// </summary>
    public class ChatCommand : Event
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string Command = "!no_command_set";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatCommand()
        {
            Name = "New chat command event";
        }
    }
}
