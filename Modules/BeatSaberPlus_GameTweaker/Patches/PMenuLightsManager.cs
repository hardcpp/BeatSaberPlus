using HarmonyLib;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// Preset replacer
    /// </summary>
    [HarmonyPatch(typeof(MenuLightsManager))]
    [HarmonyPatch(nameof(MenuLightsManager.SetColorPreset))]
    [HarmonyPriority(Priority.First)]
    public class PMenuLightsManager : MenuLightsManager
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="preset">Preset data</param>
        internal static void Prefix(ref MenuLightsPresetSO preset)
            => preset = Managers.CustomMenuLightManager.GetPresetForPatch(preset);
    }
}
