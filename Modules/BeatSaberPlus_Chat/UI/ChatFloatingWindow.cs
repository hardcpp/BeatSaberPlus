using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.FloatingScreen;
using HMUI;
using System.Linq;
using UnityEngine;
using VRUIControls;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Floating window content
    /// </summary>
    internal partial class ChatFloatingWindow : BeatSaberPlus.SDK.UI.ResourceViewController<ChatFloatingWindow>
    {
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m__AllowMovement = false;
        /// <summary>
        /// Is movement allowed
        /// </summary>
        private bool m_AllowMovement
        {
            get => m__AllowMovement;
            set {
                m__AllowMovement = value;
                ColorUtility.TryParseHtmlString(value ? "#FFFFFFFF" : "#FFFFFF80", out var l_ColH);
                ColorUtility.TryParseHtmlString(value ? "#FFFFFF80" : "#FFFFFFFF", out var l_ColD);
                m_LockIcon.HighlightColor   = l_ColH;
                m_LockIcon.DefaultColor     = l_ColD;

                var l_FloatingScreen = transform.parent.GetComponent<FloatingScreen>();
                l_FloatingScreen.ShowHandle = value;

                if (value)
                {
                    /// Refresh VR pointer due to bug
                    var l_Pointers = Resources.FindObjectsOfTypeAll<VRPointer>();
                    var l_Pointer  = CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing ? l_Pointers.LastOrDefault() : l_Pointers.FirstOrDefault();

                    if (l_Pointer != null)
                    {
                        if (l_FloatingScreen.screenMover)
                            Destroy(l_FloatingScreen.screenMover);

                        l_FloatingScreen.screenMover = l_Pointer.gameObject.AddComponent<FloatingScreenMoverPointer>();
                        l_FloatingScreen.screenMover.Init(l_FloatingScreen);
                    }
                    else
                    {
                        Logger.Instance.Warning("Failed to get VRPointer!");
                    }
                }
            }
        }
        /// <summary>
        /// Is a 360 level
        /// </summary>
        private bool m_IsRotatingLevel;
        /// <summary>
        /// FlyingGameHUDRotation instance
        /// </summary>
        private GameObject m_EnvironmentRotationRef;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private Vector2 m_ChatSize;
        private bool m_ReverseChatOrder;
        private float m_FontSize;
        private Color m_HighlightColor;
        private Color m_AccentColor;
        private Color m_TextColor;
        private Color m_PingColor;
        private bool m_FilterViewersCommands;
        private bool m_FilterBroadcasterCommands;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Lock icon
        /// </summary>
        [UIComponent("SettingsIcon")]
        private ClickableImage m_SettingsIcon = null;
        /// <summary>
        /// Lock icon
        /// </summary>
        [UIComponent("LockIcon")]
        private ClickableImage m_LockIcon = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            /// Update background color
            GetComponentInChildren<ImageView>().color = CConfig.Instance.BackgroundColor;

            /// Update message position origin
            (transform.GetChild(0).transform as RectTransform).pivot = new Vector2(0.5f, 0f);

            InitLogic();

            /// Update lock state
            m_AllowMovement = false;

            /// Hide/show the lock icon
            m_LockIcon.gameObject.SetActive(CConfig.Instance.ShowLockIcon);

            CP_SDK.Unity.GameObjectU.ChangerLayerRecursive(gameObject, LayerMask.NameToLayer("UI"));

            /// Make icons easier to click
            m_SettingsIcon.gameObject.AddComponent<SphereCollider>().radius = 10f;
            m_LockIcon.gameObject.AddComponent<SphereCollider>().radius = 10f;
        }
        /// <summary>
        /// On view destruction
        /// </summary>
        protected override sealed void OnViewDestruction()
        {
            DestroyLogic();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update UI
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        /// <param name="p_OnSceneChange">Is the scene changed ?</param>
        /// <param name="p_IsRotatingLevel">Is a 360 level</param>
        /// <param name="p_EnvironmentRotationRef">Flying hame HUD rotation</param>
        internal void UpdateUI(CP_SDK.ChatPlexSDK.EGenericScene p_Scene, bool p_OnSceneChange, bool p_IsRotatingLevel, GameObject p_EnvironmentRotationRef)
        {
            /// Disable settings in play mode
            m_SettingsIcon.gameObject.SetActive(p_Scene != CP_SDK.ChatPlexSDK.EGenericScene.Playing);

            /// On scene change, lock movement
            if (p_OnSceneChange)
                m_AllowMovement = false;

            /// Update background color
            GetComponentInChildren<ImageView>().color = CConfig.Instance.BackgroundColor;

            m_ChatSize          = CConfig.Instance.ChatSize;
            m_ReverseChatOrder  = CConfig.Instance.ReverseChatOrder;
            m_FontSize          = CConfig.Instance.FontSize;
            m_HighlightColor    = CConfig.Instance.HighlightColor;
            m_AccentColor       = CConfig.Instance.AccentColor;
            m_TextColor         = CConfig.Instance.TextColor;
            m_PingColor         = CConfig.Instance.PingColor;

            m_FilterViewersCommands     = CConfig.Instance.FilterViewersCommands;
            m_FilterBroadcasterCommands = CConfig.Instance.FilterBroadcasterCommands;

            m_IsRotatingLevel           = p_IsRotatingLevel;
            m_EnvironmentRotationRef    = p_EnvironmentRotationRef;

            if (!m_IsRotatingLevel)
                transform.parent.parent.rotation = Quaternion.identity;

            UpdateMessagesStyleFull();

            /// Hide/show the lock icon
            m_LockIcon.gameObject.SetActive(CConfig.Instance.ShowLockIcon);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIAction("settings-pressed")]
        internal void OnSettingsPressed()
        {
            var l_Items = Chat.Instance.GetSettingsUI();
            BeatSaberPlus.UI.MainViewFlowCoordinator.Instance().ChangeView(l_Items.Item1, l_Items.Item2, l_Items.Item3);
        }
        [UIAction("lock-pressed")]
        internal void OnLockPressed()
        {
            m_AllowMovement = !m_AllowMovement;
        }
    }
}
