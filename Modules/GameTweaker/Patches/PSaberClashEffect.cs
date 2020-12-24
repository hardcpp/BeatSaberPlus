using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// SaberClashEffect remover
    /// </summary>
    [HarmonyPatch(typeof(SaberClashEffect))]
    [HarmonyPatch(nameof(SaberClashEffect.Start))]
    public class PSaberClashEffect : SaberClashEffect
    {
        /// <summary>
        /// SparkleParticleSystem instance
        /// </summary>
        private static ParticleSystem m_SparkleParticleSystem   = null;
        /// <summary>
        /// GlowParticleSystem instance
        /// </summary>
        private static ParticleSystem m_GlowParticleSystem      = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____sparkleParticleSystem">SparkleParticleSystem instance</param>
        /// <param name="____glowParticleSystem">GlowParticleSystem instance</param>
        internal static void Postfix(ref ParticleSystem ____sparkleParticleSystem, ref ParticleSystem ____glowParticleSystem)
        {
            /// Cache effect instance
            m_SparkleParticleSystem = ____sparkleParticleSystem;
            m_GlowParticleSystem    = ____glowParticleSystem;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetRemoveClashEffects(Config.GameTweaker.RemoveSaberClashEffects);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect remover is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemoveClashEffects(bool p_Enabled)
        {
            if (p_Enabled)
            {
                if (m_SparkleParticleSystem != null) m_SparkleParticleSystem?.Pause();
                if (m_GlowParticleSystem != null)    m_GlowParticleSystem?.Pause();
            }
            else
            {
                if (m_SparkleParticleSystem != null) m_SparkleParticleSystem?.Play();
                if (m_GlowParticleSystem != null)    m_GlowParticleSystem?.Play();
            }
        }
    }
}
