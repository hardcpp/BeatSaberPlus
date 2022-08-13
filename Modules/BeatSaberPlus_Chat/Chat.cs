using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using CP_SDK.Chat.Interfaces;
using HMUI;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChatPlexMod_Chat
{
    /// <summary>
    /// Chat instance
    /// </summary>
    public class Chat : BeatSaberPlus.SDK.BSPModuleBase<Chat>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override CP_SDK.EIModuleBaseType Type => CP_SDK.EIModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Chat";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Allow people to distract you while playing!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => true;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => CConfig.Instance.Enabled; set { CConfig.Instance.Enabled = value; CConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override CP_SDK.EIModuleBaseActivationType ActivationType => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Root GameObject, kept alive between scenes transitions
        /// </summary>
        private GameObject m_RootGameObject = null;
        /// <summary>
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Settings left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Settings right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Floating screen instance
        /// </summary>
        private FloatingScreen m_ChatFloatingScreen = null;
        /// <summary>
        /// View controller for floating screen
        /// </summary>
        private UI.ChatFloatingWindow m_ChatFloatingScreenController = null;
        /// <summary>
        /// Mover handle material
        /// </summary>
        private Material m_ChatFloatingScreenHandleMaterial = null;
        /// <summary>
        /// Chat poll floating screen parent
        /// </summary>
        private GameObject m_ChatPollFloatingScreenOwner = null;
        /// <summary>
        /// Chat poll floating screen
        /// </summary>
        private FloatingScreen m_ChatPollFloatingScreen = null;
        /// <summary>
        /// Chat poll floating screen controller
        /// </summary>
        private UI.PollFloatingWindow m_ChatPollFloatingScreenController = null;
        /// <summary>
        /// Chat hype train floating screen parent
        /// </summary>
        private GameObject m_ChatHypeTrainFloatingScreenOwner = null;
        /// <summary>
        /// Chat hype train floating screen
        /// </summary>
        private FloatingScreen m_ChatHypeTrainFloatingScreen = null;
        /// <summary>
        /// Chat hype train floating screen controller
        /// </summary>
        private UI.HypeTrainFloatingWindow m_ChatHypeTrainFloatingScreenController = null;
        /// <summary>
        /// Chat prediction floating screen parent
        /// </summary>
        private GameObject m_ChatPredictionFloatingScreenOwner = null;
        /// <summary>
        /// Chat prediction floating screen
        /// </summary>
        private FloatingScreen m_ChatPredictionFloatingScreen = null;
        /// <summary>
        /// Chat prediction floating screen controller
        /// </summary>
        private UI.PredictionFloatingWindow m_ChatPredictionFloatingScreenController = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;
        /// <summary>
        /// Chat service action queue
        /// </summary>
        private ConcurrentQueue<Action> m_ActionQueue = new ConcurrentQueue<Action>();
        /// <summary>
        /// Is the action dequeue task running
        /// </summary>
        private bool m_ActionDequeueRun = true;
        /// <summary>
        /// Create button coroutine
        /// </summary>
        private Coroutine m_CreateButtonCoroutine = null;
        /// <summary>
        /// Moderation button
        /// </summary>
        private Button m_ModerationButton = null;
        /// <summary>
        /// Last chat users
        /// </summary>
        private CP_SDK.Misc.RingBuffer<(IChatService, IChatUser)> m_LastChatUsers = null;
        /// <summary>
        /// View count owner
        /// </summary>
        private GameObject m_ViewerCountOwner = null;
        /// <summary>
        /// Viewer count floating screen
        /// </summary>
        private FloatingScreen m_ViewerCountFloatingScreen = null;
        /// <summary>
        /// Viewer count image
        /// </summary>
        private Image m_ViewerCountImage = null;
        /// <summary>
        /// Viewer count text
        /// </summary>
        private TextMeshProUGUI m_ViewerCountText = null;
        /// <summary>
        /// Video playback status
        /// </summary>
        private ConcurrentDictionary<string, (bool, int)> m_ChannelsVideoPlaybackStatus = new ConcurrentDictionary<string, (bool, int)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Last chat users
        /// </summary>
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

            /// Clear video playback status
            m_ChannelsVideoPlaybackStatus.Clear();

            /// Bind events
            CP_SDK.ChatPlexSDK.OnGenericMenuSceneLoaded   += ChatPlexUnitySDK_OnGenericMenuSceneLoaded;
            CP_SDK.ChatPlexSDK.OnGenericSceneChange       += ChatPlexUnitySDK_OnGenericSceneChange;

            /// If we are already in menu scene, activate
            if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                ChatPlexUnitySDK_OnGenericSceneChange(CP_SDK.ChatPlexSDK.ActiveGenericScene);

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                CP_SDK.Chat.Service.Acquire();

                /// Run all services
                CP_SDK.Chat.Service.Multiplexer.OnSystemMessage            += ChatCoreMutiplixer_OnSystemMessage;
                CP_SDK.Chat.Service.Multiplexer.OnLogin                    += ChatCoreMutiplixer_OnLogin;
                CP_SDK.Chat.Service.Multiplexer.OnJoinChannel              += ChatCoreMutiplixer_OnJoinChannel;
                CP_SDK.Chat.Service.Multiplexer.OnLeaveChannel             += ChatCoreMutiplixer_OnLeaveChannel;
                CP_SDK.Chat.Service.Multiplexer.OnChannelFollow            += ChatCoreMutiplixer_OnChannelFollow;
                CP_SDK.Chat.Service.Multiplexer.OnChannelBits              += ChatCoreMutiplixer_OnChannelBits;
                CP_SDK.Chat.Service.Multiplexer.OnChannelPoints            += ChatCoreMutiplixer_OnChannelPoints;
                CP_SDK.Chat.Service.Multiplexer.OnChannelSubscription      += ChatCoreMutiplixer_OnChannelSubscription;
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived      += ChatCoreMutiplixer_OnTextMessageReceived;
                CP_SDK.Chat.Service.Multiplexer.OnRoomStateUpdated         += ChatCoreMutiplixer_OnRoomStateUpdated;
                CP_SDK.Chat.Service.Multiplexer.OnRoomVideoPlaybackUpdated += ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated;
                CP_SDK.Chat.Service.Multiplexer.OnChatCleared              += ChatCoreMutiplixer_OnChatCleared;
                CP_SDK.Chat.Service.Multiplexer.OnMessageCleared           += ChatCoreMutiplixer_OnMessageCleared;

                /// Get back channels
                foreach (var l_Channel in CP_SDK.Chat.Service.Multiplexer.Channels)
                    ChatCoreMutiplixer_OnJoinChannel(l_Channel.Item1, l_Channel.Item2);

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
                CP_SDK.Chat.Service.Multiplexer.OnSystemMessage            -= ChatCoreMutiplixer_OnSystemMessage;
                CP_SDK.Chat.Service.Multiplexer.OnLogin                    -= ChatCoreMutiplixer_OnLogin;
                CP_SDK.Chat.Service.Multiplexer.OnJoinChannel              -= ChatCoreMutiplixer_OnJoinChannel;
                CP_SDK.Chat.Service.Multiplexer.OnLeaveChannel             -= ChatCoreMutiplixer_OnLeaveChannel;
                CP_SDK.Chat.Service.Multiplexer.OnChannelFollow            -= ChatCoreMutiplixer_OnChannelFollow;
                CP_SDK.Chat.Service.Multiplexer.OnChannelBits              -= ChatCoreMutiplixer_OnChannelBits;
                CP_SDK.Chat.Service.Multiplexer.OnChannelPoints            -= ChatCoreMutiplixer_OnChannelPoints;
                CP_SDK.Chat.Service.Multiplexer.OnChannelSubscription      -= ChatCoreMutiplixer_OnChannelSubscription;
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived      -= ChatCoreMutiplixer_OnTextMessageReceived;
                CP_SDK.Chat.Service.Multiplexer.OnRoomStateUpdated         -= ChatCoreMutiplixer_OnRoomStateUpdated;
                CP_SDK.Chat.Service.Multiplexer.OnRoomVideoPlaybackUpdated -= ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated;
                CP_SDK.Chat.Service.Multiplexer.OnChatCleared              -= ChatCoreMutiplixer_OnChatCleared;
                CP_SDK.Chat.Service.Multiplexer.OnMessageCleared           -= ChatCoreMutiplixer_OnMessageCleared;

                /// Stop all chat services
                CP_SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;

                /// Stop dequeue task
                m_ActionDequeueRun = false;
            }

            /// Unbind events
            CP_SDK.ChatPlexSDK.OnGenericSceneChange       -= ChatPlexUnitySDK_OnGenericSceneChange;
            CP_SDK.ChatPlexSDK.OnGenericMenuSceneLoaded   -= ChatPlexUnitySDK_OnGenericMenuSceneLoaded;

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
            DestroyFloatingWindow();
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
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the menu loaded
        /// </summary>
        private void ChatPlexUnitySDK_OnGenericMenuSceneLoaded()
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
        private void ChatPlexUnitySDK_OnGenericSceneChange(CP_SDK.ChatPlexSDK.EGenericScene p_SceneType)
        {
            if (m_RootGameObject)
                m_RootGameObject.transform.localScale = Vector3.one;

            if (p_SceneType == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                UpdateButton();

            if (m_ChatFloatingScreen == null)
                CreateFloatingWindow(p_SceneType);
            else
                UpdateFloatingWindow(p_SceneType, true);
        }
        /// <summary>
        /// When the floating window is moved
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void OnFloatingWindowMoved(object p_Sender, FloatingScreenHandleEventArgs p_Event)
        {
            /// Always parallel to the floor
            if (CConfig.Instance.AlignWithFloor)
                m_ChatFloatingScreen.transform.localEulerAngles = new Vector3(m_ChatFloatingScreen.transform.localEulerAngles.x, m_ChatFloatingScreen.transform.localEulerAngles.y, 0);

            if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
            {
                CConfig.Instance.PlayingChatPosition = m_ChatFloatingScreen.transform.localPosition;
                CConfig.Instance.PlayingChatRotation = m_ChatFloatingScreen.transform.localEulerAngles;
            }
            else
            {
                CConfig.Instance.MenuChatPosition = m_ChatFloatingScreen.transform.localPosition;
                CConfig.Instance.MenuChatRotation = m_ChatFloatingScreen.transform.localEulerAngles;
            }

            CConfig.Instance.Save();

            m_ViewerCountOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
            m_ViewerCountOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;

            m_ChatPollFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
            m_ChatPollFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;

            m_ChatHypeTrainFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
            m_ChatHypeTrainFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;

            m_ChatPredictionFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
            m_ChatPredictionFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;
        }
        /// <summary>
        /// Toggle chat visibility
        /// </summary>
        public void ToggleVisibility()
        {
            if (m_RootGameObject && m_RootGameObject.transform.localScale.x > 0.5f)
                m_RootGameObject.transform.localScale = Vector3.zero;
            else if (m_RootGameObject)
                m_RootGameObject.transform.localScale = Vector3.one;
        }
        /// <summary>
        /// Set visible
        /// </summary>
        /// <param name="p_Visible">Is visible</param>
        public void SetVisible(bool p_Visible)
        {
            if (m_RootGameObject)
                m_RootGameObject.transform.localScale = p_Visible ? Vector3.one : Vector3.zero;
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

            m_ModerationButton = BeatSaberPlus.SDK.UI.Button.Create(p_LevelSelectionNavigationController.transform, "Chat\nModeration", () => UI.ModerationViewFlowCoordinator.Instance().Present(), null);
            m_ModerationButton.transform.localPosition      = new Vector3(72.50f, 30.00f - 3, 2.6f);
            m_ModerationButton.transform.localScale         = new Vector3(0.65f, 0.50f, 0.65f);
            m_ModerationButton.transform.SetAsFirstSibling();
            m_ModerationButton.gameObject.SetActive(true);
            m_ModerationButton.GetComponentInChildren<TextMeshProUGUI>().margin = new Vector4(0, 4, 0, 0);

            UpdateButton();

            m_CreateButtonCoroutine = null;
        }
        /// <summary>
        /// Update button text
        /// </summary>
        internal void UpdateButton()
        {
            if (m_ModerationButton == null)
                return;

            m_ModerationButton.transform.localPosition  = new Vector3(72.50f, 30.00f - 3, 2.6f);
            m_ModerationButton.transform.localScale     = new Vector3(0.65f, 0.50f, 0.65f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On system message
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">Message</param>
        private void ChatCoreMutiplixer_OnSystemMessage(IChatService p_ChatService, string p_Message)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnSystemMessage(p_ChatService, p_Message));
        }
        /// <summary>
        /// On login
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        private void ChatCoreMutiplixer_OnLogin(IChatService p_ChatService)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnLogin(p_ChatService));
        }
        /// <summary>
        /// On channel join
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnJoinChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnJoinChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel leave
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnLeaveChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnLeaveChannel(p_ChatService, p_Channel));
        }
        /// <summary>
        /// On channel follow
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        private void ChatCoreMutiplixer_OnChannelFollow(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelFollow(p_ChatService, p_Channel, p_User));
        }
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Used bits</param>
        private void ChatCoreMutiplixer_OnChannelBits(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelBits(p_ChatService, p_Channel, p_User, p_BitsUsed));
        }
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelPoints(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelPoints(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelSubscription(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChannelSubsciption(p_ChatService, p_Channel, p_User, p_Event));
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_ChatService, IChatMessage p_Message)
        {
            lock (m_LastChatUsers)
            {
                if (m_LastChatUsers.Count(x => x.Item1 == p_ChatService && x.Item2.UserName == p_Message.Sender.UserName) == 0)
                    m_LastChatUsers.Add((p_ChatService, p_Message.Sender));
            }

            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnTextMessageReceived(p_Message));
        }
        /// <summary>
        /// On room state changed
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnRoomStateUpdated(IChatService p_ChatService, IChatChannel p_Channel)
        {
            if (UI.ModerationLeft.Instance != null)
                UI.ModerationLeft.Instance.UpdateRoomState();

        }
        /// <summary>
        /// On room video playback updated
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_StreamUP">Is the stream up</param>
        /// <param name="p_ViewerCount">Viewer count</param>
        private void ChatCoreMutiplixer_OnRoomVideoPlaybackUpdated(IChatService p_ChatService, IChatChannel p_Channel, bool p_StreamUP, int p_ViewerCount)
        {
            string l_Key = "[" + p_ChatService.DisplayName + "]_" + p_Channel.Name.ToLower();

            if (!m_ChannelsVideoPlaybackStatus.ContainsKey(l_Key))
                m_ChannelsVideoPlaybackStatus.TryAdd(l_Key, (p_StreamUP, p_ViewerCount));
            else
                m_ChannelsVideoPlaybackStatus[l_Key] = (p_StreamUP, p_ViewerCount);

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => UpdateViewerCount());
        }
        /// <summary>
        /// On chat user cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_UserID">ID of the user</param>
        private void ChatCoreMutiplixer_OnChatCleared(IChatService p_ChatService, string p_UserID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnChatCleared(p_UserID));
        }
        /// <summary>
        /// On message cleared
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_MessageID">ID of the message</param>
        private void ChatCoreMutiplixer_OnMessageCleared(IChatService p_ChatService, string p_MessageID)
        {
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnMessageCleared(p_MessageID));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queue or send an action
        /// </summary>
        /// <param name="p_Action">Action</param>
        private void QueueOrSendChatAction(Action p_Action)
        {
            if (m_ChatFloatingScreenController == null || !m_ChatFloatingScreenController.isActivated)
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
                if (m_ChatFloatingScreenController == null || !m_ChatFloatingScreenController.isActivated)
                {
                    await Task.Delay(1000).ConfigureAwait(false);
                    continue;
                }

                if (m_ActionQueue.IsEmpty)
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
        /// Create the floating window
        /// </summary>
        /// <param name="p_SceneType">Scene type</param>
        private void CreateFloatingWindow(CP_SDK.ChatPlexSDK.EGenericScene p_SceneType)
        {
            if (m_RootGameObject != null)
                return;

            try
            {
                /// Prepare root game object
                m_RootGameObject = new GameObject("BeatSaberPlus_StreamChat");
                GameObject.DontDestroyOnLoad(m_RootGameObject);

                /// Prepare size, position, rotation
                Vector2 l_ChatSize      = CConfig.Instance.ChatSize;
                Vector3 l_ChatPosition  = CConfig.Instance.MenuChatPosition;
                Vector3 l_ChatRotation  = CConfig.Instance.MenuChatRotation;

                if (p_SceneType == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                {
                    l_ChatPosition = CConfig.Instance.PlayingChatPosition;
                    l_ChatRotation = CConfig.Instance.PlayingChatRotation;
                }

                /// Create floating screen
                m_ChatFloatingScreen = FloatingScreen.CreateFloatingScreen(l_ChatSize, true, Vector3.zero, Quaternion.identity);
                m_ChatFloatingScreen.GetComponent<Canvas>().sortingOrder = 3;
                m_ChatFloatingScreen.GetComponent<CurvedCanvasSettings>().SetRadius(0);

                /// Update handle material
                m_ChatFloatingScreenHandleMaterial       = GameObject.Instantiate(BeatSaberPlus.SDK.Unity.MaterialU.UINoGlowMaterial);
                m_ChatFloatingScreenHandleMaterial.color = Color.clear;
                m_ChatFloatingScreen.handle.gameObject.GetComponent<Renderer>().material = m_ChatFloatingScreenHandleMaterial;

                /// Create UI Controller
                m_ChatFloatingScreenController = BeatSaberUI.CreateViewController<UI.ChatFloatingWindow>();
                m_ChatFloatingScreen.SetRootViewController(m_ChatFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatFloatingScreenController.gameObject.SetActive(true);
                m_ChatFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = 4;

                /// Bind floating window to the root game object
                m_ChatFloatingScreen.transform.SetParent(m_RootGameObject.transform);

                /// Set position & rotation
                m_ChatFloatingScreen.transform.localPosition = l_ChatPosition;
                m_ChatFloatingScreen.transform.localRotation = Quaternion.Euler(l_ChatRotation);

                /// Bind event
                m_ChatFloatingScreen.HandleReleased += OnFloatingWindowMoved;

                /// Create viewer count owner
                m_ViewerCountOwner = new GameObject();
                m_ViewerCountOwner.transform.localPosition = l_ChatPosition;
                m_ViewerCountOwner.transform.localRotation = Quaternion.Euler(l_ChatRotation);
                m_ViewerCountOwner.transform.SetParent(m_RootGameObject.transform);

                /// Viewer count window
                m_ViewerCountFloatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(25, 8), false, Vector2.zero, Quaternion.identity, 0, false);
                m_ViewerCountFloatingScreen.transform.localPosition = new Vector3(  ((((float)CConfig.Instance.ChatSize.x) / 2f) * 0.02f) + 0.2f, (((-(float)CConfig.Instance.ChatSize.y) / 2f) * 0.02f) + 0.1f, 0f);
                m_ViewerCountFloatingScreen.transform.localRotation = Quaternion.identity;

                /// Horizontal layout
                var l_HorizontalTag = new BeatSaberMarkupLanguage.Tags.HorizontalLayoutTag();
                var l_Layout = l_HorizontalTag.CreateObject(m_ViewerCountFloatingScreen.transform);

                /// Viewer icon
                var l_ImageTag = new BeatSaberMarkupLanguage.Tags.ImageTag();
                var l_Image    = l_ImageTag.CreateObject(l_Layout.transform);
                l_Image.transform.localScale = Vector3.one * 0.5f;
                m_ViewerCountImage = l_Image.GetComponent<Image>();
                BeatSaberUI.SetImage(m_ViewerCountImage, "#PlayerIcon");

                /// Viewer text
                var l_TextTag = new BeatSaberMarkupLanguage.Tags.TextTag();
                var l_Text    = l_TextTag.CreateObject(l_Layout.transform);
                l_Text.GetComponent<TextMeshProUGUI>().margin   = new Vector4(-3, 3, 0, 0);
                l_Text.GetComponent<TextMeshProUGUI>().fontSize = 5;
                l_Text.GetComponent<TextMeshProUGUI>().text     = "0";

                m_ViewerCountText = l_Text.GetComponent<TextMeshProUGUI>();

                /// Bind floating window to the root game object
                m_ViewerCountFloatingScreen.transform.SetParent(m_ViewerCountOwner.transform, false);

                CP_SDK.Unity.GameObjectU.ChangerLayerRecursive(m_ViewerCountFloatingScreen.gameObject, LayerMask.NameToLayer("UI"));

                UpdateViewerCount();

                ///////////////////////////////////////////////
                /// Poll window
                var l_PollSize = UI.PollFloatingWindow.SIZE;
                var l_PollPosition = new Vector3(
                    ((CConfig.Instance.ChatSize.x + l_PollSize.x) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + l_PollSize.y + 16) / 2f) * 0.02f,
                    0
                    );

                /// Create viewer count owner
                m_ChatPollFloatingScreenOwner = new GameObject();
                m_ChatPollFloatingScreenOwner.transform.localPosition = l_ChatPosition;
                m_ChatPollFloatingScreenOwner.transform.localRotation = Quaternion.Euler(l_ChatRotation);
                m_ChatPollFloatingScreenOwner.transform.SetParent(m_RootGameObject.transform);

                /// Create floating screen
                m_ChatPollFloatingScreen = FloatingScreen.CreateFloatingScreen(l_PollSize, false, Vector2.zero, Quaternion.identity, 0, false);
                m_ChatPollFloatingScreen.transform.SetParent(m_ChatPollFloatingScreenOwner.transform, false);
                m_ChatPollFloatingScreen.transform.localPosition = l_PollPosition;
                m_ChatPollFloatingScreen.transform.localRotation = Quaternion.identity;

                /// Create UI Controller
                m_ChatPollFloatingScreenController = BeatSaberUI.CreateViewController<UI.PollFloatingWindow>();
                m_ChatPollFloatingScreen.SetRootViewController(m_ChatPollFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatPollFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = -1;
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// HypeTrain window
                var l_HypeTrainSize = new Vector2(CConfig.Instance.ChatSize.x, UI.HypeTrainFloatingWindow.HEIGHT);
                var l_HypeTrainPosition = new Vector3(
                    0f,
                    ((-CConfig.Instance.ChatSize.y - l_HypeTrainSize.y) / 2f) * 0.02f,
                    0f
                    );

                /// Create viewer count owner
                m_ChatHypeTrainFloatingScreenOwner = new GameObject();
                m_ChatHypeTrainFloatingScreenOwner.transform.localPosition = l_ChatPosition;
                m_ChatHypeTrainFloatingScreenOwner.transform.localRotation = Quaternion.Euler(l_ChatRotation);
                m_ChatHypeTrainFloatingScreenOwner.transform.SetParent(m_RootGameObject.transform);

                /// Create floating screen
                m_ChatHypeTrainFloatingScreen = FloatingScreen.CreateFloatingScreen(l_PollSize, false, Vector2.zero, Quaternion.identity, 0, false);
                m_ChatHypeTrainFloatingScreen.transform.SetParent(m_ChatHypeTrainFloatingScreenOwner.transform, false);
                m_ChatHypeTrainFloatingScreen.transform.localPosition = l_PollPosition;
                m_ChatHypeTrainFloatingScreen.transform.localRotation = Quaternion.identity;

                /// Create UI Controller
                m_ChatHypeTrainFloatingScreenController = BeatSaberUI.CreateViewController<UI.HypeTrainFloatingWindow>();
                m_ChatHypeTrainFloatingScreen.SetRootViewController(m_ChatHypeTrainFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatHypeTrainFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = -1;
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Prediction window
                var l_PredictionSize = UI.PredictionFloatingWindow.SIZE;
                var l_PredictionPosition = new Vector3(
                    ((-CConfig.Instance.ChatSize.x - l_PredictionSize.x) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + l_PredictionSize.y + 16) / 2f) * 0.02f,
                    0
                    );

                /// Create viewer count owner
                m_ChatPredictionFloatingScreenOwner = new GameObject();
                m_ChatPredictionFloatingScreenOwner.transform.localPosition = l_ChatPosition;
                m_ChatPredictionFloatingScreenOwner.transform.localRotation = Quaternion.Euler(l_ChatRotation);
                m_ChatPredictionFloatingScreenOwner.transform.SetParent(m_RootGameObject.transform);

                /// Create floating screen
                m_ChatPredictionFloatingScreen = FloatingScreen.CreateFloatingScreen(l_PredictionSize, false, Vector2.zero, Quaternion.identity, 0, false);
                m_ChatPredictionFloatingScreen.transform.SetParent(m_ChatPredictionFloatingScreenOwner.transform, false);
                m_ChatPredictionFloatingScreen.transform.localPosition = l_PredictionPosition;
                m_ChatPredictionFloatingScreen.transform.localRotation = Quaternion.identity;

                /// Create UI Controller
                m_ChatPredictionFloatingScreenController = BeatSaberUI.CreateViewController<UI.PredictionFloatingWindow>();
                m_ChatPredictionFloatingScreen.SetRootViewController(m_ChatPredictionFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatPredictionFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = -1;
                ///////////////////////////////////////////////

                UpdateFloatingWindow(p_SceneType, true);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to CreateFloatingWindow");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Destroy the floating window
        /// </summary>
        private void DestroyFloatingWindow()
        {
            if (m_RootGameObject == null)
                return;

            try
            {
                /// Dismiss controller
                m_ChatFloatingScreen.SetRootViewController(null, HMUI.ViewController.AnimationType.None);

                /// Destroy objects
                GameObject.Destroy(m_ChatFloatingScreenController);
                GameObject.Destroy(m_ChatFloatingScreen);
                GameObject.Destroy(m_ChatFloatingScreenHandleMaterial);
                GameObject.Destroy(m_ViewerCountFloatingScreen);
                GameObject.Destroy(m_ChatPollFloatingScreen);
                GameObject.Destroy(m_ChatHypeTrainFloatingScreen);
                GameObject.Destroy(m_ChatPredictionFloatingScreen);
                GameObject.Destroy(m_RootGameObject);

                /// Reset variables
                m_ChatFloatingScreenController      = null;
                m_ChatFloatingScreen                = null;
                m_ChatFloatingScreenHandleMaterial  = null;
                m_ViewerCountFloatingScreen         = null;
                m_ViewerCountText                   = null;
                m_ChatHypeTrainFloatingScreen       = null;
                m_ChatPollFloatingScreen            = null;
                m_ChatPredictionFloatingScreen      = null;
                m_RootGameObject                    = null;
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to DestroyFloatingPlayer");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Update floating window UI on scene change
        /// </summary>
        /// <param name="p_SceneType">New scene</param>
        /// <param name="p_OnSceneChange">Is on scene change</param>
        internal void UpdateFloatingWindow(CP_SDK.ChatPlexSDK.EGenericScene p_SceneType, bool p_OnSceneChange)
        {
            if (m_RootGameObject == null)
                return;

            try
            {
                Vector3 l_ChatPosition = CConfig.Instance.MenuChatPosition;
                Vector3 l_ChatRotation = CConfig.Instance.MenuChatRotation;

                if (p_SceneType == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                {
                    l_ChatPosition = CConfig.Instance.PlayingChatPosition;
                    l_ChatRotation = CConfig.Instance.PlayingChatRotation;
                }

                /// Prepare data for level with rotations
                var l_Is360Level            = BeatSaberPlus.SDK.Game.Logic.LevelData?.Data?.transformedBeatmapData?.spawnRotationEventsCount > 0;
                var l_FlyingGameHUDRotation = l_Is360Level ? Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault()?.gameObject : null as GameObject;

                /// Update chat messages display
                m_ChatFloatingScreen.ScreenSize                     = CConfig.Instance.ChatSize;
                m_ChatFloatingScreen.handle.transform.localScale    = CConfig.Instance.ChatSize;
                m_ChatFloatingScreen.handle.transform.localPosition = Vector3.zero;
                m_ChatFloatingScreen.handle.transform.localRotation = Quaternion.identity;
                m_ChatFloatingScreenController.UpdateUI(p_SceneType, p_OnSceneChange, l_Is360Level, l_FlyingGameHUDRotation);

                /// Update position & rotation
                m_ChatFloatingScreen.transform.localPosition = l_ChatPosition;
                m_ChatFloatingScreen.transform.localRotation = Quaternion.Euler(l_ChatRotation);

                /// Update viewer count
                m_ViewerCountFloatingScreen.transform.localPosition = new Vector3(((((float)CConfig.Instance.ChatSize.x) / 2f) * 0.02f) + 0.2f, (((-(float)CConfig.Instance.ChatSize.y) / 2f) * 0.02f) + 0.1f, 0f);
                m_ViewerCountOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
                m_ViewerCountOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;

                /// Update visibility
                m_ViewerCountImage.enabled  = CConfig.Instance.ShowViewerCount;
                m_ViewerCountText.enabled   = CConfig.Instance.ShowViewerCount;

                ///////////////////////////////////////////////
                /// Poll window
                var l_PollSize      = UI.PollFloatingWindow.SIZE;
                var l_PollPosition  = new Vector3(
                    ((CConfig.Instance.ChatSize.x + l_PollSize.x) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + l_PollSize.y + 16) / 2f) * 0.02f,
                    0
                    );
                m_ChatPollFloatingScreen.transform.localPosition = l_PollPosition;
                m_ChatPollFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
                m_ChatPollFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// HypeTrain window
                var l_HypeTrainSize     = new Vector2(CConfig.Instance.ChatSize.x, UI.HypeTrainFloatingWindow.HEIGHT);
                var l_HypeTrainPosition = new Vector3(
                    0f,
                    ((-CConfig.Instance.ChatSize.y - l_HypeTrainSize.y) / 2f) * 0.02f,
                    0f
                    );
                m_ChatHypeTrainFloatingScreen.ScreenSize = l_HypeTrainSize;
                m_ChatHypeTrainFloatingScreen.transform.localPosition = l_HypeTrainPosition;
                m_ChatHypeTrainFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
                m_ChatHypeTrainFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;
                ///////////////////////////////////////////////

                ///////////////////////////////////////////////
                /// Prediction window
                var l_PredictionSize = UI.PredictionFloatingWindow.SIZE;
                var l_PredictionPosition = new Vector3(
                    ((-CConfig.Instance.ChatSize.x - l_PredictionSize.x) / 2f) * 0.02f,
                    ((-CConfig.Instance.ChatSize.y + l_PredictionSize.y + 16) / 2f) * 0.02f,
                    0
                    );
                m_ChatPredictionFloatingScreen.transform.localPosition = l_PredictionPosition;
                m_ChatPredictionFloatingScreenOwner.transform.localPosition = m_ChatFloatingScreen.transform.localPosition;
                m_ChatPredictionFloatingScreenOwner.transform.localRotation = m_ChatFloatingScreen.transform.localRotation;
                ///////////////////////////////////////////////
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to UpdateFloatingWindow");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Update viewer count
        /// </summary>
        private void UpdateViewerCount()
        {
            bool l_ShowUp = false;
            int l_SumViewers = 0;

            foreach (var l_KVP in m_ChannelsVideoPlaybackStatus)
            {
                if (!l_KVP.Value.Item1)
                    continue;

                l_ShowUp        = true;
                l_SumViewers   += l_KVP.Value.Item2;
            }

            if (l_ShowUp)
                m_ViewerCountText.text = l_SumViewers.ToString();
            else
                m_ViewerCountText.text = "<color=red>Offline";
        }
    }
}
