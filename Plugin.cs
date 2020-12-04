using BeatSaberMarkupLanguage;
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
        internal static Plugin Instance { get; private set; }
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
        internal List<Plugins.PluginBase> Plugins = new List<Plugins.PluginBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Harmony patch holder
        /// </summary>
        private static Harmony m_Harmony;
        /// <summary>
        /// UI Flow coordinator instance
        /// </summary>
        private UI.ViewFlowCoordinator m_UIFlowCoordinator;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// </summary>
        [Init]
        public Plugin(IPA.Logging.Logger logger)
        {
            /// Set instance
            Instance = this;

            /// Setup logger
            Logger.Instance = logger;

            /// Init config
            Config.Init();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
                Utils.Game.Init();
                Utils.Songs.Init();

                Logger.Instance.Debug("Init event.");
                Utils.Game.OnMenuSceneLoaded += OnMenuSceneLoaded;

                Logger.Instance.Debug("Init sub plugins.");


                foreach (Assembly l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (Type l_Type in l_Assembly.GetTypes())
                        {
                            if (!l_Type.IsClass)
                                continue;

                            if (l_Type.BaseType != typeof(Plugins.PluginBase))
                                continue;

                            var l_Plugin = (Plugins.PluginBase)Activator.CreateInstance(l_Type);

                            Logger.Instance.Debug("- " + l_Plugin.Name);

                            /// Add plugin to the list
                            Plugins.Add(l_Plugin);

                            if (l_Plugin.IsEnabled && l_Plugin.ActivationType == BeatSaberPlus.Plugins.PluginBase.EActivationType.OnStart)
                            {
                                try {
                                    l_Plugin.Enable();
                                } catch (System.Exception p_InitException) { Logger.Instance.Error("Error on plugin init " + l_Plugin.Name); Logger.Instance.Error(p_InitException); }
                            }
                        }
                    }
                    catch (System.Exception)
                    {

                    }
                }

                Plugins.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Critical(p_Exception);
            }
        }
        [OnDisable]
        public void OnDisable()
        {
            foreach (var l_Plugin in Plugins)
                l_Plugin.Disable();

            /// Release all chat services
            Utils.ChatService.Release(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When the mod button is pressed
        /// </summary>
        private void OnModButtonPressed()
        {
            if (m_UIFlowCoordinator == null)
                m_UIFlowCoordinator = BeatSaberMarkupLanguage.BeatSaberUI.CreateFlowCoordinator<UI.ViewFlowCoordinator>();

            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(m_UIFlowCoordinator as HMUI.FlowCoordinator);
        }
        /// <summary>
        /// When the menu scene is loaded
        /// </summary>
        private void OnMenuSceneLoaded()
        {
            if (m_UIFlowCoordinator == null)
                m_UIFlowCoordinator = BeatSaberMarkupLanguage.BeatSaberUI.CreateFlowCoordinator<UI.ViewFlowCoordinator>();

            foreach (var l_Plugin in Plugins)
            {
                if (l_Plugin.IsEnabled && l_Plugin.ActivationType == BeatSaberPlus.Plugins.PluginBase.EActivationType.OnMenuSceneLoaded)
                {
                    try {
                        l_Plugin.Enable();
                    } catch (System.Exception p_InitException) { Logger.Instance.Error("Error on plugin init " + l_Plugin.Name); Logger.Instance.Error(p_InitException); }
                }
            }
        }
    }
}
