using System.Net;
#if CP_SDK_UNITY
using UnityEngine.Networking;
#endif

namespace CP_SDK.Network
{
    /// <summary>
    /// Web Response class
    /// </summary>
    public class WebResponse
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
        /// Should retry
        /// </summary>
        public readonly bool ShouldRetry;
        /// <summary>
        /// Response bytes
        /// </summary>
        public readonly string BodyString;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if CP_SDK_UNITY
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Request">Reply status</param>
        public WebResponse(UnityWebRequest p_Request)
        {
            StatusCode          = (HttpStatusCode)p_Request.responseCode;
            ReasonPhrase        = p_Request.error;
            IsSuccessStatusCode = !(p_Request.isHttpError || p_Request.isNetworkError);
            ShouldRetry         = IsSuccessStatusCode ? false : (p_Request.responseCode < 400 || p_Request.responseCode >= 500);
            BodyString          = p_Request.downloadHandler.text;
        }
#endif
    }
}
