using BeatSaberPlus.SDK.Chat.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace BeatSaberPlus.SDK.Chat.Services
{
    /// <summary>
    /// A multiplexer for all the supported streaming services.
    /// </summary>
    public class ChatServiceMultiplexer : ChatServiceBase, IChatService
    {
        /// <summary>
        /// The display name of the service(s)
        /// </summary>
        public string DisplayName { get; private set; } = "Generic";

        /// <summary>
        /// p_Channels
        /// </summary>
        public ReadOnlyCollection<(IChatService, IChatChannel)> Channels
        {
            get
            {
                List<(IChatService, IChatChannel)> l_p_Channels = new List<(IChatService, IChatChannel)>();
                foreach (var l_Service in m_Services)
                    l_p_Channels.AddRange(l_Service.Channels);

                return l_p_Channels.AsReadOnly();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Services
        /// </summary>
        private IList<IChatService> m_Services;
        /// <summary>
        /// Invoke lock object
        /// </summary>
        private object m_InvokeLock = new object();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Logger">Logger instance</param>
        /// <param name="p_Services">Services instance</param>
        public ChatServiceMultiplexer(IList<IChatService> p_Services)
        {
            m_Services = p_Services;

            StringBuilder l_NameBuilder = new StringBuilder();
            foreach (var l_Service in m_Services)
            {
                l_Service.OnLogin                       += Service_OnLogin;

                l_Service.OnJoinChannel                 += Service_OnJoinp_Channel;
                l_Service.OnLeaveChannel                += Service_OnLeavep_Channel;
                l_Service.OnRoomStateUpdated            += Service_OnRoomStateUpdated;
                l_Service.OnRoomVideoPlaybackUpdated    += Service_OnRoomVideoPlaybackUpdated;
                l_Service.OnChannelResourceDataCached   += Service_Onp_ChannelResourceDataCached;
                l_Service.OnChannelFollow               += Service_Onp_ChannelFollow;
                l_Service.OnChannelBits                 += Service_Onp_ChannelBits;
                l_Service.OnChannelPoints               += Service_Onp_ChannelPoints;
                l_Service.OnChannelSubscription         += Service_Onp_ChannelSubscription;

                l_Service.OnTextMessageReceived         += Service_OnTextMessageReceived;
                l_Service.OnChatCleared                 += Service_OnChatCleared;
                l_Service.OnMessageCleared              += Service_OnMessageCleared;

                if(l_NameBuilder.Length > 0)
                    l_NameBuilder.Append(", ");

                l_NameBuilder.Append(l_Service.DisplayName);
            }

            DisplayName = l_NameBuilder.ToString();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sends a text message to the specified IChatp_Channel
        /// </summary>
        /// <param name="p_p_Channel">The chat p_Channel to send the message to</param>
        /// <param name="p_Message">The text message to be sent</param>
        public void SendTextMessage(IChatChannel p_p_Channel, string p_Message)
        {
            foreach (var l_Service in m_Services)
                l_Service.SendTextMessage(p_p_Channel, p_Message);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnLogin(IChatService p_ChatService)
        {
            lock (m_InvokeLock)
                m_OnLoginCallbacks.InvokeAll(p_ChatService);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnJoinp_Channel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            lock (m_InvokeLock)
                m_OnJoinRoomCallbacks.InvokeAll(p_ChatService, p_Channel);
        }
        private void Service_OnLeavep_Channel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            lock (m_InvokeLock)
                m_OnLeaveRoomCallbacks.InvokeAll(p_ChatService, p_Channel);
        }
        private void Service_OnRoomStateUpdated(IChatService p_ChatService, IChatChannel p_Channel)
        {
            lock (m_InvokeLock)
                m_OnRoomStateUpdatedCallbacks.InvokeAll(p_ChatService, p_Channel);
        }

        private void Service_OnRoomVideoPlaybackUpdated(IChatService p_ChatService, IChatChannel p_Channel, bool isup, int count)
        {
            lock (m_InvokeLock)
                m_OnRoomVideoPlaybackUpdatedCallbacks.InvokeAll(p_ChatService, p_Channel, isup, count);
        }

        private void Service_Onp_ChannelResourceDataCached(IChatService p_ChatService, IChatChannel p_Channel, Dictionary<string, IChatResourceData> resources)
        {
            lock (m_InvokeLock)
                m_OnChannelResourceDataCached.InvokeAll(p_ChatService, p_Channel, resources);
        }
        private void Service_Onp_ChannelFollow(IChatService p_Service, IChatChannel p_p_Channel, IChatUser p_User)
        {
            lock (m_InvokeLock)
                m_OnChannelFollowCallbacks.InvokeAll(p_Service, p_p_Channel, p_User);
        }
        private void Service_Onp_ChannelBits(IChatService p_Service, IChatChannel p_p_Channel, IChatUser p_User, int p_BitsUsed)
        {
            lock (m_InvokeLock)
                m_OnChannelBitsCallbacks.InvokeAll(p_Service, p_p_Channel, p_User, p_BitsUsed);
        }
        private void Service_Onp_ChannelPoints(IChatService p_Service, IChatChannel p_p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            lock (m_InvokeLock)
                m_OnChannelPointsCallbacks.InvokeAll(p_Service, p_p_Channel, p_User, p_Event);
        }
        private void Service_Onp_ChannelSubscription(IChatService p_Service, IChatChannel p_p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
        {
            lock (m_InvokeLock)
                m_OnChannelSubscriptionCallbacks.InvokeAll(p_Service, p_p_Channel, p_User, p_Event);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnTextMessageReceived(IChatService p_ChatService, IChatMessage message)
        {
            lock (m_InvokeLock)
                m_OnTextMessageReceivedCallbacks.InvokeAll(p_ChatService, message);
        }
        private void Service_OnChatCleared(IChatService p_ChatService, string userId)
        {
            lock (m_InvokeLock)
                m_OnChatClearedCallbacks.InvokeAll(p_ChatService, userId);
        }
        private void Service_OnMessageCleared(IChatService p_ChatService, string messageId)
        {
            lock (m_InvokeLock)
                m_OnMessageClearedCallbacks.InvokeAll(p_ChatService, messageId);
        }
    }
}
