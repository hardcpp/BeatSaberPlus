using ChatPlexMod_ChatIntegrations;
using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Events
{
    /// <summary>
    /// Level ended event
    /// </summary>
    public class LevelEnded : IEvent<LevelEnded, ChatPlexMod_ChatIntegrations.Models.Event>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public LevelEnded()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.Integer,  "NoteCount"),
                (EValueType.Integer,  "HitCount"),
                (EValueType.Integer,  "MissCount"),
                (EValueType.Floating, "Accuracy"),
                (EValueType.String,   "SongName"),
                (EValueType.String,   "Difficulty")
            }.AsReadOnly();

            RegisterCustomCondition("GamePlay_LevelEndType", () => new Conditions.GamePlay_LevelEndType(), true);

            /// Build possible list
            AvailableConditions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalConditionsTypes)
                .Union(GetCustomConditionTypes())
                .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalActionsTypes)
                .Union(GetCustomActionTypes())
                .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                XUIVLayout.Make(
                    XUIText.Make("This event will be triggered whenever you finish/fail/restart/quit a map (Include replays)")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                ).SetBackground(true)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed bool CanBeExecuted(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            /// Ensure that we have all data
            if (p_Context.Type != ETriggerType.LevelEnded || p_Context.CustomData == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            var l_LevelCompletionData = p_Context.CustomData as CP_SDK_BS.Game.LevelCompletionData;
            Int64  l_NoteCount  = l_LevelCompletionData.Data.transformedBeatmapData.cuttableNotesCount;
            Int64  l_HitCount   = l_LevelCompletionData.Results.goodCutsCount;
            Int64  l_MissCount  = l_NoteCount - l_HitCount;
            float  l_Accuracy   = (float)System.Math.Round(100.0f * CP_SDK_BS.Game.Levels.GetAccuracy(l_LevelCompletionData.MaxMultipliedScore, l_LevelCompletionData.Results.multipliedScore), 2);
#if BEATSABER_1_35_0_OR_NEWER
            string l_GameMode   = l_LevelCompletionData.Data.beatmapKey.beatmapCharacteristic.serializedName;
            string l_Difficulty = l_LevelCompletionData.Data.beatmapKey.difficulty.Name();
            string l_SongName   = l_LevelCompletionData.Data.beatmapLevel.allMappers.FirstOrDefault() + " - " + l_LevelCompletionData.Data.beatmapLevel.songName;
#else
            string l_GameMode   = l_LevelCompletionData.Data.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string l_Difficulty = l_LevelCompletionData.Data.difficultyBeatmap.difficulty.Name();
            string l_SongName   = l_LevelCompletionData.Data.difficultyBeatmap.level.songAuthorName + " - " + l_LevelCompletionData.Data.difficultyBeatmap.level.songName;
#endif

            p_Context.AddValue(EValueType.Integer,  "NoteCount",  (Int64?)l_NoteCount);
            p_Context.AddValue(EValueType.Integer,  "HitCount",   (Int64?)l_HitCount);
            p_Context.AddValue(EValueType.Integer,  "MissCount",  (Int64?)l_MissCount);
            p_Context.AddValue(EValueType.Floating, "Accuracy",   (float?)l_Accuracy);
            p_Context.AddValue(EValueType.String,   "SongName",   l_SongName);
            p_Context.AddValue(EValueType.String,   "Difficulty", l_GameMode + " - " + l_Difficulty);
        }
    }
}
