﻿using ChatPlexMod_ChatIntegrations;
using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Events
{
    /// <summary>
    /// Level paused event
    /// </summary>
    public class LevelPaused : IEvent<LevelPaused, ChatPlexMod_ChatIntegrations.Models.Event>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public LevelPaused()
        {
            /// Build provided values list
            ProvidedValues = new List<(EValueType, string)>()
            {
                (EValueType.String, "SongName"),
                (EValueType.String, "Difficulty")
            }.AsReadOnly();

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
                    XUIText.Make("This event will be triggered whenever the map is paused")
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
            if (p_Context.Type != ETriggerType.LevelPaused || p_Context.CustomData == null)
                return false;

            return true;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            var l_LevelData     = p_Context.CustomData as CP_SDK_BS.Game.LevelData;
#if BEATSABER_1_35_0_OR_NEWER
            string l_GameMode   = l_LevelData.Data.beatmapKey.beatmapCharacteristic.serializedName;
            string l_Difficulty = l_LevelData.Data.beatmapKey.difficulty.Name();
            string l_SongName   = l_LevelData.Data.beatmapLevel.allMappers.FirstOrDefault() + " - " + l_LevelData.Data.beatmapLevel.songName;
#else
            string l_GameMode   = l_LevelData.Data.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string l_Difficulty = l_LevelData.Data.difficultyBeatmap.difficulty.Name();
            string l_SongName   = l_LevelData.Data.difficultyBeatmap.level.songAuthorName + " - " + l_LevelData.Data.difficultyBeatmap.level.songName;
#endif

            p_Context.AddValue(EValueType.String, "SongName",   l_SongName);
            p_Context.AddValue(EValueType.String, "Difficulty", l_GameMode + " - " + l_Difficulty);
        }
    }
}
