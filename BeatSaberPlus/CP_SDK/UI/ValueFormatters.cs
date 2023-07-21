using System;

namespace CP_SDK.UI
{
    public static class ValueFormatters
    {
        public static string Percentage(float p_Value)
            => Math.Round(p_Value * 100f, 2) + " %";

        public static string Minutes(float p_Value)
            => ((int)p_Value) + " Minute" + (((int)p_Value) > 1 ? "s" : "");

        public static string TimeShortBaseSeconds(float p_Value)
        {
            const int SECONDS_PER_MINUTE = 60;
            const int SECONDS_PER_HOUR   = 60 * SECONDS_PER_MINUTE;
            const int SECONDS_PER_DAY    = 24 * SECONDS_PER_HOUR;

            int l_TotalSeconds = (int)p_Value;

            int l_Days = l_TotalSeconds / SECONDS_PER_DAY;
            l_TotalSeconds -= l_Days * SECONDS_PER_DAY;

            int l_Hours = l_TotalSeconds / SECONDS_PER_HOUR;
            l_TotalSeconds -= l_Hours * SECONDS_PER_HOUR;

            int l_Minutes = l_TotalSeconds / SECONDS_PER_MINUTE;
            l_TotalSeconds -= l_Minutes * SECONDS_PER_MINUTE;

            if (l_Days > 0)
                return $"{l_Days}d {l_Hours}h {l_Minutes}m {l_TotalSeconds}s";
            else if (l_Hours > 0)
                return $"{l_Hours}h {l_Minutes}m {l_TotalSeconds}s";
            else if (l_Minutes > 0)
                return $"{l_Minutes}m {l_TotalSeconds}s";
            else
                return $"{l_TotalSeconds}s";
        }

        public static string MillisecondsShort(float p_Value)
            => ((int)p_Value) + "ms";

        public static string DateMonthFrom2018(float p_Value)
        {
            int l_Year = 2018 + (((int)p_Value) / 12);
            return Misc.Time.MonthNames[((int)p_Value) % 12] + " " + l_Year;
        }
        public static string DateMonthFrom2018Short(float p_Value)
        {
            int l_Year = 2018 + (((int)p_Value) / 12);
            return Misc.Time.MonthNamesShort[((int)p_Value) % 12] + " " + l_Year;
        }
    }
}
