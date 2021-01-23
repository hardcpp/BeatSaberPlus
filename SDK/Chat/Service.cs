using BeatSaberPlusChatCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

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
        private static BeatSaberPlusChatCore.ChatCoreInstance m_ChatCore = null;
        /// <summary>
        /// Chat core multiplexer
        /// </summary>
        private static BeatSaberPlusChatCore.Services.ChatServiceMultiplexer m_ChatCoreMutiplixer = null;
        /// <summary>
        /// Reference count
        /// </summary>
        private static int m_ReferenceCount = 0;
        /// <summary>
        /// Lock object
        /// </summary>
        private static object m_Object = new object();

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

                if (m_ReferenceCount < 0) m_ReferenceCount = 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Open web configurator
        /// </summary>
        public static void OpenWebConfigurator()
        {
            if (m_ChatCore != null)
                m_ChatCore.LaunchWebApp();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static BeatSaberPlusChatCore.Services.ChatServiceMultiplexer Multiplexer => m_ChatCoreMutiplixer;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create
        /// </summary>
        private static void Create()
        {
            if (m_ChatCore != null)
                return;

            /// Init chat core
            m_ChatCore = BeatSaberPlusChatCore.ChatCoreInstance.Create();
            m_ChatCore.OnLogReceived += ChatCore_OnLogReceived;

            /// Run all services
            m_ChatCoreMutiplixer = m_ChatCore.RunAllServices();
            m_ChatCoreMutiplixer.OnChannelResourceDataCached += ChatCoreMutiplixer_OnChannelResourceDataCached;
        }
        /// <summary>
        /// Destroy
        /// </summary>
        private static void Destroy()
        {
            if (m_ChatCore == null)
                return;

            /// Clear cache
            ImageProvider.ClearCache();

            /// Unbind services
            m_ChatCoreMutiplixer.OnChannelResourceDataCached -= ChatCoreMutiplixer_OnChannelResourceDataCached;
            m_ChatCoreMutiplixer = null;

            /// Stop all chat services
            m_ChatCore.StopAllServices();
            m_ChatCore.OnLogReceived -= ChatCore_OnLogReceived;
            m_ChatCore = null;
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
        /// Forward chat core message
        /// </summary>
        /// <param name="p_Level">Logging level</param>
        /// <param name="p_Category">Message category</param>
        /// <param name="p_Message">Message</param>
        private static void ChatCore_OnLogReceived(BeatSaberPlusChatCore.Logging.CustomLogLevel p_Level, string p_Category, string p_Message)
        {
            string l_Message = string.Format("[{0}] {1}", p_Category, p_Message);
            switch (p_Level)
            {
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Critical:      Logger.Instance.Critical(l_Message);    break;
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Debug:         Logger.Instance.Debug(l_Message);       break;
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Error:         Logger.Instance.Error(l_Message);       break;
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Information:   Logger.Instance.Info(l_Message);        break;
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Trace:         Logger.Instance.Trace(l_Message);       break;
                case BeatSaberPlusChatCore.Logging.CustomLogLevel.Warning:       Logger.Instance.Warn(l_Message);        break;
            }
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
            SDK.Unity.MainThreadInvoker.Enqueue(() =>
            {
                int l_Count = 0;
                foreach (var l_Current in p_Resources)
                {
                    if (l_Current.Value.IsAnimated)
                    {
                        ImageProvider.PrecacheAnimatedImage(l_Current.Value.Uri, l_Current.Key);
                        l_Count++;
                    }
                }

                Logger.Instance.Info($"Pre-cached {l_Count} animated emotes.");
            });
        }
    }
}
