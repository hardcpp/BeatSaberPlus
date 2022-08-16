using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BeatSaberPlus.SDK.Chat.Interfaces
{
    public interface IChatService
    {
        string DisplayName { get; }

        ReadOnlyCollection<(IChatService, IChatChannel)> Channels { get; }

        event Action<IChatService, string> OnSystemMessage;
        event Action<IChatService> OnLogin;

        event Action<IChatService, IChatChannel> OnJoinChannel;
        event Action<IChatService, IChatChannel> OnLeaveChannel;

        event Action<IChatService, IChatMessage> OnTextMessageReceived;
        event Action<IChatService, string> OnChatCleared;
        event Action<IChatService, string> OnMessageCleared;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        void Start();
        void Stop();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        string WebPageHTMLForm();
        string WebPageHTML();
        string WebPageJS();
        string WebPageJSValidate();
        void WebPageOnPost(Dictionary<string, string> p_PostData);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        void SendTextMessage(IChatChannel p_Channel, string p_Message);
    }

    internal class IChatServiceImpl : IChatService
    {
        public string DisplayName => throw new NotImplementedException();

        private CP_SDK.Chat.Interfaces.IChatService m_Base;

        public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => throw new NotImplementedException();

        public event Action<IChatService, string> OnSystemMessage;
        public event Action<IChatService> OnLogin;
        public event Action<IChatService, IChatChannel> OnJoinChannel;
        public event Action<IChatService, IChatChannel> OnLeaveChannel;
        public event Action<IChatService, IChatMessage> OnTextMessageReceived;
        public event Action<IChatService, string> OnChatCleared;
        public event Action<IChatService, string> OnMessageCleared;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public IChatServiceImpl(CP_SDK.Chat.Interfaces.IChatService p_Original)
        {
            m_Base = p_Original;

            m_Base.OnSystemMessage          += (x, y)   => OnSystemMessage?.Invoke(Service.Multiplexer.GetService(x), y);
            m_Base.OnLogin                  += (x)      => OnLogin?.Invoke(this);
            m_Base.OnJoinChannel            += (x, y)   => OnJoinChannel?.Invoke(this, Service.Multiplexer.GetChannel(y));
            m_Base.OnLeaveChannel           += (x, y)   => OnLeaveChannel?.Invoke(this, Service.Multiplexer.GetChannel(y));
            m_Base.OnTextMessageReceived    += (x, y)   => OnTextMessageReceived?.Invoke(this, new IChatMessageImpl(y, Service.Multiplexer.GetChannel(y.Channel)));
            m_Base.OnChatCleared            += (x, y)   => OnChatCleared?.Invoke(Service.Multiplexer.GetService(x), y);
            m_Base.OnMessageCleared         += (x, y)   => OnMessageCleared?.Invoke(Service.Multiplexer.GetService(x), y);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void Start() => throw new NotImplementedException();
        public void Stop() => throw new NotImplementedException();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public string WebPageHTML() => throw new NotImplementedException();
        public string WebPageHTMLForm() => throw new NotImplementedException();
        public string WebPageJS() => throw new NotImplementedException();
        public string WebPageJSValidate() => throw new NotImplementedException();
        public void WebPageOnPost(Dictionary<string, string> p_PostData) => throw new NotImplementedException();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void SendTextMessage(IChatChannel p_Channel, string p_Message)
            => m_Base.SendTextMessage(Service.Multiplexer.GetChannelInv(p_Channel), p_Message);
    }
}
