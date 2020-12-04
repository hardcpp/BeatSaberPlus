using BeatSaverSharp;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Utils
{
    internal class BeatSaver_CustomBeatmapLevel : CustomBeatmapLevel
    {
        internal BeatSaver_CustomBeatmapLevel(Beatmap p_BeatMap, CustomPreviewBeatmapLevel customPreviewBeatmapLevel, AudioClip previewAudioClip)
            : base(customPreviewBeatmapLevel, previewAudioClip)
        {
            List<IDifficultyBeatmapSet> l_IDifficultyBeatmapSet = new List<IDifficultyBeatmapSet>();
            foreach (var l_Current in p_BeatMap.Metadata.Characteristics)
            {
                if (l_Current.Difficulties.Count == 0)
                    continue;

                var l_Entry = new BeatSaver_DifficultyBeatmapSet(
                    this,
                    SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_Current.Name),
                    l_Current
                );

                l_IDifficultyBeatmapSet.Add(l_Entry);
            }

            SetBeatmapLevelData(new BeatmapLevelData(null, l_IDifficultyBeatmapSet.ToArray()));
        }

        internal static BeatSaver_CustomBeatmapLevel FromBeatSaver(Beatmap p_BeatMap)
        {
            List<PreviewDifficultyBeatmapSet> l_PreviewDifficultyBeatmapSet = new List<PreviewDifficultyBeatmapSet>();
            foreach (var l_Current in p_BeatMap.Metadata.Characteristics)
            {
                if (l_Current.Difficulties.Count == 0)
                    continue;

                var l_Difficulties = new List<BeatmapDifficulty>();
                foreach (var l_CurrentDiff in l_Current.Difficulties.Where(x => x.Value.HasValue).ToList())
                {
                    if (!l_CurrentDiff.Value.HasValue)
                        continue;

                    l_Difficulties.Add(Utils.Songs.SerializedToDifficulty(l_CurrentDiff.Key));
                }

                l_PreviewDifficultyBeatmapSet.Add(new PreviewDifficultyBeatmapSet(
                    SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName(l_Current.Name),
                    l_Difficulties.ToArray()
                ));
            }

            var l_CustomPreviewBeatmapLevel = new CustomPreviewBeatmapLevel(
                null,
                null,
                "",
                null,
                null,
                "custom_level_" + p_BeatMap.Hash.ToUpper(),
                p_BeatMap.Metadata.SongName,
                p_BeatMap.Metadata.SongSubName,
                p_BeatMap.Metadata.SongAuthorName,
                p_BeatMap.Metadata.LevelAuthorName,
                p_BeatMap.Metadata.BPM,
                0f,    ///< todo
                0f,    ///< todo
                0f,    ///< todo
                0f,    ///< todo
                p_BeatMap.Metadata.Duration,
                null,
                null,
                l_PreviewDifficultyBeatmapSet.ToArray()
            );

            return new BeatSaver_CustomBeatmapLevel(p_BeatMap, l_CustomPreviewBeatmapLevel, null);
        }
    }

    internal class BeatSaver_DifficultyBeatmapSet : IDifficultyBeatmapSet
    {
        public BeatmapCharacteristicSO beatmapCharacteristic { get; internal set; }
        public IDifficultyBeatmap[] difficultyBeatmaps { get; internal set; }

        internal BeatSaver_DifficultyBeatmapSet(BeatSaver_CustomBeatmapLevel p_BSBeatmapLevel, BeatmapCharacteristicSO p_CharacteristicSO, BeatmapCharacteristic p_Characteristic)
        {
            beatmapCharacteristic = p_CharacteristicSO;

            List<IDifficultyBeatmap> l_Difficulties = new List<IDifficultyBeatmap>();
            foreach (var l_Current in p_Characteristic.Difficulties)
            {
                if (!l_Current.Value.HasValue)
                    continue;

                l_Difficulties.Add(new BeatSaver_DifficultyBeatmap(this, p_BSBeatmapLevel, p_CharacteristicSO, Utils.Songs.SerializedToDifficulty(l_Current.Key), l_Current.Value.Value));
            }

            difficultyBeatmaps = l_Difficulties.ToArray();
        }
    }

    internal class BeatSaver_DifficultyBeatmap : IDifficultyBeatmap
    {
        public IBeatmapLevel level                              { get; internal set; }
        public IDifficultyBeatmapSet parentDifficultyBeatmapSet { get; internal set; }
        public BeatmapDifficulty difficulty                     { get; internal set; }
        public int difficultyRank                               { get; internal set; }
        public float noteJumpMovementSpeed                      { get; internal set; }
        public float noteJumpStartBeatOffset                    { get; internal set; }
        public BeatmapData beatmapData                          { get; internal set; }

        internal BeatSaver_DifficultyBeatmap(BeatSaver_DifficultyBeatmapSet p_Parent, BeatSaver_CustomBeatmapLevel p_BSBeatmapLevel, BeatmapCharacteristicSO p_CharacteristicSO, BeatmapDifficulty p_Difficulty, BeatmapCharacteristicDifficulty p_CharacteristicDifficulty)
        {
            level                      = p_BSBeatmapLevel;
            parentDifficultyBeatmapSet = p_Parent;
            difficulty                 = p_Difficulty;
            noteJumpMovementSpeed      = p_CharacteristicDifficulty.NoteJumpSpeed;
            noteJumpStartBeatOffset    = p_CharacteristicDifficulty.NoteJumpSpeedOffset;
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

            for (int l_I = 0; l_I < p_CharacteristicDifficulty.Notes; ++l_I)
                beatmapData.AddBeatmapObjectData(NoteData.CreateBasicNoteData(0f, 0, NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any));
        }
    }
}
