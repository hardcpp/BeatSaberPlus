using Newtonsoft.Json;
using System;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models
{
    /// <summary>
    /// Condition data modal
    /// </summary>
    [Serializable]
    public class Condition
    {
        [JsonProperty]
        public string Type = "?";
        [JsonProperty]
        public bool Enabled = false;
        [JsonProperty]
        public string EncodedUserValue = "";
    }
}
