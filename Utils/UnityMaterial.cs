using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// Unity material helper
    /// </summary>
    public class UnityMaterial
    {
        /// <summary>
        /// UI no glow material
        /// </summary>
        private static Material m_UINoGlowMaterial;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// UI no glow material
        /// </summary>
        public static Material UINoGlowMaterial
        {
            get
            {
                if (m_UINoGlowMaterial == null)
                {
                    m_UINoGlowMaterial = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.name == "UINoGlow").FirstOrDefault();

                    if (m_UINoGlowMaterial != null)
                        m_UINoGlowMaterial = Material.Instantiate(m_UINoGlowMaterial);
                }

                return m_UINoGlowMaterial;
            }
        }
    }
}
