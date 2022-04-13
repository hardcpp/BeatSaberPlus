using HarmonyLib;
using IPA.Utilities;

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
        internal static void Postfix(ref MultiplayerLevelScenesTransitionSetupDataSO __instance)
        {
            var l_LevelData = new LevelData()
            {
                Type = LevelType.Multiplayer,
                Data = __instance.GetProperty<GameplayCoreSceneSetupData, LevelScenesTransitionSetupDataSO>("gameplayCoreSceneSetupData")
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
