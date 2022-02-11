using System.Text;

namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Unity TextMeshPro tools
    /// </summary>
    public class TextMeshPro
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
