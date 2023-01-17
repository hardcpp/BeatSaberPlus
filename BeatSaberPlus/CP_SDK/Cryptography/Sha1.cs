using System.Security.Cryptography;
using System.Text;

namespace CP_SDK.Cryptography
{
    /// <summary>
    /// Sha1 Helper
    /// </summary>
    public class SHA1
    {
        /// <summary>
        /// Get Sha1 hash of a string
        /// </summary>
        /// <param name="p_InputString">Input string</param>
        /// <returns>Bytes of Sha1 hash of InputString</returns>
        public static byte[] GetHash(byte[] p_Input)
        {
            HashAlgorithm l_Algorithm = System.Security.Cryptography.SHA1.Create();
            return l_Algorithm.ComputeHash(p_Input);
        }
        /// <summary>
        /// Get Sha1 hash of a string
        /// </summary>
        /// <param name="p_InputString">Input string</param>
        /// <returns>Bytes of Sha1 hash of InputString</returns>
        public static byte[] GetHash(string p_InputString)
        {
            HashAlgorithm l_Algorithm = System.Security.Cryptography.SHA1.Create();
            return l_Algorithm.ComputeHash(Encoding.UTF8.GetBytes(p_InputString));
        }
        /// <summary>
        /// Get Sha1 hash of a string
        /// </summary>
        /// <param name="p_Input">Input</param>
        /// <returns>Bytes of Sha1 hash of InputString</returns>
        public static string GetHashString(byte[] p_Input)
        {
            StringBuilder l_StringBuilder = new StringBuilder();
            foreach (byte l_Byte in GetHash(p_Input))
                l_StringBuilder.Append(l_Byte.ToString("X2"));

            return l_StringBuilder.ToString();
        }
        /// <summary>
        /// Get Sha1 string hash of InputString
        /// </summary>
        /// <param name="p_InputString">Input string</param>
        /// <returns>Sha1 string hash of InputString</returns>
        public static string GetHashString(string p_InputString)
        {
            StringBuilder l_StringBuilder = new StringBuilder();
            foreach (byte l_Byte in GetHash(p_InputString))
                l_StringBuilder.Append(l_Byte.ToString("X2"));

            return l_StringBuilder.ToString();
        }
    }
}
