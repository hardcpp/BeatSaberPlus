using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using IPA;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus
{
    /*[HarmonyPatch(typeof(BeatmapObjectsInstaller))]
    [HarmonyPatch(nameof(BeatmapObjectsInstaller.InstallBindings))]
    internal class PBeatmapObjectsInstaller
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var l_Current in instructions)
            {
                if (l_Current.opcode == OpCodes.Ldc_I4_S && l_Current.operand is sbyte && (sbyte)l_Current.operand == (sbyte)25)
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 300);
                else
                    yield return l_Current;
            }
        }
    }*/

    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        /// <summary>
        /// Plugin version
        /// </summary>
        internal static Hive.Versioning.Version Version => IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlusCORE").HVersion;
        /// <summary>
        /// Harmony ID for patches
        /// </summary>
        internal static string HarmonyID => "com.github.hardcpp.beatsaberplus";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Harmony patch holder
        /// </summary>
        private static Harmony m_Harmony;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        [Init]
        public Plugin(IPA.Logging.Logger p_Logger)
        {
            CP_SDK.ChatPlexSDK.Configure(new CP_SDK.Logging.IPALogger(p_Logger), "BeatSaberPlus");
            CP_SDK.ChatPlexSDK.OnAssemblyLoaded();

            CP_SDK.Chat.Service.Discrete_OnTextMessageReceived += Service_Discrete_OnTextMessageReceived;

            CP_SDK.Unity.FontManager.Setup(p_FontClone: (p_Input) =>
            {
                var l_MainFont  = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(t => t.name == "Teko-Medium SDF");
                var l_NewFont   = UnityEngine.Object.Instantiate(p_Input);

                if (l_MainFont)
                    l_NewFont.material.shader = l_MainFont.material.shader;

                l_NewFont.material.EnableKeyword("CURVED");
                l_NewFont.material.EnableKeyword("UNITY_UI_CLIP_RECT");

                return l_NewFont;
            });
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
            => UI.MainViewFlowCoordinator.Instance().Present(true);
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
            }
        }
    }
}
