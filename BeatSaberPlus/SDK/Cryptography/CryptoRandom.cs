using System.Security.Cryptography;
using System.Text;

namespace BeatSaberPlus.SDK.Cryptography
{
    /// <summary>
    /// CryptoRandom Helper
    /// </summary>
    public class CryptoRandom
    {
        /// <summary>
        /// Get random data
        /// </summary>
        /// <param name="p_Length">Data length</param>
        /// <returns></returns>
        public static byte[] RandomData(uint p_Length)
        {
            RNGCryptoServiceProvider l_CryptoServiceProvider = new RNGCryptoServiceProvider();
            var l_Result = new byte[p_Length];
            l_CryptoServiceProvider.GetBytes(l_Result);

            return l_Result;
        }
    }
}
