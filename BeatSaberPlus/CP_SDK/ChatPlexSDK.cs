using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CP_SDK
{
    /// <summary>
    /// ChatPlex SDK main class
    /// </summary>
    public static class ChatPlexSDK
    {
        public enum ERenderPipeline
        {
            BuiltIn,
            URP
        }
        public enum EGenericScene
        {
            None,
            Menu,
            Playing
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static List<IModuleBase> m_Modules = new List<IModuleBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Logging.ILogger Logger { get; private set; }

        public static string            ProductName         { get; private set; } = string.Empty;
        public static string            ProductVersion      { get; private set; } = string.Empty;
        public static string            BasePath            { get; private set; } = string.Empty;
        public static string            NetworkUserAgent    { get; private set; } = string.Empty;
        public static ERenderPipeline   RenderPipeline      { get; private set; } = ERenderPipeline.BuiltIn;
        public static EGenericScene     ActiveGenericScene  { get; private set; } = EGenericScene.None;

        public static event Action<EGenericScene>   OnGenericSceneChange;
        public static event Action                  OnGenericMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        /// <param name="p_BasePath">Base path</param>
        /// <param name="p_ProductName">Product name</param>
        /// <param name="p_RenderPipeline">Rendering pipeline</param>
        public static void Configure(Logging.ILogger p_Logger, string p_ProductName, string p_BasePath, ERenderPipeline p_RenderPipeline)
        {
            Logger = p_Logger;

            var l_Version = Assembly.GetExecutingAssembly().GetName().Version;

            ProductName         = p_ProductName;
            ProductVersion      = $"{l_Version.Major}.{l_Version.Minor}.{l_Version.Build}";
            BasePath            = p_BasePath;
            NetworkUserAgent    = $"ChatPlexSDK_{p_ProductName}/{Application.version}";
            RenderPipeline      = p_RenderPipeline;
        }
        /// <summary>
        /// When the assembly is loaded
        /// </summary>
        public static void OnAssemblyLoaded()
        {
            InstallWEBPCodecs();

            /// Init config
            Chat.Service.Init();
            OBS.Service.Init();
            //VoiceAttack.Service.Acquire();
        }
        /// <summary>
        /// On assembly exit
        /// </summary>
        public static void OnAssemblyExit()
        {
            try
            {
                Chat.Service.Release(true);
                VoiceAttack.Service.Release(true);
                OBS.Service.Release(true);
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnAssemblyExit] Error:");
                Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#if CP_SDK_UNITY
        /// <summary>
        /// When unity is ready
        /// </summary>
        public static void OnUnityReady()
        {
            try
            {
                Unity.MTCoroutineStarter.Initialize();
                Unity.MTMainThreadInvoker.Initialize();
                Unity.MTThreadInvoker.Initialize();

                /// Init fonts
                Unity.FontManager.Init();

                /// Init UI
                UI.UISystem.Init();
                UI.ModMenu.Register(new UI.ModButton(ProductName, () => {
                    UI.FlowCoordinators.MainFlowCoordinator.Instance().Present(true);
                }, ProductVersion));
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnUnityReady] Error:");
                Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// When unity is exiting
        /// </summary>
        public static void OnUnityExit()
        {
            try
            {
                OnGenericSceneChange        = null;
                OnGenericMenuSceneLoaded    = null;

                UI.UISystem.Destroy();
                UI.LoadingProgressBar.Destroy();

                Unity.EnhancedImageParticleMaterialProvider.Destroy();
                Unity.EnhancedImageParticleSystemProvider.Destroy();

                Unity.MTThreadInvoker.Destroy();
                Unity.MTMainThreadInvoker.Destroy();
                Unity.MTCoroutineStarter.Destroy();

                Animation.AnimationControllerManager.Destroy();
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnUnityExit] Error:");
                Logger.Error(p_Exception);
            }
        }
#endif

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init all the available modules
        /// </summary>
        public static void InitModules()
        {
            try
            {
                Logger.Debug("[CP_SDK][ChatPlexSDK.InitModules] Init modules.");

                foreach (var l_Assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var l_Type in l_Assembly.GetTypes())
                        {
                            if (!l_Type.IsClass || l_Type.ContainsGenericParameters)
                                continue;

                            if (!typeof(IModuleBase).IsAssignableFrom(l_Type))
                                continue;

                            var l_Module = (IModuleBase)Activator.CreateInstance(l_Type);

                            Logger.Debug("[CP_SDK][ChatPlexSDK.InitModules] - " + l_Module.Name);

                            /// Add plugin to the list
                            m_Modules.Add(l_Module);
                        }
                    }
                    catch (Exception l_Exception)
                    {
                        Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Failed to find modules in " + l_Assembly.FullName);
                        Logger.Error(l_Exception);
                    }
                }

                m_Modules.Sort((x, y) => x.FancyName.CompareTo(y.FancyName));

                for (int l_I = 0; l_I < m_Modules.Count; l_I++)
                {
                    var l_Module = m_Modules[l_I];

                    try                               { l_Module.CheckForActivation(EIModuleBaseActivationType.OnStart);                                                        }
                    catch (Exception p_InitException) { Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Error on module init " + l_Module.Name); Logger.Error(p_InitException); }
                }

                Chat.Service.StartServices();
            }
            catch (Exception p_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.InitModules] Error:");
                Logger.Error(p_Exception);
            }
        }
        /// <summary>
        /// Stop modules
        /// </summary>
        public static void StopModules()
        {
            for (int l_I = 0; l_I < m_Modules.Count; l_I++)
            {
                try
                {
                    var l_Module = m_Modules[l_I];
                    l_Module.OnApplicationExit();
                }
                catch (Exception p_Exception)
                {
                    Logger.Error("[CP_SDK][ChatPlexSDK.StopModules] Error:");
                    Logger.Error(p_Exception);
                }
            }

            m_Modules.Clear();
        }
        /// <summary>
        /// Get modules
        /// </summary>
        /// <returns></returns>
        public static List<IModuleBase> GetModules()
            => new List<IModuleBase>(m_Modules);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On generic menu scene
        /// </summary>
        public static void Fire_OnGenericMenuSceneLoaded()
        {
            try
            {
                UI.LoadingProgressBar.TouchInstance();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.Fire_OnGenericMenuSceneLoaded] Error :");
                Logger.Error(l_Exception);
            }

            ActiveGenericScene = EGenericScene.Menu;

            for (int l_I = 0; l_I < m_Modules.Count; l_I++)
            {
                var l_Module = m_Modules[l_I];

                try                                 { l_Module.CheckForActivation(EIModuleBaseActivationType.OnMenuSceneLoaded);                                                                }
                catch (Exception p_InitException)   { Logger.Error("[CP_SDK][ChatPlexSDK.Fire_OnGenericMenuSceneLoaded] Error on module init " + l_Module.Name); Logger.Error(p_InitException); }
            }

            try
            {
                OnGenericMenuSceneLoaded?.Invoke();
                Chat.Service.StartServices();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericMenuScene] Error :");
                Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On generic menu scene
        /// </summary>
        public static void Fire_OnGenericMenuScene()
        {
            ActiveGenericScene = EGenericScene.Menu;

            try
            {
                OnGenericSceneChange?.Invoke(EGenericScene.Menu);
                Chat.Service.StartServices();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericMenuScene] Error :");
                Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On generic play scene
        /// </summary>
        public static void Fire_OnGenericPlayingScene()
        {
            ActiveGenericScene = EGenericScene.Playing;

            try
            {
                OnGenericSceneChange?.Invoke(EGenericScene.Playing);
                Chat.Service.StartServices();
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.OnGenericPlayingScene] Error:");
                Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Install WEBP codecs
        /// </summary>
        private static void InstallWEBPCodecs()
        {
            try
            {
                /// Installing WEBP codec
                if (!Directory.Exists("Libs/Natives/"))
                    Directory.CreateDirectory("Libs/Natives/");

                var l_LibWEBP       = Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.libwebp.dll");
                var l_LibWEBPDemux  = Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.libwebpdemux.dll");

                if (!File.Exists("Libs/Natives/libwebp.dll")
                    || Cryptography.SHA1.GetHashString(File.ReadAllBytes("Libs/Natives/libwebp.dll")) != Cryptography.SHA1.GetHashString(l_LibWEBP))
                {
                    File.WriteAllBytes("Libs/Natives/libwebp.dll", l_LibWEBP);
                }

                if (!File.Exists("Libs/Natives/libwebpdemux.dll")
                    || Cryptography.SHA1.GetHashString(File.ReadAllBytes("Libs/Natives/libwebpdemux.dll")) != Cryptography.SHA1.GetHashString(l_LibWEBPDemux))
                {
                    File.WriteAllBytes("Libs/Natives/libwebpdemux.dll", l_LibWEBPDemux);
                }
            }
            catch (Exception l_Exception)
            {
                Logger.Error("[CP_SDK][ChatPlexSDK.InstallWEBPCodecs] Error:");
                Logger.Error(l_Exception);
            }
        }
    }
}