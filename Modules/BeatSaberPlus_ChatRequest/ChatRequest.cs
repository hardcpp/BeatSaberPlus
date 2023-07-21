using IPA.Utilities;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest
{
    /// <summary>
    /// Chat Request instance
    /// </summary>
    public partial class ChatRequest : CP_SDK.ModuleBase<ChatRequest>
    {
        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Chat Request";
        public override string                              Description         => "Take song request from your chat!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#chat-request";
        public override bool                                UseChatFeatures     => true;
        public override bool                                IsEnabled           { get => CRConfig.Instance.Enabled; set { CRConfig.Instance.Enabled = value; CRConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView     m_SettingsLeftView  = null;
        private UI.SettingsMainView     m_SettingsMainView  = null;
        private UI.SettingsRightView    m_SettingsRightView = null;

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
                Logger.Instance.Error($"[ChatRequest][ChatRequest.OnEnable] Failed to create directory \"{System.IO.Path.GetDirectoryName(m_DBFilePath)}\"");
                Logger.Instance.Error(l_Exception);
            }

            /// Try to load DB
            LoadDatabase();
            OnQueueChanged(true, true);

            /// Build command table
            BuildCommandTable();

            /// Bind events
            BeatSaberPlus.SDK.Game.Logic.OnMenuSceneLoaded += OnMenuSceneLoaded;
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange     += OnSceneChange;

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                CP_SDK.Chat.Service.Acquire();

                /// Run all services
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(CreateButtonCoroutine());

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
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                CP_SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            /// Stop coroutine
            if (m_CreateButtonCoroutine != null)
            {
                CP_SDK.Unity.MTCoroutineStarter.Stop(m_CreateButtonCoroutine);
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

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsRightView);

            UI.ManagerViewFlowCoordinator.Destroy();

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
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsLeftView == null)     m_SettingsLeftView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();
            if (m_SettingsMainView == null)     m_SettingsMainView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsRightView == null)    m_SettingsRightView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsRightView>();

            return (m_SettingsMainView, m_SettingsLeftView, m_SettingsRightView);
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
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_CreateButtonCoroutine);
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
                    m_CreateButtonCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(CreateButtonCoroutine());
            }
        }
        /// <summary>
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void OnSceneChange(BeatSaberPlus.SDK.Game.Logic.ESceneType p_Scene)
        {
            if (p_Scene == BeatSaberPlus.SDK.Game.Logic.ESceneType.Menu)
                UpdateButton();
            else if (p_Scene == BeatSaberPlus.SDK.Game.Logic.ESceneType.Playing)
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
                            m_LastPlayingLevelResponse  = CRConfig.Instance.SafeMode2 ? "" : l_CurrentMap.level.songName.Replace(".", " . ") + " by " + l_CurrentMap.level.levelAuthorName.Replace(".", " . ");

                            if (l_CurrentMap.level is CustomBeatmapLevel
                                && l_CurrentMap.level.levelID.StartsWith("custom_level_"))
                            {
                                var l_Hash = l_CurrentMap.level.levelID.Substring("custom_level_".Length).ToLower();
                                if (l_Hash != "")
                                {
                                    var l_CachedEntry = null as Data.SongEntry;

                                    lock (SongHistory)
                                        l_CachedEntry = SongHistory.Where(x => x.BeatSaver_Map != null && x.BeatSaver_Map.SelectMapVersion().hash.ToLower() == l_Hash).FirstOrDefault();

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
                                        m_LastPlayingLevelResponse += " https://beatsaver.com/maps/" + l_CachedEntry.BeatSaver_Map.id;
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
            var l_LevelSelectionNavigationController    = null as LevelSelectionNavigationController;
            var l_Waiter                                = new WaitForSeconds(0.25f);
            while (true)
            {
                if (!l_LevelSelectionNavigationController)
                    l_LevelSelectionNavigationController = Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().LastOrDefault();

                if (l_LevelSelectionNavigationController != null && l_LevelSelectionNavigationController.gameObject.transform.childCount >= 2)
                    break;

                yield return l_Waiter;
            }

            m_ManagerButtonP = BeatSaberPlus.SDK.UI.Button.CreatePrimary(l_LevelSelectionNavigationController.transform, "Chat\nRequest", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonP.transform.localPosition = new Vector3(72.50f, 41.50f - 3, 2.6f);
            m_ManagerButtonP.transform.localScale    = new Vector3(0.8f, 0.6f, 0.8f);
            m_ManagerButtonP.transform.SetAsFirstSibling();
            m_ManagerButtonP.gameObject.SetActive(false);
            m_ManagerButtonP.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

            m_ManagerButtonS = BeatSaberPlus.SDK.UI.Button.Create(l_LevelSelectionNavigationController.transform, "Chat\nRequest", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonS.transform.localPosition = new Vector3(72.50f, 38.50f - 3, 2.6f);
            m_ManagerButtonS.transform.localScale    = new Vector3(0.8f, 0.6f, 0.8f);
            m_ManagerButtonS.transform.SetAsFirstSibling();
            m_ManagerButtonS.gameObject.SetActive(true);
            m_ManagerButtonP.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Normal;
            m_ManagerButtonS.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 4, 0, 0);

            var l_Images = m_ManagerButtonP.GetComponentsInChildren<HMUI.ImageView>();
            foreach (var l_Image in l_Images)
            {
                l_Image._skew = 0f;
                l_Image.SetAllDirty();
            }
            l_Images = m_ManagerButtonS.GetComponentsInChildren<HMUI.ImageView>();
            foreach (var l_Image in l_Images)
            {
                l_Image._skew = 0f;
                l_Image.SetAllDirty();
            }

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
