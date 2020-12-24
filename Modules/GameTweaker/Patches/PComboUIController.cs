using HarmonyLib;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// ComboUIController full combo loss animation remover
    /// </summary>
    [HarmonyPatch(typeof(ComboUIController))]
    [HarmonyPatch(nameof(ComboUIController.HandleComboBreakingEventHappened))]
    public class PComboUIController : ComboUIController
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">ComboUIController instance</param>
        internal static bool Prefix(ref ComboUIController __instance, ref bool ____comboLost)
        {
            if (!Config.GameTweaker.Enabled || !Config.GameTweaker.RemoveFullComboLossAnimation)
                return true; ///< Continue to original function

            if (!____comboLost)
            {
                ____comboLost = true;

                __instance.transform.Find("Line0")?.gameObject.SetActive(false);
                __instance.transform.Find("Line1")?.gameObject.SetActive(false);

                return false;   ///< Skip original function
            }

            return true; ///< Continue to original function
        }
    }
}
