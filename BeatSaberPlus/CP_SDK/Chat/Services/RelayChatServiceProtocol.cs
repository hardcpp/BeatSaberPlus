using CP_SDK.Network;
using System;

namespace CP_SDK.Chat.Services
{
    public static class RelayChatServiceProtocol
    {
        public const string PROTOCOL_VERSION = "v1.4";

        public static void Send_AuthModLicense(WebSocketClient p_Socket, byte[] p_ModLicense)
            => p_Socket.SendMessage($"AuthModLicense|{PROTOCOL_VERSION}|{(p_ModLicense?.Length > 0 ? Convert.ToBase64String(p_ModLicense) : "null")}");

        public static void Send_SendMessage(WebSocketClient p_Socket, string p_ChannelID, string p_Message)
            => p_Socket.SendMessage("SendMessage|" + p_ChannelID + "|" + p_Message);
    }
}
