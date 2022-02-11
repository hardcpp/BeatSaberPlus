using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.SDK.Game.Internal
{
    internal class BeatMaps_CustomBeatmapLevel : CustomBeatmapLevel
    {
        internal BeatMaps_CustomBeatmapLevel(   BeatMaps.MapDetail          p_MapDetail,
                                                BeatMaps.MapVersion         p_Version,
                                                CustomPreviewBeatmapLevel   p_CustomPreviewBeatmapLevel,
                                                AudioClip                   p_PreviewAudioClip)
            : base(p_CustomPreviewBeatmapLevel)
        {
            var l_IDifficultyBeatmapSet     = new List<IDifficultyBeatmapSet>();
            var l_VersionCharacteristics    = p_Version.GetCharacteristicsInOrder();

            foreach (var l_Current in l_VersionCharacteristics)
            {
                var l_Entry = new BeatMaps_DifficultyBeatmapSet(
                    this,
                    Levels.GetCharacteristicSOBySerializedName(l_Current),
                    p_Version.GetDifficultiesPerCharacteristic(l_Current)
                );

                l_IDifficultyBeatmapSet.Add(l_Entry);
            }

            SetBeatmapLevelData(new BeatmapLevelData(null, l_IDifficultyBeatmapSet.ToArray()));
        }

        internal static BeatMaps_CustomBeatmapLevel FromBeatSaver(BeatMaps.MapDetail p_MapDetail, BeatMaps.MapVersion p_Version)
        {
            var l_PreviewDifficultyBeatmapSet   = new List<PreviewDifficultyBeatmapSet>();
            var l_VersionCharacteristics        = p_Version.GetCharacteristicsInOrder();

            foreach (var l_Current in l_VersionCharacteristics)
            {
                var l_Difficulties = new List<BeatmapDifficulty>();

                foreach (var l_CurrentDiff in p_Version.GetDifficultiesPerCharacteristic(l_Current))
                    l_Difficulties.Add(Levels.SerializedToDifficulty(l_CurrentDiff.difficulty));

                l_PreviewDifficultyBeatmapSet.Add(new PreviewDifficultyBeatmapSet(
                    Levels.GetCharacteristicSOBySerializedName(l_Current),
                    l_Difficulties.ToArray()
                ));
            }

            var l_CustomPreviewBeatmapLevel = new CustomPreviewBeatmapLevel(
                null,
                null,
                "",
                null,
                "custom_level_" + p_Version.hash.ToUpper(),
                p_MapDetail.metadata.songName,
                p_MapDetail.metadata.songSubName,
                p_MapDetail.metadata.songAuthorName,
                p_MapDetail.metadata.levelAuthorName,
                p_MapDetail.metadata.bpm,
                0f,    ///< todo
                0f,    ///< todo
                0f,    ///< todo
                0f,    ///< todo
                p_MapDetail.metadata.duration,
                null,
                null,
                l_PreviewDifficultyBeatmapSet.ToArray()
            );

            return new BeatMaps_CustomBeatmapLevel(p_MapDetail, p_Version, l_CustomPreviewBeatmapLevel, null);
        }
    }

    internal class BeatMaps_DifficultyBeatmapSet : IDifficultyBeatmapSet
    {
        public BeatmapCharacteristicSO beatmapCharacteristic { get; internal set; }
        public IDifficultyBeatmap[] difficultyBeatmaps { get; internal set; }

        internal BeatMaps_DifficultyBeatmapSet( BeatMaps_CustomBeatmapLevel    p_BSBeatmapLevel,
                                                BeatmapCharacteristicSO         p_CharacteristicSO,
                                                List<BeatMaps.MapDifficulty>    p_Characteristic)
        {
            beatmapCharacteristic = p_CharacteristicSO;

            List<IDifficultyBeatmap> l_Difficulties = new List<IDifficultyBeatmap>();
            foreach (var l_Current in p_Characteristic)
            {
                l_Difficulties.Add(
                    new BeatMaps_DifficultyBeatmap(
                        this,
                        p_BSBeatmapLevel,
                        p_CharacteristicSO,
                        Levels.SerializedToDifficulty(l_Current.difficulty),
                        l_Current
                    )
                );
            }

            difficultyBeatmaps = l_Difficulties.ToArray();
        }
    }

    internal class BeatMaps_DifficultyBeatmap : IDifficultyBeatmap
    {
        public IBeatmapLevel level                              { get; internal set; }
        public IDifficultyBeatmapSet parentDifficultyBeatmapSet { get; internal set; }
        public BeatmapDifficulty difficulty                     { get; internal set; }
        public int difficultyRank                               { get; internal set; }
        public float noteJumpMovementSpeed                      { get; internal set; }
        public float noteJumpStartBeatOffset                    { get; internal set; }
        public BeatmapData beatmapData                          { get; internal set; }

        internal BeatMaps_DifficultyBeatmap(   BeatMaps_DifficultyBeatmapSet    p_Parent,
                                                BeatMaps_CustomBeatmapLevel     p_BSBeatmapLevel,
                                                BeatmapCharacteristicSO         p_CharacteristicSO,
                                                BeatmapDifficulty               p_Difficulty,
                                                BeatMaps.MapDifficulty          p_CharacteristicDifficulty)
        {
            level                      = p_BSBeatmapLevel;
            parentDifficultyBeatmapSet = p_Parent;
            difficulty                 = p_Difficulty;
            noteJumpMovementSpeed      = p_CharacteristicDifficulty.njs;
            noteJumpStartBeatOffset    = p_CharacteristicDifficulty.offset;
            beatmapData                = new BeatmapData(4);

            /// From DefaultRating
            switch  (p_Difficulty)
            {
                case BeatmapDifficulty.Easy:        difficultyRank = 1; break;
                case BeatmapDifficulty.Normal:      difficultyRank = 3; break;
                case BeatmapDifficulty.Hard:        difficultyRank = 5; break;
                case BeatmapDifficulty.Expert:      difficultyRank = 7; break;
                case BeatmapDifficulty.ExpertPlus:  difficultyRank = 9; break;
            }

            for (int l_I = 0; l_I < p_CharacteristicDifficulty.notes; ++l_I)
                beatmapData.AddBeatmapObjectData(NoteData.CreateBasicNoteData(0f, 0, NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any));
        }
    }
}
