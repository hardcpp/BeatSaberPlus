using HarmonyLib;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// PauseController back confirm
    /// </summary>
    [HarmonyPatch(typeof(PauseController))]
    [HarmonyPatch(nameof(PauseController.HandlePauseMenuManagerDidPressMenuButton))]
    internal class PPauseController_Back
    {
        /// <summary>
        /// Prefix
        /// </summary>
        private static bool Prefix()
        {
            if (!Config.GameTweaker.Enabled || Config.GameTweaker.FPFCEscape || !Config.GameTweaker.SongBackButtonConfirm)
                return true;

            return true;
        }
    }
    /// <summary>
    /// PauseController restart confirm
    /// </summary>
    [HarmonyPatch(typeof(PauseController))]
    [HarmonyPatch(nameof(PauseController.HandlePauseMenuManagerDidPressRestartButton))]
    internal class PPauseController_Restart
    {
        /// <summary>
        /// Prefix
        /// </summary>
        private static bool Prefix()
        {
            if (!Config.GameTweaker.Enabled || Config.GameTweaker.FPFCEscape || !Config.GameTweaker.SongRestartButtonConfirm)
                return true;

            return true;
        }
    }
}
