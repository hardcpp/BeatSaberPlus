using System.Linq;

namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Unity material helper
    /// </summary>
    public class MaterialU
    {
        /// <summary>
        /// UI no glow material
        /// </summary>
        private static UnityEngine.Material m_UINoGlowMaterial;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// UI no glow material
        /// </summary>
        public static UnityEngine.Material UINoGlowMaterial
        {
            get
            {
                if (m_UINoGlowMaterial == null)
                {
                    m_UINoGlowMaterial = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Material>().Where(x => x.name == "UINoGlow").FirstOrDefault();

                    if (m_UINoGlowMaterial != null)
                        m_UINoGlowMaterial = UnityEngine.Material.Instantiate(m_UINoGlowMaterial);
                }

                return m_UINoGlowMaterial;
            }
        }
    }
}
