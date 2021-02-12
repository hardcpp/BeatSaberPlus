using HarmonyLib;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// LightSwitchEventEffect patch
    /// </summary>
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch(nameof(LightSwitchEventEffect.SetColor))]
    public class PLightSwitchEventEffect : LightSwitchEventEffect
    {
        /// <summary>
        /// Intensity cache
        /// </summary>
        private static float m_Intensity = 1.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="color">Input color</param>
        internal static void Prefix(ref UnityEngine.Color color)
        {
            if (m_Intensity == 1f)
                return;

            float l_Alpha = color.a;

            UnityEngine.Color.RGBToHSV(color, out var l_H, out var l_S, out var l_V);
            l_V *= m_Intensity;
            color = UnityEngine.Color.HSVToRGB(l_H, l_S, l_V);

            color.a = l_Alpha;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        internal static void SetFromConfig()
        {
            m_Intensity = (Config.GameTweaker.Enabled && Config.GameTweaker.AddOverrideLightIntensityOption) ? Config.GameTweaker.OverrideLightIntensity : 1.0f;
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_Intensity">New intensity</param>
        internal static void SetTemp(float p_Intensity)
        {
            m_Intensity = p_Intensity;
        }
    }
}
