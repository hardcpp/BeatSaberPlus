using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CP_SDK.Chat
{
    /// <summary>
    /// Chat service holder
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Chat core instance
        /// </summary>
        private static List<IChatService> m_ExternalServices = new List<IChatService>();
        /// <summary>
        /// Chat core instance
        /// </summary>
        private static List<IChatService> m_Services = null;
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
        /// <summary>
        /// Is service started
        /// </summary>
        private static bool m_Started = false;
        /// <summary>
        /// Loading emote count
        /// </summary>
        private static int m_LoadingEmotes = 0;
        /// <summary>
        /// Loaded emote count
        /// </summary>
        private static int m_LoadedEmotes = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        internal static void Init()
        {
            ChatModSettings.Instance.Warmup();

            ChatImageProvider.Init();

            m_UsingFastLoad = Environment.CommandLine.ToLower().Contains("-fastload");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register external chat service
        /// </summary>
        /// <param name="p_Service"></param>
        public static void RegisterExternalService(IChatService p_Service)
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
        /// Start the services
        /// </summary>
        internal static void StartServices()
        {
            if (m_Started || m_Services == null)
                return;

            /// Start services
            foreach (var l_Service in m_Services)
                l_Service.Start();

            m_Started = true;
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
            Process.Start($"http://localhost:{ChatModSettings.Instance.WebAppPort}");
        }
        /// <summary>
        /// Recache all the emotes
        /// </summary>
        public static void RecacheEmotes()
        {
            m_LoadingEmotes = 0;
            m_LoadedEmotes = 0;

            Multiplexer.RecacheEmotes();
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
            m_Services.Add(new Services.Twitch.TwitchService());
            m_Services.AddRange(m_ExternalServices);

            /// Run all services
            m_ChatCoreMutiplixer = new Services.ChatServiceMultiplexer(m_Services);
            m_ChatCoreMutiplixer.OnChannelResourceDataCached += ChatCoreMutiplixer_OnChannelResourceDataCached;
            m_ChatCoreMutiplixer.OnTextMessageReceived       += ChatCoreMutiplixer_OnTextMessageReceived;

            /// WebApp
            WebApp.Start();
            if (ChatModSettings.Instance.LaunchWebAppOnStartup)
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

            /// Clear cache
            ChatImageProvider.ClearCache();

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

            OnLoadingStateChanged?.Invoke(true);

            m_LoadingEmotes += p_Resources.Count(x => x.Value.Animation != Animation.EAnimationType.NONE && x.Value.Category == EChatResourceCategory.Emote);
            var l_ProgressA = m_LoadingEmotes > 0 ? (float)m_LoadedEmotes / (float)m_LoadingEmotes : 0f;

            OnLoadingProgressChanged?.Invoke(l_ProgressA, m_LoadingEmotes);
            Unity.MTMainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.Instance.ShowLoadingProgressBar($"Loading {m_LoadingEmotes} emotes...", l_ProgressA));

            foreach (var l_Current in p_Resources)
            {
                if (l_Current.Value.Animation != Animation.EAnimationType.NONE && l_Current.Value.Category == EChatResourceCategory.Emote)
                {
                    ChatImageProvider.TryCacheSingleImage(l_Current.Value.Category, l_Current.Key, l_Current.Value.Uri, l_Current.Value.Animation, (x) =>
                    {
                        var l_CurrentLoaded = System.Threading.Interlocked.Increment(ref m_LoadedEmotes);
                        var l_ProgressB     = (float)l_CurrentLoaded / (float)m_LoadingEmotes;

                        OnLoadingProgressChanged?.Invoke(l_ProgressB, m_LoadingEmotes);
                        Unity.MTMainThreadInvoker.Enqueue(() => UI.LoadingProgressBar.Instance.SetProgress($"Loading {m_LoadingEmotes} emotes...", l_ProgressB));

                        if (l_CurrentLoaded == m_LoadingEmotes)
                        {
                            OnLoadingStateChanged?.Invoke(false);
                            OnLoadingProgressChanged?.Invoke(1f, m_LoadingEmotes);

                            Unity.MTMainThreadInvoker.Enqueue(() =>
                            {
                                UI.LoadingProgressBar.Instance.SetProgress($"Loading {m_LoadingEmotes} emotes...", 1f);
                                UI.LoadingProgressBar.Instance.HideTimed(3f);
                            });

                            Multiplexer.InternalBroadcastSystemMessage($"Loaded {m_LoadingEmotes} animated emotes.");
                            ChatPlexSDK.Logger.Info($"Loaded {m_LoadingEmotes} animated emotes.");
                        }
                    });
                }
            }

            if (m_LoadingEmotes == 0)
            {
                OnLoadingStateChanged?.Invoke(false);
                OnLoadingProgressChanged?.Invoke(1f, m_LoadingEmotes);

                Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    UI.LoadingProgressBar.Instance.SetProgress($"Loading {m_LoadingEmotes} emotes...", 1f);
                    UI.LoadingProgressBar.Instance.HideTimed(3f);
                });

                Multiplexer.InternalBroadcastSystemMessage($"Loaded {m_LoadingEmotes} animated emotes.");
                ChatPlexSDK.Logger.Info($"Loaded {m_LoadingEmotes} animated emotes.");
            }
        }
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_Service">Chat service</param>
        /// <param name="p_Message">Chat message</param>
        private static void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
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
