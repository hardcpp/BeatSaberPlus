using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using UnityEngine.Networking;

namespace CP_SDK.Network
{
    /// <summary>
    /// Web Response class
    /// </summary>
    public sealed class WebResponse
    {
        private byte[] m_BodyBytes;
        private string m_BodyString = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        public byte[] BodyBytes => m_BodyBytes;
        /// <summary>
        /// Response string
        /// </summary>
        public string BodyString
        {
            get
            {
                if (m_BodyString == null)
                {
                    if (m_BodyBytes?.Length > 0)
                    {
                        var l_Preamble = Encoding.UTF8.GetPreamble();
                        if (l_Preamble?.Length > 0 && m_BodyBytes.Length >= l_Preamble.Length && m_BodyBytes.Take(l_Preamble.Length).SequenceEqual(l_Preamble))
                            m_BodyString = Encoding.UTF8.GetString(m_BodyBytes, l_Preamble.Length, m_BodyBytes.Length - l_Preamble.Length);
                        else
                            m_BodyString = Encoding.UTF8.GetString(m_BodyBytes);
                    }
                    else
                        m_BodyString = string.Empty;
                }

                return m_BodyString;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Response">Reply status</param>
        public WebResponse(HttpResponseMessage p_Response)
        {
            StatusCode          = p_Response.StatusCode;
            ReasonPhrase        = p_Response.ReasonPhrase;
            IsSuccessStatusCode = p_Response.IsSuccessStatusCode;
            ShouldRetry         = IsSuccessStatusCode ? false : ((int)p_Response.StatusCode < 400 || (int)p_Response.StatusCode >= 500);
        }
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

            m_BodyBytes         = p_Request.downloadHandler.data;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Populate data
        /// </summary>
        /// <param name="p_BodyBytes">Body bytes</param>
        internal void Populate(byte[] p_BodyBytes)
            => m_BodyBytes = p_BodyBytes;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get JObject from serialized JSON
        /// </summary>
        /// <param name="p_JObject">Result object</param>
        /// <returns></returns>
        public bool TryAsJObject(out JObject p_JObject)
        {
            p_JObject = null;
            try
            {
                p_JObject = JObject.Parse(BodyString);
            }
            catch (Exception) { return false; }

            return p_JObject != null;
        }
        /// <summary>
        /// Get JObject from serialized JSON
        /// </summary>
        /// <param name="p_Deserialized">Input</param>
        /// <param name="p_JObject">Result object</param>
        /// <returns></returns>
        public bool TryGetObject<T>(out T p_JObject)
            where T : class, new()
        {
            p_JObject = null;
            try
            {
                p_JObject = JsonConvert.DeserializeObject<T>(BodyString);
            }
            catch (Exception) { return false; }

            return p_JObject != null;
        }
    }
}
