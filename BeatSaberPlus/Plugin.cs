using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using IPA;
using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

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

    /*
    class test : MonoBehaviour
    {
        /// <summary>
        /// Canvas instance
        /// </summary>
        private Canvas m_Canvas;
        /// <summary>
        /// Text instance
        /// </summary>
        private TMP_Text m_Text;
        private void Awake()
        {


            var l_RectTransform = null as RectTransform;

            gameObject.transform.position = Vector3.zero;
            gameObject.transform.eulerAngles = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;

            m_Canvas = gameObject.AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            l_RectTransform = m_Canvas.transform as RectTransform;
            l_RectTransform.sizeDelta = new Vector2(200, 200);

            gameObject.AddComponent<HMUI.CurvedCanvasSettings>().SetRadius(0);

            m_Text = CreateText(m_Canvas.transform as RectTransform, "hello world", Vector2.zero, new Vector2(100, 100));
            l_RectTransform = m_Text.transform as RectTransform;
            l_RectTransform.anchoredPosition = Vector2.zero;
            m_Text.fontSize = 50;
            m_Text.alignment = TextAlignmentOptions.Center;

            ///var l_Background            = new GameObject("Background").AddComponent<Image>();
            ///l_RectTransform             = l_Background.transform as RectTransform;
            ///l_RectTransform.SetParent(m_Canvas.transform, false);
            ///l_RectTransform.sizeDelta   = CanvasSize;
            ///l_Background.color          = new Color(0, 1, 0, 0.5f);

            string TT = "GCMode " + UnityEngine.Scripting.GarbageCollector.GCMode + "\n";
            TT += "isIncremental " + UnityEngine.Scripting.GarbageCollector.isIncremental + "\n";
            TT += "incrementalTimeSliceNanoseconds " + UnityEngine.Scripting.GarbageCollector.incrementalTimeSliceNanoseconds + "\n";
            TT += "MaxGeneration " + System.GC.MaxGeneration + "\n";

            System.IO.File.WriteAllText("gc.txt", TT);


            /// Don't destroy this object on scene changes
            DontDestroyOnLoad(this);
        }

        public void Update()
        {
            m_Text.text = "Gc mode " + UnityEngine.Scripting.GarbageCollector.GCMode;
        }

        private TextMeshProUGUI CreateText(RectTransform p_Parent, string p_Text, Vector2 p_AnchoredPosition, Vector2 p_SizeDelta)
        {
            GameObject l_GameObject = new GameObject("CustomUIText");
            l_GameObject.SetActive(false);

            TextMeshProUGUI l_Text = l_GameObject.AddComponent<HMUI.CurvedTextMeshPro>();
            l_Text.rectTransform.SetParent(p_Parent, false);
            l_Text.font = Resources.Load<TMP_FontAsset>("Teko-Medium SDF");
            l_Text.text = p_Text;
            l_Text.fontSize = 4;
            l_Text.color = Color.white;
            l_Text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            l_Text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            l_Text.rectTransform.sizeDelta = p_SizeDelta;
            l_Text.rectTransform.anchoredPosition = p_AnchoredPosition;

            l_GameObject.SetActive(true);
            return l_Text;
        }
    }

    [HarmonyPatch(typeof(StandardLevelGameplayManager))]
    [HarmonyPatch(nameof(StandardLevelGameplayManager.HandleGameEnergyDidReach0))]
    public class PHandleGameEnergyDidReach0
    {

    internal static bool Prefix()
        {
            return false;
        }
    }


    [HarmonyPatch(typeof(GameEnergyUIPanel))]
    [HarmonyPatch(nameof(GameEnergyUIPanel.RefreshEnergyUI))]
    public class PRefreshEnergyUI
    {

        internal static void Prefix(ref float energy)
        {
            energy = Mathf.Max(0.01f, energy);
        }
    }

                _ = SDK.Game.Level.LoadSong("custom_level_F769E1E77F0CA8196B05DE7734C99047E6CCEEAF", (x) =>
                {
                    var l_DifficultyBeatmap = x.beatmapLevelData.GetDifficultyBeatmap(SongCore.Loader.beatmapCharacteristicCollection.GetBeatmapCharacteristicBySerializedName("Standard"), BeatmapDifficulty.ExpertPlus);
                    ScoreSaber.ReplayPlayer.StartZ(System.IO.File.ReadAllBytes("UserData\\ScoreSaber\\Replays\\76561198154857191-Scattered_Faith-ExpertPlus-Standard-F769E1E77F0CA8196B05DE7734C99047E6CCEEAF.dat"), new ScoreSaber.Core.Data.Score()
                    {
                        rawName = "<color=#16E68E>OMDN | Vilanya</color>",
                        isLocalReplay = true,
                        percent = 84.67,
                        modifiers = new GameplayModifiers(false, false, GameplayModifiers.EnergyType.Bar, false, false, false, GameplayModifiers.EnabledObstacleType.All, false, false, false, false, GameplayModifiers.SongSpeed.Normal, false, false, false, false, false),
                        country = "fr",
                        name = "<color=#16E68E>OMDN | Vilanya</color> - <size=75%>(<color=#FFD42A>84,67%</color>)</size>",
                        playerId = "76561198154857191",
                        parent = new ScoreSaber.Core.Data.LeaderboardScoreData()
                        {
                            level = l_DifficultyBeatmap,
                            playerScore = "4280320",
                            ranked = "",
                            scores = new System.Collections.Generic.List<ScoreSaber.Core.Data.Score>(),
                            uid = "291723"
                        },
                        //level = l_DifficultyBeatmap,
                        replay = true,
                    }); ;

                    ///public string timeset { get; set; }
                    ///public int hmd { get; set; }
                    ///public int fullCombo { get; set; }
                    ///public int maxCombo { get; set; }
                    ///public int missedNotes { get; set; }
                    ///public int badCuts { get; set; }
                    ///public string mods { get; set; }
                    ///public double pp { get; set; }
                    ///public string country { get; set; }
                    ///public int score { get; set; }
                    ///public int rank { get; set; }
                    ///public string name { get; set; }
                    ///public string playerId { get; set; }
                    ///public LeaderboardScoreData parent { get; set; }
                    ///public double weight { get; set; }
                    ///public bool replay { get; set; }
                });
    */

    /// <summary>
    /// Main plugin class
    /// </summary>
    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        /// <summary>
        /// Plugin instance
        /// </summary>
        public static Plugin Instance { get; private set; }
        /// <summary>
        /// Plugin version
        /// </summary>
        internal static SemVer.Version Version => IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlusCORE").Version;
        /// <summary>
        /// Plugin name
        /// </summary>
        internal static string Name => "BeatSaberPlus";
        /// <summary>
        /// Harmony ID for patches
        /// </summary>
        internal const string HarmonyID = "com.github.hardcpp.beatsaberplus";
        /// <summary>
        /// Plugins
        /// </summary>
        public List<SDK.IModuleBase> Modules = new List<SDK.IModuleBase>();

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
            /// Set instance
            Instance = this;

            /// Setup logger
            Logger.Instance = p_Logger;

            /// Init config
            Config.Init();
            SDK.Chat.Service.Init();

            try
            {
                /// Cleaning old BeatSaberPlusChatCore
                if (File.Exists("Plugins/BeatSaberPlusChatCore.manifest"))
                    File.Delete("Plugins/BeatSaberPlusChatCore.manifest");

                if (File.Exists("Libs/BeatSaberPlusChatCore.dll"))
                    File.Delete("Libs/BeatSaberPlusChatCore.dll");
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error(l_Exception);
            }

            try
            {
                /// Installing WEBP codec
                if (!Directory.Exists("Libs/Natives/"))
                    Directory.CreateDirectory("Libs/Natives/");

                if (!File.Exists("Libs/Natives/libwebp.dll"))
                    File.WriteAllBytes("Libs/Natives/libwebp.dll", GetResource(Assembly.GetExecutingAssembly(), "BeatSaberPlus.Resources.libwebp.dll"));
                if (!File.Exists("Libs/Natives/libwebpdemux.dll"))
                    File.WriteAllBytes("Libs/Natives/libwebpdemux.dll", GetResource(Assembly.GetExecutingAssembly(), "BeatSaberPlus.Resources.libwebpdemux.dll"));
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On BeatSaberPlus enable
        /// </summary>
        [OnEnable]
        public void OnEnable()
        {
            try
            {
                Logger.Instance.Debug("Applying Harmony patches.");

                /// Setup harmony
                m_Harmony = new Harmony(HarmonyID);
                m_Harmony.PatchAll(Assembly.GetExecutingAssembly());

                Logger.Instance.Debug("Adding menu button.");

                /// Register mod button
                MenuButtons.instance.RegisterButton(new MenuButton("BeatSaberPlus", "Feel good!", OnModButtonPressed, true));

                Logger.Instance.Debug("Init helpers.");
                SDK.Game.Logic.Init();

                Logger.Instance.Debug("Init event.");
                SDK.Game.Logic.OnMenuSceneLoaded += OnMenuSceneLoaded;

                Logger.Instance.Debug("Init modules.");
                foreach (Assembly l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (Type l_Type in l_Assembly.GetTypes())
                        {
                            if (!l_Type.IsClass || l_Type.ContainsGenericParameters)
                                continue;

                            if (!typeof(SDK.IModuleBase).IsAssignableFrom(l_Type))
                                continue;

                            var l_Module = (SDK.IModuleBase)Activator.CreateInstance(l_Type);

                            Logger.Instance.Debug("- " + l_Module.Name);

                            /// Add plugin to the list
                            Modules.Add(l_Module);

                            try {
                                l_Module.CheckForActivation(SDK.IModuleBaseActivationType.OnStart);
                            } catch (System.Exception p_InitException) { Logger.Instance.Error("Error on module init " + l_Module.Name); Logger.Instance.Error(p_InitException); }
                        }
                    }
                    catch (System.Exception l_Exception)
                    {
                        Logger.Instance?.Error("Failed to find modules in " + l_Assembly.FullName);
                        Logger.Instance?.Error(l_Exception);
                    }
                }

                Modules.Sort((x, y) => x.Name.CompareTo(y.Name));

                SDK.VoiceAttack.Service.Acquire();
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Critical(p_Exception);
            }
        }
        /// <summary>
        /// On BeatSaberPlus disable
        /// </summary>
        [OnDisable]
        public void OnDisable()
        {
            foreach (var l_Module in Modules)
                l_Module.OnApplicationExit();

            /// Release all chat services
            SDK.Chat.Service.Release(true);

            /// Release voice attack service
            SDK.VoiceAttack.Service.Release(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the mod button is pressed
        /// </summary>
        private void OnModButtonPressed()
        {
            UI.MainViewFlowCoordinator.Instance().Present(true);
        }
        /// <summary>
        /// When the menu scene is loaded
        /// </summary>
        private void OnMenuSceneLoaded()
        {
            foreach (var l_Module in Modules)
            {
                try {
                    l_Module.CheckForActivation(SDK.IModuleBaseActivationType.OnMenuSceneLoaded);
                } catch (System.Exception p_InitException) { Logger.Instance.Error("Error on module init " + l_Module.Name); Logger.Instance.Error(p_InitException); }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get resource bytes
        /// </summary>
        /// <param name="p_Assembly">Target assembly</param>
        /// <param name="p_ResourceName">Target resource</param>
        /// <returns></returns>
        public static byte[] GetResource(Assembly p_Assembly, string p_ResourceName)
        {
            var l_Stream = p_Assembly.GetManifestResourceStream(p_ResourceName);
            var l_Data = new byte[l_Stream.Length];
            l_Stream.Read(l_Data, 0, (int)l_Stream.Length);
            return l_Data;
        }
    }
}
