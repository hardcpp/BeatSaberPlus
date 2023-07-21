using System;
using System.Globalization;

namespace CP_SDK.Misc
{
    /// <summary>
    /// Time helper
    /// </summary>
    public static class Time
    {
        private static readonly DateTime s_UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static string[] s_Months = new string[] {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "October",
            "November",
            "December"
        };
        private static string[] s_MonthsShort = new string[] {
            "Jan.",
            "Feb.",
            "Mar.",
            "Apr.",
            "May",
            "Jun.",
            "Jul.",
            "Aug.",
            "Sept.",
            "Oct.",
            "Nov.",
            "Dec."
        };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static string[] MonthNames       => s_Months;
        public static string[] MonthNamesShort  => s_MonthsShort;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get UnixTimestamp
        /// </summary>
        /// <returns>Unix timestamp</returns>
        public static Int64 UnixTimeNow()
            => (Int64)(DateTime.UtcNow - s_UnixEpoch).TotalSeconds;
        /// <summary>
        /// Get UnixTimestamp
        /// </summary>
        /// <returns>Unix timestamp</returns>
        public static Int64 UnixTimeNowMS()
            => (Int64)(DateTime.UtcNow - s_UnixEpoch).TotalMilliseconds;
        /// <summary>
        /// Convert DateTime to UnixTimestamp
        /// </summary>
        /// <param name="p_DateTime">The DateTime to convert</param>
        /// <returns></returns>
        public static Int64 ToUnixTime(DateTime p_DateTime)
            => (Int64)p_DateTime.ToUniversalTime().Subtract(s_UnixEpoch).TotalSeconds;
        /// <summary>
        /// Convert DateTime to UnixTimestamp
        /// </summary>
        /// <param name="p_DateTime">The DateTime to convert</param>
        /// <returns></returns>
        public static Int64 ToUnixTimeMS(DateTime p_DateTime)
            => (Int64)p_DateTime.ToUniversalTime().Subtract(s_UnixEpoch).TotalMilliseconds;
        /// <summary>
        /// Convert UnixTimestamp to DateTime
        /// </summary>
        /// <param name="p_TimeStamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTime(Int64 p_TimeStamp)
            => s_UnixEpoch.AddSeconds(p_TimeStamp).ToLocalTime();
        /// <summary>
        /// Convert UnixTimestamp to DateTime
        /// </summary>
        /// <param name="p_TimeStamp"></param>
        /// <returns></returns>
        public static DateTime FromUnixTimeMS(Int64 p_TimeStamp)
            => s_UnixEpoch.AddMilliseconds(p_TimeStamp).ToLocalTime();
        /// <summary>
        /// Try parse international data
        /// </summary>
        /// <param name="p_Input"></param>
        /// <param name="p_Result"></param>
        /// <returns></returns>
        public static bool TryParseInternational(string p_Input, out DateTime p_Result)
            => DateTime.TryParse(p_Input, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out p_Result);
    }
}
