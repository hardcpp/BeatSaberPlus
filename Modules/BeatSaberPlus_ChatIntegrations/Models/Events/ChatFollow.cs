namespace BeatSaberPlus_ChatIntegrations.Models.Events
{
    /// <summary>
    /// Chat follow event model
    /// </summary>
    public class ChatFollow : Event
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ChatFollow()
        {
            Name = "New chat follow event";
        }
    }
}
