using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using CP_SDK.Unity.Extensions;
using HarmonyLib;
using IPA;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using VRUIControls;

namespace BeatSaberPlus
{
    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        internal static Hive.Versioning.Version Version     => IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlusCORE").HVersion;
        internal static string                  HarmonyID   => "com.github.hardcpp.beatsaberplus";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Harmony              m_Harmony;
        private static BasicUIAudioManager  m_BasicUIAudioManager = null;
        private static UnityEngine.Material m_UINoGlowMaterial;
        private static VRGraphicRaycaster   m_VRGraphicRaycasterCache;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private HMUI.ScreenSystem   m_HMUIScreenSystem;
        private List<GameObject>    m_HMUIDeactivatedScreens = new List<GameObject>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public Plugin(IPA.Logging.Logger p_Logger)
        {
            CP_SDK.ChatPlexSDK.Configure(
                new CP_SDK.Logging.IPALogger(p_Logger),
                "BeatSaberPlus",
                Environment.CurrentDirectory,
                CP_SDK.ChatPlexSDK.ERenderPipeline.BuiltIn
            );
            CP_SDK.ChatPlexSDK.OnAssemblyLoaded();

            CP_SDK.Chat.Service.Discrete_OnTextMessageReceived += Service_Discrete_OnTextMessageReceived;

            CP_SDK.Unity.FontManager.Setup(p_TMPFontAssetSetup: (p_Input) =>
            {
                var l_MainFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(t => t.name == "Teko-Medium SDF");
                if (l_MainFont && p_Input)
                {
                    p_Input.material.shader = l_MainFont.material.shader;
                    p_Input.material.SetColor("_FaceColor", p_Input.material.GetColor("_FaceColor"));
                    p_Input.material.EnableKeyword("CURVED");
                    p_Input.material.EnableKeyword("UNITY_UI_CLIP_RECT");
                }

                return p_Input;
            });

            PatchUI();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On BeatSaberPlus enable
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            CP_SDK.ChatPlexSDK.OnUnityReady();

            try
            {
                CP_SDK.ChatPlexSDK.Logger.Debug("Applying Harmony patches.");

                /// Setup harmony
                m_Harmony = new Harmony(HarmonyID);
                m_Harmony.PatchAll(Assembly.GetExecutingAssembly());

                CP_SDK.ChatPlexSDK.Logger.Debug("Adding menu button.");

                /// Register mod button
                MenuButtons.instance.RegisterButton(new MenuButton("BeatSaberPlus", "Feel good!", OnModButtonPressed, true));

                CP_SDK.ChatPlexSDK.Logger.Debug("Init helpers.");
                SDK.Game.BeatMapsClient.Init();
                SDK.Game.Logic.Init();

                CP_SDK.ChatPlexSDK.InitModules();
            }
            catch (Exception p_Exception)
            {
                CP_SDK.ChatPlexSDK.Logger.Error("[BeatSaberPlus][Plugin.OnEnable] Error:");
                CP_SDK.ChatPlexSDK.Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// On BeatSaberPlus disable
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {
            CP_SDK.ChatPlexSDK.StopModules();
            CP_SDK.ChatPlexSDK.OnUnityExit();
            CP_SDK.ChatPlexSDK.OnAssemblyExit();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the mod button is pressed
        /// </summary>
        private void OnModButtonPressed()
            => CP_SDK.UI.FlowCoordinators.MainFlowCoordinator.Instance().Present(true);
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Source service</param>
        /// <param name="p_Message">Message</param>
        private void Service_Discrete_OnTextMessageReceived(CP_SDK.Chat.Interfaces.IChatService p_Service, CP_SDK.Chat.Interfaces.IChatMessage p_Message)
        {
            if (p_Message.Message.Length > 2 && p_Message.Message[0] == '!')
            {
                string l_LMessage = p_Message.Message.ToLower();

                if (l_LMessage.StartsWith("!bsplusversion"))
                {
                    p_Service.SendTextMessage(p_Message.Channel,
                        $"! @{p_Message.Sender.DisplayName} The current BS+ version is "
                        + $"{Version.Major}.{Version.Minor}.{Version.Patch}!"
                        + $" The game version is "
                        + $"{Application.version}");
                }
                else if (l_LMessage.StartsWith("!bsprofile"))
                {
                    var l_Message = $"! @{p_Message.Sender.DisplayName} You can find the streamer's ";

                    if (IPA.Loader.PluginManager.GetPluginFromId("ScoreSaber") != null)
                        l_Message += $" ScoreSaber Profile at https://scoresaber.com/u/{SDK.Game.UserPlatform.GetUserID() ?? "unk"} ";

                    if (IPA.Loader.PluginManager.GetPluginFromId("BeatLeader") != null)
                        l_Message += $" BeatLeader Profile at https://www.beatleader.xyz/u/{SDK.Game.UserPlatform.GetUserID() ?? "unk"} ";

                    p_Service.SendTextMessage(p_Message.Channel, l_Message);
                }
                /*else if (l_LMessage.StartsWith("!score") && CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                {
                    var l_LevelCompletionData = SDK.Game.Logic.LevelCompletionData;
                    if (l_LevelCompletionData != null)
                    {
                        var l_MaxMultiplied     = l_LevelCompletionData.MaxMultipliedScore;
                        var l_MultipliedScore   = l_LevelCompletionData.Results.multipliedScore;
                        var l_Percentage        = SDK.Game.Levels.GetScorePercentage(l_MaxMultiplied, l_MultipliedScore);

                        p_Service.SendTextMessage(
                            p_Message.Channel,
                            $"! Acc: {(l_Percentage * 100):F2}"
                        );
                    }
                }*/
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Patch UI system
        /// </summary>
        private void PatchUI()
        {
            CP_SDK.UI.UISystem.FloatingPanelFactory = new SDK.UI.DefaultFactoriesOverrides.BS_FloatingPanelFactory();

            CP_SDK.UI.UISystem.UILayer = LayerMask.NameToLayer("UI");

            CP_SDK.UI.UISystem.Override_UnityComponent_Image            = typeof(HMUI.ImageView);
            CP_SDK.UI.UISystem.Override_UnityComponent_TextMeshProUGUI  = typeof(HMUI.CurvedTextMeshPro);

            CP_SDK.UI.UISystem.Override_GetUIMaterial = () =>
            {
                if (m_UINoGlowMaterial == null)
                {
                    m_UINoGlowMaterial = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.name == "UINoGlow").FirstOrDefault();

                    if (m_UINoGlowMaterial != null)
                        m_UINoGlowMaterial = Material.Instantiate(m_UINoGlowMaterial);
                }

                return m_UINoGlowMaterial;
            };
            CP_SDK.UI.UISystem.Override_OnClick = (p_MonoBehavior) =>
            {
                if (!m_BasicUIAudioManager || CP_SDK.ChatPlexSDK.ActiveGenericScene != CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                    m_BasicUIAudioManager = Resources.FindObjectsOfTypeAll<BasicUIAudioManager>().FirstOrDefault();

                m_BasicUIAudioManager?.HandleButtonClickEvent();
            };

            CP_SDK.UI.UISystem.OnScreenCreated = (x) =>
            {
                if (x.GetComponent<VRGraphicRaycaster>())
                    return;

                if (!m_VRGraphicRaycasterCache)
                    m_VRGraphicRaycasterCache = Resources.FindObjectsOfTypeAll<VRGraphicRaycaster>().FirstOrDefault(y => y._physicsRaycaster != null);

                if (m_VRGraphicRaycasterCache)
                    x.gameObject.AddComponent<VRGraphicRaycaster>().SetField("_physicsRaycaster", m_VRGraphicRaycasterCache._physicsRaycaster);
            };

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            CP_SDK.UI.ScreenSystem.OnCreated += () =>
            {
                var l_Instance = CP_SDK.UI.ScreenSystem.Instance;
                l_Instance.LeftScreen.SetTransformDirect(new Vector3(-2.47f, 0.00f, -1.30f), new Vector3(0.0f, -55.0f, 0.0f));
                l_Instance.LeftScreen.SetRadius(140.0f);

                l_Instance.TopScreen.SetRadius(140.0f);
                l_Instance.MainScreen.SetRadius(140.0f);

                l_Instance.RightScreen.SetTransformDirect(new Vector3(2.47f, 0.00f, -1.30f), new Vector3(0.0f, 55.0f, 0.0f));
                l_Instance.RightScreen.SetRadius(140.0f);
            };
            CP_SDK.UI.ScreenSystem.OnPresent += ScreenSystem_OnPresent;
            CP_SDK.UI.ScreenSystem.OnDismiss += ScreenSystem_OnDismiss;
        }
        /// <summary>
        /// Screen system on present
        /// </summary>
        private void ScreenSystem_OnPresent()
        {
            void DeactivateScreenSafe(HMUI.Screen p_HMUIScreen)
            {
                if (!p_HMUIScreen.gameObject.activeSelf)
                    return;

                m_HMUIDeactivatedScreens.Add(p_HMUIScreen.gameObject);
                p_HMUIScreen.gameObject.SetActive(false);
            }

            if (m_HMUIScreenSystem == null || !m_HMUIScreenSystem)
            {
                m_HMUIDeactivatedScreens.Clear();
                m_HMUIScreenSystem = Resources.FindObjectsOfTypeAll<HMUI.ScreenSystem>().FirstOrDefault();
            }

            if (!m_HMUIScreenSystem)
                return;

            m_HMUIDeactivatedScreens.Clear();
            DeactivateScreenSafe(m_HMUIScreenSystem.leftScreen);
            DeactivateScreenSafe(m_HMUIScreenSystem.mainScreen);
            DeactivateScreenSafe(m_HMUIScreenSystem.rightScreen);
            DeactivateScreenSafe(m_HMUIScreenSystem.bottomScreen);
            DeactivateScreenSafe(m_HMUIScreenSystem.topScreen);

            CP_SDK.UI.ScreenSystem.Instance.transform.localScale = m_HMUIScreenSystem.transform.localScale;
        }
        /// <summary>
        /// Screen system on dismiss
        /// </summary>
        private void ScreenSystem_OnDismiss()
        {
            for (var l_I = 0; l_I < m_HMUIDeactivatedScreens.Count; ++l_I)
                m_HMUIDeactivatedScreens[l_I].SetActive(true);

            m_HMUIDeactivatedScreens.Clear();
        }
    }
}
