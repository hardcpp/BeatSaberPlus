using Newtonsoft.Json;
using System;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models
{
    /// <summary>
    /// Event data modal
    /// </summary>
    [Serializable]
    public class Event
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string GUID = "";
        [JsonProperty]
        public string Name = "?";
        [JsonProperty]
        public string Type = "?";
        [JsonProperty]
        public bool Enabled = false;
        [JsonProperty]
        public UInt64 UsageCount = 0;
        [JsonProperty]
        public Int64 CreationDate = 0;
        [JsonProperty]
        public Int64 LastUsageDate = 0;
    }
}
