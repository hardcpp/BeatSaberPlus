using HarmonyLib;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// SaberBurnMarkSparkles remover
    /// </summary>
    [HarmonyPatch(typeof(SaberBurnMarkSparkles))]
    [HarmonyPatch(nameof(SaberBurnMarkSparkles.Start))]
    public class PSaberBurnMarkSparkles : SaberBurnMarkSparkles
    {
        /// <summary>
        /// SaberBurnMarkSparkles instance
        /// </summary>
        private static SaberBurnMarkSparkles m_SaberBurnMarkSparkles = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">SaberBurnMarkSparkles instance</param>
        internal static void Postfix(ref SaberBurnMarkSparkles __instance)
        {
            m_SaberBurnMarkSparkles = __instance;

            if (GTConfig.Instance.Enabled)
            {
                /// Apply
                SetRemoveSaberBurnMarkSparkles(GTConfig.Instance.RemoveSaberBurnMarkSparkles);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is removed
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemoveSaberBurnMarkSparkles(bool p_Enabled)
        {
            if (m_SaberBurnMarkSparkles == null || !m_SaberBurnMarkSparkles)
                return;

            m_SaberBurnMarkSparkles.gameObject.SetActive(!p_Enabled);
        }
    }
}
