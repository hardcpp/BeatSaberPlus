using System;

namespace CP_SDK.Misc
{
    /// <summary>
    /// Base64 utils
    /// </summary>
    public class Base64
    {
        /// <summary>
        /// Base64 encode for URL without padding
        /// </summary>
        /// <param name="p_Buffer">Input buffer</param>
        /// <returns></returns>
        public static string URLEncodeNoPadding(byte[] p_Buffer)
        {
            var l_Result = Convert.ToBase64String(p_Buffer);
            l_Result = l_Result.Replace("+", "-");
            l_Result = l_Result.Replace("/", "_");
            l_Result = l_Result.Replace("=", "");

            return l_Result;
        }
    }
}
