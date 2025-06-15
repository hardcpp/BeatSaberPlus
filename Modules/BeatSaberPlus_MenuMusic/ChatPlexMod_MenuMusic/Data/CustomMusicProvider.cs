﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ChatPlexMod_MenuMusic.Data
{
    /// <summary>
    /// Custom music provider
    /// </summary>
    public class CustomMusicProvider : IMusicProvider
    {
        private List<Music> m_Musics    = new List<Music>();
        private bool        m_IsLoading = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override MusicProviderType.E Type                => MusicProviderType.E.CustomMusic;
        public override bool                IsReady             => !m_IsLoading;
        public override bool                SupportPlayIt       => false;
        public override bool                SupportAddToQueue   => false;
        public override List<Music>         Musics              => m_Musics;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public CustomMusicProvider()
        {
            m_IsLoading = true;
            CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_Load());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Per game implementation of the Play It button
        /// </summary>
        /// <param name="music">Target music</param>
        /// <param name="viewController">Source view controller</param>
        public override void StartGameSpecificGamePlay(Music music, CP_SDK.UI.IViewController viewController)
        {

        }
        /// <summary>
        /// Per game implementation of the Add to queue button
        /// </summary>
        /// <param name="music">Target music</param>
        /// <param name="viewController">Source view controller</param>
        public override void AddToQueue(Music music, CP_SDK.UI.IViewController viewController)
        {

        }
        /// <summary>
        /// Shuffle music collection
        /// </summary>
        public override void Shuffle()
        {
            for (var l_I = 0; l_I < m_Musics.Count; ++l_I)
            {
                var l_Swapped = m_Musics[l_I];
                var l_NewIndex = UnityEngine.Random.Range(l_I, m_Musics.Count);

                m_Musics[l_I] = m_Musics[l_NewIndex];
                m_Musics[l_NewIndex] = l_Swapped;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load game songs
        /// </summary>
        /// <returns></returns>
        private IEnumerator Coroutine_Load()
        {
            yield return null;

            try
            {
                var l_BaseDirectory = $"UserData/{CP_SDK.ChatPlexSDK.ProductName}Plus/MenuMusic/CustomMusic";
                var l_Files         = new List<String>();

                if (!Directory.Exists(l_BaseDirectory))
                    Directory.CreateDirectory(l_BaseDirectory);

                l_Files.AddRange(Directory.GetFiles(l_BaseDirectory, "*.ogg").Union(Directory.GetFiles(l_BaseDirectory, "*.egg")));

                foreach (var l_File in l_Files)
                {
                    var l_FixedFile             = Path.GetFullPath(l_File);
                    var l_PathWithoutExtension  = Path.Combine(Path.GetDirectoryName(l_FixedFile), Path.GetFileNameWithoutExtension(l_FixedFile));
                    var l_CoverPath             = null as string;

                    if (File.Exists(l_PathWithoutExtension + ".jpg"))
                        l_CoverPath = l_PathWithoutExtension + ".jpg";
                    else if (File.Exists(l_PathWithoutExtension + ".png"))
                        l_CoverPath = l_PathWithoutExtension + ".png";

                    m_Musics.Add(new Music(
                        this,
                        l_FixedFile,
                        l_CoverPath,
                        Path.GetFileNameWithoutExtension(l_FixedFile),
                        " "
                    ));
                }
                Shuffle();
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_MenuMusic.Data][CustomMusicProvider.Coroutine_Load] GetSongsInDirectory");
                Logger.Instance.Error(p_Exception);
            }

            m_IsLoading = false;
        }
    }
}
