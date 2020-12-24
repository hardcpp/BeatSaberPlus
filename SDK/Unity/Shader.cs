using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Unity shader helper
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// TextMeshPro no glow font shader
        /// </summary>
        private static UnityEngine.Shader m_TMPNoGlowFontShader;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// TextMeshPro no glow font shader
        /// </summary>
        public static UnityEngine.Shader TMPNoGlowFontShader
        {
            get
            {
                if (m_TMPNoGlowFontShader == null)
                    m_TMPNoGlowFontShader = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().Last(x => x.name == "Teko-Medium SDF No Glow")?.material?.shader;

                return m_TMPNoGlowFontShader;
            }
        }
    }
}
