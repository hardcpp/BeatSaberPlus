using HarmonyLib;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// Debris remover
    /// </summary>
    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch(nameof(NoteDebrisSpawner.SpawnDebris))]
    internal class PNoteDebrisSpawner
    {
        /// <summary>
        /// Remove debris cache
        /// </summary>
        private static bool m_RemoveDebris = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        private static bool Prefix() => !m_RemoveDebris;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        internal static void SetFromConfig()
        {
            m_RemoveDebris = Config.GameTweaker.Enabled && Config.GameTweaker.RemoveDebris;
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_RemoveDebris">Should remove debris</param>
        internal static void SetTemp(bool p_RemoveDebris)
        {
            m_RemoveDebris = p_RemoveDebris;
        }
    }
}
