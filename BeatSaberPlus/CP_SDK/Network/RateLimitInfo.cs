using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
#if CP_SDK_UNITY
using UnityEngine.Networking;
#endif

namespace CP_SDK.Network
{
    /// <summary>
    /// Rate Limit Info
    /// </summary>
    public sealed class RateLimitInfo
    {
        /// <summary>
        /// Total allowed requests for a given time window
        /// </summary>
        public int Limit { get; private set; }
        /// <summary>
        /// Number of requests remaining
        /// </summary>
        public int Remaining { get; private set; }
        /// <summary>
        /// Time at which rate limit window resets
        /// </summary>
        public DateTime Reset { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get RateLimitInfo from HttpResponseMessage
        /// </summary>
        /// <param name="p_Response">Response</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static RateLimitInfo Get(HttpResponseMessage p_Response)
        {
            if (p_Response == null)
                throw new ArgumentNullException(nameof(p_Response));

            var l_Headers = GetTransformedHeaders(p_Response);

            return new RateLimitInfo()
            {
                Limit       = GetLimit(l_Headers),
                Remaining   = GetRemaining(l_Headers),
                Reset       = GetReset(l_Headers),
            };
        }
#if CP_SDK_UNITY
        /// <summary>
        /// Get RateLimitInfo from UnityWebRequest
        /// </summary>
        /// <param name="p_Request">Request</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static RateLimitInfo Get(UnityWebRequest p_Request)
        {
            if (p_Request == null)
                throw new ArgumentNullException(nameof(p_Request));

            var l_Headers = GetTransformedHeaders(p_Request);

            return new RateLimitInfo() {
                Limit       = GetLimit(l_Headers),
                Remaining   = GetRemaining(l_Headers),
                Reset       = GetReset(l_Headers),
            };
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get transformed headers from a HttpResponseMessage
        /// </summary>
        /// <param name="p_Response">Response</param>
        /// <returns></returns>
        private static Dictionary<string, string> GetTransformedHeaders(HttpResponseMessage p_Response)
        {
            var l_Result = new Dictionary<string, string>();

            foreach (var l_KVP in p_Response.Headers)
            {
                if (l_KVP.Value.FirstOrDefault().Contains(","))
                    l_Result.Add(l_KVP.Key, l_KVP.Value.FirstOrDefault().Split(',').FirstOrDefault()?.Trim());
                else
                    l_Result.Add(l_KVP.Key, l_KVP.Value.FirstOrDefault().Trim());
            }

            return l_Result;
        }
#if CP_SDK_UNITY
        /// <summary>
        /// Get transformed headers from a UnityWebRequest
        /// </summary>
        /// <param name="p_Request">Request</param>
        /// <returns></returns>
        private static Dictionary<string, string> GetTransformedHeaders(UnityWebRequest p_Request)
        {
            var l_Result        = new Dictionary<string, string>();
            var l_BaseHeaders   = p_Request.GetResponseHeaders();

            foreach (var l_KVP in l_BaseHeaders)
            {
                if (l_KVP.Value.Contains(","))
                    l_Result.Add(l_KVP.Key, l_KVP.Value.Split(',').FirstOrDefault()?.Trim());
                else
                    l_Result.Add(l_KVP.Key, l_KVP.Value.Trim());
            }

            return l_Result;
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get limit value from header
        /// </summary>
        /// <param name="p_TransformedHeaders">Transformed headers</param>
        /// <returns></returns>
        private static int GetLimit(Dictionary<string, string> p_TransformedHeaders)
        {
            foreach (var l_KVP in p_TransformedHeaders)
            {
                var l_Lower = l_KVP.Key.ToLower();
                if (   l_Lower == "x-rate-limit-limit" || l_Lower == "x-ratelimit-limit"
                    || l_Lower == "rate-limit-limit"   || l_Lower == "ratelimit-limit"
                    || l_Lower == "x-rate-limit-total" || l_Lower == "x-ratelimit-total"
                    || l_Lower == "rate-limit-total"   || l_Lower == "ratelimit-total")
                {
                    if (int.TryParse(l_KVP.Value, out var l_Value))
                        return l_Value;
                    else
                        return -1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Get remaining value from header
        /// </summary>
        /// <param name="p_TransformedHeaders">Transformed headers</param>
        /// <returns></returns>
        private static int GetRemaining(Dictionary<string, string> p_TransformedHeaders)
        {
            foreach (var l_KVP in p_TransformedHeaders)
            {
                var l_Lower = l_KVP.Key.ToLower();
                if (   l_Lower == "x-rate-limit-remaining" || l_Lower == "x-ratelimit-remaining"
                    || l_Lower == "rate-limit-remaining"   || l_Lower == "ratelimit-remaining")
                {
                    if (int.TryParse(l_KVP.Value, out var l_Value))
                        return l_Value;
                    else
                        return -1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Get reset time from header
        /// </summary>
        /// <param name="p_TransformedHeaders">Transformed headers</param>
        /// <returns></returns>
        private static DateTime GetReset(Dictionary<string, string> p_TransformedHeaders)
        {
            foreach (var l_KVP in p_TransformedHeaders)
            {
                var l_Lower = l_KVP.Key.ToLower();
                if (   l_Lower == "x-rate-limit-reset" || l_Lower == "x-ratelimit-reset"
                    || l_Lower == "rate-limit-reset"   || l_Lower == "ratelimit-reset")
                {
                    if (!long.TryParse(l_KVP.Value, out var l_Value))
                        return DateTime.Now.AddSeconds(2);

                    if (l_Value < 1000000000)
                        return Misc.Time.FromUnixTime(Misc.Time.UnixTimeNow() + l_Value);

                    return Misc.Time.FromUnixTime(l_Value);
                }
            }

            return DateTime.Now.AddSeconds(2);
        }
    }
}
