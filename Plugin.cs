using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using IPA;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeatSaberPlus
{
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
        internal static SemVer.Version Version => IPA.Loader.PluginManager.GetPluginFromId("BeatSaberPlus").Version;
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
                        Logger.Instance?.Error("Failed to find modules");
                        Logger.Instance?.Error(l_Exception);
                    }
                }

                Modules.Sort((x, y) => x.Name.CompareTo(y.Name));
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
                } catch (System.Exception p_InitException) { Logger.Instance.Error("Error on plugin init " + l_Module.Name); Logger.Instance.Error(p_InitException); }
            }
        }
    }
}
