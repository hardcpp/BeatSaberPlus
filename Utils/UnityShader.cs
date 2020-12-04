using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// Unity shader helper
    /// </summary>
    public class UnityShader
    {
        /// <summary>
        /// TextMeshPro no glow font shader
        /// </summary>
        private static Shader m_TMPNoGlowFontShader;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// TextMeshPro no glow font shader
        /// </summary>
        public static Shader TMPNoGlowFontShader
        {
            get
            {
                if (m_TMPNoGlowFontShader == null)
                    m_TMPNoGlowFontShader = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Last(f2 => f2.name == "Teko-Medium SDF No Glow")?.material?.shader;

                return m_TMPNoGlowFontShader;
            }
        }
    }
}
