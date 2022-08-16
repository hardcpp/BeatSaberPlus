using System.Security.Cryptography;
using System.Text;

namespace CP_SDK.Cryptography
{
    /// <summary>
    /// Sha256 Helper
    /// </summary>
    public class SHA256
    {
        /// <summary>
        /// Get Sha256 hash of a string
        /// </summary>
        /// <param name="p_InputString">Input string</param>
        /// <returns>Bytes of Sha1 hash of InputString</returns>
        public static byte[] GetHash(string p_InputString)
        {
            HashAlgorithm l_Algorithm = System.Security.Cryptography.SHA256.Create();
            return l_Algorithm.ComputeHash(Encoding.UTF8.GetBytes(p_InputString));
        }
        /// <summary>
        /// Get Sha256 hash of a string
        /// </summary>
        /// <param name="p_InputString">Input string</param>
        /// <returns>Bytes of Sha1 hash of InputString</returns>
        public static byte[] GetHashASCII(string p_InputString)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(p_InputString);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }
        /// <summary>
        /// Get Sha256 string hash of InputString
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
