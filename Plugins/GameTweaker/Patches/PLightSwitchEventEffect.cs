using HarmonyLib;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// LightSwitchEventEffect patch
    /// </summary>
    [HarmonyPatch(typeof(LightSwitchEventEffect))]
    [HarmonyPatch(nameof(LightSwitchEventEffect.SetColor))]
    public class PLightSwitchEventEffect : LightSwitchEventEffect
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="color">Input color</param>
        internal static void Prefix(ref UnityEngine.Color color)
        {
            if (   !Config.GameTweaker.Enabled
                || !Config.GameTweaker.AddOverrideLightIntensityOption
                || (BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.playerSpecificSettings?.staticLights ?? false))
                return;

            float l_Alpha = color.a;

            UnityEngine.Color.RGBToHSV(color, out var l_H, out var l_S, out var l_V);
            l_V *= Config.GameTweaker.OverrideLightIntensity;
            color = UnityEngine.Color.HSVToRGB(l_H, l_S, l_V);

            color.a = l_Alpha;
        }
    }
}
