using CP_SDK.Unity.Extensions;
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
        internal static void Postfix(BeatmapLevel beatmapLevel, bool isFavorite,
                                     ref TextMeshProUGUI ____songNameText)
        {
            if (GTConfig.Instance.LevelSelection.HighlightEnabled)
            {
                CP_SDK_BS.Game.Levels.GetScoresByLevelID(beatmapLevel.levelID, out var l_HaveAnyScore, out var l_HaveAllScores);

                var l_ColorPrefix = "";
                if (l_HaveAllScores)
                    l_ColorPrefix = "<" + ColorU.ToHexRGB(GTConfig.Instance.LevelSelection.HighlightAllPlayed) + ">";
                else if (l_HaveAnyScore)
                    l_ColorPrefix = "<" + ColorU.ToHexRGB(GTConfig.Instance.LevelSelection.HighlightPlayed) + ">";

                ____songNameText.text = l_ColorPrefix + ____songNameText.text;
            }
        }
    }
}
