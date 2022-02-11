using HarmonyLib;

namespace BeatSaberPlus.SDK.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch(nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
    public class PStandardLevelScenesTransitionSetupDataSO : StandardLevelScenesTransitionSetupDataSO
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
        internal static void Postfix(ref StandardLevelScenesTransitionSetupDataSO    __instance,
                                     ref IDifficultyBeatmap                          difficultyBeatmap,
                                     ref IPreviewBeatmapLevel                        previewBeatmapLevel,
                                     ref ColorScheme                                 overrideColorScheme,
                                     ref OverrideEnvironmentSettings                 overrideEnvironmentSettings,
                                     ref GameplayModifiers                           gameplayModifiers,
                                     ref PlayerSpecificSettings                      playerSpecificSettings,
                                     ref PracticeSettings                            practiceSettings,
                                     ref bool                                        useTestNoteCutSoundEffects)
        {
            EnvironmentInfoSO l_EnvironmentInfoSO = difficultyBeatmap.GetEnvironmentInfo();
            if (overrideEnvironmentSettings != null && overrideEnvironmentSettings.overrideEnvironments)
                l_EnvironmentInfoSO = overrideEnvironmentSettings.GetOverrideEnvironmentInfoForType(l_EnvironmentInfoSO.environmentType);

            m_LevelData = new LevelData()
            {
                Type = LevelType.Solo,
                Data = new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, l_EnvironmentInfoSO, overrideColorScheme)
            };

            Logic.FireLevelStarted(m_LevelData);

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
        private static void OnDidFinishEvent(StandardLevelScenesTransitionSetupDataSO p_Transition, LevelCompletionResults p_LevelCompletionResult)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Solo, Data = m_LevelData.Data, Results = p_LevelCompletionResult });
            m_LevelData = null;
        }
    }
}
