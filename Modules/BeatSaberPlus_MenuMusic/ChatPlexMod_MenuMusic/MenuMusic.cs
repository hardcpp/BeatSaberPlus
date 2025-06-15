using CP_SDK.Chat.Interfaces;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_MenuMusic
{
    /// <summary>
    /// Menu Music Module
    /// </summary>
    internal class MenuMusic : CP_SDK.ModuleBase<MenuMusic>
    {
        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Menu Music";
        public override string                              Description         => "Replace boring ambient noise by music!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#menu-music";
        public override bool                                UseChatFeatures     => false;
        public override bool                                IsEnabled           { get => MMConfig.Instance.Enabled; set { MMConfig.Instance.Enabled = value; MMConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsMainView m_SettingsMainView = null;
        private UI.SettingsLeftView m_SettingsLeftView = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private CP_SDK.UI.Components.CFloatingPanel m_PlayerFloatingPanel       = null;
        private UI.PlayerFloatingPanel              m_PlayerFloatingPanelView   = null;

        private Coroutine m_CreateFloatingPlayerCoroutine   = null;
        private Coroutine m_WaitAndPlayNextSongCoroutine    = null;

        private bool                m_WantsToQuit                   = false;
        private SongPreviewPlayer   m_PreviewPlayer                 = null;
        private AudioClip           m_OriginalMenuMusic             = null;
        private float               m_OriginalAmbientVolumeScale    = 1f;
        private Data.Music          m_CurrentMusic                  = null;
        private AudioClip           m_CurrentMusicAudioClip         = null;
        private AudioClip           m_BackupTimeClip                = null;
        private float               m_BackupTime                    = 0f;
        private bool                m_IsPaused                      = false;

        private Data.IMusicProvider                 m_MusicProvider             = null;
        private int                                 m_CurrentSongIndex          = 0;
        private Coroutine                           m_WaitUntillReadyCoroutine  = null;
        private CP_SDK.Misc.FastCancellationToken   m_FastCancellationToken     = new CP_SDK.Misc.FastCancellationToken();

        private CP_SDK.EGenericScene                m_LastActiveScene;
        private bool                                m_LastPlayingRescue         = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind event
            CP_SDK.ChatPlexSDK.OnGenericSceneChange             += ChatPlexSDK_OnGenericSceneChange;
            CP_SDK.Chat.Service.Discrete_OnTextMessageReceived  += ChatService_Discrete_OnTextMessageReceived;
            Application.wantsToQuit                             += Application_wantsToQuit;

            /// Try to find existing preview player
            m_PreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().FirstOrDefault();

            /// Backup original settings
            if (m_PreviewPlayer != null)
            {
                m_OriginalMenuMusic             = m_PreviewPlayer._defaultAudioClip;
                m_OriginalAmbientVolumeScale    = m_PreviewPlayer._ambientVolumeScale;
            }

            if (UpdateMusicProvider(true))
            {
                /// Enable at start if in menu
                if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.EGenericScene.Menu)
                    ChatPlexSDK_OnGenericSceneChange(CP_SDK.EGenericScene.Menu);
            }
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Unbind event
            Application.wantsToQuit                             -= Application_wantsToQuit;
            CP_SDK.Chat.Service.Discrete_OnTextMessageReceived  -= ChatService_Discrete_OnTextMessageReceived;
            CP_SDK.ChatPlexSDK.OnGenericSceneChange             -= ChatPlexSDK_OnGenericSceneChange;

            /// Stop wait and play next song coroutine
            if (m_WaitAndPlayNextSongCoroutine != null)
            {
                CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitAndPlayNextSongCoroutine);
                m_WaitAndPlayNextSongCoroutine = null;
            }

            /// Destroy floating window
            DestroyFloatingPlayer();

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);

            /// Restore original settings
            if (!m_WantsToQuit && m_PreviewPlayer != null && m_OriginalMenuMusic != null)
            {
                m_PreviewPlayer._defaultAudioClip   = m_OriginalMenuMusic;
                m_PreviewPlayer._ambientVolumeScale = m_OriginalAmbientVolumeScale;
                m_PreviewPlayer.CrossfadeToDefault();
            }
            else if (m_WantsToQuit && m_PreviewPlayer)
                m_PreviewPlayer.PauseCurrentChannel();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsMainView == null) m_SettingsMainView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsLeftView == null) m_SettingsLeftView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();

            /// Change main view
            return (m_SettingsMainView, m_SettingsLeftView, null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the active scene change
        /// </summary>
        /// <param name="p_Scene">Scene type</param>
        private void ChatPlexSDK_OnGenericSceneChange(CP_SDK.EGenericScene p_Scene)
        {
            /// Skip if it's not the menu
            if (p_Scene != CP_SDK.EGenericScene.Menu)
            {
                if (m_PreviewPlayer != null && m_PreviewPlayer && m_OriginalMenuMusic != null && m_OriginalMenuMusic)
                    m_PreviewPlayer._defaultAudioClip = m_OriginalMenuMusic;

                DestroyFloatingPlayer();
                m_LastActiveScene = p_Scene;
                return;
            }

            /// Create player window
            if (MMConfig.Instance.ShowPlayer)
                CreateFloatingPlayer();

            m_PreviewPlayer._ambientVolumeScale = 0f;
            m_PreviewPlayer._volumeScale        = 0f;

            /// Start a new music
            if (p_Scene != m_LastActiveScene && MMConfig.Instance.StartANewMusicOnSceneChange)
                StartNewMusic(false, true);
            else
                LoadNextMusic(true);

            m_LastActiveScene = p_Scene;
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private void ChatService_Discrete_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            if (p_Message.Message.Length < 2 || p_Message.Message[0] != '!')
                return;

            string l_LMessage = p_Message.Message.ToLower();
            if (l_LMessage.StartsWith("!menumusic"))
                p_Service.SendTextMessage(p_Message.Channel, $"!: @{p_Message.Sender.DisplayName} current song: {m_CurrentMusic?.GetSongArtist().Replace(".", " . ")} - {m_CurrentMusic?.GetSongName().Replace(".", " . ")}");
        }
        /// <summary>
        /// Application wants to quit
        /// </summary>
        /// <returns></returns>
        private bool Application_wantsToQuit()
        {
            m_WantsToQuit = true;
            if (m_PreviewPlayer)
                m_PreviewPlayer.PauseCurrentChannel();
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update the music provider
        /// </summary>
        /// <param name="p_SkipIfAlreadySet">Skip if a music provider already exist</param>
        /// <returns>True if a new music provider is set</returns>
        internal bool UpdateMusicProvider(bool p_SkipIfAlreadySet = false)
        {
            if (p_SkipIfAlreadySet && m_MusicProvider != null)
                return false;

            switch (MMConfig.Instance.MusicProvider)
            {
                case Data.MusicProviderType.E.CustomMusic:
                    m_MusicProvider = new Data.CustomMusicProvider();
                    break;

                default:
                    m_MusicProvider = new Data.GameMusicProvider();
                    break;
            }

            StartNewMusic();
            return true;
        }
        /// <summary>
        /// Update playback volume
        /// </summary>
        /// <param name="p_FromConfig">From config?</param>
        internal void UpdatePlaybackVolume(bool p_FromConfig)
        {
            if (m_PreviewPlayer == null || !m_PreviewPlayer)
                return;

            if (CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Audio Tweaker"))
            {
                var l_ChannelsController = m_PreviewPlayer._audioSourceControllers;
                if (l_ChannelsController != null)
                {
                    for (var l_I = 0; l_I < l_ChannelsController.Length; ++l_I)
                    {
                        var l_ChannelController = l_ChannelsController[l_I];
                        var l_Channel           = l_ChannelController.audioSource;
                        if (l_Channel.isPlaying && l_Channel.clip == m_CurrentMusicAudioClip)
                        {
                            m_PreviewPlayer._ambientVolumeScale = 1.0f;
                            m_PreviewPlayer._volumeScale        = 1.0f;
                        }
                    }
                }
            }
            else
            {
                m_PreviewPlayer._ambientVolumeScale = MMConfig.Instance.PlaybackVolume;
                m_PreviewPlayer._volumeScale        = MMConfig.Instance.PlaybackVolume;
            }

            if (p_FromConfig && m_PlayerFloatingPanelView != null && m_PlayerFloatingPanelView)
                m_PlayerFloatingPanelView.UpdateVolume();

            if (!p_FromConfig && m_SettingsMainView && UI.SettingsMainView.CanBeUpdated)
                m_SettingsMainView.UpdateVolume();

            MMConfig.Instance.Save();
        }
        /// <summary>
        /// Update player
        /// </summary>
        internal void UpdatePlayer()
        {
            if (MMConfig.Instance.ShowPlayer && (m_PlayerFloatingPanel == null || !m_PlayerFloatingPanel))
                CreateFloatingPlayer();
            else if (!MMConfig.Instance.ShowPlayer && m_PlayerFloatingPanel != null && m_PlayerFloatingPanel)
                DestroyFloatingPlayer();
        }
        /// <summary>
        /// Toggle pause status
        /// </summary>
        internal void TogglePause()
        {
            m_IsPaused = !m_IsPaused;

            if (m_PlayerFloatingPanelView)
                m_PlayerFloatingPanelView.SetIsPaused(m_IsPaused);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create floating player window
        /// </summary>
        private void CreateFloatingPlayer()
        {
            if ((m_PlayerFloatingPanel != null && m_PlayerFloatingPanel) || m_CreateFloatingPlayerCoroutine != null)
                return;

            m_CreateFloatingPlayerCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(CreateFloatingPlayer_Coroutine());
        }
        /// <summary>
        /// Create floating player window
        /// </summary>
        private IEnumerator CreateFloatingPlayer_Coroutine()
        {
            if (m_PlayerFloatingPanel != null)
            {
                m_CreateFloatingPlayerCoroutine = null;
                yield break;
            }

            GameObject l_ScreenContainer = null;

            var l_Waiter = new WaitForSeconds(0.25f);
            while (true)
            {
                l_ScreenContainer = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "ScreenContainer" && x.activeInHierarchy && x.transform.parent?.parent?.name == "UI");

                if (l_ScreenContainer != null && l_ScreenContainer)
                    break;

                yield return l_Waiter;
            }

            var l_PlayerPosition = new Vector3(-140.0f, 55.0f, 0f);
            if (IPA.Loader.PluginManager.GetPluginFromId("BetterSongSearch") != null)
                l_PlayerPosition.y = 62;

            try
            {
                m_PlayerFloatingPanel = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("ChatPlexMod_MenuMusic", l_ScreenContainer.transform);
                m_PlayerFloatingPanel.SetSize(new Vector2(80.0f, 20.0f));
                m_PlayerFloatingPanel.SetRadius(140.0f);
                m_PlayerFloatingPanel.SetTransformDirect(l_PlayerPosition, new Vector3(0.0f, 0.0f, 0.0f));
                m_PlayerFloatingPanel.SetBackground(false);
                m_PlayerFloatingPanel.RTransform.localScale = Vector3.one;

                m_PlayerFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.PlayerFloatingPanel>();
                m_PlayerFloatingPanel.SetViewController(m_PlayerFloatingPanelView);
                m_PlayerFloatingPanel.SetGearIcon(CP_SDK.UI.Components.CFloatingPanel.ECorner.TopRight);
                m_PlayerFloatingPanel.OnGearIcon((_) =>
                {
                    var l_Items = GetSettingsViewControllers();
                    CP_SDK.UI.FlowCoordinators.MainFlowCoordinator.Instance().Present();
                    CP_SDK.UI.FlowCoordinators.MainFlowCoordinator.Instance().ChangeViewControllers(l_Items.Item1, l_Items.Item2, l_Items.Item3);
                });

                m_PlayerFloatingPanelView.OnMusicChanged(m_CurrentMusic);
                m_PlayerFloatingPanelView.SetIsPaused(m_IsPaused);
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
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_CreateFloatingPlayerCoroutine);
                    m_CreateFloatingPlayerCoroutine = null;
                }

                CP_SDK.UI.UISystem.DestroyUI(ref m_PlayerFloatingPanel, ref m_PlayerFloatingPanelView);
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
        /// Start a previous music
        /// </summary>
        internal void StartPreviousMusic()
        {
            if (!m_MusicProvider.IsReady)
            {
                if (m_WaitUntillReadyCoroutine != null)
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitUntillReadyCoroutine);

                m_WaitUntillReadyCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_WaitUntilReady(() => StartPreviousMusic()));
                return;
            }

            /// Decrement for next song
            m_CurrentSongIndex--;

            /// Handle overflow
            if (m_CurrentSongIndex < 0)                             m_CurrentSongIndex = m_MusicProvider.Musics.Count - 1;
            if (m_CurrentSongIndex >= m_MusicProvider.Musics.Count) m_CurrentSongIndex = 0;

            /// Load and play audio clip
            LoadNextMusic(false);
        }
        /// <summary>
        /// Start a new music
        /// </summary>
        /// <param name="p_Random">Pick a random song?</param>
        /// <param name="p_OnSceneTransition">On scene transition?</param>
        internal void StartNewMusic(bool p_Random = false, bool p_OnSceneTransition = false)
        {
            if (!m_MusicProvider.IsReady)
            {
                if (m_WaitUntillReadyCoroutine != null)
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitUntillReadyCoroutine);

                m_WaitUntillReadyCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_WaitUntilReady(() => StartNewMusic(p_Random, p_OnSceneTransition)));
                return;
            }

            if (p_Random)
                m_MusicProvider.Shuffle();

            m_CurrentSongIndex++;

            if (m_LastPlayingRescue)
            {
                var l_SongIndex = m_MusicProvider.Musics.FindIndex(x => x.GetSongPath() == MMConfig.Instance.LastPlayingSongPath);
                if (l_SongIndex != -1)
                    m_CurrentSongIndex = l_SongIndex;

                m_LastPlayingRescue = false;
            }

            /// Load and play audio clip
            LoadNextMusic(p_OnSceneTransition);
        }
        /// <summary>
        /// Start a next music
        /// </summary>
        internal void StartNextMusic()
        {
            if (!m_MusicProvider.IsReady)
            {
                if (m_WaitUntillReadyCoroutine != null)
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitUntillReadyCoroutine);

                m_WaitUntillReadyCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_WaitUntilReady(() => StartNextMusic()));
                return;
            }

            /// Increment for next song
            m_CurrentSongIndex++;

            /// Load and play audio clip
            LoadNextMusic(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load the next music
        /// </summary>
        /// <param name="p_OnSceneTransition">Is on scene transition?</param>
        private void LoadNextMusic(bool p_OnSceneTransition)
        {
            if (m_MusicProvider.Musics.Count == 0)
                return;

            /// Handle overflow
            if (m_CurrentSongIndex < 0)                             m_CurrentSongIndex = m_MusicProvider.Musics.Count - 1;
            if (m_CurrentSongIndex >= m_MusicProvider.Musics.Count) m_CurrentSongIndex = 0;

            var l_MusicToLoad = m_MusicProvider.Musics[m_CurrentSongIndex];

            /// Save
            MMConfig.Instance.LastPlayingSongPath = l_MusicToLoad.GetSongPath();
            MMConfig.Instance.Save();

            m_FastCancellationToken.Cancel();
            l_MusicToLoad.GetAudioAsync(m_FastCancellationToken, (p_AudioClip) => {
                CP_SDK.Unity.MTCoroutineStarter.EnqueueFromThread(Coroutine_LoadAudioClip(p_OnSceneTransition, l_MusicToLoad, p_AudioClip));
            }, () => StartNextMusic());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Wait until music provider is ready
        /// </summary>
        /// <param name="p_Callback">Callback action</param>
        /// <returns></returns>
        private IEnumerator Coroutine_WaitUntilReady(Action p_Callback)
        {
            yield return new WaitUntil(() => m_MusicProvider != null && m_MusicProvider.IsReady);
            p_Callback?.Invoke();
        }
        /// <summary>
        /// Load the song into the preview player
        /// </summary>
        /// <param name="p_OnSceneTransition">On scene transition?</param>
        /// <returns></returns>
        private IEnumerator Coroutine_LoadAudioClip(bool p_OnSceneTransition, Data.Music p_Music, AudioClip p_AudioClip)
        {
            if (m_WaitAndPlayNextSongCoroutine != null)
            {
                CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitAndPlayNextSongCoroutine);
                m_WaitAndPlayNextSongCoroutine = null;
            }

            /// Skip if it's not the menu
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            yield return new WaitUntil(() => m_PreviewPlayer = Resources.FindObjectsOfTypeAll<SongPreviewPlayer>().First());

            if (p_OnSceneTransition)
            {
                if (m_PreviewPlayer)
                    m_PreviewPlayer.FadeOut(m_PreviewPlayer._crossFadeToDefaultSpeed);

                yield return new WaitForSeconds(2f);
            }

            /// Skip if it's not the menu
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                yield break;

            m_CurrentMusic          = p_Music;
            m_CurrentMusicAudioClip = p_AudioClip;

            if (m_PreviewPlayer != null && m_PreviewPlayer && m_CurrentMusicAudioClip != null)
            {
                /// Wait that the song is loaded in background
                while (m_CurrentMusicAudioClip.loadState != AudioDataLoadState.Loaded
                    && m_CurrentMusicAudioClip.loadState != AudioDataLoadState.Failed)
                {
                    yield return null;
                }

                /// Check if we changed scene during loading
                if (!m_PreviewPlayer || CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu)
                    yield break;

                if (m_CurrentMusicAudioClip.loadState == AudioDataLoadState.Loaded)
                {
                    bool l_Failed = false;

                    try
                    {
                        if (m_WaitAndPlayNextSongCoroutine != null)
                        {
                            CP_SDK.Unity.MTCoroutineStarter.Stop(m_WaitAndPlayNextSongCoroutine);
                            m_WaitAndPlayNextSongCoroutine = null;
                        }

                        m_PreviewPlayer._defaultAudioClip = m_CurrentMusicAudioClip;

                        var l_Volume = MMConfig.Instance.PlaybackVolume;
                        if (CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Audio Tweaker"))
                            l_Volume = 1.0f;

                        m_PreviewPlayer._ambientVolumeScale = l_Volume;
                        m_PreviewPlayer._volumeScale        = l_Volume;

                        float l_StartTime = (MMConfig.Instance.StartSongFromBeginning || m_CurrentMusicAudioClip.length < 60) ? 0f : Mathf.Max(UnityEngine.Random.Range(m_CurrentMusicAudioClip.length * 0.2f, m_CurrentMusicAudioClip.length * 0.8f), 0.0f);

                        m_PreviewPlayer.CrossfadeTo(m_CurrentMusicAudioClip, l_Volume, l_StartTime, -1f, () => { });

                        m_BackupTimeClip    = m_CurrentMusicAudioClip;
                        m_BackupTime        = l_StartTime;

                        if (m_PlayerFloatingPanelView != null)
                            m_PlayerFloatingPanelView.OnMusicChanged(p_Music);

                        m_WaitAndPlayNextSongCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_WaitAndPlayNextMusic(m_CurrentMusicAudioClip.length));
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
                        if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.EGenericScene.Menu)
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
        private IEnumerator Coroutine_WaitAndPlayNextMusic(float p_EndTime)
        {
            var l_Interval          = 1f / 16f;
            var l_Waiter            = new WaitForSeconds(l_Interval);

            do
            {
                /// Skip if it's not the menu
                if (CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.EGenericScene.Menu || !m_PreviewPlayer)
                {
                    m_WaitAndPlayNextSongCoroutine = null;
                    yield break;
                }

                var l_ChannelsController = m_PreviewPlayer._audioSourceControllers;
                if (l_ChannelsController != null)
                {
                    for (var l_I = 0; l_I < l_ChannelsController.Length; ++l_I)
                    {
                        var l_ChannelController = l_ChannelsController[l_I];
                        var l_Channel           = l_ChannelController.audioSource;

                        if (!m_IsPaused
                            && !l_Channel.isPlaying
                            && l_Channel.clip == m_CurrentMusicAudioClip
                            && Array.IndexOf(l_ChannelsController, l_ChannelController) == m_PreviewPlayer._activeChannel)
                        {
                            l_Channel.UnPause();
                        }

                        if (l_Channel.isPlaying && l_Channel.clip == m_CurrentMusicAudioClip)
                        {
                            if (m_BackupTimeClip == null || m_BackupTimeClip != m_CurrentMusicAudioClip)
                            {
                                m_BackupTimeClip = m_CurrentMusicAudioClip;
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
                                var l_Volume = MMConfig.Instance.PlaybackVolume;
                                if (CP_SDK.ChatPlexSDK.GetModules().Any(x => x.Name == "Audio Tweaker"))
                                    l_Volume = 1.0f;

                                m_PreviewPlayer._ambientVolumeScale = l_Volume;
                                m_PreviewPlayer._volumeScale        = l_Volume;
                            }

                            if (Mathf.Abs(p_EndTime - l_Channel.time) < (MMConfig.Instance.LoopCurrentMusic ? l_Interval : 3f))
                            {
                                m_WaitAndPlayNextSongCoroutine = null;
                                if (MMConfig.Instance.LoopCurrentMusic && l_Channel.clip.length >= 10f)
                                {
                                    l_Channel.time = 0f;
                                    m_BackupTime   = 0f;
                                }
                                else
                                {
                                    StartNextMusic();
                                    yield break;
                                }
                            }
                        }
                    }
                }

                yield return l_Waiter;

            } while (true);
        }
    }
}
