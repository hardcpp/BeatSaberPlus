using BeatSaberMarkupLanguage;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest
{
    /// <summary>
    /// Chat Request instance
    /// </summary>
    public partial class ChatRequest : BeatSaberPlus.SDK.ModuleBase<ChatRequest>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseType Type => BeatSaberPlus.SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Chat Request";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Take song request from your chat!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => true;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => CRConfig.Instance.Enabled; set { CRConfig.Instance.Enabled = value; CRConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseActivationType ActivationType => BeatSaberPlus.SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create button coroutine
        /// </summary>
        private Coroutine m_CreateButtonCoroutine = null;
        /// <summary>
        /// Manager button
        /// </summary>
        private Button m_ManagerButtonP = null;
        /// <summary>
        /// Manager button
        /// </summary>
        private Button m_ManagerButtonS = null;
        /// <summary>
        /// Manager flow coordinator
        /// </summary>
        private UI.ManagerViewFlowCoordinator m_ManagerViewFlowCoordinator = null;
        /// <summary>
        /// Chat Request view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Chat Request left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Chat Request right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Create directory
            try
            {
                var l_Path = System.IO.Path.GetDirectoryName(m_DBFilePath);
                if (!System.IO.Directory.Exists(l_Path))
                    System.IO.Directory.CreateDirectory(l_Path);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Critical($"[ChatRequest][ChatRequest.OnEnable] Failed to create directory \"{System.IO.Path.GetDirectoryName(m_DBFilePath)}\"");
                Logger.Instance.Critical(l_Exception);
            }

            /// Move old file
            try
            {
                if (System.IO.File.Exists(m_DBFilePathOld))
                {
                    if (System.IO.File.Exists(m_DBFilePath))
                        System.IO.File.Delete(m_DBFilePathOld);
                    else
                        System.IO.File.Move(m_DBFilePathOld, m_DBFilePath);
                }
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Critical($"[ChatRequest][ChatRequest.OnEnable] Failed to move database \"{m_DBFilePathOld}\"");
                Logger.Instance.Critical(l_Exception);
            }

            /// Move old file
            try
            {
                if (System.IO.File.Exists(m_SimpleQueueFilePathOld))
                {
                    if (System.IO.File.Exists(m_SimpleQueueFilePath))
                        System.IO.File.Delete(m_SimpleQueueFilePathOld);
                    else
                        System.IO.File.Move(m_SimpleQueueFilePathOld, m_SimpleQueueFilePath);
                }
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Critical($"[ChatRequest][ChatRequest.OnEnable] Failed to move database \"{m_DBFilePathOld}\"");
                Logger.Instance.Critical(l_Exception);
            }

            /// Try to load DB
            LoadDatabase();
            UpdateSimpleQueueFile();

            /// Build command table
            BuildCommandTable();

            /// Bind events
            BeatSaberPlus.SDK.Game.Logic.OnMenuSceneLoaded += OnMenuSceneLoaded;
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange     += OnSceneChange;

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                BeatSaberPlus.SDK.Chat.Service.Acquire();

                /// Run all services
                BeatSaberPlus.SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());

            /// Set queue status
            QueueOpen = CRConfig.Instance.QueueOpen;
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Save database
            SaveDatabase();

            /// Unbind events
            BeatSaberPlus.SDK.Game.Logic.OnMenuSceneLoaded -= OnMenuSceneLoaded;
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange     -= OnSceneChange;

            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                BeatSaberPlus.SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                BeatSaberPlus.SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            /// Stop coroutine
            if (m_CreateButtonCoroutine != null)
            {
                SharedCoroutineStarter.instance.StopCoroutine(m_CreateButtonCoroutine);
                m_CreateButtonCoroutine = null;
            }

            /// Destroy manager button
            if (m_ManagerButtonP != null)
            {
                GameObject.Destroy(m_ManagerButtonP.gameObject);
                m_ManagerButtonP = null;
            }
            if (m_ManagerButtonS != null)
            {
                GameObject.Destroy(m_ManagerButtonS.gameObject);
                m_ManagerButtonS = null;
            }

            /// Destroy flow coordinator
            if (m_ManagerViewFlowCoordinator != null)
            {
                GameObject.Destroy(m_ManagerViewFlowCoordinator.gameObject);
                m_ManagerViewFlowCoordinator = null;
            }

            /// Clear database
            SongQueue.Clear();
            SongHistory.Clear();
            SongBlackList.Clear();
            BannedUsers.Clear();
            BannedMappers.Clear();
            Remaps.Clear();
            AllowList.Clear();
            m_RequestedThisSession = new System.Collections.Concurrent.ConcurrentBag<string>();
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
            /// Create view if needed
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the menu loaded
        /// </summary>
        private void OnMenuSceneLoaded()
        {
            if (m_ManagerButtonP == null || !m_ManagerButtonP || m_ManagerButtonS == null || !m_ManagerButtonS)
            {
                /// Stop coroutine
                if (m_CreateButtonCoroutine != null)
                {
                    SharedCoroutineStarter.instance.StopCoroutine(m_CreateButtonCoroutine);
                    m_CreateButtonCoroutine = null;
                }

                /// Destroy manager button
                if (m_ManagerButtonP != null)
                {
                    GameObject.Destroy(m_ManagerButtonP.gameObject);
                    m_ManagerButtonP = null;
                }
                if (m_ManagerButtonS != null)
                {
                    GameObject.Destroy(m_ManagerButtonS.gameObject);
                    m_ManagerButtonS = null;
                }

                /// Add button
                if (m_CreateButtonCoroutine == null)
                    m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());
            }
        }
        /// <summary>
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            if (p_Scene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                UpdateButton();
            else if (p_Scene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
            {
                try
                {
                    if (BeatSaberPlus.SDK.Game.Logic.LevelData != null
                        && BeatSaberPlus.SDK.Game.Logic.LevelData?.Data != null
                        && BeatSaberPlus.SDK.Game.Logic.LevelData?.Data.difficultyBeatmap != null)
                    {
                        var l_CurrentMap = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data.difficultyBeatmap;

                        if (m_LastPlayingLevel != l_CurrentMap.level)
                        {
                            m_LastPlayingLevel          = l_CurrentMap.level;
                            m_LastPlayingLevelResponse  = l_CurrentMap.level.songName.Replace(".", " . ") + " by " + l_CurrentMap.level.levelAuthorName.Replace(".", " . ");

                            if (l_CurrentMap.level is CustomBeatmapLevel
                                && l_CurrentMap.level.levelID.StartsWith("custom_level_"))
                            {
                                var l_Hash = l_CurrentMap.level.levelID.Substring("custom_level_".Length).ToLower();
                                if (l_Hash != "")
                                {
                                    SongEntry l_CachedEntry = null;

                                    lock (SongHistory)
                                        l_CachedEntry = SongHistory.Where(x => x.BeatMap != null && x.BeatMap.SelectMapVersion().hash.ToLower() == l_Hash).FirstOrDefault();

                                    if (l_CachedEntry == null)
                                    {
                                        BeatSaberPlus.SDK.Game.BeatMapsClient.GetOnlineByHash(l_Hash, (p_Valid, p_BeatMap) =>
                                        {
                                            if (   !p_Valid
                                                || l_CurrentMap.level != (BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.difficultyBeatmap?.level ?? null))
                                                return;

                                            m_LastPlayingLevelResponse += " https://beatsaver.com/maps/" + p_BeatMap.id;
                                        });
                                    }
                                    else
                                    {
                                        m_LastPlayingLevelResponse += " https://beatsaver.com/maps/" + l_CachedEntry.BeatMap.id;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (System.Exception p_Exception)
                {
                    Logger.Instance.Error("ChatRequest OnSceneChange");
                    Logger.Instance.Error(p_Exception);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create button coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateButtonCoroutine()
        {
            LevelSelectionNavigationController p_LevelSelectionNavigationController = null;

            while (true)
            {
                p_LevelSelectionNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().LastOrDefault();

                if (p_LevelSelectionNavigationController != null && p_LevelSelectionNavigationController.gameObject.transform.childCount >= 2)
                    break;

                yield return new WaitForSeconds(0.25f);
            }

            m_ManagerButtonP = BeatSaberPlus.SDK.UI.Button.CreatePrimary(p_LevelSelectionNavigationController.transform, "Chat\nRequest", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonP.transform.localPosition = new Vector3(72.50f, 41.50f - 3, 2.6f);
            m_ManagerButtonP.transform.localScale    = new Vector3(0.8f, 0.6f, 0.8f);
            m_ManagerButtonP.transform.SetAsFirstSibling();
            m_ManagerButtonP.gameObject.SetActive(false);

            m_ManagerButtonS = BeatSaberPlus.SDK.UI.Button.Create(p_LevelSelectionNavigationController.transform, "Chat\nRequest", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonS.transform.localPosition = new Vector3(72.50f, 38.50f - 3, 2.6f);
            m_ManagerButtonS.transform.localScale    = new Vector3(0.8f, 0.6f, 0.8f);
            m_ManagerButtonS.transform.SetAsFirstSibling();
            m_ManagerButtonS.gameObject.SetActive(true);
            m_ManagerButtonS.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 4, 0, 0);

            UpdateButton();

            m_CreateButtonCoroutine = null;
        }
        /// <summary>
        /// Update button text
        /// </summary>
        internal void UpdateButton()
        {
            if (m_ManagerButtonP == null || m_ManagerButtonS == null)
                return;

            m_ManagerButtonP.transform.localPosition = new Vector3(72.50f, 41.50f - 3, 2.6f);
            m_ManagerButtonP.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);

            m_ManagerButtonS.transform.localPosition = new Vector3(72.50f, 38.50f - 3, 2.6f);
            m_ManagerButtonS.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);

            m_ManagerButtonP.gameObject.SetActive(SongQueue.Count != 0);
            m_ManagerButtonS.gameObject.SetActive(SongQueue.Count == 0);
        }
    }
}
