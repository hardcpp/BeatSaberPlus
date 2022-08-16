using CP_SDK_WebSocketSharp;
using CP_SDK_WebSocketSharp.Server;

namespace BeatSaberPlus_SongOverlay.Network
{
    /// <summary>
    /// Web socket client session
    /// </summary>
    internal class OverlaySession : WebSocketBehavior
    {
        /// <summary>
        /// On connection open
        /// </summary>
        protected override void OnOpen()
        {
            OverlayServer.OnClientConnected(this);
        }
        /// <summary>
        /// On connection close
        /// </summary>
        /// <param name="p_Event"></param>
        protected override void OnClose(CloseEventArgs p_Event)
        {
            OverlayServer.OnClientDisconnected(this);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send data to the client
        /// </summary>
        /// <param name="p_Data">Data to send</param>
        internal void SendData(string p_Data)
        {
            Send(p_Data);
        }
    }
}
