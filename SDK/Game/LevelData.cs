﻿using System.Linq;

namespace BeatSaberPlus.SDK.Game
{
    /// <summary>
    /// Level data instance
    /// </summary>
    public class LevelData
    {
        /// <summary>
        /// Level type
        /// </summary>
        public LevelType Type { get; internal set; }
        /// <summary>
        /// Level data
        /// </summary>
        public GameplayCoreSceneSetupData Data { get; internal set; }
        /// <summary>
        /// Is a noodle extension map?
        /// </summary>
        public bool IsNoodle
        {
            get
            {
                var l_ExtraData = SongCore.Collections.RetrieveDifficultyData(Data.difficultyBeatmap);
                if (l_ExtraData != null)
                    return l_ExtraData.additionalDifficultyData._requirements.Count(x => x.ToLower() == "Noodle Extensions".ToLower()) != 0;

                return false;
            }
        }
        /// <summary>
        /// Is a chroma extension map?
        /// </summary>
        public bool IsChroma
        {
            get
            {
                var l_ExtraData = SongCore.Collections.RetrieveDifficultyData(Data.difficultyBeatmap);
                if (l_ExtraData != null)
                    return l_ExtraData.additionalDifficultyData._requirements.Count(x => x.ToLower() == "Chroma".ToLower()) != 0;

                return false;
            }
        }
        /// <summary>
        /// Is a replay
        /// </summary>
        public bool IsReplay { get; internal set; }
    }
}
