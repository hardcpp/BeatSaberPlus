using BeatSaberMarkupLanguage;
using BeatSaberPlus.Utils;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.ChatRequest
{
    /// <summary>
    /// Chat Request instance
    /// </summary>
    internal partial class ChatRequest : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name => "Chat Request";
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => Config.ChatRequest.Enabled; set => Config.ChatRequest.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override EActivationType ActivationType => EActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton
        /// </summary>
        internal static ChatRequest Instance = null;

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
        /// <summary>
        /// DB File path
        /// </summary>
        private string m_DBFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequestDB.json";
        /// <summary>
        /// Simple queue File path
        /// </summary>
        private string m_SimpleQueueFilePath = System.IO.Directory.GetCurrentDirectory() + "\\UserData\\BeatSaberPlus_ChatRequest_SimpleQueue.txt";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the plugin
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind singleton
            Instance = this;

            /// Create BeatSaver instance
            m_BeatSaver = new BeatSaverSharp.BeatSaver();
            m_BeatSaver.UseBeatSaberPlusCustomMapsServer(Config.Online.Enabled && Config.Online.UseBSPCustomMapsServer);

            /// Try to load DB
            LoadDatabase();
            UpdateSimpleQueueFile();

            /// Bind events
            Utils.Game.OnMenuSceneLoaded += OnMenuSceneLoaded;
            Utils.Game.OnSceneChange     += OnSceneChange;

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                Utils.ChatService.Acquire();

                /// Run all services
                Utils.ChatService.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = SharedCoroutineStarter.instance.StartCoroutine(CreateButtonCoroutine());

            /// Set queue status
            QueueOpen = Config.ChatRequest.QueueOpen;
        }
        /// <summary>
        /// Disable the plugin
        /// </summary>
        protected override void OnDisable()
        {
            /// Save database
            SaveDatabase();

            /// Unbind events
            Utils.Game.OnMenuSceneLoaded -= OnMenuSceneLoaded;
            Utils.Game.OnSceneChange     -= OnSceneChange;

            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                Utils.ChatService.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                Utils.ChatService.Release();
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
            m_RequestedThisSession.Clear();
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
        private void OnSceneChange(Utils.Game.SceneType p_Scene)
        {
            if (p_Scene == Utils.Game.SceneType.Menu)
                UpdateButton();
            else if (p_Scene == Utils.Game.SceneType.Playing)
            {
                if (BS_Utils.Plugin.LevelData != null
                    || BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData != null
                    || BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap != null)
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
                                BeatSaverSharp.Beatmap l_CachedBeatMap = null;

                                lock (SongHistory)
                                    l_CachedBeatMap = SongHistory.Where(x => x.BeatMap.Hash.ToLower() == l_Hash).FirstOrDefault()?.BeatMap ?? null;

                                if (l_CachedBeatMap == null)
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
                                    m_LastPlayingLevelResponse += " https://beatsaver.com/beatmap/" + l_CachedBeatMap.Key;
                                }
                            }
                        }
                    }
                }
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
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            BeatSaberPlus.UI.ViewFlowCoordinator.Instance.ChangeMainViewController(m_SettingsView, null, m_SettingsRightView);
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

            PrimaryButtonTag l_ButtonCreator = new PrimaryButtonTag();
            m_ManagerButtonP = l_ButtonCreator.CreateObject(p_LevelSelectionNavigationController.transform).GetComponent<Button>();
            m_ManagerButtonP.transform.localPosition = new Vector3(62.50f, 41.50f, 2.6f);
            m_ManagerButtonP.transform.localScale    = new Vector3(1.0f, 0.8f, 1.0f);
            m_ManagerButtonP.transform.SetAsLastSibling();
            m_ManagerButtonP.SetButtonText("Chat Request");
            m_ManagerButtonP.gameObject.SetActive(false);
            m_ManagerButtonP.onClick.RemoveAllListeners();
            m_ManagerButtonP.onClick.AddListener(() => OnButtonPressed());

            BeatSaberMarkupLanguage.Tags.ButtonTag l_ButtonCreator2 = new BeatSaberMarkupLanguage.Tags.ButtonTag();
            m_ManagerButtonS = l_ButtonCreator2.CreateObject(p_LevelSelectionNavigationController.transform).GetComponent<Button>();
            m_ManagerButtonS.transform.localPosition = new Vector3(62.50f, 38.50f, 2.6f);
            m_ManagerButtonS.transform.localScale    = new Vector3(1.0f, 0.8f, 1.0f);
            m_ManagerButtonS.transform.SetAsLastSibling();
            m_ManagerButtonS.SetButtonText("Chat Request");
            m_ManagerButtonS.gameObject.SetActive(true);
            m_ManagerButtonS.onClick.RemoveAllListeners();
            m_ManagerButtonS.onClick.AddListener(() => OnButtonPressed());

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
        /// <summary>
        /// On button pressed
        /// </summary>
        private void OnButtonPressed()
        {
            /// Create flow coordinator if needed
            if (m_ManagerViewFlowCoordinator == null)
                m_ManagerViewFlowCoordinator = BeatSaberMarkupLanguage.BeatSaberUI.CreateFlowCoordinator<UI.ManagerViewFlowCoordinator>();

            m_ManagerViewFlowCoordinator.Show();
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Primary button tag creator
    /// </summary>
    internal class PrimaryButtonTag : BeatSaberMarkupLanguage.Tags.ButtonTag
    {
        public override string[] Aliases => new[] { "primary-button", "action-button" };
        public override string PrefabButton => "PlayButton";

        public override GameObject CreateObject(Transform parent)
        {
            return base.CreateObject(parent).AddComponent<LayoutElement>().gameObject;
        }
    }
}
