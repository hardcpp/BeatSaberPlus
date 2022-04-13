using HarmonyLib;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// SliderHapticFeedbackInteractionEffect patch
    /// </summary>
    [HarmonyPatch(typeof(SliderHapticFeedbackInteractionEffect))]
    [HarmonyPatch("StartEffect")]
    public class PSliderHapticFeedbackInteractionEffect : SliderHapticFeedbackInteractionEffect
    {
        private static bool m_Enabled = false;
        private static bool m_Haptic = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        internal static bool Prefix()
        {
            /// Forward to base method
            if (!m_Enabled || m_Haptic)
                return true;

            /// Interrupt base method (Preventing behaviour enabling)
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        /// <param name="p_OnSceneSwitch">Reset on scene switch</param>
        internal static void SetFromConfig(bool p_OnSceneSwitch)
        {
            m_Enabled   = NTConfig.Instance.Enabled;
            m_Haptic    = NTConfig.Instance.GetActiveProfile().ArcsHaptics;
        }
    }
}
