﻿using HarmonyLib;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// Cut ribbon remover
    /// </summary>
    [HarmonyPatch(typeof(NoteCutCoreEffectsSpawner))]
    [HarmonyPatch(nameof(NoteCutCoreEffectsSpawner.Start))]
    public class PNoteCutCoreEffectsSpawner : NoteCutCoreEffectsSpawner
    {
        /// <summary>
        /// NoteCutParticlesEffect instance
        /// </summary>
        private static NoteCutParticlesEffect m_NoteCutParticlesEffect = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____noteCutParticlesEffect">NoteCutParticlesEffect instance</param>
        internal static void Postfix(ref NoteCutParticlesEffect ____noteCutParticlesEffect)
        {
            /// Cache effect instance
            m_NoteCutParticlesEffect = ____noteCutParticlesEffect;

            if (GTConfig.Instance.Enabled)
            {
                /// Apply
                SetRemoveCutParticles(GTConfig.Instance.RemoveAllCutParticles);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemoveCutParticles(bool p_Enabled)
        {
            if (m_NoteCutParticlesEffect == null || !m_NoteCutParticlesEffect)
                return;

            m_NoteCutParticlesEffect.gameObject.SetActive(!p_Enabled);
        }
    }
}
