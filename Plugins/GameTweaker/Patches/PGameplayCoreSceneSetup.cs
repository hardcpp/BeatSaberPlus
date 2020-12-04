using HarmonyLib;
using System.Collections.Generic;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// 360 HUD remover
    /// </summary>
    [HarmonyPatch(typeof(GameplayCoreInstaller))]
    [HarmonyPatch(nameof(GameplayCoreInstaller.InstallBindings))]
    public class PGameplayCoreSceneSetup : GameplayCoreInstaller
    {
        /// <summary>
        /// Beatmap to restore
        /// </summary>
        static private BeatmapData m_BeatMapToRestore = null;
        /// <summary>
        /// Value to restore
        /// </summary>
        static private int m_BeatMapToRestoreValue = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">GameplayCoreInstaller instance</param>
        internal static void Prefix(ref GameplayCoreInstaller __instance, ref GameplayCoreSceneSetupData ____sceneSetupData)
        {
            if (!(Config.GameTweaker.Enabled && Config.GameTweaker.NoFake360HUD))
                return;

            IDifficultyBeatmap              l_IDifficultyBeatmap        = ____sceneSetupData.difficultyBeatmap;
            IReadOnlyList<BeatmapEventData> l_BeatmapEventData          = l_IDifficultyBeatmap.beatmapData.beatmapEventsData;
            SpawnRotationProcessor          l_SpawnRotationProcessor    = new SpawnRotationProcessor();
            bool                            l_Is360                     = false;

            foreach (BeatmapEventData l_CurrentEvent in l_BeatmapEventData)
            {
                if (!(l_CurrentEvent.type == BeatmapEventType.Event14 || l_CurrentEvent.type == BeatmapEventType.Event15))
                    continue;

                if (l_SpawnRotationProcessor.RotationForEventValue(l_CurrentEvent.value) != 0)
                {
                    l_Is360 = true;
                    break;
                }
            }

            if (!l_Is360)
            {
                /// Backup data
                m_BeatMapToRestore      = l_IDifficultyBeatmap.beatmapData;
                m_BeatMapToRestoreValue = l_IDifficultyBeatmap.beatmapData.spawnRotationEventsCount;

                /// Add on scene change event callback
                Utils.Game.OnSceneChange += Game_OnSceneChange;

                /// Update rotation count
                IPA.Utilities.ReflectionUtil.SetProperty<BeatmapData, int>(l_IDifficultyBeatmap.beatmapData, "spawnRotationEventsCount", 0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_SceneType"></param>
        private static void Game_OnSceneChange(Utils.Game.SceneType p_SceneType)
        {
            if (p_SceneType != Utils.Game.SceneType.Menu)
                return;

            /// Restore rotation count
            if (m_BeatMapToRestore != null)
                IPA.Utilities.ReflectionUtil.SetProperty<BeatmapData, int>(m_BeatMapToRestore, "spawnRotationEventsCount", m_BeatMapToRestoreValue);

            /// Remove on scene change event callback
            Utils.Game.OnSceneChange -= Game_OnSceneChange;

            /// Remove reference
            m_BeatMapToRestore = null;
        }
    }
}
