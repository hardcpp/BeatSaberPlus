using BS_Utils.Utilities;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Plugins.RankedAssistant
{
    /// <summary>
    /// Ranked Assistant instance
    /// </summary>
    class RankedAssistant : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Ranked Assistant";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.RankedAssistant.Enabled; set => Config.RankedAssistant.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.Never;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.Main m_MainView = null;
        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.MainLeft m_MainLeftView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnEnable()
        {

          //SoloFreePlayFlowCoordinator freePlayCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
          //PlayerDataModel dataModel = freePlayCoordinator.GetPrivateField<PlayerDataModel>("_playerDataModel");
          //
          //var playerLevelStats = dataModel.playerData.GetPlayerLevelStatsData(difficultyBeatmap);
          //
          //playerLevelStats.UpdateScoreData(levelCompletionResults.modifiedScore, levelCompletionResults.maxCombo, levelCompletionResults.fullCombo, levelCompletionResults.rank);
          //freePlayCoordinator.GetPrivateField<PlatformLeaderboardsModel>("_platformLeaderboardsModel").UploadScore(difficultyBeatmap, levelCompletionResults.rawScore, levelCompletionResults.modifiedScore, levelCompletionResults.fullCombo, levelCompletionResults.goodCutsCount, levelCompletionResults.badCutsCount, levelCompletionResults.missedCount, levelCompletionResults.maxCombo, levelCompletionResults.gameplayModifiers);
          //
        }
        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnDisable()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected override void ShowUIImplementation()
        {
            /// Create view if needed
            if (m_MainView == null)
                m_MainView = BeatSaberMarkupLanguage.BeatSaberUI.CreateViewController<UI.Main>();
            /// Create view if needed
            if (m_MainLeftView == null)
                m_MainLeftView = BeatSaberMarkupLanguage.BeatSaberUI.CreateViewController<UI.MainLeft>();

            /// Change main view
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_MainView, m_MainLeftView);
        }
    }
}
