using CP_SDK.Chat.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if BEATSABER
using IPA.Utilities;
#endif

namespace ChatPlexMod_Chat
{
    /// <summary>
    /// Chat instance
    /// </summary>
    public class Chat : CP_SDK.ModuleBase<Chat>
    {
        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Chat";
        public override string                              Description         => "Allow people to distract you while playing!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#chat";
        public override bool                                UseChatFeatures     => true;
        public override bool                                IsEnabled           { get => CConfig.Instance.Enabled; set { CConfig.Instance.Enabled = value; CConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView     m_SettingsLeftView  = null;
        private UI.SettingsMainView     m_SettingsMainView  = null;
        private UI.SettingsRightView    m_SettingsRightView = null;

        private Transform                           m_RootTransform                     = null;
        private Transform                           m_DockedFloatingPanelTransform      = null;
        private CP_SDK.UI.Components.CFloatingPanel m_ChatFloatingPanel                 = null;
        private UI.ChatFloatingPanelView            m_ChatFloatingPanelView             = null;
        private CP_SDK.UI.Components.CFloatingPanel m_ChatHypeTrainFloatingPanel        = null;
        private UI.HypeTrainFloatingPanelView       m_ChatHypeTrainFloatingPanelView    = null;
        private CP_SDK.UI.Components.CFloatingPanel m_PollFloatingPanel                 = null;
        private UI.PollFloatingPanelView            m_PollFloatingPanelView             = null;
        private CP_SDK.UI.Components.CFloatingPanel m_ChatPredictionFloatingPanel       = null;
        private UI.PredictionFloatingPanelView      m_ChatPredictionFloatingPanelView   = null;
        private CP_SDK.UI.Components.CFloatingPanel m_StatusFloatingPanel               = null;
        private UI.StatusFloatingPanelView          m_StatusFloatingPanelView           = null;

        private bool                    m_ChatCoreAcquired  = false;
        private ConcurrentQueue<Action> m_ActionQueue       = new ConcurrentQueue<Action>();
        private bool                    m_ActionDequeueRun  = true;

        private Coroutine   m_CreateButtonCoroutine = null;
        private Button      m_ModerationButton      = null;

        private CP_SDK.Misc.RingBuffer<(IChatService, IChatUser)>   m_LastChatUsers     = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal List<(IChatService, IChatUser)> LastChatUsers => m_LastChatUsers == null ? new List<(IChatService, IChatUser)>() : m_LastChatUsers.ToList();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Create ring buffer
            m_LastChatUsers = new CP_SDK.Misc.RingBuffer<(IChatService, IChatUser)>(40);

            /// Bind events
            CP_SDK.ChatPlexSDK.OnGenericMenuSceneLoaded   += ChatPlexSDK_OnGenericMenuSceneLoaded;
            CP_SDK.ChatPlexSDK.OnGenericSceneChange       += ChatPlexSDK_OnGenericSceneChange;

            /// If we are already in menu scene, activate
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.EGenericScene.Menu)
                ChatPlexSDK_OnGenericSceneChange(CP_SDK.ChatPlexSDK.ActiveGenericScene);

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                CP_SDK.Chat.Service.Acquire();

                /// Run all services
                var l_Multiplexer = CP_SDK.Chat.Service.Multiplexer;
                l_Multiplexer.OnSystemMessage         += Mutiplixer_OnSystemMessage;
                l_Multiplexer.OnLogin                 += Mutiplixer_OnLogin;
                l_Multiplexer.OnJoinChannel           += Mutiplixer_OnJoinChannel;
                l_Multiplexer.OnLeaveChannel          += Mutiplixer_OnLeaveChannel;
                l_Multiplexer.OnChannelFollow         += Mutiplixer_OnChannelFollow;
                l_Multiplexer.OnChannelBits           += Mutiplixer_OnChannelBits;
                l_Multiplexer.OnChannelPoints         += Mutiplixer_OnChannelPoints;
                l_Multiplexer.OnChannelSubscription   += Mutiplixer_OnChannelSubscription;
                l_Multiplexer.OnTextMessageReceived   += Mutiplixer_OnTextMessageReceived;
                l_Multiplexer.OnRoomStateUpdated      += Mutiplixer_OnRoomStateUpdated;
                l_Multiplexer.OnChatCleared           += Mutiplixer_OnChatCleared;
                l_Multiplexer.OnMessageCleared        += Mutiplixer_OnMessageCleared;

                /// Get back channels
                foreach (var l_Channel in CP_SDK.Chat.Service.Multiplexer.Channels)
                    Mutiplixer_OnJoinChannel(l_Channel.Item1, l_Channel.Item2);

                /// Enable dequeue system
                m_ActionDequeueRun = true;

                /// Start dequeue task
                Task.Run(ActionDequeueTask).ConfigureAwait(false);
            }

            /// Add button
            if (m_CreateButtonCoroutine == null)
                m_CreateButtonCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(CreateButtonCoroutine());
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                var l_Multiplexer = CP_SDK.Chat.Service.Multiplexer;
                l_Multiplexer.OnSystemMessage         -= Mutiplixer_OnSystemMessage;
                l_Multiplexer.OnLogin                 -= Mutiplixer_OnLogin;
                l_Multiplexer.OnJoinChannel           -= Mutiplixer_OnJoinChannel;
                l_Multiplexer.OnLeaveChannel          -= Mutiplixer_OnLeaveChannel;
                l_Multiplexer.OnChannelFollow         -= Mutiplixer_OnChannelFollow;
                l_Multiplexer.OnChannelBits           -= Mutiplixer_OnChannelBits;
                l_Multiplexer.OnChannelPoints         -= Mutiplixer_OnChannelPoints;
                l_Multiplexer.OnChannelSubscription   -= Mutiplixer_OnChannelSubscription;
                l_Multiplexer.OnTextMessageReceived   -= Mutiplixer_OnTextMessageReceived;
                l_Multiplexer.OnRoomStateUpdated      -= Mutiplixer_OnRoomStateUpdated;
                l_Multiplexer.OnChatCleared           -= Mutiplixer_OnChatCleared;
                l_Multiplexer.OnMessageCleared        -= Mutiplixer_OnMessageCleared;

                /// Stop all chat services
                CP_SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;

                /// Stop dequeue task
                m_ActionDequeueRun = false;

                m_ActionQueue = new ConcurrentQueue<Action>();
                m_LastChatUsers.Clear();
            }

            /// Unbind events
            CP_SDK.ChatPlexSDK.OnGenericSceneChange       -= ChatPlexSDK_OnGenericSceneChange;
            CP_SDK.ChatPlexSDK.OnGenericMenuSceneLoaded   -= ChatPlexSDK_OnGenericMenuSceneLoaded;

            /// Stop coroutine
            if (m_CreateButtonCoroutine != null)
            {
                CP_SDK.Unity.MTCoroutineStarter.Stop(m_CreateButtonCoroutine);
                m_CreateButtonCoroutine = null;
            }

            /// Destroy moderation button
            if (m_ModerationButton != null)
            {
                GameObject.Destroy(m_ModerationButton.gameObject);
                m_ModerationButton = null;
            }

            /// Destroy
            DestroyFloatingPanels();

            UI.ModerationViewFlowCoordinator.Destroy();

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsRightView);
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
        private void ChatPlexSDK_OnGenericMenuSceneLoaded()
        {
            if (m_ModerationButton == null || !m_ModerationButton )
            {
                /// Stop coroutine
                if (m_CreateButtonCoroutine != null)
                {
                    CP_SDK.Unity.MTCoroutineStarter.Stop(m_CreateButtonCoroutine);
                    m_CreateButtonCoroutine = null;
                }

                /// Destroy moderation button
                if (m_ModerationButton != null)
                {
                    GameObject.Destroy(m_ModerationButton.gameObject);
                    m_ModerationButton = null;
                }

                /// Add button
                if (m_CreateButtonCoroutine == null)
                    m_CreateButtonCoroutine = CP_SDK.Unity.MTCoroutineStarter.Start(CreateButtonCoroutine());
            }
        }
        /// <summary>
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_SceneType"></param>
        private void ChatPlexSDK_OnGenericSceneChange(CP_SDK.EGenericScene p_SceneType)
        {
            if (m_RootTransform)
                m_RootTransform.transform.localScale = Vector3.one;

            if (p_SceneType == CP_SDK.EGenericScene.Menu)
                UpdateButton();

            if (m_ChatFloatingPanel == null)    CreateFloatingPanels();
            else                                UpdateFloatingPanels();

#if DANCEDASH
            var l_Player = Component.FindObjectsOfType<GameObject>().FirstOrDefault(x => x.activeSelf && x.transform.parent == null && x.name == "Player");
            if (l_Player)
                m_RootTransform.position = l_Player.transform.position;
#endif
        }
        /// <summary>
        /// When the floating window is moved
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void OnFloatingWindowMoved(CP_SDK.UI.Components.CFloatingPanel p_FloatingPanel)
        {
            m_DockedFloatingPanelTransform.localPosition = m_ChatFloatingPanel.RTransform.localPosition;
            m_DockedFloatingPanelTransform.localRotation = m_ChatFloatingPanel.RTransform.localRotation;
        }
        /// <summary>
        /// Toggle chat visibility
        /// </summary>
        public void ToggleVisibility()
        {
                 if (m_RootTransform && m_RootTransform.localScale.x > 0.5f)  m_RootTransform.localScale = Vector3.zero;
            else if (m_RootTransform)                                         m_RootTransform.localScale = Vector3.one;
        }
        /// <summary>
        /// Set visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        public void SetVisible(bool p_Visible)
        {
            if (!m_RootTransform)
                return;

            m_RootTransform.localScale = p_Visible ? Vector3.one : Vector3.zero;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create button coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CreateButtonCoroutine()
        {
#if BEATSABER
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

            m_ModerationButton = CP_SDK_BS.UI.Button.Create(l_LevelSelectionNavigationController.transform, "Chat\nModeration", () => UI.ModerationViewFlowCoordinator.Instance().Present(), null);
            m_ModerationButton.transform.localPosition      = new Vector3(72.50f, 27.00f, 2.60f);
            m_ModerationButton.transform.localScale         = new Vector3( 0.65f,  0.50f, 0.65f);
            m_ModerationButton.transform.SetAsFirstSibling();
            m_ModerationButton.gameObject.SetActive(true);
            m_ModerationButton.GetComponentInChildren<TextMeshProUGUI>().margin    = new Vector4(0, 4, 0, 0);
            m_ModerationButton.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Normal;

            var l_Images = m_ModerationButton.GetComponentsInChildren<HMUI.ImageView>();
            foreach (var l_Image in l_Images)
            {
                l_Image._skew = 0f;
                l_Image.SetAllDirty();
            }

            UpdateButton();

            m_CreateButtonCoroutine = null;
#elif UNITY_TESTING || SYNTHRIDERS || AUDIOTRIP || BOOMBOX || DANCEDASH
            yield return null;
#else
#error Missing game implementation
#endif
        }
        /// <summary>
        /// Update button text
        /// </summary>
        internal void UpdateButton()
        {
            if (m_ModerationButton == null)
                return;

#if BEATSABER
            m_ModerationButton.transform.localPosition  = new Vector3(72.50f, 27.00f, 2.60f);
            m_ModerationButton.transform.localScale     = new Vector3( 0.65f,  0.50f, 0.65f);
#elif UNITY_TESTING || SYNTHRIDERS || AUDIOTRIP || BOOMBOX || DANCEDASH
#else
#error Missing game implementation
#endif
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create floating panels
        /// </summary>
        private void CreateFloatingPanels()
        {
            if (m_RootTransform != null)
                return;

            try
            {
                /// Prepare root game object
                m_RootTransform = new GameObject("ChatPlexSDK_Chat").transform;
                GameObject.DontDestroyOnLoad(m_RootTransform.gameObject);

                m_DockedFloatingPanelTransform = new GameObject("DockedFloatingPanelTransform").transform;
                m_DockedFloatingPanelTransform.SetParent(m_RootTransform);

                ///////////////////////////////////////////////
                /// Chat
                m_ChatFloatingPanel     = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("ChatFloatingPanel", m_RootTransform);
                m_ChatFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.ChatFloatingPanelView>();
                m_ChatFloatingPanel.SetSize(CConfig.Instance.ChatSize);
                m_ChatFloatingPanel.SetAlignWithFloor(CConfig.Instance.AlignWithFloor);
                m_ChatFloatingPanel.SetBackground(true, CConfig.Instance.BackgroundColor);
                m_ChatFloatingPanel.SetRadius(0);
                m_ChatFloatingPanel.SetViewController(m_ChatFloatingPanelView);
                m_ChatFloatingPanel.OnRelease(OnFloatingWindowMoved);
                m_ChatFloatingPanel.SetSceneTransform(CP_SDK.EGenericScene.Menu, CConfig.Instance.MenuChatPosition,     CConfig.Instance.MenuChatRotation);
                m_ChatFloatingPanel.SetSceneTransform(CP_SDK.EGenericScene.Menu, CConfig.Instance.PlayingChatPosition,  CConfig.Instance.PlayingChatRotation);
                m_ChatFloatingPanel.OnSceneRelease(CP_SDK.EGenericScene.Menu, (p_LocalPosition, p_LocalRotation) =>
                {
                    CConfig.Instance.MenuChatPosition = p_LocalPosition;
                    CConfig.Instance.MenuChatRotation = p_LocalRotation;
                    CConfig.Instance.Save();
                });
                m_ChatFloatingPanel.OnSceneRelease(CP_SDK.EGenericScene.Playing, (p_LocalPosition, p_LocalRotation) =>
                {
                    CConfig.Instance.PlayingChatPosition = p_LocalPosition;
                    CConfig.Instance.PlayingChatRotation = p_LocalRotation;
                    CConfig.Instance.Save();
                });
                m_ChatFloatingPanel.OnSceneRelocated(OnFloatingWindowMoved);
                m_ChatFloatingPanel.OnGearIcon((_) =>
                {
                    var l_Items = GetSettingsViewControllers();
                    CP_SDK.UI.FlowCoordinators.MainFlowCoordinator.Instance().Present();
                    CP_SDK.UI.FlowCoordinators.MainFlowCoordinator.Instance().ChangeViewControllers(l_Items.Item1, l_Items.Item2, l_Items.Item3);
                });
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// HypeTrain
                m_ChatHypeTrainFloatingPanel     = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("ChatHypeTrainFloatingPanel", m_DockedFloatingPanelTransform.transform);
                m_ChatHypeTrainFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.HypeTrainFloatingPanelView>();
                m_ChatHypeTrainFloatingPanel.SetRadius(0);
                m_ChatHypeTrainFloatingPanel.SetBackground(false);
                m_ChatHypeTrainFloatingPanel.SetViewController(m_ChatHypeTrainFloatingPanelView);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Poll window
                m_PollFloatingPanel     = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("PollFloatingPanel", m_DockedFloatingPanelTransform.transform);
                m_PollFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.PollFloatingPanelView>();
                m_PollFloatingPanel.SetSize(UI.PollFloatingPanelView.SIZE);
                m_PollFloatingPanel.SetRadius(0);
                m_PollFloatingPanel.SetBackground(true);
                m_PollFloatingPanel.SetViewController(m_PollFloatingPanelView);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Prediction window
                m_ChatPredictionFloatingPanel       = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("PredictionFloatingPanel", m_DockedFloatingPanelTransform.transform);
                m_ChatPredictionFloatingPanelView   = CP_SDK.UI.UISystem.CreateViewController<UI.PredictionFloatingPanelView>();
                m_ChatPredictionFloatingPanel.SetSize(UI.PredictionFloatingPanelView.SIZE);
                m_ChatPredictionFloatingPanel.SetRadius(0);
                m_ChatPredictionFloatingPanel.SetBackground(true);
                m_ChatPredictionFloatingPanel.SetViewController(m_ChatPredictionFloatingPanelView);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Status
                m_StatusFloatingPanel     = CP_SDK.UI.UISystem.FloatingPanelFactory.Create("StatusFloatingPanel", m_DockedFloatingPanelTransform.transform);
                m_StatusFloatingPanelView = CP_SDK.UI.UISystem.CreateViewController<UI.StatusFloatingPanelView>();
                m_StatusFloatingPanel.SetSize(UI.StatusFloatingPanelView.SIZE);
                m_StatusFloatingPanel.SetBackground(false);
                m_StatusFloatingPanel.SetRadius(0);
                m_StatusFloatingPanel.SetViewController(m_StatusFloatingPanelView);
                ///////////////////////////////////////////////

                UpdateFloatingPanels();
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_Chat][Chat.CreateFloatingPanels] Failed to CreateFloatingPanels");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Destroy floating panels
        /// </summary>
        private void DestroyFloatingPanels()
        {
            if (m_RootTransform == null)
                return;

            try
            {
                CP_SDK.UI.UISystem.DestroyUI(ref m_ChatFloatingPanel,            ref m_ChatFloatingPanelView);
                CP_SDK.UI.UISystem.DestroyUI(ref m_ChatHypeTrainFloatingPanel,   ref m_ChatHypeTrainFloatingPanelView);
                CP_SDK.UI.UISystem.DestroyUI(ref m_PollFloatingPanel,            ref m_PollFloatingPanelView);
                CP_SDK.UI.UISystem.DestroyUI(ref m_ChatPredictionFloatingPanel,  ref m_ChatPredictionFloatingPanelView);
                CP_SDK.UI.UISystem.DestroyUI(ref m_StatusFloatingPanel,          ref m_StatusFloatingPanelView);

                GameObject.Destroy(m_RootTransform.gameObject);

                m_DockedFloatingPanelTransform      = null;
                m_RootTransform                     = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_Chat][Chat.DestroyFloatingPanels] Failed to DestroyFloatingPanels");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Update floating panels
        /// </summary>
        internal void UpdateFloatingPanels()
        {
            if (m_RootTransform == null)
                return;

            try
            {
                m_RootTransform.localPosition = Vector3.zero;
                m_RootTransform.localRotation = Quaternion.identity;

                m_DockedFloatingPanelTransform.localPosition = m_ChatFloatingPanel.RTransform.localPosition;
                m_DockedFloatingPanelTransform.localRotation = m_ChatFloatingPanel.RTransform.localRotation;

                ///////////////////////////////////////////////
                /// Chat
                m_ChatFloatingPanel.SetSize(CConfig.Instance.ChatSize);
                m_ChatFloatingPanel.SetAlignWithFloor(CConfig.Instance.AlignWithFloor);
                m_ChatFloatingPanel.SetBackgroundColor(CConfig.Instance.BackgroundColor);
                m_ChatFloatingPanel.SetSceneTransform(CP_SDK.EGenericScene.Menu,    CConfig.Instance.MenuChatPosition,      CConfig.Instance.MenuChatRotation);
                m_ChatFloatingPanel.SetSceneTransform(CP_SDK.EGenericScene.Playing, CConfig.Instance.PlayingChatPosition,   CConfig.Instance.PlayingChatRotation);

                if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.EGenericScene.Menu)
                    m_ChatFloatingPanel.SetGearIcon(CConfig.Instance.ReverseChatOrder ? CP_SDK.UI.Components.CFloatingPanel.ECorner.BottomLeft : CP_SDK.UI.Components.CFloatingPanel.ECorner.TopLeft);
                else
                    m_ChatFloatingPanel.SetGearIcon(CP_SDK.UI.Components.CFloatingPanel.ECorner.None);

                if (CConfig.Instance.ShowLockIcon)
                    m_ChatFloatingPanel.SetLockIcon(CConfig.Instance.ReverseChatOrder ? CP_SDK.UI.Components.CFloatingPanel.ECorner.BottomRight : CP_SDK.UI.Components.CFloatingPanel.ECorner.TopRight);
                else
                    m_ChatFloatingPanel.SetLockIcon(CP_SDK.UI.Components.CFloatingPanel.ECorner.None);

                /// Prepare data for level with rotations
#if BEATSABER
                var l_Is360Level    = CP_SDK_BS.Game.Logic.LevelData?.HasRotations ?? false;
                var l_RotationRef   = l_Is360Level ? Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault()?.gameObject : null as GameObject;
#elif SYNTHRIDERS
                var l_RotationRef   = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(x => x.name == "[Score & Misc]");
#elif UNITY_TESTING || AUDIOTRIP || BOOMBOX || DANCEDASH
                var l_RotationRef = null as GameObject;
#else
#error Missing game implementation
#endif
                /// Update chat messages display
                m_ChatFloatingPanelView.UpdateUI(l_RotationRef);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// HypeTrain
                var l_HypeTrainSize     = new Vector2(CConfig.Instance.ChatSize.x, UI.HypeTrainFloatingPanelView.HEIGHT);
                var l_HypeTrainPosition = new Vector3(
                    0f,
                    ((-CConfig.Instance.ChatSize.y - l_HypeTrainSize.y) / 2.0f) * 0.02f,
                    0.0f
                );
                m_ChatHypeTrainFloatingPanel.SetSize(l_HypeTrainSize);
                m_ChatHypeTrainFloatingPanel.SetTransformDirect(l_HypeTrainPosition, Vector3.zero);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Poll window
                var l_PollPosition = new Vector3(
                    (( CConfig.Instance.ChatSize.x + UI.PollFloatingPanelView.SIZE.x     ) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + UI.PollFloatingPanelView.SIZE.y + 16) / 2f) * 0.02f,
                    0
                );
                m_PollFloatingPanel.SetBackgroundColor(CConfig.Instance.BackgroundColor);
                m_PollFloatingPanel.SetTransformDirect(l_PollPosition, Vector3.zero);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Prediction window
                var l_PredictionPosition = new Vector3(
                    ((-CConfig.Instance.ChatSize.x - UI.PredictionFloatingPanelView.SIZE.x     ) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + UI.PredictionFloatingPanelView.SIZE.y + 16) / 2f) * 0.02f,
                    0
                );
                m_ChatPredictionFloatingPanel.SetBackgroundColor(CConfig.Instance.BackgroundColor);
                m_ChatPredictionFloatingPanel.SetTransformDirect(l_PredictionPosition, Vector3.zero);
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Status
                var l_StatusPosition = new Vector3(
                    (( CConfig.Instance.ChatSize.x + UI.StatusFloatingPanelView.SIZE.x) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + UI.StatusFloatingPanelView.SIZE.y) / 2f) * 0.02f,
                    0
                );
                m_StatusFloatingPanel.gameObject.SetActive(CConfig.Instance.ShowViewerCount);
                m_StatusFloatingPanel.SetTransformDirect(l_StatusPosition, Vector3.zero);
                ///////////////////////////////////////////////
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_Chat][Chat.UpdateFloatingPanels] Failed to UpdateFloatingPanels");
                Logger.Instance.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queue or send an action
        /// </summary>
        /// <param name="p_Action">Action</param>
        private void QueueOrSendChatAction(Action p_Action)
        {
            if (m_ChatFloatingPanelView == null || !m_ChatFloatingPanelView.UICreated)
                m_ActionQueue.Enqueue(p_Action);
            else
                p_Action.Invoke();
        }
        /// <summary>
        /// Dequeue actions
        /// </summary>
        /// <returns></returns>
        private async Task ActionDequeueTask()
        {
            await Task.Yield();

            while (m_ActionDequeueRun)
            {
                if (m_ChatFloatingPanelView == null || !m_ChatFloatingPanelView.UICreated || m_ActionQueue.IsEmpty)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                /// Work through the queue of messages that has piled up one by one until they're all gone.
                while (m_ActionDequeueRun && m_ActionQueue.TryDequeue(out var l_Action))
                    l_Action.Invoke();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On system message
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">Message</param>
        private void Mutiplixer_OnSystemMessage(IChatService p_ChatService, string p_Message)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnSystemMessage(p_ChatService, p_Message));
        }
        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        private void Mutiplixer_OnLogin(IChatService p_ChatService)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnLogin(p_ChatService));
        }
        /// <summary>
        /// On channel join
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void Mutiplixer_OnJoinChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnJoinChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel leave
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void Mutiplixer_OnLeaveChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnLeaveChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel follow
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        private void Mutiplixer_OnChannelFollow(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnChannelFollow(p_ChatService, p_Channel, p_User));
        }
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Used bits</param>
        private void Mutiplixer_OnChannelBits(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnChannelBits(p_ChatService, p_Channel, p_User, p_BitsUsed));
        }
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void Mutiplixer_OnChannelPoints(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnChannelPoints(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void Mutiplixer_OnChannelSubscription(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnChannelSubsciption(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void Mutiplixer_OnTextMessageReceived(IChatService p_ChatService, IChatMessage p_Message)
        {
            lock (m_LastChatUsers)
            {
                if (m_LastChatUsers.Count(x => x.Item1 == p_ChatService && x.Item2.UserName == p_Message.Sender.UserName) == 0)
                    m_LastChatUsers.Add((p_ChatService, p_Message.Sender));
            }

            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnTextMessageReceived(p_ChatService, p_Message));
        }
        /// <summary>
        /// On room state changed
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void Mutiplixer_OnRoomStateUpdated(IChatService p_ChatService, IChatChannel p_Channel)
            => UI.ModerationLeftView.Instance?.UpdateRoomState();
        /// <summary>
        /// On chat user cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_UserID">ID of the user</param>
        private void Mutiplixer_OnChatCleared(IChatService p_ChatService, string p_UserID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnChatCleared(p_UserID));
        }
        /// <summary>
        /// On message cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_MessageID">ID of the message</param>
        private void Mutiplixer_OnMessageCleared(IChatService p_ChatService, string p_MessageID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingPanelView.OnMessageCleared(p_MessageID));
        }
    }
}
