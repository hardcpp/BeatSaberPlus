using HarmonyLib;

namespace BeatSaberPlus.SDK.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch(nameof(MissionLevelScenesTransitionSetupDataSO.Init))]
    public class PMissionLevelScenesTransitionSetupDataSO : MissionLevelScenesTransitionSetupDataSO
    {
        /// <summary>
        /// Level data cache
        /// </summary>
        static private LevelData m_LevelData = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        internal static void Postfix(ref MissionLevelScenesTransitionSetupDataSO __instance,
                                     ref IDifficultyBeatmap                      difficultyBeatmap,
                                     ref IPreviewBeatmapLevel                    previewBeatmapLevel,
                                     ref ColorScheme                             overrideColorScheme,
                                     ref GameplayModifiers                       gameplayModifiers,
                                     ref PlayerSpecificSettings                  playerSpecificSettings)
        {
            EnvironmentInfoSO l_EnvironmentInfoSO = difficultyBeatmap.GetEnvironmentInfo();

            var l_LevelData = new LevelData()
            {
                Type = LevelType.Solo,
                Data = new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, PracticeSettings.defaultPracticeSettings, false, l_EnvironmentInfoSO, overrideColorScheme)
            };

            Logic.FireLevelStarted(l_LevelData);

            m_LevelData = l_LevelData;

            __instance.didFinishEvent -= OnDidFinishEvent;
            __instance.didFinishEvent += OnDidFinishEvent;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On level finish
        /// </summary>
        /// <param name="p_Transition">Transition data</param>
        /// <param name="p_LevelCompletionResult">Completion result</param>
        private static void OnDidFinishEvent(MissionLevelScenesTransitionSetupDataSO p_Transition, MissionCompletionResults p_LevelCompletionResult)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Solo, Data = m_LevelData.Data, Results = p_LevelCompletionResult.levelCompletionResults });
            m_LevelData = null;
        }
    }
}
