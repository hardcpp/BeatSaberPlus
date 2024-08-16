using CP_SDK_WebSocketSharp.Server;
using IPA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus_SongOverlay.Network
{
    /// <summary>
    /// Socket server
    /// </summary>
    class OverlayServer
    {
        private const int SERVER_PORT = 2947;
        private const int PROTOCOL_VERSION = 1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static HttpServer m_HttpServer;
        private static bool m_ThreadRunning = true;
        private static bool m_HavingClients = false;
        private static List<OverlaySession> m_Clients = new List<OverlaySession>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static string m_Handshake = "{}";
        private static Models.Event m_GameStateEvent    = new Models.Event() { gameStateChanged = "None" };
        private static Models.Event m_MapInfoEvent      = new Models.Event() { mapInfoChanged = new Models.MapInfo() };
        private static Models.Event m_PauseEvent        = new Models.Event() { pauseTime = 0 };
        private static Models.Event m_ResumeEvent       = new Models.Event() { resumeTime = 0 };
        private static Models.Event m_ScoreEvent        = new Models.Event() { scoreEvent = new Models.Score() };

        private static bool m_MapInfoEventQueued;
        private static bool m_GameStateEventQueued;
        private static bool m_PauseEventQueued;
        private static bool m_ResumeEventQueued;
        private static bool m_ScoreEventQueued;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is the gameplay paused
        /// </summary>
        private static bool m_IsPaused;
        /// <summary>
        /// Audio time sync controller
        /// </summary>
        private static AudioTimeSyncController m_AudioTimeSyncController;
        /// <summary>
        /// Score controller instance
        /// </summary>
        private static ScoreController m_ScoreController;
        /// <summary>
        /// Combo controller instance
        /// </summary>
        private static ComboController m_ComboController;
        /// <summary>
        /// Game energy counter instance
        /// </summary>
        private static GameEnergyCounter m_GameEnergyCounter;
        /// <summary>
        /// Pause controller
        /// </summary>
        private static PauseController m_PauseController;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize external overlay server
        /// </summary>
        internal static void Start()
        {
            /// Bind events
            CP_SDK_BS.Game.Logic.OnSceneChange += Logic_OnSceneChange;
            Application.quitting += Stop;

            /// Prepare events
            m_GameStateEvent.FeedEvent();
            m_MapInfoEvent.FeedEvent();
            m_PauseEvent.FeedEvent();
            m_ResumeEvent.FeedEvent();
            m_ScoreEvent.FeedEvent();

            m_ThreadRunning = true;

            /// Start web socket server
            new Thread(() =>
            {
                var l_WaitCount = 0;
                while (string.IsNullOrEmpty(CP_SDK_BS.Game.UserPlatform.GetUserID()) && l_WaitCount < 20)
                {
                    Thread.Sleep(1000);
                    l_WaitCount++;
                }

                var l_Handshake = new JObject()
                {
                    ["_type"]               = "handshake",
                    ["protocolVersion"]     = PROTOCOL_VERSION,
                    ["gameVersion"]         = Application.version,
                    ["playerName"]          = CP_SDK_BS.Game.UserPlatform.GetUserName(),
                    ["playerPlatformId"]    = CP_SDK_BS.Game.UserPlatform.GetUserID(),
                };

#if DEBUG
                m_Handshake = JsonConvert.SerializeObject(l_Handshake, Formatting.Indented);
#else
                m_Handshake = JsonConvert.SerializeObject(l_Handshake);
#endif

                InitServer();

                while (m_ThreadRunning)
                {
                    if (m_HavingClients)
                    {
                        lock (m_Clients)
                        {
                            if (m_MapInfoEventQueued)
                            {
                                var l_SerializedData = JsonConvert.SerializeObject(m_MapInfoEvent);
                                m_MapInfoEventQueued = false;

                                for (int l_I = 0; l_I < m_Clients.Count; ++l_I)
                                    m_Clients[l_I].SendData(l_SerializedData);
                            }

                            if (m_GameStateEventQueued)
                            {
                                var l_SerializedData = JsonConvert.SerializeObject(m_GameStateEvent);
                                m_GameStateEventQueued = false;

                                for (int l_I = 0; l_I < m_Clients.Count; ++l_I)
                                    m_Clients[l_I].SendData(l_SerializedData);
                            }

                            if (m_ResumeEventQueued)
                            {
                                var l_SerializedData = JsonConvert.SerializeObject(m_ResumeEvent);
                                m_ResumeEventQueued = false;

                                for (int l_I = 0; l_I < m_Clients.Count; ++l_I)
                                    m_Clients[l_I].SendData(l_SerializedData);
                            }

                            if (m_PauseEventQueued)
                            {
                                var l_SerializedData = JsonConvert.SerializeObject(m_PauseEvent);
                                m_PauseEventQueued = false;

                                for (int l_I = 0; l_I < m_Clients.Count; ++l_I)
                                    m_Clients[l_I].SendData(l_SerializedData);
                            }

                            if (m_ScoreEventQueued)
                            {
                                var l_SerializedData = JsonConvert.SerializeObject(m_ScoreEvent);
                                m_ScoreEventQueued = false;

                                for (int l_I = 0; l_I < m_Clients.Count; ++l_I)
                                    m_Clients[l_I].SendData(l_SerializedData);
                            }
                        }
                    }

                    Thread.Sleep(TimeSpan.FromMilliseconds(33));
                }

                StopServer();
            }).Start();
        }
        /// <summary>
        /// On application quitting
        /// </summary>
        internal static void Stop()
        {
            /// Unbing events
            Application.quitting -= Stop;
            CP_SDK_BS.Game.Logic.OnSceneChange -= Logic_OnSceneChange;

            m_ThreadRunning = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On client connected
        /// </summary>
        /// <param name="p_Client">Client session</param>
        internal static void OnClientConnected(OverlaySession p_Client)
        {
            lock (m_Clients)
            {
                if (!m_Clients.Contains(p_Client))
                    m_Clients.Add(p_Client);

                m_HavingClients = true;
            }

            try
            {
                p_Client.SendData(m_Handshake);

                if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
                {
                    p_Client.SendData(JsonConvert.SerializeObject(m_MapInfoEvent));
                    p_Client.SendData(JsonConvert.SerializeObject(m_ScoreEvent));
                    p_Client.SendData(JsonConvert.SerializeObject(m_GameStateEvent));

                    if (m_PauseController)
                    {
                        if (m_IsPaused)
                        {
                            m_PauseEvent.pauseTime = m_AudioTimeSyncController.songTime;
                            p_Client.SendData(JsonConvert.SerializeObject(m_PauseEvent));
                        }
                        else
                        {
                            m_ResumeEvent.resumeTime = m_AudioTimeSyncController.songTime;
                            p_Client.SendData(JsonConvert.SerializeObject(m_ResumeEvent));
                        }
                    }
                }
                else
                    p_Client.SendData(JsonConvert.SerializeObject(m_GameStateEvent));
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error(p_Exception);
            }
        }
        /// <summary>
        /// On client disconnected
        /// </summary>
        /// <param name="p_Client">Client session</param>
        internal static void OnClientDisconnected(OverlaySession p_Client)
        {
            lock (m_Clients)
            {
                m_Clients.Remove(p_Client);
                m_HavingClients = m_Clients.Count > 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On Game State changed
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private static void Logic_OnSceneChange(CP_SDK_BS.Game.Logic.ESceneType p_Scene)
        {
            if (p_Scene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
            {
                m_IsPaused = false;

                var l_Map = CP_SDK_BS.Game.Logic.LevelData;
                if (l_Map == null)
                    return;

                var l_WorkingMapInfo = m_MapInfoEvent.mapInfoChanged;
                var l_CoverTask      = null as Task<Sprite>;

#if BEATSABER_1_35_0_OR_NEWER
                if (l_WorkingMapInfo.level_id != l_Map.Data.beatmapLevel.levelID)
                {
                    try { l_CoverTask = l_Map.Data.beatmapLevel.previewMediaData.GetCoverSpriteAsync(CancellationToken.None); } catch { }

                    l_WorkingMapInfo.level_id   = l_Map.Data.beatmapLevel.levelID;
                    l_WorkingMapInfo.name       = l_Map.Data.beatmapLevel.songName;
                    l_WorkingMapInfo.sub_name   = l_Map.Data.beatmapLevel.songSubName;
                    l_WorkingMapInfo.artist     = l_Map.Data.beatmapLevel.songAuthorName;
                    l_WorkingMapInfo.mapper     = l_Map.Data.beatmapLevel.allMappers.FirstOrDefault();
                    l_WorkingMapInfo.duration   = (uint)(l_Map.Data.beatmapLevel.songDuration * 1000f);
                    l_WorkingMapInfo.BPM        = l_Map.Data.beatmapLevel.beatsPerMinute;
                    l_WorkingMapInfo.PP         = 0f;
                    l_WorkingMapInfo.BSRKey     = "";
                }

                l_WorkingMapInfo.characteristic = l_Map.Data.beatmapKey.beatmapCharacteristic.serializedName.ToString();
                l_WorkingMapInfo.difficulty     = l_Map.Data.beatmapKey.difficulty.ToString();
#else
                if (l_WorkingMapInfo.level_id != l_Map.Data.previewBeatmapLevel.levelID)
                {
                    try { l_CoverTask = l_Map.Data.previewBeatmapLevel.GetCoverImageAsync(CancellationToken.None); } catch { }

                    l_WorkingMapInfo.level_id   = l_Map.Data.previewBeatmapLevel.levelID;
                    l_WorkingMapInfo.name       = l_Map.Data.previewBeatmapLevel.songName;
                    l_WorkingMapInfo.sub_name   = l_Map.Data.previewBeatmapLevel.songSubName;
                    l_WorkingMapInfo.artist     = l_Map.Data.previewBeatmapLevel.songAuthorName;
                    l_WorkingMapInfo.mapper     = l_Map.Data.previewBeatmapLevel.levelAuthorName;
                    l_WorkingMapInfo.duration   = (uint)(l_Map.Data.previewBeatmapLevel.songDuration * 1000f);
                    l_WorkingMapInfo.BPM        = l_Map.Data.previewBeatmapLevel.beatsPerMinute;
                    l_WorkingMapInfo.PP         = 0f;
                    l_WorkingMapInfo.BSRKey     = "";
                }

                l_WorkingMapInfo.characteristic = l_Map.Data.difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName.ToString();
                l_WorkingMapInfo.difficulty     = l_Map.Data.difficultyBeatmap.difficulty.ToString();
#endif

                CP_SDK.Unity.MTCoroutineStarter.Start(Coroutine_WaitForGameplayReady(l_Map.Type, l_CoverTask));
            }
            else if (p_Scene == CP_SDK_BS.Game.Logic.ESceneType.Menu)
            {
                m_IsPaused                  = false;
                m_ScoreController           = null;
                m_AudioTimeSyncController   = null;
                m_PauseController           = null;

                m_GameStateEvent.gameStateChanged = p_Scene.ToString();

                m_GameStateEventQueued = true;
            }
        }
        /// <summary>
        /// On gameplay start coroutine
        /// </summary>
        /// <returns></returns>
        private static IEnumerator Coroutine_WaitForGameplayReady(CP_SDK_BS.Game.LevelType p_Type, Task<Sprite> p_CoverTask)
        {
            if (p_CoverTask != null)
            {
                yield return new WaitUntil(() => p_CoverTask.IsCompleted || p_CoverTask.Exception != null);

                var l_BackupRenderTexture = RenderTexture.active;
                try
                {
                    var l_Texture           = p_CoverTask.Result.texture;
                    var l_NewRenderTexture  = RenderTexture.GetTemporary(l_Texture.width, l_Texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

                    Graphics.Blit(l_Texture, l_NewRenderTexture);
                    RenderTexture.active = l_NewRenderTexture;

                    var l_Rect  = p_CoverTask.Result.rect;
                    var l_UV    = p_CoverTask.Result.uv[0];

                    var l_NewCover = new Texture2D((int)l_Rect.width, (int)l_Rect.height);

                    l_NewCover.ReadPixels(new Rect(
                        l_UV.x * l_Texture.width,
                        l_Texture.height - l_UV.y * l_Texture.height,
                        l_Rect.width,
                        l_Rect.height
                    ), 0, 0);
                    l_NewCover.Apply();

                    RenderTexture.ReleaseTemporary(l_NewRenderTexture);

                    m_MapInfoEvent.mapInfoChanged.coverRaw = System.Convert.ToBase64String(ImageConversion.EncodeToPNG(l_NewCover));
                }
                catch
                {
                    m_MapInfoEvent.mapInfoChanged.coverRaw = "";
                }
                RenderTexture.active = l_BackupRenderTexture;
            }

            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault());
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault());
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<ComboController>().LastOrDefault());
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<GameEnergyCounter>().LastOrDefault());

            if (p_Type != CP_SDK_BS.Game.LevelType.Multiplayer)
                yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<PauseController>().LastOrDefault());

            m_AudioTimeSyncController   = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            m_ScoreController           = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault();
            m_ComboController           = Resources.FindObjectsOfTypeAll<ComboController>().LastOrDefault();
            m_GameEnergyCounter         = Resources.FindObjectsOfTypeAll<GameEnergyCounter>().LastOrDefault();

            m_MapInfoEvent.mapInfoChanged.time              = m_AudioTimeSyncController.songTime;
            m_MapInfoEvent.mapInfoChanged.timeMultiplier    = m_AudioTimeSyncController.timeScale;

            m_MapInfoEventQueued = true;

            m_GameStateEvent.gameStateChanged = CP_SDK_BS.Game.Logic.ActiveScene.ToString();

            m_GameStateEventQueued = true;

            m_ScoreEvent.scoreEvent.time            = m_AudioTimeSyncController.songTime;
            m_ScoreEvent.scoreEvent.score           = 0;
            m_ScoreEvent.scoreEvent.accuracy        = 1f;
            m_ScoreEvent.scoreEvent.combo           = 0;
            m_ScoreEvent.scoreEvent.missCount       = 0;
            m_ScoreEvent.scoreEvent.currentHealth   = m_GameEnergyCounter.energy;

            m_ScoreEventQueued = true;

            if (p_Type != CP_SDK_BS.Game.LevelType.Multiplayer)
                m_PauseController = Resources.FindObjectsOfTypeAll<PauseController>().LastOrDefault();

            m_ComboController.comboDidChangeEvent           += ComboController_comboDidChangeEvent;
            m_ScoreController.scoreDidChangeEvent           += ScoreController_scoreDidChangeEvent;
            m_GameEnergyCounter.gameEnergyDidChangeEvent    += GameEnergyCounter_gameEnergyDidChangeEvent;

            var l_BeatmapObjectManager = m_ScoreController._beatmapObjectManager;
            if (l_BeatmapObjectManager != null)
            {
                l_BeatmapObjectManager.noteWasCutEvent += ScoreController_noteWasCutEvent;
                l_BeatmapObjectManager.noteWasMissedEvent += ScoreController_noteWasMissedEvent;
            }

            if (m_PauseController)
            {
                m_PauseController.didPauseEvent += () =>
                {
                    m_IsPaused = true;

                    m_PauseEvent.pauseTime  = m_AudioTimeSyncController.songTime;
                    m_PauseEventQueued      = true;
                };
                m_PauseController.didResumeEvent += () =>
                {
                    m_IsPaused = false;

                    m_ResumeEvent.resumeTime    = m_AudioTimeSyncController.songTime;
                    m_ResumeEventQueued         = true;
                };

                m_IsPaused = m_PauseController._paused;
            }
            else
                m_IsPaused = false;

            if (m_IsPaused)
            {
                m_PauseEvent.pauseTime  = m_AudioTimeSyncController.songTime;
                m_PauseEventQueued      = true;
            }
            else
            {
                m_ResumeEvent.resumeTime    = m_AudioTimeSyncController.songTime;
                m_ResumeEventQueued         = true;
            }
        }
        /// <summary>
        /// Note was cut
        /// </summary>
        /// <param name="p_NoteController">Note controller</param>
        /// <param name="p_NoteCutInfo">Note Cut info</param>
        private static void ScoreController_noteWasCutEvent(NoteController p_NoteController, in NoteCutInfo p_NoteCutInfo)
        {
            if (   p_NoteCutInfo.noteData.scoringType   == NoteData.ScoringType.Ignore
                || p_NoteCutInfo.failReason             == NoteCutInfo.FailReason.None
                || p_NoteCutInfo.noteData.colorType     == ColorType.None)
                return;

            m_ScoreEvent.scoreEvent.missCount++;
            m_ScoreEventQueued = true;
        }
        /// <summary>
        /// Note was missed
        /// </summary>
        /// <param name="p_Note">Missed note data</param>
        private static void ScoreController_noteWasMissedEvent(NoteController p_Note)
        {
            if (   p_Note.noteData.scoringType  == NoteData.ScoringType.Ignore
                || p_Note.noteData.colorType    == ColorType.None)
                return;

            m_ScoreEvent.scoreEvent.missCount++;
            m_ScoreEventQueued = true;
        }
        /// <summary>
        /// Combo did change
        /// </summary>
        /// <param name="p_Combo">New combo</param>
        private static void ComboController_comboDidChangeEvent(int p_Combo)
        {
            m_ScoreEvent.scoreEvent.time            = m_AudioTimeSyncController.songTime;
            m_ScoreEvent.scoreEvent.combo           = (uint)p_Combo;
            m_ScoreEventQueued = true;
        }
        /// <summary>
        /// On score change
        /// </summary>
        /// <param name="p_RawScore">Raw score</param>
        /// <param name="p_Score">Modified score</param>
        private static void ScoreController_scoreDidChangeEvent(int p_RawScore, int p_Score)
        {
            m_ScoreEvent.scoreEvent.time            = m_AudioTimeSyncController.songTime;
            m_ScoreEvent.scoreEvent.score           = (uint)p_RawScore;
            m_ScoreEvent.scoreEvent.accuracy        = (float)p_Score / (float)m_ScoreController.immediateMaxPossibleMultipliedScore;
            m_ScoreEventQueued = true;
        }
        /// <summary>
        /// On game energy change
        /// </summary>
        /// <param name="p_Health">New health</param>
        private static void GameEnergyCounter_gameEnergyDidChangeEvent(float p_Health)
        {
            m_ScoreEvent.scoreEvent.time            = m_AudioTimeSyncController.songTime;
            m_ScoreEvent.scoreEvent.currentHealth   = p_Health;
            m_ScoreEventQueued = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the Http server
        /// </summary>
        private static void InitServer()
        {
#if DEBUG
            Logger.Instance.Debug("Starting SongOverlay server");
#endif

            m_HttpServer = new HttpServer(SERVER_PORT);
            m_HttpServer.AddWebSocketService<OverlaySession>("/socket");

            m_HttpServer.Start();
        }
        /// <summary>
        /// Stop the Http server
        /// </summary>
        private static void StopServer()
        {
#if DEBUG
            Logger.Instance.Debug("Stopping SongOverlay server");
#endif

            m_HttpServer.Stop();
        }
    }
}
