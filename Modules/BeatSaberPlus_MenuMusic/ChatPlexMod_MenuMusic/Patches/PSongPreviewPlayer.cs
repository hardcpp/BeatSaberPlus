using HarmonyLib;

namespace ChatPlexMod_MenuMusic.Patches
{
    /// <summary>
    /// PauseController patcher
    /// </summary>
    [HarmonyPatch(typeof(SongPreviewPlayer))]
    [HarmonyPatch(nameof(SongPreviewPlayer.CrossfadeToNewDefault))]
    internal class PSongPreviewPlayer : PauseController
    {
        internal static bool Prefix()
        {
            if (MMConfig.Instance.Enabled)
                return false;   ///< Skip original function

            return true; ///< Continue to original function
        }
    }
}
