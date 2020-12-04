using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BS_Utils.Utilities;
using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.Plugins.MenuMusic
{
    /// <summary>
    /// Menu Music instance
    /// </summary>
    class MenuMusic : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Menu Music";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.MenuMusic.Enabled; set => Config.MenuMusic.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static MenuMusic Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.SettingsCredits m_SettingsCreditsView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Floating screen instance
        /// </summary>
        private FloatingScreen m_PlayerFloatingScreen = null;
        /// <summary>
        /// Floating screen controller
        /// </summary>
        private UI.Player m_PlayerFloatingScreenController;
        /// <summary>
        /// CreateFloatingPlayer coroutine
        /// </summary>
        private Coroutine m_CreateFloatingPlayerCoroutine = null;
        /// <summary>
        /// Preview music player
        /// </summary>
        private SongPreviewPlayer m_PreviewPlayer = null;
        /// <summary>
        /// Original menu music
        /// </summary>
        private AudioClip m_OriginalMenuMusic = null;
        /// <summary>
        /// Original ambient volume scale
        /// </summary>
        private float m_OriginalAmbientVolumeScale = 1f;
        /// <summary>
        /// All songs
        /// </summary>
        private string[] m_AllSongs = new string[0];
        /// <summary>
        /// Music to load
        /// </summary>
        private string m_MusicToLoad = "";
        /// <summary>
        /// Current music
        /// </summary>
        private AudioClip m_CurrentMusic = null;
        /// <summary>
        /// Current song index
        /// </summary>
        private int m_CurrentSongIndex = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Set singleton
            Instance = this;

            /// Bind event
            Utils.Game.OnSceneChange += Game_OnSceneChange;

            /// Create CustomMenuSongs directory if not existing
            try
            {
                if (!Directory.Exists("CustomMenuSongs"))
                    Directory.CreateDirectory("CustomMenuSongs");
            }
            catch
            {

            }

            /// Try to find existing preview player
            m_PreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();

            /// Backup original settings
            if (m_PreviewPlayer != null)
            {
                m_OriginalMenuMusic             = m_PreviewPlayer.GetField<AudioClip>("_defaultAudioClip");
                m_OriginalAmbientVolumeScale    = m_PreviewPlayer.GetField<float>("_ambientVolumeScale");
            }

            /// Refresh song list
            RefreshSongs();

            /// Enable at start if in menu
            if (Utils.Game.ActiveScene == Utils.Game.SceneType.Menu)
                Game_OnSceneChange(Utils.Game.SceneType.Menu);
        }
        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            Utils.Game.OnSceneChange -= Game_OnSceneChange;

            /// Destroy floating window
            DestroyFloatingPlayer();

            /// Restore original settings
            if (m_PreviewPlayer != null && m_OriginalMenuMusic != null)
            {
                m_PreviewPlayer.SetField("_defaultAudioClip",   m_OriginalMenuMusic);
                m_PreviewPlayer.SetField("_ambientVolumeScale", m_OriginalAmbientVolumeScale);
                m_PreviewPlayer.CrossfadeToDefault();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show plugin UI
        /// </summary>
        protected override void ShowUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            /// Create view if needed
            if (m_SettingsCreditsView == null)
                m_SettingsCreditsView = BeatSaberUI.CreateViewController<UI.SettingsCredits>();

            /// Change main view
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_SettingsView, m_SettingsCreditsView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create floating player window
        /// </summary>
        internal void CreateFloatingPlayer()
        {
            if (m_PlayerFloatingScreen != null || m_CreateFloatingPlayerCoroutine != null)
                return;

            m_CreateFloatingPlayerCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateFloatingPlayer_Coroutine());
        }
        /// <summary>
        /// Create floating player window
        /// </summary>
        private IEnumerator CreateFloatingPlayer_Coroutine()
        {
            if (m_PlayerFloatingScreen != null)
                yield break;

            GameObject l_ScreenContainer = null;

            while (true)
            {
                l_ScreenContainer = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "ScreenContainer" && x.activeInHierarchy);

                if (l_ScreenContainer != null && l_ScreenContainer)
                    break;

                yield return new WaitForSeconds(0.25f);
            }

            try
            {
                Vector2 l_PlayerSize     = new Vector2( 120f, 20f);
                Vector3 l_PlayerPosition = new Vector3(-140f, 55f, 0f);
                Vector3 l_PlayerRotation = new Vector3(   0f,  0f, 0f);

                /// Create floating screen
                m_PlayerFloatingScreen = FloatingScreen.CreateFloatingScreen(l_PlayerSize, false, Vector3.zero, Quaternion.identity);
                m_PlayerFloatingScreen.GetComponent<Canvas>().sortingOrder = 3;
                m_PlayerFloatingScreen.GetComponent<CurvedCanvasSettings>().SetRadius(150);
                m_PlayerFloatingScreen.gameObject.SetActive(true);

                /// Bind floating window to the root game object
                m_PlayerFloatingScreen.transform.SetParent(l_ScreenContainer.transform);

                /// Set rotation
                m_PlayerFloatingScreen.transform.localPosition  = l_PlayerPosition;
                m_PlayerFloatingScreen.transform.localScale     = Vector3.one;
                m_PlayerFloatingScreen.ScreenRotation           = Quaternion.Euler(l_PlayerRotation);

                /// Create UI Controller
                m_PlayerFloatingScreenController = BeatSaberUI.CreateViewController<UI.Player>();
                m_PlayerFloatingScreen.SetRootViewController(m_PlayerFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_PlayerFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = 4;

                /// Update song name
                m_PlayerFloatingScreenController.SetPlayingSong(GetCurrenltyPlayingSongName());

                m_CreateFloatingPlayerCoroutine = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[MenuMusic] Failed to DestroyFloatingPlayer");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Destroy floating player window
        /// </summary>
        internal void DestroyFloatingPlayer()
        {
            try
            {
                if (m_CreateFloatingPlayerCoroutine != null)
                {
                    SharedCoroutineStarter.instance.StopCoroutine(m_CreateFloatingPlayerCoroutine);
                    m_CreateFloatingPlayerCoroutine = null;
                }

                if (m_PlayerFloatingScreen == null)
                    return;

                /// Destroy objects
                GameObject.Destroy(m_PlayerFloatingScreen.gameObject);

                /// Reset variables
                m_PlayerFloatingScreenController = null;
                m_PlayerFloatingScreen           = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[MenuMusic] Failed to DestroyFloatingPlayer");
                Logger.Instance.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh songs
        /// </summary>
        internal void RefreshSongs()
        {
            try
            {
                if (Config.MenuMusic.UseOnlyCustomMenuSongsFolder)
                    m_AllSongs = GetSongsInDirectory("CustomMenuSongs", true).ToArray();

                if (!Config.MenuMusic.UseOnlyCustomMenuSongsFolder || m_AllSongs.Length == 0)
                    m_AllSongs = GetSongsInDirectory("Beat Saber_Data\\CustomLevels", false).ToArray();

                m_CurrentSongIndex = 0;

                System.Random l_Random = new System.Random(Environment.TickCount);
                m_AllSongs = m_AllSongs.OrderBy((x) => l_Random.Next()).ToArray();
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[MenuMusic] Failed to refresh songs");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Start a previous music
        /// </summary>
        internal void StartPreviousMusic()
        {
            /// Do nothing if no custom song available
            if (m_AllSongs.Length == 0)
                return;

            /// Decrement for next song
            m_CurrentSongIndex--;

            /// Handle overflow
            if (m_CurrentSongIndex < 0)
                m_CurrentSongIndex = m_AllSongs.Length - 1;
            if (m_CurrentSongIndex >= m_AllSongs.Length)
                m_CurrentSongIndex = 0;

            /// Select song to load
            m_MusicToLoad = m_AllSongs[m_CurrentSongIndex];

            /// Load and play audio clip
            SharedCoroutineStarter.instance.StartCoroutine(LoadAudioClip(false));
        }
        /// <summary>
        /// Start a new music
        /// </summary>
        internal void StartNewMusic(bool p_Random = false, bool p_OnSceneTransition = false)
        {
            /// Do nothing if no custom song available
            if (m_AllSongs.Length == 0)
                return;

            if (p_Random)
            {
                System.Random l_Random = new System.Random(Environment.TickCount);
                m_CurrentSongIndex = l_Random.Next(0, m_AllSongs.Length);
            }
            else
                m_CurrentSongIndex++;

            /// Handle overflow
            if (m_CurrentSongIndex < 0)
                m_CurrentSongIndex = m_AllSongs.Length - 1;
            if (m_CurrentSongIndex >= m_AllSongs.Length)
                m_CurrentSongIndex = 0;

            /// Select song to load
            m_MusicToLoad = m_AllSongs[m_CurrentSongIndex];

            /// Load and play audio clip
            SharedCoroutineStarter.instance.StartCoroutine(LoadAudioClip(p_OnSceneTransition));
        }
        /// <summary>
        /// Start a next music
        /// </summary>
        internal void StartNextMusic()
        {
            /// Do nothing if no custom song available
            if (m_AllSongs.Length == 0)
                return;

            /// Increment for next song
            m_CurrentSongIndex++;

            /// Handle overflow
            if (m_CurrentSongIndex < 0)
                m_CurrentSongIndex = m_AllSongs.Length - 1;
            if (m_CurrentSongIndex >= m_AllSongs.Length)
                m_CurrentSongIndex = 0;

            /// Select song to load
            m_MusicToLoad = m_AllSongs[m_CurrentSongIndex];

            /// Load and play audio clip
            SharedCoroutineStarter.instance.StartCoroutine(LoadAudioClip(false));
        }
        /// <summary>
        /// Update playback volume
        /// </summary>
        internal void UpdatePlaybackVolume()
        {
            if (m_PreviewPlayer == null || !m_PreviewPlayer)
                return;

            m_PreviewPlayer.SetField("_volumeScale", Config.MenuMusic.PlaybackVolume);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load the song into the preview player
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadAudioClip(bool p_OnSceneTransition)
        {
            /// Skip if it's not the menu
            if (Utils.Game.ActiveScene != Utils.Game.SceneType.Menu)
                yield break;

            yield return new WaitUntil(() => m_PreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First());

            if (p_OnSceneTransition)
            {
                if (m_PreviewPlayer)
                    m_PreviewPlayer.FadeOut();

                yield return new WaitForSeconds(2f);
            }

            ///m_PreviewPlayer.GetField<AudioSource[]>("_audioSources")[m_PreviewPlayer.GetField<int>("_activeChannel")].Stop();

            UnityWebRequest l_Song = UnityWebRequestMultimedia.GetAudioClip($"{Environment.CurrentDirectory}\\{m_MusicToLoad}", AudioType.OGGVORBIS);
            yield return l_Song.SendWebRequest();

            try
            {
                ((DownloadHandlerAudioClip)l_Song.downloadHandler).streamAudio = true;

                m_CurrentMusic = DownloadHandlerAudioClip.GetContent(l_Song);

                if (m_CurrentMusic != null)
                    m_CurrentMusic.name = m_MusicToLoad;
                else
                {
                    Logger.Instance.Debug("No audio found!");

                    /// Try next music if loading failed
                    StartNextMusic();

                    yield break;
                }
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("Can't load audio! Exception: ");
                Logger.Instance.Error(p_Exception);

                /// Try next music if loading failed
                StartNextMusic();

                yield break;
            }

            yield return new WaitUntil(() => m_CurrentMusic);

            if (m_PreviewPlayer != null && m_PreviewPlayer && m_CurrentMusic != null)
            {
                /// Wait that the song is loaded in background
                while (m_CurrentMusic.loadState != AudioDataLoadState.Loaded
                    && m_CurrentMusic.loadState != AudioDataLoadState.Failed)
                {
                    yield return null;
                }

                /// Check if we changed scene during loading
                if (!m_PreviewPlayer || BeatSaberPlus.Utils.Game.ActiveScene != Utils.Game.SceneType.Menu)
                    yield break;

                if (m_CurrentMusic.loadState == AudioDataLoadState.Loaded)
                {
                    bool l_Failed = false;

                    try
                    {
                        m_PreviewPlayer.SetField("_defaultAudioClip", m_CurrentMusic);
                        m_PreviewPlayer.SetField("_ambientVolumeScale", Config.MenuMusic.PlaybackVolume);
                        if (Config.MenuMusic.StartSongFromBeginning)
                            m_PreviewPlayer.CrossfadeTo(m_CurrentMusic, 0f, -1f, Config.MenuMusic.PlaybackVolume);
                        else
                            m_PreviewPlayer.CrossfadeToDefault();

                        if (m_PlayerFloatingScreenController != null)
                            m_PlayerFloatingScreenController.SetPlayingSong(GetCurrenltyPlayingSongName());
                    }
                    catch (Exception p_Exception)
                    {
                        Logger.Instance.Error("Can't play audio! Exception: ");
                        Logger.Instance.Error(p_Exception);

                        l_Failed = true;
                    }

                    if (l_Failed)
                    {
                        /// Wait until next try
                        yield return new WaitForSeconds(2f);

                        /// Try next music if loading failed
                        StartNextMusic();

                        yield break;
                    }
                }
                /// Try next music if loading failed
                else
                    StartNextMusic();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Search songs in a folder
        /// </summary>
        /// <param name="p_BaseDirectory">Base search directory</param>
        /// <returns></returns>
        private List<string> GetSongsInDirectory(string p_BaseDirectory, bool p_CustomMenuSongs)
        {
            List<string> l_Files = new List<String>();
            try
            {
                if (p_CustomMenuSongs)
                {
                    l_Files.AddRange(Directory.GetFiles(p_BaseDirectory, "*.ogg").Union(Directory.GetFiles(p_BaseDirectory, "*.egg")));

                    foreach (string l_Directory in Directory.GetDirectories(p_BaseDirectory))
                        l_Files.AddRange(GetSongsInDirectory(l_Directory, true));
                }
                else
                {
                    foreach (string l_Directory in Directory.GetDirectories(p_BaseDirectory))
                        l_Files.AddRange(Directory.GetFiles(l_Directory, "*.ogg").Union(Directory.GetFiles(l_Directory, "*.egg")));
                }
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("[MenuMusic] GetSongsInDirectory");
                Logger.Instance.Error(p_Exception);
            }

            return l_Files;
        }
        /// <summary>
        /// Get currently playing song name
        /// </summary>
        /// <returns></returns>
        private string GetCurrenltyPlayingSongName()
        {
            string l_Result = "<i>No song playing</i>";

            try
            {
                if (m_CurrentMusic != null)
                {
                    if (m_CurrentMusic.name.StartsWith("Beat Saber_Data\\CustomLevels\\"))
                    {
                        l_Result = m_CurrentMusic.name.Substring("Beat Saber_Data\\CustomLevels\\".Length);

                        if (l_Result.ToLower().Contains("\\"))
                            l_Result = l_Result.Substring(0, l_Result.LastIndexOf("\\"));

                        var l_CustomSong = SongCore.Loader.CustomLevels.Where(x => x.Value.customLevelPath.Contains(l_Result)).Select(x => x.Value).FirstOrDefault();

                        if (l_CustomSong != null)
                            l_Result = l_CustomSong.levelAuthorName + " - " + l_CustomSong.songName;
                        else if (l_Result.Contains("(") && l_Result.Contains(")"))
                        {
                            l_Result = l_Result.Substring(l_Result.IndexOf("(") + 1);
                            l_Result = l_Result.Substring(0, l_Result.Length - 1);
                        }
                    }
                    else
                        l_Result = Path.GetFileName(m_CurrentMusic.name);
                }
            }
            catch (System.Exception)
            {

            }

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">Scene type</param>
        private void Game_OnSceneChange(Utils.Game.SceneType p_Scene)
        {
            /// Skip if it's not the menu
            if (p_Scene != Utils.Game.SceneType.Menu)
            {
                if (m_PreviewPlayer != null && m_PreviewPlayer && m_OriginalMenuMusic != null)
                    m_PreviewPlayer.SetField("_defaultAudioClip", m_OriginalMenuMusic);

                DestroyFloatingPlayer();
                return;
            }

            /// Create player window
            if (Config.MenuMusic.ShowPlayer)
                CreateFloatingPlayer();

            /// Start a new music
            StartNewMusic(false, true);
        }
    }
}
