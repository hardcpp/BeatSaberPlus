using HarmonyLib;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// Debris remover
    /// </summary>
    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch(nameof(NoteDebrisSpawner.SpawnDebris))]
    internal class PNoteDebrisSpawner
    {
        /// <summary>
        /// Prefix
        /// </summary>
        private static bool Prefix() => !(Config.GameTweaker.Enabled && Config.GameTweaker.RemoveDebris);
    }
}
