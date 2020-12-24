using HarmonyLib;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// ObstacleSaberSparkleEffect remover
    /// </summary>
    [HarmonyPatch(typeof(ObstacleSaberSparkleEffectManager))]
    [HarmonyPatch(nameof(ObstacleSaberSparkleEffectManager.Start))]
    public class PObstacleSaberSparkleEffectManager : ObstacleSaberSparkleEffect
    {
        /// <summary>
        /// ObstacleSaberSparkleEffect instances
        /// </summary>
        private static ObstacleSaberSparkleEffect[] m_ObstacleSaberSparkleEffects = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____effects">ObstacleSaberSparkleEffect instances</param>
        internal static void Postfix(ref ObstacleSaberSparkleEffect[] ____effects)
        {
            m_ObstacleSaberSparkleEffects = ____effects;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetRemoveObstacleParticles(Config.GameTweaker.RemoveObstacleParticles);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is removed
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemoveObstacleParticles(bool p_Enabled)
        {
            if (m_ObstacleSaberSparkleEffects == null || m_ObstacleSaberSparkleEffects.Length == 0)
                return;

            foreach (var l_Current in m_ObstacleSaberSparkleEffects)
            {
                if (!l_Current || !l_Current.gameObject)
                    continue;

                l_Current.gameObject.SetActive(!p_Enabled);
            }
        }
    }
}
