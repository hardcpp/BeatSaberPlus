#if CP_SDK_UNITY
using System.Text;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Unity TextMeshPro tools
    /// </summary>
    public class TextMeshProU
    {
        /// <summary>
        /// Sanitize user input of TextMeshPro tags.
        /// </summary>
        /// <param name="p_String">A <see cref="string"/> containing user input.</param>
        /// <returns>Sanitized <see cref="string"/>.</returns>
        public static string EscapeString(string p_String)
        {
            return new StringBuilder(p_String).Replace("<", "<\u200B").Replace(">", "\u200B>").ToString();
        }
    }
}
#endif
