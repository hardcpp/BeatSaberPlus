using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_MenuMusic.Data
{
    /// <summary>
    /// Game music provider
    /// </summary>
    public class GameMusicProvider : IMusicProvider
    {
        private List<Music> m_Musics    = new List<Music>();
        private bool        m_IsLoading = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override MusicProviderType.E Type            => MusicProviderType.E.GameMusic;
        public override bool                IsReady         => !m_IsLoading;
#if BEATSABER
        public override bool                SupportPlayIt   => true;
#else
#error Missing game implementation
#endif
        public override List<Music>         Musics          => m_Musics;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public GameMusicProvider()
        {
            m_IsLoading = true;
            CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_LoadGameSongs());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Per game implementation of the Play It button
        /// </summary>
        /// <param name="p_Music">Target music</param>
        public override bool StartGameSpecificGamePlay(Music p_Music)
        {
#if BEATSABER
            var l_CustomPreviewBeatmapLevel = null as CustomPreviewBeatmapLevel;
            try
            {
                if (p_Music != null)
                {
                    var l_FullPath = p_Music.GetSongPath();
                    if (l_FullPath.Contains("Beat Saber_Data/CustomLevels/"))
                    {
                        var l_RelativeFolder = l_FullPath.Substring(l_FullPath.IndexOf("Beat Saber_Data/CustomLevels/") + "Beat Saber_Data/CustomLevels/".Length);
                        if (l_RelativeFolder.Contains("/"))
                            l_RelativeFolder = l_RelativeFolder.Substring(0, l_RelativeFolder.LastIndexOf("/"));

                        l_CustomPreviewBeatmapLevel = SongCore.Loader.CustomLevels.Where(x => x.Value.customLevelPath.Contains(l_RelativeFolder)).Select(x => x.Value).FirstOrDefault();
                    }
                }
            }
            catch (System.Exception)
            {

            }

            if (l_CustomPreviewBeatmapLevel == null)
                return false;

            CP_SDK_BS.Game.LevelSelection.FilterToSpecificSong(l_CustomPreviewBeatmapLevel);
            return true;
#else
#error Missing game implementation
#endif
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Load game songs
    /// </summary>
    /// <returns></returns>
    private IEnumerator Coroutine_LoadGameSongs()
    {
#if BEATSABER
            yield return new WaitUntil(() => !SongCore.Loader.AreSongsLoading && SongCore.Loader.CustomLevels.Count > 0);

            foreach (var l_Current in SongCore.Loader.CustomLevels)
            {
                var l_Extension = Path.GetExtension(l_Current.Value.standardLevelInfoSaveData.songFilename).ToLower();
                if (l_Extension != ".egg" && l_Extension != ".ogg")
                    continue;

                m_Musics.Add(new Music(
                    this,
                    Path.Combine("Beat Saber_Data\\CustomLevels\\", l_Current.Value.customLevelPath, l_Current.Value.standardLevelInfoSaveData.songFilename),
                    Path.Combine("Beat Saber_Data\\CustomLevels\\", l_Current.Value.customLevelPath, l_Current.Value.standardLevelInfoSaveData.coverImageFilename),
                    l_Current.Value.songName,
                    l_Current.Value.songAuthorName
                ));
            }

            for (var l_I = 0; l_I < m_Musics.Count; ++l_I)
            {
                var l_Swapped  = m_Musics[l_I];
                var l_NewIndex = Random.Range(l_I, m_Musics.Count);

                m_Musics[l_I]           = m_Musics[l_NewIndex];
                m_Musics[l_NewIndex]    = l_Swapped;
            }
#else
#error Missing game implementation
#endif

            m_IsLoading = false;
        }
    }
}
