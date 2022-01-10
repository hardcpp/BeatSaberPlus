using HarmonyLib;
using System.Collections.Generic;

namespace BeatSaberPlus.SDK.Game.Patches
{
    /// <summary>
    /// Level data finder
    /// </summary>
    [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO))]
    [HarmonyPatch(nameof(MultiplayerLevelScenesTransitionSetupDataSO.Init))]
    public class PMultiplayerLevelScenesTransitionSetupDataSO : MultiplayerLevelScenesTransitionSetupDataSO
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
        internal static void Postfix(ref MultiplayerLevelScenesTransitionSetupDataSO __instance,
                                     ref EnvironmentInfoSO                           ____multiplayerEnvironmentInfo,
                                     ref IDifficultyBeatmap                          difficultyBeatmap,
                                     ref IPreviewBeatmapLevel                        previewBeatmapLevel,
                                     ref GameplayModifiers                           gameplayModifiers,
                                     ref PlayerSpecificSettings                      playerSpecificSettings,
                                     ref PracticeSettings                            practiceSettings,
                                     ref ColorScheme                                 overrideColorScheme,
                                     ref bool                                        useTestNoteCutSoundEffects)
        {
            var l_LevelData = new LevelData()
            {
                Type = LevelType.Multiplayer,
                Data = new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, ____multiplayerEnvironmentInfo, overrideColorScheme)
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
        /// <param name="p_OtherPlayersLevelCompletionResults">Other player results</param>
        private static void OnDidFinishEvent(MultiplayerLevelScenesTransitionSetupDataSO p_Transition, MultiplayerResultsData p_MultiplayerResultsData)
        {
            if (m_LevelData == null)
                return;

            Logic.FireLevelEnded(new LevelCompletionData() { Type = LevelType.Multiplayer, Data = m_LevelData.Data, Results = p_MultiplayerResultsData?.localPlayerResultData?.multiplayerLevelCompletionResults?.levelCompletionResults ?? null });
            m_LevelData = null;
        }
    }
}
