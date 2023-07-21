using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace ChatPlexMod_ChatIntegrations.Models
{
    /// <summary>
    /// Action data modal
    /// </summary>
    [Serializable]
    public class Action
    {
        [JsonProperty]
        public string Type = "?";
        [JsonProperty]
        public bool Enabled = false;
        [JsonProperty]
        public string BaseValue = "";

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
