using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// User interface utils
    /// </summary>
    public class GameUI
    {

        /// <summary>
        /// Sanitize user input of TextMeshPro tags.
        /// </summary>
        /// <param name="p_String">A <see cref="string"/> containing user input.</param>
        /// <returns>Sanitized <see cref="string"/>.</returns>
        public static string EscapeTextMeshProTags(string p_String)
        {
            return new StringBuilder(p_String).Replace("<", "<\u200B").Replace(">", "\u200B>").ToString();
        }


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////


    }
}
