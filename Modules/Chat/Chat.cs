using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberPlusChatCore.Interfaces;
using HMUI;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat
{
    /// <summary>
    /// Chat instance
    /// </summary>
    internal class Chat : SDK.ModuleBase<Chat>
    {
        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
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
        public override bool IsEnabled { get => Config.Chat.Enabled; set => Config.Chat.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

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
        private UI.FloatingWindow m_ChatFloatingScreenController = null;
        /// <summary>
        /// Mover handle material
        /// </summary>
        private Material m_ChatFloatingScreenHandleMaterial = null;
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind events
            SDK.Game.Logic.OnSceneChange += OnSceneChange;

            /// If we are already in menu scene, activate
            if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu)
                OnSceneChange(BeatSaberPlus.SDK.Game.Logic.ActiveScene);

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                SDK.Chat.Service.Acquire();

                /// Run all services
                SDK.Chat.Service.Multiplexer.OnLogin                 += ChatCoreMutiplixer_OnLogin;
                SDK.Chat.Service.Multiplexer.OnJoinChannel           += ChatCoreMutiplixer_OnJoinChannel;
                SDK.Chat.Service.Multiplexer.OnLeaveChannel          += ChatCoreMutiplixer_OnLeaveChannel;
                SDK.Chat.Service.Multiplexer.OnChannelFollow         += ChatCoreMutiplixer_OnChannelFollow;
                SDK.Chat.Service.Multiplexer.OnChannelBits           += ChatCoreMutiplixer_OnChannelBits;
                SDK.Chat.Service.Multiplexer.OnChannelPoints         += ChatCoreMutiplixer_OnChannelPoints;
                SDK.Chat.Service.Multiplexer.OnChannelSubscription   += ChatCoreMutiplixer_OnChannelSubscription;
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived   += ChatCoreMutiplixer_OnTextMessageReceived;
                SDK.Chat.Service.Multiplexer.OnChatCleared           += ChatCoreMutiplixer_OnChatCleared;
                SDK.Chat.Service.Multiplexer.OnMessageCleared        += ChatCoreMutiplixer_OnMessageCleared;

                /// Get back channels
                foreach (var l_Channel in SDK.Chat.Service.Multiplexer.Channels)
                    ChatCoreMutiplixer_OnJoinChannel(l_Channel.Item1, l_Channel.Item2);

                /// Enable dequeue system
                m_ActionDequeueRun = true;

                /// Start dequeue task
                Task.Run(ActionDequeueTask);
            }
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
                SDK.Chat.Service.Multiplexer.OnLogin               -= ChatCoreMutiplixer_OnLogin;
                SDK.Chat.Service.Multiplexer.OnJoinChannel         -= ChatCoreMutiplixer_OnJoinChannel;
                SDK.Chat.Service.Multiplexer.OnLeaveChannel        -= ChatCoreMutiplixer_OnLeaveChannel;
                SDK.Chat.Service.Multiplexer.OnChannelFollow       -= ChatCoreMutiplixer_OnChannelFollow;
                SDK.Chat.Service.Multiplexer.OnChannelBits         -= ChatCoreMutiplixer_OnChannelBits;
                SDK.Chat.Service.Multiplexer.OnChannelPoints       -= ChatCoreMutiplixer_OnChannelPoints;
                SDK.Chat.Service.Multiplexer.OnChannelSubscription -= ChatCoreMutiplixer_OnChannelSubscription;
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;
                SDK.Chat.Service.Multiplexer.OnChatCleared         -= ChatCoreMutiplixer_OnChatCleared;
                SDK.Chat.Service.Multiplexer.OnMessageCleared      -= ChatCoreMutiplixer_OnMessageCleared;

                /// Stop all chat services
                SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;

                /// Stop dequeue task
                m_ActionDequeueRun = false;
            }

            /// Unbind events
            SDK.Game.Logic.OnSceneChange -= OnSceneChange;

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
        /// When the active scene is changed
        /// </summary>
        /// <param name="p_SceneType"></param>
        private void OnSceneChange(SDK.Game.Logic.SceneType p_SceneType)
        {
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
            if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
            {
                Config.Chat.PlayingChatPositionX = m_ChatFloatingScreen.transform.position.x;
                Config.Chat.PlayingChatPositionY = m_ChatFloatingScreen.transform.position.y;
                Config.Chat.PlayingChatPositionZ = m_ChatFloatingScreen.transform.position.z;
                Config.Chat.PlayingChatRotationX = m_ChatFloatingScreen.ScreenRotation.eulerAngles.x;
                Config.Chat.PlayingChatRotationY = m_ChatFloatingScreen.ScreenRotation.eulerAngles.y;
                Config.Chat.PlayingChatRotationZ = m_ChatFloatingScreen.ScreenRotation.eulerAngles.z;
            }
            else
            {
                Config.Chat.MenuChatPositionX = m_ChatFloatingScreen.transform.position.x;
                Config.Chat.MenuChatPositionY = m_ChatFloatingScreen.transform.position.y;
                Config.Chat.MenuChatPositionZ = m_ChatFloatingScreen.transform.position.z;
                Config.Chat.MenuChatRotationX = m_ChatFloatingScreen.ScreenRotation.eulerAngles.x;
                Config.Chat.MenuChatRotationY = m_ChatFloatingScreen.ScreenRotation.eulerAngles.y;
                Config.Chat.MenuChatRotationZ = m_ChatFloatingScreen.ScreenRotation.eulerAngles.z;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
            QueueOrSendChatAction(() => m_ChatFloatingScreenController.OnTextMessageReceived(p_Message));
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
            while (m_ActionDequeueRun)
            {
                if (m_ChatFloatingScreenController == null || !m_ChatFloatingScreenController.isActivated)
                {
                    await Task.Delay(1000);
                    continue;
                }

                if (m_ActionQueue.IsEmpty)
                {
                    await Task.Delay(1000);
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
        private void CreateFloatingWindow(SDK.Game.Logic.SceneType p_SceneType)
        {
            if (m_RootGameObject != null)
                return;

            try
            {
                /// Prepare root game object
                m_RootGameObject = new GameObject("BeatSaberPlus_StreamChat");
                GameObject.DontDestroyOnLoad(m_RootGameObject);

                /// Prepare size, position, rotation
                Vector2 l_ChatSize      = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                Vector3 l_ChatPosition  = new Vector3(Config.Chat.MenuChatPositionX, Config.Chat.MenuChatPositionY, Config.Chat.MenuChatPositionZ);
                Vector3 l_ChatRotation  = new Vector3(Config.Chat.MenuChatRotationX, Config.Chat.MenuChatRotationY, Config.Chat.MenuChatRotationZ);

                if (p_SceneType == SDK.Game.Logic.SceneType.Playing)
                {
                    l_ChatPosition = new Vector3(Config.Chat.PlayingChatPositionX, Config.Chat.PlayingChatPositionY, Config.Chat.PlayingChatPositionZ);
                    l_ChatRotation = new Vector3(Config.Chat.PlayingChatRotationX, Config.Chat.PlayingChatRotationY, Config.Chat.PlayingChatRotationZ);
                }

                /// Create floating screen
                m_ChatFloatingScreen = FloatingScreen.CreateFloatingScreen(l_ChatSize, true, l_ChatPosition, Quaternion.identity);
                m_ChatFloatingScreen.GetComponent<Canvas>().sortingOrder = 3;
                m_ChatFloatingScreen.GetComponent<CurvedCanvasSettings>().SetRadius(0);

                /// Update handle material
                m_ChatFloatingScreenHandleMaterial       = GameObject.Instantiate(SDK.Unity.Material.UINoGlowMaterial);
                m_ChatFloatingScreenHandleMaterial.color = Color.clear;
                m_ChatFloatingScreen.handle.gameObject.GetComponent<Renderer>().material = m_ChatFloatingScreenHandleMaterial;

                /// Create UI Controller
                m_ChatFloatingScreenController = BeatSaberUI.CreateViewController<UI.FloatingWindow>();
                m_ChatFloatingScreen.SetRootViewController(m_ChatFloatingScreenController, HMUI.ViewController.AnimationType.None);
                m_ChatFloatingScreenController.gameObject.SetActive(true);
                m_ChatFloatingScreen.GetComponentInChildren<Canvas>().sortingOrder = 4;

                /// Bind floating window to the root game object
                m_ChatFloatingScreen.transform.SetParent(m_RootGameObject.transform);

                /// Set rotation
                m_ChatFloatingScreen.ScreenRotation = Quaternion.Euler(l_ChatRotation);

                /// Bind event
                m_ChatFloatingScreen.HandleReleased += OnFloatingWindowMoved;

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
                GameObject.Destroy(m_RootGameObject);

                /// Reset variables
                m_ChatFloatingScreenController      = null;
                m_ChatFloatingScreen                = null;
                m_ChatFloatingScreenHandleMaterial  = null;
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
        internal void UpdateFloatingWindow(SDK.Game.Logic.SceneType p_SceneType, bool p_OnSceneChange)
        {
            if (m_RootGameObject == null)
                return;

            try
            {
                Vector3 l_ChatPosition = new Vector3(Config.Chat.MenuChatPositionX, Config.Chat.MenuChatPositionY, Config.Chat.MenuChatPositionZ);
                Vector3 l_ChatRotation = new Vector3(Config.Chat.MenuChatRotationX, Config.Chat.MenuChatRotationY, Config.Chat.MenuChatRotationZ);

                if (p_SceneType == SDK.Game.Logic.SceneType.Playing)
                {
                    l_ChatPosition = new Vector3(Config.Chat.PlayingChatPositionX, Config.Chat.PlayingChatPositionY, Config.Chat.PlayingChatPositionZ);
                    l_ChatRotation = new Vector3(Config.Chat.PlayingChatRotationX, Config.Chat.PlayingChatRotationY, Config.Chat.PlayingChatRotationZ);
                }

                /// Prepare data for level with rotations
                var l_Is360Level            = BS_Utils.Plugin.LevelData?.GameplayCoreSceneSetupData?.difficultyBeatmap?.beatmapData?.spawnRotationEventsCount > 0;
                var l_FlyingGameHUDRotation = l_Is360Level ? Resources.FindObjectsOfTypeAll<FlyingGameHUDRotation>().FirstOrDefault()?.gameObject : null as GameObject;

                /// Update chat messages display
                m_ChatFloatingScreen.ScreenSize                     = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                m_ChatFloatingScreen.handle.transform.localScale    = new Vector2(Config.Chat.ChatWidth, Config.Chat.ChatHeight);
                m_ChatFloatingScreen.handle.transform.localPosition = Vector3.zero;
                m_ChatFloatingScreen.handle.transform.localRotation = Quaternion.identity;
                m_ChatFloatingScreenController.UpdateUI(p_SceneType, p_OnSceneChange, l_Is360Level, l_FlyingGameHUDRotation);

                /// Update chat position
                m_ChatFloatingScreen.transform.position = l_ChatPosition;

                /// Update rotation
                m_ChatFloatingScreen.ScreenRotation = Quaternion.Euler(l_ChatRotation);
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[Chat] Failed to UpdateFloatingWindow");
                Logger.Instance.Error(l_Exception);
            }
        }
    }
}
