using BeatSaberMarkupLanguage.Parser;

namespace BeatSaberPlus.SDK.UI
{
    /// <summary>
    /// BSML Setting formatter
    /// </summary>
    public class BSMLSettingFormarter
    {
        private static BSMLAction m_DateMonthFrom2018;
        private static BSMLAction m_Time;
        private static BSMLAction m_Seconds;
        private static BSMLAction m_Minutes;
        private static BSMLAction m_Percentage;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static BSMLAction DateMonthFrom2018  { get { if (m_DateMonthFrom2018 == null)    m_DateMonthFrom2018 = BuildAction(nameof(FNDateMonthFrom2018)); return m_DateMonthFrom2018; } }
        public static BSMLAction Time               { get { if (m_Time == null)                 m_Time = BuildAction(nameof(FNTime));                           return m_Time; } }
        public static BSMLAction Seconds            { get { if (m_Seconds == null)              m_Seconds = BuildAction(nameof(FNSeconds));                     return m_Seconds; } }
        public static BSMLAction Minutes            { get { if (m_Minutes == null)              m_Minutes = BuildAction(nameof(FNMinutes));                     return m_Minutes; } }
        public static BSMLAction Percentage         { get { if (m_Percentage == null)           m_Percentage = BuildAction(nameof(FNPercentage));               return m_Percentage; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static BSMLAction BuildAction(string p_Name)
        {
            return new BSMLAction(null, typeof(BSMLSettingFormarter).GetMethod(p_Name, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On date setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string FNDateMonthFrom2018(int p_Value)
        {
            string[] s_Months = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            int l_Year = 2018 + (p_Value / 12);

            return s_Months[p_Value % 12] + " " + l_Year;
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string FNTime(int p_Value)
        {
            int l_Seconds = p_Value / 60;
            int l_Minutes = (p_Value > 60) ? (p_Value - l_Seconds) / 60 : 0;

            string l_Result = (l_Minutes != 0 ? l_Minutes : l_Seconds).ToString();
            if (l_Minutes != 0)
                l_Result += "m " + l_Seconds + "s";

            return l_Result;
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string FNSeconds(int p_Value)
        {
            return p_Value + " Second" + (p_Value > 1 ? "s" : "");
        }
        /// <summary>
        /// On tile setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string FNMinutes(int p_Value)
        {
            return p_Value + " Minute" + (p_Value > 1 ? "s" : "");
        }
        /// <summary>
        /// On percentage setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        /// <returns></returns>
        private static string FNPercentage(float p_Value)
        {
            return System.Math.Round(p_Value * 100f, 2) + " %";
        }
    }
}
