using System.Net;
using System.Net.Http;

namespace BeatSaberPlus.SDK.Network
{
    /// <summary>
    /// API Response class
    /// </summary>
    public class APIResponse
    {
        /// <summary>
        /// Result code
        /// </summary>
        public readonly HttpStatusCode StatusCode;
        /// <summary>
        /// Reason phrase
        /// </summary>
        public readonly string ReasonPhrase;
        /// <summary>
        /// Is success
        /// </summary>
        public readonly bool IsSuccessStatusCode;
        /// <summary>
        /// Response bytes
        /// </summary>
        public readonly byte[] BodyBytes;
        /// <summary>
        /// Response string
        /// </summary>
        public readonly string BodyString;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Reply">Reply status</param>
        /// <param name="p_Body">Reply body</param>
        public APIResponse(HttpResponseMessage p_Reply, byte[] p_BodyBytes, string p_BodyString)
        {
            StatusCode          = p_Reply.StatusCode;
            ReasonPhrase        = p_Reply.ReasonPhrase;
            IsSuccessStatusCode = p_Reply.IsSuccessStatusCode;
            BodyBytes           = p_BodyBytes;
            BodyString          = p_BodyString;
#if DEBUG
            Logger.Instance.Debug("[SDK.Network][APIResponse.APIResponse] Result " + p_Reply.RequestMessage.RequestUri.ToString() + " - " + StatusCode);
            /*foreach (var l_Header in p_Reply.RequestMessage.Headers)
            {
                Logger.Instance.Debug(l_Header.Key);
                foreach (var l_Value in l_Header.Value)
                    Logger.Instance.Debug("    " + l_Value);
            }*/
#endif

            p_Reply.Dispose();
        }
    }
}
