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
#if BEATSABER_1_38_0_OR_NEWER
        internal static void Postfix(BeatmapLevel beatmapLevel, bool isFavorite,
                                     ref TextMeshProUGUI ____songNameText)
#elif BEATSABER_1_35_0_OR_NEWER
        internal static void Postfix(BeatmapLevel level, bool isFavorite,
                                     ref TextMeshProUGUI ____songNameText)
#else
        internal static void Postfix(IPreviewBeatmapLevel level, bool isFavorite,
                                     ref TextMeshProUGUI ____songNameText)
#endif
        {
            if (GTConfig.Instance.LevelSelection.HighlightEnabled)
            {
#if BEATSABER_1_38_0_OR_NEWER
                CP_SDK_BS.Game.Levels.GetScoresByLevelID(beatmapLevel.levelID, out var l_HaveAnyScore, out var l_HaveAllScores);
#else
                CP_SDK_BS.Game.Levels.GetScoresByLevelID(level.levelID, out var l_HaveAnyScore, out var l_HaveAllScores);
#endif

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
