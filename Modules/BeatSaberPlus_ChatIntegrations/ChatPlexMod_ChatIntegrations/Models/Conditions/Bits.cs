using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ChatPlexMod_ChatIntegrations.Models.Conditions
{
    public class Bits_Amount : Condition
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include), JsonConverter(typeof(StringEnumConverter))]
        public Enums.Comparison.E Comparison = Enums.Comparison.E.GreaterOrEqual;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public uint Count = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void OnDeserialized(JObject p_Serialized)
        {
            if (p_Serialized.ContainsKey("Comparison")
            || !p_Serialized.ContainsKey("IsGreaterThan"))
                return;

            if (p_Serialized["IsGreaterThan"].Value<bool>())
                Comparison = Enums.Comparison.E.Greater;
            else
                Comparison = Enums.Comparison.E.Less;
        }
    }
}
