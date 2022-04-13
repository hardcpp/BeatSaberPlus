using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace BeatSaberPlus_ChatIntegrations.Models
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On deserialized
        /// </summary>
        /// <param name="p_Serialized">Input data</param>
        public virtual void OnDeserialized(JObject p_Serialized)
        {

        }
    }
}
