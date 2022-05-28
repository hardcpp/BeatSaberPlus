using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace BeatSaberPlus.SDK.Chat
{
    /// <summary>
    /// Chat service holder
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Chat core instance
        /// </summary>
        private static List<Interfaces.IChatService> m_ExternalServices = new List<IChatService>();
        /// <summary>
        /// Chat core instance
        /// </summary>
        private static List<Interfaces.IChatService> m_Services = null;
        /// <summary>
        /// Chat core multiplexer
        /// </summary>
        private static Services.ChatServiceMultiplexer m_ChatCoreMutiplixer = null;
        /// <summary>
        /// Reference count
        /// </summary>
        private static int m_ReferenceCount = 0;
        /// <summary>
        /// Lock object
        /// </summary>
        private static object m_Object = new object();
        /// <summary>
        /// Is using fast load
        /// </summary>
        private static bool m_UsingFastLoad = false;
        /// <summary>
        /// Was warned about fastload
        /// </summary>
        private static bool m_FastLoadWarned = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            AuthConfig.Init();
            SettingsConfig.Init();

            m_UsingFastLoad = System.Environment.CommandLine.ToLower().Contains("-fastload");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register external chat service
        /// </summary>
        /// <param name="p_Service"></param>
        public static void RegisterExternalService(Interfaces.IChatService p_Service)
        {
            m_ExternalServices.Add(p_Service);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Acquire
        /// </summary>
        public static void Acquire()
        {
            lock (m_Object)
            {
                if (m_ReferenceCount == 0)
                    Create();

                m_ReferenceCount++;
            }
        }
        /// <summary>
        /// Release
        /// </summary>
        /// <param name="p_OnExit">Should release all instances</param>
        public static void Release(bool p_OnExit = false)
        {
            lock (m_Object)
            {
                if (p_OnExit)
                {
                    Destroy();
                    m_ReferenceCount = 0;
                }
                else
                    m_ReferenceCount--;

                if (m_ReferenceCount < 0)
                    m_ReferenceCount = 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Open web configurator
        /// </summary>
        public static void OpenWebConfigurator()
        {
            Process.Start($"http://localhost:{SettingsConfig.WebApp.WebAppPort}");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Chat multiplexer
        /// </summary>
        public static Services.ChatServiceMultiplexer Multiplexer => m_ChatCoreMutiplixer;
        /// <summary>
        /// Discrete OnTextMessageReceived
        /// </summary>
        public static event Action<IChatService, IChatMessage> Discrete_OnTextMessageReceived;
        /// <summary>
        /// On channel resource loading state changed
        /// </summary>
        public static event Action<bool> OnLoadingStateChanged;
        /// <summary>
        /// On channel resource loading progress changed
        /// </summary>
        public static event Action<float, int> OnLoadingProgressChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create
        /// </summary>
        private static void Create()
        {
            if (m_Services != null)
                return;

            /// Init services
            m_Services = new List<IChatService>();
#if !TEST_APP
            m_Services.Add(new Services.Twitch.TwitchService());
#endif
            m_Services.AddRange(m_ExternalServices);

            /// Run all services
            m_ChatCoreMutiplixer = new Services.ChatServiceMultiplexer(m_Services);
            m_ChatCoreMutiplixer.OnChannelResourceDataCached += ChatCoreMutiplixer_OnChannelResourceDataCached;
            m_ChatCoreMutiplixer.OnTextMessageReceived       += ChatCoreMutiplixer_OnTextMessageReceived;

            /// Start services
            foreach (var l_Service in m_Services)
                l_Service.Start();

            /// WebApp
            WebApp.Start();
            if (SettingsConfig.WebApp.LaunchWebAppOnStartup)
                OpenWebConfigurator();
        }
        /// <summary>
        /// Destroy
        /// </summary>
        private static void Destroy()
        {
            if (m_Services == null)
                return;

            /// WebApp
            WebApp.Stop();

#if !TEST_APP
            /// Clear cache
            ImageProvider.ClearCache();
#endif

            /// Unbind services
            m_ChatCoreMutiplixer.OnChannelResourceDataCached -= ChatCoreMutiplixer_OnChannelResourceDataCached;
            m_ChatCoreMutiplixer.OnTextMessageReceived       -= ChatCoreMutiplixer_OnTextMessageReceived;
            m_ChatCoreMutiplixer = null;

            /// Stop all chat services
            foreach (var l_Service in m_Services)
                l_Service.Stop();

            m_Services = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Broadcast message
        /// </summary>
        /// <param name="p_Message">Message to broadcast</param>
        /// <returns></returns>
        public static void BroadcastMessage(string p_Message)
        {
            foreach (var l_Current in Multiplexer.Channels)
                l_Current.Item1.SendTextMessage(l_Current.Item2, p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On channel resources received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_Resources">Resources</param>
        private static void ChatCoreMutiplixer_OnChannelResourceDataCached(IChatService p_ChatService, IChatChannel p_Channel, Dictionary<string, IChatResourceData> p_Resources)
        {
            if (m_UsingFastLoad)
            {
                if (!m_FastLoadWarned)
                {
                    Multiplexer.InternalBroadcastSystemMessage("<b><color=red>You are running the game with -fastload, you might encounter frame drops when animated emotes are used</color></b>");
                    m_FastLoadWarned = true;
                }

                return;
            }

#if !TEST_APP
            int l_Count = 0;
            int l_Loaded = 0;
#endif

            OnLoadingStateChanged?.Invoke(true);

#if !TEST_APP
            Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.ShowLoadingProgressBar("Loading animated emotes...", 0f));

            foreach (var l_Current in p_Resources)
            {
                if (l_Current.Value.Animation != Animation.AnimationType.NONE && l_Current.Value.Category == EChatResourceCategory.Emote)
                    l_Count++;
            }

            foreach (var l_Current in p_Resources)
            {
                if (l_Current.Value.Animation != Animation.AnimationType.NONE && l_Current.Value.Category == EChatResourceCategory.Emote)
                {
                    ImageProvider.PrecacheAnimatedImage(l_Current.Value.Category, l_Current.Value.Uri, l_Current.Key, l_Current.Value.Animation, (x) =>
                    {
                        var l_CurrentLoaded = System.Threading.Interlocked.Increment(ref l_Loaded);
                        var l_Progress      = (float)l_CurrentLoaded / (float)l_Count;

                        OnLoadingProgressChanged?.Invoke(l_Progress, l_Count);
                        Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.SetProgress("Loading emotes...", l_Progress));

                        if (l_CurrentLoaded == l_Count)
                        {
                            OnLoadingStateChanged?.Invoke(false);
                            Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.SetProgress($"Loaded {l_Count} emotes", 1f));
                            Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.HideTimed(4f));

                            Multiplexer.InternalBroadcastSystemMessage($"Pre-cached {l_Count} animated emotes for {p_Channel.Name}.");
                            Logger.Instance.Info($"Pre-cached {l_Count} animated emotes for {p_Channel.Name}.");
                        }
                    });
                }
            }

            if (l_Count == 0)
            {
                OnLoadingStateChanged?.Invoke(false);

                Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.SetProgress("Loaded 0 animated emotes...", 1f));
                Unity.MainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.instance.HideTimed(4f));
            }
#endif
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private static void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
#if !TEST_APP
            if (p_Message.Message.Length > 2 && p_Message.Message[0] == '!')
            {
                string l_LMessage = p_Message.Message.ToLower();

                if (l_LMessage.StartsWith("!bsplusversion"))
                {
                    p_Service.SendTextMessage(p_Message.Channel,
                        $"! @{p_Message.Sender.DisplayName} The current BS+ version is "
                        + $"{Plugin.Version.Major}.{Plugin.Version.Minor}.{Plugin.Version.Patch}!"
                        + $" The game version is "
                        + $"{Application.version}");
                }
                else if (l_LMessage.StartsWith("!bsprofile"))
                {
                    p_Service.SendTextMessage(p_Message.Channel,
                        $"! @{p_Message.Sender.DisplayName} You can find the streamer's ScoreSaber Profile at "
                        + $"https://scoresaber.com/u/{Game.UserPlatform.GetUserID() ?? "unk"}");
                }
            }
#endif

            try
            {
                Discrete_OnTextMessageReceived?.Invoke(p_Service, p_Message);
            }
            catch
            {

            }
        }
    }
}
