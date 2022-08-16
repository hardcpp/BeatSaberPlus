using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BeatSaberPlus.SDK.Chat.Services
{
    public class ChatServiceMultiplexer : ChatServiceBase, IChatService
    {
        public string DisplayName { get; private set; } = "System";
        public IReadOnlyList<IChatService> Services  => m_ConvServices.Values.ToList().AsReadOnly();
        public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => m_Multiplexer.Channels.Select(x => (GetService(x.Item1), GetChannel(x.Item2))).ToList().AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private object m_InvokeLock = new object();
        private CP_SDK.Chat.Services.ChatServiceMultiplexer m_Multiplexer;
        private Dictionary<CP_SDK.Chat.Interfaces.IChatService, IChatService> m_ConvServices = new Dictionary<CP_SDK.Chat.Interfaces.IChatService, IChatService>();
        private Dictionary<CP_SDK.Chat.Interfaces.IChatChannel, IChatChannel> m_ConvChannels = new Dictionary<CP_SDK.Chat.Interfaces.IChatChannel, IChatChannel>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public ChatServiceMultiplexer(CP_SDK.Chat.Services.ChatServiceMultiplexer p_Multiplexer)
        {
            m_Multiplexer = p_Multiplexer;

            foreach (var l_Service in m_Multiplexer.Services)
            {
                GetService(l_Service);

                l_Service.OnSystemMessage               += Service_OnSystemMessage;
                l_Service.OnLogin                       += Service_OnLogin;

                l_Service.OnJoinChannel                 += Service_OnJoinChannel;
                l_Service.OnLeaveChannel                += Service_OnLeaveChannel;

                l_Service.OnTextMessageReceived         += Service_OnTextMessageReceived;
                l_Service.OnChatCleared                 += Service_OnChatCleared;
                l_Service.OnMessageCleared              += Service_OnMessageCleared;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void Start() => throw new NotImplementedException();
        public void Stop() => throw new NotImplementedException();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string WebPageHTMLForm() => throw new NotImplementedException();
        public string WebPageHTML() => throw new NotImplementedException();
        public string WebPageJS() => throw new NotImplementedException();
        public string WebPageJSValidate() => throw new NotImplementedException();
        public void WebPageOnGet(Dictionary<string, string> p_DataToReplace) => throw new NotImplementedException();
        public void WebPageOnPost(Dictionary<string, string> p_PostData) => throw new NotImplementedException();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void SendTextMessage(IChatChannel p_Channel, string p_Message)
            => m_Multiplexer.SendTextMessage(GetChannelInv(p_Channel), p_Message);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void InternalBroadcastSystemMessage(string p_Message)
            => m_Multiplexer.InternalBroadcastSystemMessage(p_Message);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnSystemMessage(CP_SDK.Chat.Interfaces.IChatService p_ChatService, string p_Message)
        {
            lock (m_InvokeLock)
                m_OnSystemMessageCallbacks.InvokeAll(GetService(p_ChatService), p_Message);
        }
        private void Service_OnLogin(CP_SDK.Chat.Interfaces.IChatService p_ChatService)
        {
            lock (m_InvokeLock)
                m_OnLoginCallbacks.InvokeAll(GetService(p_ChatService));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnJoinChannel(CP_SDK.Chat.Interfaces.IChatService p_ChatService, CP_SDK.Chat.Interfaces.IChatChannel p_Channel)
        {
            lock (m_InvokeLock)
                m_OnJoinRoomCallbacks.InvokeAll(GetService(p_ChatService), GetChannel(p_Channel));
        }
        private void Service_OnLeaveChannel(CP_SDK.Chat.Interfaces.IChatService p_ChatService, CP_SDK.Chat.Interfaces.IChatChannel p_Channel)
        {
            lock (m_InvokeLock)
                m_OnLeaveRoomCallbacks.InvokeAll(GetService(p_ChatService), GetChannel(p_Channel));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Service_OnTextMessageReceived(CP_SDK.Chat.Interfaces.IChatService p_ChatService, CP_SDK.Chat.Interfaces.IChatMessage message)
        {
            lock (m_InvokeLock)
                m_OnTextMessageReceivedCallbacks.InvokeAll(GetService(p_ChatService), new IChatMessageImpl(message, Service.Multiplexer.GetChannel(message.Channel)));
        }
        private void Service_OnChatCleared(CP_SDK.Chat.Interfaces.IChatService p_ChatService, string userId)
        {
            lock (m_InvokeLock)
                m_OnChatClearedCallbacks.InvokeAll(GetService(p_ChatService), userId);
        }
        private void Service_OnMessageCleared(CP_SDK.Chat.Interfaces.IChatService p_ChatService, string messageId)
        {
            lock (m_InvokeLock)
                m_OnMessageClearedCallbacks.InvokeAll(GetService(p_ChatService), messageId);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal IChatService GetService(CP_SDK.Chat.Interfaces.IChatService p_Input)
        {
            if (m_ConvServices.TryGetValue(p_Input, out IChatService existing))
                return existing;

            var l_New = new IChatServiceImpl(p_Input);
            m_ConvServices.Add(p_Input, l_New);

            return l_New;
        }
        internal CP_SDK.Chat.Interfaces.IChatService GetServiceInv(IChatService p_Input)
        {
            foreach (var l_KVP in m_ConvServices)
            {
                if (l_KVP.Value == p_Input)
                    return l_KVP.Key;
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal IChatChannel GetChannel(CP_SDK.Chat.Interfaces.IChatChannel p_Input)
        {
            if (m_ConvChannels.TryGetValue(p_Input, out IChatChannel existing))
                return existing;

            var l_New = new IChatChannelImpl(p_Input);
            m_ConvChannels.Add(p_Input, l_New);

            return l_New;
        }
        internal CP_SDK.Chat.Interfaces.IChatChannel GetChannelInv(IChatChannel p_Input)
        {
            foreach (var l_KVP in m_ConvChannels)
            {
                if (l_KVP.Value == p_Input)
                    return l_KVP.Key;
            }

            return null;
        }
    }
}
