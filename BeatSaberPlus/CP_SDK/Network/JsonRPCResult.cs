using Newtonsoft.Json.Linq;

namespace CP_SDK.Network
{
    /// <summary>
    /// JsonRPCResult
    /// </summary>
    public sealed class JsonRPCResult
    {
        public WebResponse  RawResponse;
        public JObject      Result;
        public JObject      Error;
    }
}
