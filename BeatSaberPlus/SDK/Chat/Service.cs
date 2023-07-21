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
        /// Chat core multiplexer
        /// </summary>
        private static Services.ChatServiceMultiplexer m_ChatCoreMutiplixer = null;
        /// <summary>
        /// Reference count
        /// </summary>
        private static bool m_Initialized = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Acquire
        /// </summary>
        public static void Acquire()
        {
            CP_SDK.Chat.Service.Acquire();

            if (!m_Initialized)
            {
                OnLoadingStateChanged?.Invoke(false);
                OnLoadingProgressChanged?.Invoke(0f, 0);

                m_ChatCoreMutiplixer = new Services.ChatServiceMultiplexer(CP_SDK.Chat.Service.Multiplexer);
                m_ChatCoreMutiplixer.OnTextMessageReceived += (x, y) => Discrete_OnTextMessageReceived?.Invoke(x, y);

                m_Initialized = true;
            }
        }
        /// <summary>
        /// Release
        /// </summary>
        /// <param name="p_OnExit">Should release all instances</param>
        public static void Release(bool p_OnExit = false)
            => CP_SDK.Chat.Service.Release();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Open web configurator
        /// </summary>
        public static void OpenWebConfigurator()
            => CP_SDK.Chat.Service.OpenWebConfiguration();

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
        /// Broadcast message
        /// </summary>
        /// <param name="p_Message">Message to broadcast</param>
        /// <returns></returns>
        public static void BroadcastMessage(string p_Message)
            => CP_SDK.Chat.Service.BroadcastMessage(p_Message);
    }
}
