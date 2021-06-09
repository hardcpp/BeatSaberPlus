namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Events
{
    /// <summary>
    /// Chat subscription event model
    /// </summary>
    public class ChatSubscription : Event
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ChatSubscription()
        {
            Name = "New chat subscription event";
        }
    }
}
