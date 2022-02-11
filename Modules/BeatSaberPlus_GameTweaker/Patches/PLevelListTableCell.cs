using HarmonyLib;
using TMPro;

namespace BeatSaberPlus_GameTweaker.Patches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch(nameof(LevelListTableCell.SetDataFromLevelAsync))]
    public class PLevelListTableCell
    {
        /// <summary>
        /// DidActivate
        /// </summary>
        /// <param name="____beatmapEditorButton">BeatMap editor button instance</param>
        /// <param name="____musicPackPromoBanner">Promo banner instance</param>
        internal static void Postfix(IPreviewBeatmapLevel level, bool isFavorite,
                                     ref TextMeshProUGUI ____songNameText)
        {
            if (GTConfig.Instance.HighlightPlayedSong && level.levelID.StartsWith("custom_level_"))
            {
                var l_LevelHash = level.levelID.Substring("custom_level_".Length);
                BeatSaberPlus.SDK.Game.Levels.GetScoresByHash(l_LevelHash, out var l_HaveAnyScore, out var l_HaveAllScores);

                var l_ColorPrefix = "";
                if (l_HaveAllScores)
                    l_ColorPrefix = "<#52F700>";
                else if (l_HaveAnyScore)
                    l_ColorPrefix = "<#F8E600>";

                ____songNameText.text = l_ColorPrefix + ____songNameText.text;
            }
        }
    }
}
