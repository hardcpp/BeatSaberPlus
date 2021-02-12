using BeatSaberMarkupLanguage;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.ChatRequest
{
    /// <summary>
    /// Chat Request instance
    /// </summary>
    internal partial class ChatRequest : SDK.ModuleBase<ChatRequest>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
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
        public override bool IsEnabled { get => Config.ChatRequest.Enabled; set => Config.ChatRequest.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

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
        /// BeatSaver API
        /// </summary>
        private BeatSaverSharp.BeatSaver m_BeatSaver = null;
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
            /// Create BeatSaver instance
            m_BeatSaver = new BeatSaverSharp.BeatSaver("bsp_chat_request", Plugin.Version.Major + "." + Plugin.Version.Minor + "." + Plugin.Version.Patch);

            /// Try to load DB
            LoadDatabase();
            UpdateSimpleQueueFile();

            /// Bind events
            SDK.Game.Logic.OnMenuSceneLoaded += OnMenuSceneLoaded;
            SDK.Game.Logic.OnSceneChange     += OnSceneChange;

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                SDK.Chat.Service.Acquire();

                /// Run all services
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());

            /// Set queue status
            QueueOpen = Config.ChatRequest.QueueOpen;
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Save database
            SaveDatabase();

            /// Unbind events
            SDK.Game.Logic.OnMenuSceneLoaded -= OnMenuSceneLoaded;
            SDK.Game.Logic.OnSceneChange     -= OnSceneChange;

            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            /// Destroy BeatSaver
            if (m_BeatSaver != null)
                m_BeatSaver = null;

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
        private void OnSceneChange(SDK.Game.Logic.SceneType p_Scene)
        {
            if (p_Scene == SDK.Game.Logic.SceneType.Menu)
                UpdateButton();
            else if (p_Scene == SDK.Game.Logic.SceneType.Playing)
            {
                try
                {
                    if (BS_Utils.Plugin.LevelData != null
                        && BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData != null
                        && BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap != null)
                    {
                        var l_CurrentMap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;

                        if (m_LastPlayingLevel != l_CurrentMap.level)
                        {
                            m_LastPlayingLevel          = l_CurrentMap.level;
                            m_LastPlayingLevelResponse  = l_CurrentMap.level.songName + " by " + l_CurrentMap.level.levelAuthorName;

                            if (l_CurrentMap.level is CustomBeatmapLevel
                                && l_CurrentMap.level.levelID.StartsWith("custom_level_"))
                            {
                                var l_Hash = l_CurrentMap.level.levelID.Substring("custom_level_".Length).ToLower();
                                if (l_Hash != "")
                                {
                                    SongEntry l_CachedEntry = null;

                                    lock (SongHistory)
                                        l_CachedEntry = SongHistory.Where(x => x.BeatMap != null && x.BeatMap.Hash.ToLower() == l_Hash).FirstOrDefault();

                                    if (l_CachedEntry == null)
                                    {
                                        m_BeatSaver.Hash(l_Hash).ContinueWith((x) =>
                                        {
                                            if (   x.Status != TaskStatus.RanToCompletion
                                                || x.Result == null
                                                || l_CurrentMap.level != (BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.difficultyBeatmap?.level ?? null))
                                                return;

                                            m_LastPlayingLevelResponse += " https://beatsaver.com/beatmap/" + x.Result.Key;
                                        });
                                    }
                                    else
                                    {
                                        m_LastPlayingLevelResponse += " https://beatsaver.com/beatmap/" + l_CachedEntry.BeatMap.Key;
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

            m_ManagerButtonP = SDK.UI.Button.CreatePrimary(p_LevelSelectionNavigationController.transform, "Chat Request", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonP.transform.localPosition = new Vector3(62.50f, 41.50f, 2.6f);
            m_ManagerButtonP.transform.localScale    = new Vector3(1.0f, 0.8f, 1.0f);
            m_ManagerButtonP.transform.SetAsLastSibling();
            m_ManagerButtonP.gameObject.SetActive(false);

            m_ManagerButtonS = SDK.UI.Button.Create(p_LevelSelectionNavigationController.transform, "Chat Request", () => UI.ManagerViewFlowCoordinator.Instance().Present(), null);
            m_ManagerButtonS.transform.localPosition = new Vector3(62.50f, 38.50f, 2.6f);
            m_ManagerButtonS.transform.localScale    = new Vector3(1.0f, 0.8f, 1.0f);
            m_ManagerButtonS.transform.SetAsLastSibling();
            m_ManagerButtonS.gameObject.SetActive(true);

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

            m_ManagerButtonP.transform.localPosition = new Vector3(62.50f, 41.50f, 2.6f);
            m_ManagerButtonP.transform.localScale = new Vector3(1.0f, 0.8f, 1.0f);

            m_ManagerButtonS.transform.localPosition = new Vector3(62.50f, 38.50f, 2.6f);
            m_ManagerButtonS.transform.localScale = new Vector3(1.0f, 0.8f, 1.0f);

            m_ManagerButtonP.gameObject.SetActive(SongQueue.Count != 0);
            m_ManagerButtonS.gameObject.SetActive(SongQueue.Count == 0);
        }
    }
}
