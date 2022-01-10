using System;
using System.Linq;
using System.Net.Http;

namespace BeatSaberPlus.SDK.Network
{
    /// <summary>
    /// Rate Limit Info
    /// </summary>
    public class RateLimitInfo
    {
        /// <summary>
        /// Number of requests remaining
        /// </summary>
        public int Remaining { get; private set; }
        /// <summary>
        /// Time at which rate limit bucket resets
        /// </summary>
        public DateTime Reset { get; private set; }
        /// <summary>
        /// Total allowed requests for a given bucket window
        /// </summary>
        public int Total { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static RateLimitInfo FromHttp(HttpResponseMessage p_Response)
        {
            if (p_Response == null)
                throw new ArgumentNullException(nameof(p_Response));

            if (!p_Response.Headers.TryGetValues("Rate-Limit-Remaining", out var l_Remainings))
                return null;
            string l_RemainingStr = l_Remainings.FirstOrDefault<string>();
            if (l_RemainingStr == null)
                return null;
            if (!int.TryParse(l_RemainingStr, out var l_Remaining))
                return null;

            if (!p_Response.Headers.TryGetValues("Rate-Limit-Total", out var l_Totals))
                return null;
            string l_TotalStr = l_Totals.FirstOrDefault<string>();
            if (l_TotalStr == null)
                return null;
            if (!int.TryParse(l_TotalStr, out var l_Total))
                return null;

            if (!p_Response.Headers.TryGetValues("Rate-Limit-Reset", out var l_Resets))
                return null;
            string l_ResetStr = l_Resets.FirstOrDefault<string>();
            if (l_ResetStr == null)
                return null;

            DateTime l_Reset = new DateTime();
            if (!ulong.TryParse(l_ResetStr, out var l_ResetVal))
                return null;
            l_Reset = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            l_Reset = l_Reset.AddSeconds((double)l_ResetVal).ToLocalTime();

            return new RateLimitInfo() { Remaining = l_Remaining, Reset = l_Reset, Total = l_Total };
        }
    }
}
