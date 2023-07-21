using CP_SDK.Chat.Interfaces;
using CP_SDK.Network;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace CP_SDK.Chat.Services
{
    public abstract class RelayChatService<t_EmoteType, t_ServiceResourcerProvider, t_ChannelType, t_UserType>
        : ChatServiceBase, IChatService
        where t_EmoteType                   : IChatResourceData
        where t_ServiceResourcerProvider    : class, IChatServiceResourceManager<t_EmoteType>
        where t_ChannelType                 : IChatChannel
        where t_UserType                    : IChatUser
    {
        public abstract string                                              DisplayName { get; }
        public abstract Color                                               AccentColor { get; }
        public          ReadOnlyCollection<(IChatService, IChatChannel)>    Channels    => m_Channels.Select(x => (this as IChatService, x.Value as IChatChannel)).ToList().AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        protected WebSocketClient                               m_WebSocket     = null;
        protected t_ServiceResourcerProvider                    m_DataProvider  = null;
        protected ConcurrentDictionary<string, t_ChannelType>   m_Channels      = new ConcurrentDictionary<string, t_ChannelType>();
        protected ConcurrentDictionary<string, t_UserType>      m_Users         = new ConcurrentDictionary<string, t_UserType>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the service
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// Stop the service
        /// </summary>
        public abstract void Stop();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Recache emotes
        /// </summary>
        public abstract void RecacheEmotes();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Web page HTML content
        /// </summary>
        /// <returns></returns>
        public abstract string WebPageHTMLForm();
        /// <summary>
        /// Web page HTML content
        /// </summary>
        /// <returns></returns>
        public abstract string WebPageHTML();
        /// <summary>
        /// Web page javascript content
        /// </summary>
        /// <returns></returns>
        public abstract string WebPageJS();
        /// <summary>
        /// Web page javascript content
        /// </summary>
        /// <returns></returns>
        public abstract string WebPageJSValidate();
        /// <summary>
        /// On web page post data
        /// </summary>
        /// <param name="p_PostData">Post data</param>
        public abstract void WebPageOnPost(Dictionary<string, string> p_PostData);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sends a text message to the specified IChatChannel
        /// </summary>
        /// <param name="p_Channel">The chat channel to send the message to</param>
        /// <param name="p_Message">The text message to be sent</param>
        public void SendTextMessage(IChatChannel p_Channel, string p_Message)
        {
            if (!(p_Channel is t_ChannelType))
                return;

            RelayChatServiceProtocol.Send_SendMessage(m_WebSocket, p_Channel.Id, p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is connected
        /// </summary>
        /// <returns></returns>
        public abstract bool IsConnectedAndLive();
        /// <summary>
        /// Get primary channel name
        /// </summary>
        /// <returns></returns>
        public abstract string PrimaryChannelName();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Join temp channel with group identifier
        /// </summary>
        /// <param name="p_GroupIdentifier">Group identifier</param>
        /// <param name="p_ChannelName">Name of the channel</param>
        /// <param name="p_Prefix">Messages prefix</param>
        /// <param name="p_CanSendMessage">Can send message</param>
        public abstract void JoinTempChannel(string p_GroupIdentifier, string p_ChannelName, string p_Prefix, bool p_CanSendMessage);
        /// <summary>
        /// Leave temp channel
        /// </summary>
        /// <param name="p_ChannelName">Name of the channel</param>
        public abstract void LeaveTempChannel(string p_ChannelName);
        /// <summary>
        /// Is in temp channel
        /// </summary>
        /// <param name="p_ChannelName">Channel name</param>
        /// <returns></returns>
        public abstract bool IsInTempChannel(string p_ChannelName);
        /// <summary>
        /// Leave all temp channel by group identifier
        /// </summary>
        /// <param name="p_GroupIdentifier"></param>
        public abstract void LeaveAllTempChannel(string p_GroupIdentifier);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Web socket open
        /// </summary>
        private void WebSocket_OnOpen()
        {
            m_OnSystemMessageCallbacks?.InvokeAll(this, $"Connected to {DisplayName}");

            ChatPlexSDK.Logger.Info($"{DisplayName} connection opened");
            ChatPlexSDK.Logger.Info("Trying to login!");

            //if (ModLicense.IsReady && ModLicense.Raw != null)
            //    RelayChatServiceProtocol.Send_AuthModLicense(m_WebSocket, ModLicense.Raw);
            //else
            //    m_OnSystemMessageCallbacks?.InvokeAll(this, "Invalid license file!");

#if DEBUG
            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] WebSocket_OnOpen");
#endif
        }
        /// <summary>
        /// Web socket close
        /// </summary>
        private void WebSocket_OnClose()
        {
            ChatPlexSDK.Logger.Info($"{DisplayName} connection closed");

            CleanUpChannels();

            m_OnSystemMessageCallbacks?.InvokeAll(this, $"Disconnected from {DisplayName}");
#if DEBUG
            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] WebSocket_OnClose");
#endif
        }
        /// <summary>
        /// Web socket error
        /// </summary>
        private void WebSocket_OnError()
        {
            ChatPlexSDK.Logger.Error($"An error occurred in {DisplayName} connection");

            m_OnSystemMessageCallbacks?.InvokeAll(this, $"Disconnected from {DisplayName}, error");
#if DEBUG
            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] WebSocket_OnError");
#endif
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clean up all connected channels
        /// </summary>
        private void CleanUpChannels()
        {
            foreach (var l_Channel in m_Channels)
            {
                m_DataProvider.TryReleaseChannelResources(l_Channel.Value);
                ChatPlexSDK.Logger.Info($"Removed channel {l_Channel.Value.Id} from the channel list.");
                m_OnLiveStatusUpdatedCallbacks?.InvokeAll(this, l_Channel.Value, false, 0);
                m_OnLeaveRoomCallbacks?.InvokeAll(this, l_Channel.Value);
            }
            m_Channels.Clear();
        }
    }
}
