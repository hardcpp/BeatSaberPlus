using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// SliderController patch
    /// </summary>
    [HarmonyPatch(typeof(SliderController))]
    [HarmonyPatch(nameof(SliderController.Init))]
    public class PSliderController : SliderController
    {
        private static bool m_Enabled = false;
        private static float m_Opacity = 1.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____initColor">Start color</param>
        internal static void Postfix(ref Color ____initColor)
        {
            if (m_Enabled)
                ____initColor = ____initColor.ColorWithAlpha(m_Opacity);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        /// <param name="p_OnSceneSwitch">Reset on scene switch</param>
        internal static void SetFromConfig(bool p_OnSceneSwitch)
        {
            m_Enabled = NTConfig.Instance.Enabled;
            m_Opacity = NTConfig.Instance.GetActiveProfile().ArcsIntensity;
        }
    }
}
