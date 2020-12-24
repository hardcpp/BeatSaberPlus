using HarmonyLib;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// SaberBurnMarkArea remover
    /// </summary>
    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch(nameof(SaberBurnMarkArea.Start))]
    public class PSaberBurnMarkArea : SaberBurnMarkArea
    {
        /// <summary>
        /// SaberBurnMarkArea instance
        /// </summary>
        private static SaberBurnMarkArea m_SaberBurnMarkArea = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">SaberBurnMarkArea instance</param>
        internal static void Postfix(ref SaberBurnMarkArea __instance)
        {
            m_SaberBurnMarkArea = __instance;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetRemoveSaberBurnMarks(Config.GameTweaker.RemoveSaberBurnMarks);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is removed
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemoveSaberBurnMarks(bool p_Enabled)
        {
            if (m_SaberBurnMarkArea == null || !m_SaberBurnMarkArea)
                return;

            m_SaberBurnMarkArea.gameObject.SetActive(!p_Enabled);
        }
    }
}
