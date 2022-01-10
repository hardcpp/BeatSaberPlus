using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Level helper
    /// </summary>
    public class Levels
    {
        /// <summary>
        /// Get level cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetLevelCancellationTokenSource;
        /// <summary>
        /// Get status cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetStatusCancellationTokenSource;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Has a DLC level
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <param name="p_AdditionalContentModel">Additional content</param>
        /// <returns></returns>
        public static async Task<bool> HasDLCLevel(string p_LevelID, AdditionalContentModel p_AdditionalContentModel = null)
        {
            /*
               Code from https://github.com/MatrikMoon/TournamentAssistant

               MIT License

               Permission is hereby granted, free of charge, to any person obtaining a copy
               of this software and associated documentation files (the "Software"), to deal
               in the Software without restriction, including without limitation the rights
               to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
               copies of the Software, and to permit persons to whom the Software is
               furnished to do so, subject to the following conditions:

               The above copyright notice and this permission notice shall be included in all
               copies or substantial portions of the Software.

               THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
               IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
               FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
               AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
               LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
               OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
               SOFTWARE.
            */
            p_AdditionalContentModel = p_AdditionalContentModel ?? Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();
            if (p_AdditionalContentModel != null)
            {
                m_GetStatusCancellationTokenSource?.Cancel();
                m_GetStatusCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetStatusCancellationTokenSource.Token;

                return await p_AdditionalContentModel.GetLevelEntitlementStatusAsync(p_LevelID, l_Token) == AdditionalContentModel.EntitlementStatus.Owned;
            }

            return false;
        }
        /// <summary>
        /// Load a song by ID
        /// </summary>
        /// <param name="p_LevelID">Song ID</param>
        /// <param name="p_LoadCallback">Load callback</param>
        public static async Task LoadSong(string p_LevelID, Action<IBeatmapLevel> p_LoadCallback)
        {
            /*
               Code from https://github.com/MatrikMoon/TournamentAssistant

               MIT License

               Permission is hereby granted, free of charge, to any person obtaining a copy
               of this software and associated documentation files (the "Software"), to deal
               in the Software without restriction, including without limitation the rights
               to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
               copies of the Software, and to permit persons to whom the Software is
               furnished to do so, subject to the following conditions:

               The above copyright notice and this permission notice shall be included in all
               copies or substantial portions of the Software.

               THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
               IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
               FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
               AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
               LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
               OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
               SOFTWARE.
            */

            IPreviewBeatmapLevel l_Level = Loader.CustomLevelsCollection.beatmapLevels.Where(x => x.levelID == p_LevelID).First();

            /// Load IBeatmapLevel
            if (l_Level is PreviewBeatmapLevelSO || l_Level is CustomPreviewBeatmapLevel)
            {
                if (l_Level is PreviewBeatmapLevelSO)
                {
                    if (!await HasDLCLevel(l_Level.levelID))
                        return; /// In the case of unowned DLC, just bail out and do nothing
                }

                var l_Result = await GetLevelFromPreview(l_Level);
                if (l_Result != null && !(l_Result?.isError == true))
                {
                    /// HTTPstatus requires cover texture to be applied in here
                    var l_LoadedLevel = l_Result?.beatmapLevel;
                    //l_LoadedLevel.SetField("_coverImageTexture2D", l_Level.GetField<Texture2D>("_coverImageTexture2D"));

                    p_LoadCallback(l_LoadedLevel);
                }
            }
            else if (l_Level is BeatmapLevelSO)
            {
                p_LoadCallback(l_Level as IBeatmapLevel);
            }
        }
        /// <summary>
        /// Play a loaded song
        /// </summary>
        /// <param name="p_Level">Loaded level</param>
        /// <param name="p_Characteristic">Beatmap game mode</param>
        /// <param name="p_Difficulty">Beatmap difficulty</param>
        /// <param name="p_OverrideEnvironmentSettings">Environment settings</param>
        /// <param name="p_ColorScheme">Color scheme</param>
        /// <param name="p_GameplayModifiers">Modifiers</param>
        /// <param name="p_PlayerSettings">Player settings</param>
        /// <param name="p_SongFinishedCallback">Callback when the song is finished</param>
        public static async void PlaySong(  IPreviewBeatmapLevel            p_Level,
                                            BeatmapCharacteristicSO         p_Characteristic,
                                            BeatmapDifficulty               p_Difficulty,
                                            OverrideEnvironmentSettings     p_OverrideEnvironmentSettings   = null,
                                            ColorScheme                     p_ColorScheme                   = null,
                                            GameplayModifiers               p_GameplayModifiers             = null,
                                            PlayerSpecificSettings          p_PlayerSettings                = null,
                                            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults, IDifficultyBeatmap> p_SongFinishedCallback = null)
        {
             /*
                Code from https://github.com/MatrikMoon/TournamentAssistant

                MIT License

                Permission is hereby granted, free of charge, to any person obtaining a copy
                of this software and associated documentation files (the "Software"), to deal
                in the Software without restriction, including without limitation the rights
                to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                copies of the Software, and to permit persons to whom the Software is
                furnished to do so, subject to the following conditions:

                The above copyright notice and this permission notice shall be included in all
                copies or substantial portions of the Software.

                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                SOFTWARE.
            */

            Action<IBeatmapLevel> l_SongLoaded = (p_LoadedLevel) =>
            {
                MenuTransitionsHelper l_MenuSceneSetupData = Resources.FindObjectsOfTypeAll<MenuTransitionsHelper>().First();

                var l_DifficultyBeatmap = p_LoadedLevel.beatmapLevelData.GetDifficultyBeatmap(p_Characteristic, p_Difficulty);

                l_MenuSceneSetupData.StartStandardLevel(
                    p_Characteristic.name,
                    l_DifficultyBeatmap,
                    p_Level,
                    p_OverrideEnvironmentSettings,
                    p_ColorScheme,
                    p_GameplayModifiers ?? new GameplayModifiers(),
                    p_PlayerSettings    ?? new PlayerSpecificSettings(),
                    null,
                    "Menu",
                    false,
                    null,
                    (p_StandardLevelScenesTransitionSetupData, p_Results) => p_SongFinishedCallback?.Invoke(p_StandardLevelScenesTransitionSetupData, p_Results, l_DifficultyBeatmap)
                );
            };

            if ((p_Level is PreviewBeatmapLevelSO && await HasDLCLevel(p_Level.levelID)) || p_Level is CustomPreviewBeatmapLevel)
            {
                Logger.Instance?.Debug("[SDK.Game][Level.PlaySong] Loading DLC/Custom level...");

                var l_Result = await GetLevelFromPreview(p_Level);
                if (l_Result != null && !(l_Result?.isError == true))
                {
                    /// HTTPstatus requires cover texture to be applied in here, and due to a fluke
                    var l_LoadedLevel = l_Result?.beatmapLevel;
                    //l_LoadedLevel.SetField("_coverImageTexture2D", p_Level.GetField<Texture2D>("_coverImageTexture2D"));

                    l_SongLoaded(l_LoadedLevel);
                }
            }
            else if (p_Level is BeatmapLevelSO)
            {
                Logger.Instance?.Debug("[SDK.Game][Level.PlaySong] Reading OST data without songloader...");
                l_SongLoaded(p_Level as IBeatmapLevel);
            }
            else
            {
                Logger.Instance?.Debug($"[SDK.Game][Level.PlaySong] Skipping unowned DLC ({p_Level.songName})");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get a level from a level preview
        /// </summary>
        /// <param name="p_Level">Level instance</param>
        /// <param name="p_BeatmapLevelsModel">Model</param>
        /// <returns>Level instance</returns>
        private static async Task<BeatmapLevelsModel.GetBeatmapLevelResult?> GetLevelFromPreview(IPreviewBeatmapLevel p_Level, BeatmapLevelsModel p_BeatmapLevelsModel = null)
        {
            /*
                Code from https://github.com/MatrikMoon/TournamentAssistant

                MIT License

                Permission is hereby granted, free of charge, to any person obtaining a copy
                of this software and associated documentation files (the "Software"), to deal
                in the Software without restriction, including without limitation the rights
                to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
                copies of the Software, and to permit persons to whom the Software is
                furnished to do so, subject to the following conditions:

                The above copyright notice and this permission notice shall be included in all
                copies or substantial portions of the Software.

                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
                IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
                FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
                AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
                LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
                OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
                SOFTWARE.
            */
            p_BeatmapLevelsModel = p_BeatmapLevelsModel ?? Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();

            if (p_BeatmapLevelsModel != null)
            {
                m_GetLevelCancellationTokenSource?.Cancel();
                m_GetLevelCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelCancellationTokenSource.Token;

                BeatmapLevelsModel.GetBeatmapLevelResult? l_Result = null;

                try
                {
                    l_Result = await p_BeatmapLevelsModel.GetBeatmapLevelAsync(p_Level.levelID, l_Token);
                }
                catch (OperationCanceledException)
                {

                }

                if (l_Result?.isError == true || l_Result?.beatmapLevel == null)
                    return null; /// Null out entirely in case of error

                return l_Result;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get BeatmapCharacteristicSO by name
        /// </summary>
        /// <param name="p_Name"></param>
        /// <returns></returns>
        public static BeatmapCharacteristicSO GetCharacteristicSOBySerializedName(string p_Name)
        {
            return SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(
                SanitizeCharacteristic(p_Name)
            );
        }
        /// <summary>
        /// Sanitize characteristic
        /// </summary>
        /// <param name="p_Characteristic">Input characteristic</param>
        /// <returns></returns>
        public static string SanitizeCharacteristic(string p_Characteristic)
        {
            switch (p_Characteristic)
            {
                case "Standard":    return "Standard";

                case "One Saber":
                case "OneSaber":    return "OneSaber";

                case "No Arrows":
                case "NoArrows":    return "NoArrows";
                case "360Degree":   return "360Degree";
                case "Lawless":     return "Lawless";
                case "90Degree":    return "90Degree";

                case "LightShow":
                case "Lightshow":   return "Lightshow";
            }

            return null;
        }
        /// <summary>
        /// Get ordering value for a characteristic
        /// </summary>
        /// <param name="p_CharacteristicName">Input characteristic</param>
        /// <returns></returns>
        public static int GetCharacteristicOrdering(string p_CharacteristicName)
        {
            var l_SerializedName    = SanitizeCharacteristic(p_CharacteristicName);
            var l_CharacteristicSO  = GetCharacteristicSOBySerializedName(l_SerializedName);

            if (l_CharacteristicSO == null)
                return 1000;

            return l_CharacteristicSO.sortingOrder;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serialized difficulty name to difficulty name
        /// </summary>
        /// <param name="p_SerializedName">Serialized name</param>
        /// <returns>Difficulty name</returns>
        public static string SerializedToDifficultyName(string p_SerializedName)
        {
            var l_LowerCase = p_SerializedName.ToLower();
            if (l_LowerCase == "easy")
                return "Easy";
            else if (l_LowerCase == "normal")
                return "Normal";
            else if (l_LowerCase == "hard")
                return "Hard";
            else if (l_LowerCase == "expert")
                return "Expert";
            else if (l_LowerCase == "expertplus")
                return "Expert+";

            Logger.Instance?.Error($"[SDK.Game][Level.SerializedToDifficultyName] Unknown serialized difficulty \"{p_SerializedName}\", fall back to \"? Expert+ ?\"");

            return "? Expert+ ?";
        }
        /// <summary>
        /// Serialized to difficulty
        /// </summary>
        /// <param name="p_SerializedName">Serialized name</param>
        /// <returns></returns>
        public static BeatmapDifficulty SerializedToDifficulty(string p_SerializedName)
        {
            var l_LowerCase = p_SerializedName.ToLower();
            if (l_LowerCase == "easy")
                return BeatmapDifficulty.Easy;
            else if (l_LowerCase == "normal")
                return BeatmapDifficulty.Normal;
            else if (l_LowerCase == "hard")
                return BeatmapDifficulty.Hard;
            else if (l_LowerCase == "expert")
                return BeatmapDifficulty.Expert;
            else if (l_LowerCase == "expertplus")
                return BeatmapDifficulty.ExpertPlus;

            Logger.Instance?.Error($"[SDK.Game][Level.SerializedToDifficulty] Unknown serialized difficulty \"{p_SerializedName}\", fall back to ExpertPlus");

            return BeatmapDifficulty.ExpertPlus;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get song max score
        /// </summary>
        /// <param name="p_NoteCount">Note count</param>
        /// <param name="p_ScoreMultiplier">Score mutiplier</param>
        /// <returns></returns>
        public static int GetMaxScore(int p_NoteCount, float p_ScoreMultiplier = 1f)
        {
            if (p_NoteCount < 14)
            {
                if (p_NoteCount == 1)
                    return (int)((float)115 * p_ScoreMultiplier);
                else if (p_NoteCount < 5)
                    return (int)((float)((p_NoteCount - 1) * 230 + 115) * p_ScoreMultiplier);
                else
                    return (int)((float)((p_NoteCount - 5) * 460 + 1035) * p_ScoreMultiplier);
            }

            return (int)((float)((p_NoteCount - 13) * 920 + 4715) * p_ScoreMultiplier);
        }
        /// <summary>
        /// Get song max score
        /// </summary>
        /// <param name="p_Difficulty">Song difficulty</param>
        /// <param name="p_ScoreMultiplier">Score mutiplier</param>
        /// <returns></returns>
        public static int GetMaxScore(IDifficultyBeatmap p_Difficulty, float p_ScoreMultiplier = 1f) => GetMaxScore(p_Difficulty.beatmapData.cuttableNotesCount, p_ScoreMultiplier);
        /// <summary>
        /// Get score percentage
        /// </summary>
        /// <param name="p_MaxScore">Max score</param>
        /// <param name="p_Score">Result score</param>
        /// <returns></returns>
        public static float GetScorePercentage(int p_MaxScore, int p_Score)
        {
            return (float)Math.Round(1.0 / (double)p_MaxScore * (double)p_Score, 4);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get scores from local cache for a song
        /// </summary>
        /// <param name="p_SongHash">Level ID</param>
        /// <param name="p_HaveAnyScore"></param>
        /// <param name="p_HaveAllScores"></param>
        /// <returns></returns>
        public static Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>> GetScoresByHash(string p_SongHash, out bool p_HaveAnyScore, out bool p_HaveAllScores)
        {
            var l_Results = new Dictionary<BeatmapCharacteristicSO, List<(BeatmapDifficulty, int)>>();

            p_HaveAnyScore = false;
            p_HaveAllScores = true;

            if (Loader.CustomLevelsCollection == null || Loader.CustomLevelsCollection.beatmapLevels == null)
            {
                p_HaveAllScores = false;
                return l_Results;
            }

            var l_PlayerDataModel   = Resources.FindObjectsOfTypeAll<PlayerDataModel>().FirstOrDefault();
            var l_CustomLevelID     = "custom_level_" + p_SongHash.ToUpper();
            var l_Level             = Loader.CustomLevelsCollection.beatmapLevels.Where(x => x.levelID == l_CustomLevelID).FirstOrDefault();

            if (l_Level == null)
            {
                p_HaveAllScores = false;
                return l_Results;
            }

            foreach (var l_DifficultySet in l_Level.previewDifficultyBeatmapSets)
            {
                if (!l_Results.ContainsKey(l_DifficultySet.beatmapCharacteristic))
                    l_Results.Add(l_DifficultySet.beatmapCharacteristic, new List<(BeatmapDifficulty, int)>());

                foreach (var l_Difficulty in l_DifficultySet.beatmapDifficulties)
                {
                    var l_ScoreSO = l_PlayerDataModel.playerData.GetPlayerLevelStatsData(l_CustomLevelID, l_Difficulty, l_DifficultySet.beatmapCharacteristic);

                    if (l_ScoreSO.validScore)
                    {
                        p_HaveAnyScore = true;
                        l_Results[l_DifficultySet.beatmapCharacteristic].Add((l_Difficulty, l_ScoreSO.highScore));
                    }
                    else
                    {
                        p_HaveAllScores = false;
                        l_Results[l_DifficultySet.beatmapCharacteristic].Add((l_Difficulty, -1));
                    }
                }
            }

            return l_Results;
        }
    }
}
