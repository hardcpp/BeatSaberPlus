using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus_MenuMusic
{
    /// <summary>
    /// Menu Music Module
    /// </summary>
    internal class MenuMusic : BeatSaberPlus.SDK.ModuleBase<MenuMusic>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseType Type => BeatSaberPlus.SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Menu Music";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Replace boring ambient noise by music!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => MMConfig.Instance.Enabled; set { MMConfig.Instance.Enabled = value; MMConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseActivationType ActivationType => BeatSaberPlus.SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Saber tweaker view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;

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
        /// Wait and play next song coroutine
        /// </summary>
        private Coroutine m_WaitAndPlayNextSongCoroutine = null;
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
        /// <summary>
        /// Backup time clip instance
        /// </summary>
        private AudioClip m_BackupTimeClip = null;
        /// <summary>
        /// Backup time
        /// </summary>
        private float m_BackupTime = 0f;
        /// <summary>
        /// Is music paused
        /// </summary>
        private bool m_IsPaused = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current play total duration
        /// </summary>
        internal float CurrentDuration => m_CurrentMusic?.length ?? 0;
        /// <summary>
        /// Current play position
        /// </summary>
        internal float CurrentPosition => (m_CurrentMusic == m_BackupTimeClip) ? m_BackupTime : 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange += Game_OnSceneChange;

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
                m_OriginalMenuMusic             = m_PreviewPlayer.GetField<AudioClip, SongPreviewPlayer>("_defaultAudioClip");
                m_OriginalAmbientVolumeScale    = m_PreviewPlayer.GetField<float, SongPreviewPlayer>("_ambientVolumeScale");
            }

            /// Refresh song list
            RefreshSongs();

            /// Enable at start if in menu
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                Game_OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType.Menu);

            BeatSaberPlus.SDK.Chat.Service.Discrete_OnTextMessageReceived += ChatService_Discrete_OnTextMessageReceived;
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            BeatSaberPlus.SDK.Chat.Service.Discrete_OnTextMessageReceived -= ChatService_Discrete_OnTextMessageReceived;

            /// Unbind event
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange -= Game_OnSceneChange;

            /// Stop wait and play next song coroutine
            if (m_WaitAndPlayNextSongCoroutine != null)
            {
                SharedCoroutineStarter.instance.StopCoroutine(m_WaitAndPlayNextSongCoroutine);
                m_WaitAndPlayNextSongCoroutine = null;
            }

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
        /// Get Module settings UI
        /// </summary>
        protected override (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            /// Create view if needed
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();

            /// Change main view
            return (m_SettingsView, m_SettingsLeftView, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">Scene type</param>
        private void Game_OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            /// Skip if it's not the menu
            if (p_Scene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
            {
                if (m_PreviewPlayer != null && m_PreviewPlayer && m_OriginalMenuMusic != null)
                {
                    m_PreviewPlayer.SetField("_defaultAudioClip",   m_OriginalMenuMusic);
                }

                DestroyFloatingPlayer();
                return;
            }

            /// Create player window
            if (MMConfig.Instance.ShowPlayer)
                CreateFloatingPlayer();

            m_PreviewPlayer.SetField("_ambientVolumeScale", 0f);
            m_PreviewPlayer.SetField("_volumeScale",        0f);

            /// Start a new music
            if (MMConfig.Instance.StartANewMusicOnSceneChange)
                StartNewMusic(false, true);
            else
                SharedCoroutineStarter.instance.StartCoroutine(LoadAudioClip(true));
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void ChatService_Discrete_OnTextMessageReceived(BeatSaberPlus.SDK.Chat.Interfaces.IChatService p_Service, BeatSaberPlus.SDK.Chat.Interfaces.IChatMessage p_Message)
        {
            if (p_Message.Message.Length < 2 || p_Message.Message[0] != '!')
                return;

            string l_LMessage = p_Message.Message.ToLower();
            if (l_LMessage.StartsWith("!menumusic"))
                p_Service.SendTextMessage(p_Message.Channel, $"!: @{p_Message.Sender.DisplayName} current song: {GetCurrenltyPlayingSongName().Replace(".", " . ")}");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update playback volume
        /// </summary>
        internal void UpdatePlaybackVolume(bool p_FromSettings)
        {
            if (m_PreviewPlayer == null || !m_PreviewPlayer)
                return;

            m_PreviewPlayer.SetField("_volumeScale", MMConfig.Instance.PlaybackVolume);

            if (p_FromSettings && m_PlayerFloatingScreenController != null && m_PlayerFloatingScreenController)
                m_PlayerFloatingScreenController.UpdateVolume();

            if (!p_FromSettings && m_SettingsView && UI.Settings.CanBeUpdated)
                m_SettingsView.OnResetButton();

            MMConfig.Instance.Save();
        }
        /// <summary>
        /// Update player
        /// </summary>
        internal void UpdatePlayer()
        {
            if (MMConfig.Instance.ShowPlayer && (m_PlayerFloatingScreen == null || !m_PlayerFloatingScreen))
                CreateFloatingPlayer();
            else if (!MMConfig.Instance.ShowPlayer && m_PlayerFloatingScreen != null && m_PlayerFloatingScreen)
                DestroyFloatingPlayer();

            if (m_PlayerFloatingScreen != null && m_PlayerFloatingScreen)
            {
                m_PlayerFloatingScreenController.UpdateBackgroundColor();
                m_PlayerFloatingScreenController.UpdateText();
            }
        }
        /// <summary>
        /// Toggle pause status
        /// </summary>
        internal void TogglePause()
        {
            m_IsPaused = !m_IsPaused;

            if (m_PlayerFloatingScreenController)
                m_PlayerFloatingScreenController.SetIsPaused(m_IsPaused);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create floating player window
        /// </summary>
        private void CreateFloatingPlayer()
        {
            if ((m_PlayerFloatingScreen != null && m_PlayerFloatingScreen) || m_CreateFloatingPlayerCoroutine != null)
                return;

            m_CreateFloatingPlayerCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateFloatingPlayer_Coroutine());
        }
        /// <summary>
        /// Create floating player window
        /// </summary>
        private IEnumerator CreateFloatingPlayer_Coroutine()
        {
            if (m_PlayerFloatingScreen != null)
            {
                m_CreateFloatingPlayerCoroutine = null;
                yield break;
            }

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

                if (IPA.Loader.PluginManager.GetPluginFromId("BetterSongSearch") != null)
                {
                    l_PlayerPosition.y = 62;
                }

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
                m_PlayerFloatingScreenController.SetIsPaused(m_IsPaused);

                /// Update song name
                m_PlayerFloatingScreenController.SetPlayingSong(GetCurrenltyPlayingSongName());
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[MenuMusic] Failed to CreateFloatingPlayer");
                Logger.Instance.Error(l_Exception);
            }

            m_CreateFloatingPlayerCoroutine = null;
        }
        /// <summary>
        /// Destroy floating player window
        /// </summary>
        private void DestroyFloatingPlayer()
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
                if (MMConfig.Instance.UseOnlyCustomMenuSongsFolder)
                    m_AllSongs = GetSongsInDirectory("CustomMenuSongs", true).ToArray();

                if (!MMConfig.Instance.UseOnlyCustomMenuSongsFolder || m_AllSongs.Length == 0)
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load the song into the preview player
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadAudioClip(bool p_OnSceneTransition)
        {
            if (m_WaitAndPlayNextSongCoroutine != null)
            {
                SharedCoroutineStarter.instance.StopCoroutine(m_WaitAndPlayNextSongCoroutine);
                m_WaitAndPlayNextSongCoroutine = null;
            }

            /// Skip if it's not the menu
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                yield break;

            yield return new WaitUntil(() => m_PreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First());

            if (p_OnSceneTransition)
            {
                if (m_PreviewPlayer)
                    m_PreviewPlayer.FadeOut(m_PreviewPlayer.GetField<float, SongPreviewPlayer>("_crossFadeToDefaultSpeed"));

                yield return new WaitForSeconds(2f);
            }

            ///m_PreviewPlayer.GetField<AudioSource[]>("_audioSources")[m_PreviewPlayer.GetField<int>("_activeChannel")].Stop();

            UnityWebRequest l_Song = UnityWebRequestMultimedia.GetAudioClip($"{Environment.CurrentDirectory}\\{m_MusicToLoad}", AudioType.OGGVORBIS);
            yield return l_Song.SendWebRequest();

            /// Skip if it's not the menu
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                yield break;

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
                    if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                        StartNextMusic();

                    yield break;
                }
            }
            catch (Exception p_Exception)
            {
                Logger.Instance.Error("Can't load audio! Exception: ");
                Logger.Instance.Error(p_Exception);

                /// Try next music if loading failed
                if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                    StartNextMusic();

                yield break;
            }

            yield return new WaitUntil(() => m_CurrentMusic);

            /// Skip if it's not the menu
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                yield break;

            if (m_PreviewPlayer != null && m_PreviewPlayer && m_CurrentMusic != null)
            {
                /// Wait that the song is loaded in background
                while (m_CurrentMusic.loadState != AudioDataLoadState.Loaded
                    && m_CurrentMusic.loadState != AudioDataLoadState.Failed)
                {
                    yield return null;
                }

                /// Check if we changed scene during loading
                if (!m_PreviewPlayer || BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                    yield break;

                if (m_CurrentMusic.loadState == AudioDataLoadState.Loaded)
                {
                    bool l_Failed = false;

                    try
                    {
                        if (m_WaitAndPlayNextSongCoroutine != null)
                        {
                            SharedCoroutineStarter.instance.StopCoroutine(m_WaitAndPlayNextSongCoroutine);
                            m_WaitAndPlayNextSongCoroutine = null;
                        }

                        m_PreviewPlayer.SetField("_defaultAudioClip",   m_CurrentMusic);

                        m_PreviewPlayer.SetField("_ambientVolumeScale", MMConfig.Instance.PlaybackVolume);
                        m_PreviewPlayer.SetField("_volumeScale",        MMConfig.Instance.PlaybackVolume);

                        float l_StartTime = (MMConfig.Instance.StartSongFromBeginning || m_CurrentMusic.length < 60) ? 0f : Mathf.Max(UnityEngine.Random.Range(m_CurrentMusic.length * 0.2f, m_CurrentMusic.length * 0.8f), 0.0f);

                        m_PreviewPlayer.CrossfadeTo(m_CurrentMusic, MMConfig.Instance.PlaybackVolume, l_StartTime, -1f, () => { });

                        m_BackupTimeClip    = m_CurrentMusic;
                        m_BackupTime        = l_StartTime;

                        if (m_PlayerFloatingScreenController != null)
                            m_PlayerFloatingScreenController.SetPlayingSong(GetCurrenltyPlayingSongName());

                        m_WaitAndPlayNextSongCoroutine = SharedCoroutineStarter.instance.StartCoroutine(WaitAndPlayNextMusic(m_CurrentMusic.length));
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
                        if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                            StartNextMusic();

                        yield break;
                    }
                }
                /// Try next music if loading failed
                else
                    StartNextMusic();
            }
        }
        /// <summary>
        /// Wait and play next music
        /// </summary>
        /// <param name="p_WaitTime">Time to wait</param>
        /// <returns></returns>
        private IEnumerator WaitAndPlayNextMusic(float p_EndTime)
        {
            var l_Field             = typeof(SongPreviewPlayer).GetField("_audioSourceControllers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var l_AudioSourceField  = null as FieldInfo;
            var l_Waiter            = new WaitForSeconds(1f / 4f);

            do
            {
                /// Skip if it's not the menu
                if (BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.Menu || !m_PreviewPlayer)
                {
                    m_WaitAndPlayNextSongCoroutine = null;
                    yield break;
                }

                var l_ChannelsController = l_Field.GetValue(m_PreviewPlayer) as object[];
                if (l_ChannelsController != null)
                {
                    foreach (var l_ChannelController in l_ChannelsController)
                    {
                        if (l_AudioSourceField == null)
                            l_AudioSourceField = l_ChannelController.GetType().GetField("audioSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

                        var l_Channel = (AudioSource)l_AudioSourceField.GetValue(l_ChannelController);

                        //if (l_Channel.isPlaying && l_Channel.clip == m_CurrentMusic)
                        //    Logger.Instance.Error(string.Format("{0}/{1}", l_Channel.time, p_EndTime));

                        if (!m_IsPaused && !l_Channel.isPlaying && l_Channel.clip == m_CurrentMusic && l_ChannelsController.IndexOf(l_ChannelController) == m_PreviewPlayer.GetField<int, SongPreviewPlayer>("_activeChannel"))
                            l_Channel.UnPause();

                        if (l_Channel.isPlaying && l_Channel.clip == m_CurrentMusic)
                        {
                            if (m_BackupTimeClip == null || m_BackupTimeClip != m_CurrentMusic)
                            {
                                m_BackupTimeClip = m_CurrentMusic;
                                m_BackupTime     = l_Channel.time;
                            }
                            else if (Mathf.Abs(l_Channel.time - m_BackupTime) > 1f)
                                l_Channel.time = m_BackupTime;
                            else
                                m_BackupTime = l_Channel.time;

                            if (m_IsPaused)
                                l_Channel.Pause();
                            else
                            {
                                m_PreviewPlayer.SetField("_ambientVolumeScale", MMConfig.Instance.PlaybackVolume);
                                m_PreviewPlayer.SetField("_volumeScale",        MMConfig.Instance.PlaybackVolume);
                            }

                            if (Mathf.Abs(p_EndTime - l_Channel.time) < 3f)
                            {
                                m_WaitAndPlayNextSongCoroutine = null;
                                if (MMConfig.Instance.LoopCurrentMusic && l_Channel.clip.length >= 10f)
                                    SharedCoroutineStarter.instance.StartCoroutine(LoadAudioClip(false));
                                else
                                    StartNextMusic();
                                yield break;
                            }
                        }
                    }
                }

                /// Update 4 time a second
                yield return l_Waiter;

            } while (true);
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
        /// <summary>
        /// Get currently played CustomPreviewBeatmapLevel
        /// </summary>
        /// <returns></returns>
        internal CustomPreviewBeatmapLevel GetCurrentlyPlayingSongPreviewBeatmap()
        {
            try
            {
                if (m_CurrentMusic != null && m_CurrentMusic.name.StartsWith("Beat Saber_Data\\CustomLevels\\"))
                {
                    var l_Folder = m_CurrentMusic.name.Substring("Beat Saber_Data\\CustomLevels\\".Length);

                    if (l_Folder.ToLower().Contains("\\"))
                        l_Folder = l_Folder.Substring(0, l_Folder.LastIndexOf("\\"));

                    var l_CustomSong = SongCore.Loader.CustomLevels.Where(x => x.Value.customLevelPath.Contains(l_Folder)).Select(x => x.Value).FirstOrDefault();

                    return l_CustomSong;
                }
            }
            catch (System.Exception)
            {

            }

            return null;
        }
    }
}
