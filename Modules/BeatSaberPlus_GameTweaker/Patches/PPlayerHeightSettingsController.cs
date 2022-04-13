using HarmonyLib;
using TMPro;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// PlayerHeightSettingsController remover
    /// </summary>
    [HarmonyPatch(typeof(PlayerHeightSettingsController))]
    [HarmonyPatch(nameof(PlayerHeightSettingsController.RefreshUI))]
    public class PPlayerHeightSettingsController : PlayerHeightSettingsController
    {
        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____text">Target text display</param>
        /// <param name="____value">Player height meters</param>
        internal static void Postfix(ref TextMeshProUGUI ____text, ref float ____value)
        {
            if (GTConfig.Instance.Enabled)
                ____text.text = string.Format("{0:0.00}", ____value);
        }
    }
}
