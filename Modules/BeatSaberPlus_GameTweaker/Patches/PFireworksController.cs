using HarmonyLib;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// FireworksController disabler
    /// </summary>
    [HarmonyPatch(typeof(FireworksController))]
    [HarmonyPatch(nameof(FireworksController.OnEnable))]
    public class PFireworksController : FireworksController
    {
        /// <summary>
        /// Prefix
        /// </summary>
        internal static bool Prefix()
        {
            if (!GTConfig.Instance.Enabled || !GTConfig.Instance.MainMenu.DisableFireworks)
                return true; ///< Continue to original function

            return false;   ///< Skip original function
        }
    }
}
