using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.Utils
{
    /// <summary>
    /// Song utils
    /// </summary>
    public class Songs
    {
        /// <summary>
        /// Get level cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetLevelCancellationTokenSource;
        /// <summary>
        /// Get status cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetStatusCancellationTokenSource;
        /// <summary>
        /// Master level list
        /// </summary>
        private static List<IPreviewBeatmapLevel> m_MasterLevelList;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable song loaded event
        /// </summary>
        internal static void Init()
        {
            Loader.SongsLoadedEvent += (arg1, arg2) => RefreshLoadedSongs();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh loaded song list
        /// </summary>
        public static void RefreshLoadedSongs()
        {
            if (   Loader.CustomLevelsCollection == null
                || Loader.CustomLevelsCollection.beatmapLevels == null)
                return;

            m_MasterLevelList = new List<IPreviewBeatmapLevel>();
            m_MasterLevelList.AddRange(Loader.CustomLevelsCollection.beatmapLevels);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            IPreviewBeatmapLevel l_Level = m_MasterLevelList.Where(x => x.levelID == p_LevelID).First();

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
                Logger.Instance?.Debug("Loading DLC/Custom level...");

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
                Logger.Instance?.Debug("Reading OST data without songloader...");
                l_SongLoaded(p_Level as IBeatmapLevel);
            }
            else
            {
                Logger.Instance?.Debug($"Skipping unowned DLC ({p_Level.songName})");
            }
        }

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
        /// Serialized difficulty name to difficulty name
        /// </summary>
        /// <param name="p_SerializedName">Serialized name</param>
        /// <returns>Difficulty name</returns>
        public static string SerializedToDifficultyName(string p_SerializedName)
        {
            if (p_SerializedName.ToLower() == "easy".ToLower())
                return "Easy";
            else if (p_SerializedName.ToLower() == "normal".ToLower())
                return "Normal";
            else if (p_SerializedName.ToLower() == "hard".ToLower())
                return "Hard";
            else if (p_SerializedName.ToLower() == "expert".ToLower())
                return "Expert";
            else if (p_SerializedName.ToLower() == "expertPlus".ToLower())
                return "Expert+";
            else
                return "--";
        }
        /// <summary>
        /// Serialized difficulty name to difficulty name
        /// </summary>
        /// <param name="p_SerializedName">Serialized name</param>
        /// <returns>Difficulty name</returns>
        public static BeatmapDifficulty SerializedToDifficulty(string p_SerializedName)
        {
            if (p_SerializedName.ToLower() == "easy".ToLower())
                return BeatmapDifficulty.Easy;
            else if (p_SerializedName.ToLower() == "normal".ToLower())
                return BeatmapDifficulty.Normal;
            else if (p_SerializedName.ToLower() == "hard".ToLower())
                return BeatmapDifficulty.Hard;
            else if (p_SerializedName.ToLower() == "expert".ToLower())
                return BeatmapDifficulty.Expert;
            else if (p_SerializedName.ToLower() == "expertPlus".ToLower())
                return BeatmapDifficulty.ExpertPlus;
            else
                return BeatmapDifficulty.Easy;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get song max score
        /// </summary>
        /// <param name="p_Difficulty">Song difficulty</param>
        /// <param name="p_ScoreMultiplier">Score mutiplier</param>
        /// <returns></returns>
        public static int GetMaxScore(IDifficultyBeatmap p_Difficulty, float p_ScoreMultiplier = 1f)
        {
            if (p_Difficulty.beatmapData.cuttableNotesType < 14)
            {
                if (p_Difficulty.beatmapData.cuttableNotesType == 1)
                    return 115;
                else if (p_Difficulty.beatmapData.cuttableNotesType < 5)
                    return (p_Difficulty.beatmapData.cuttableNotesType - 1) * 230 + 115;
                else
                    return (p_Difficulty.beatmapData.cuttableNotesType - 5) * 460 + 1035;
            }

            return (p_Difficulty.beatmapData.cuttableNotesType - 13) * 920 + 4715;
        }
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
    }
}
