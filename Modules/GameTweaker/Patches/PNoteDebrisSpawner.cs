using HarmonyLib;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// Debris remover
    /// </summary>
    [HarmonyPatch(typeof(NoteDebrisSpawner))]
    [HarmonyPatch(nameof(NoteDebrisSpawner.SpawnDebris))]
    public class PNoteDebrisSpawner
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
        public static void SetFromConfig()
        {
            m_RemoveDebris = GTConfig.Instance.Enabled && GTConfig.Instance.RemoveDebris;
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_RemoveDebris">Should remove debris</param>
        public static void SetTemp(bool p_RemoveDebris)
        {
            m_RemoveDebris = p_RemoveDebris;
        }
    }
}
