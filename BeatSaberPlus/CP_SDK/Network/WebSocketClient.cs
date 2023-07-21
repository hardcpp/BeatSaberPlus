using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CP_SDK.Network
{
    /// <summary>
    /// Web socket client
    /// </summary>
    public class WebSocketClient
    {
        /// <summary>
        /// Lock object
        /// </summary>
        private object m_LockObject = new object();
        /// <summary>
        /// Web socket client
        /// </summary>
        private System.Net.WebSockets.ClientWebSocket m_Client;
        /// <summary>
        /// Start time
        /// </summary>
        private DateTime m_StartTime;
        /// <summary>
        /// Endpoint
        /// </summary>
        private string m_URI = "";
        /// <summary>
        /// Cancel token
        /// </summary>
        private CancellationTokenSource m_CancellationToken;
        /// <summary>
        /// Receive buffer
        /// </summary>
        private byte[] m_ReceiveBuffer = new byte[1024 * 10];
        /// <summary>
        /// Send buffer
        /// </summary>
        private byte[] m_SendBuffer = new byte[1024 * 10];
        /// <summary>
        /// Send buffer lock
        /// </summary>
        private SemaphoreSlim m_SendSemaphoreSlim = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Reconnect lock semaphore
        /// </summary>
        private SemaphoreSlim m_ReconnectLock = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Is disconnecting?
        /// </summary>
        private bool m_Disconnecting = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is connected
        /// </summary>
        public bool IsConnected => !(m_Client is null) && (m_Client.State == System.Net.WebSockets.WebSocketState.Open || m_Client.State == System.Net.WebSockets.WebSocketState.Connecting);
        /// <summary>
        /// Should auto reconnect
        /// </summary>
        public bool AutoReconnect { get; set; } = true;
        /// <summary>
        /// Reconnect delay in milliseconds
        /// </summary>
        public int ReconnectDelay { get; set; } = 500;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On connection open
        /// </summary>
        public event Action OnOpen;
        /// <summary>
        /// On connection close
        /// </summary>
        public event Action OnClose;
        /// <summary>
        /// On error
        /// </summary>
        public event Action OnError;
        /// <summary>
        /// On message received
        /// </summary>
        public event Action<string> OnMessageReceived;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Release socket data
        /// </summary>
        public void Dispose()
        {
            if (m_Client != null)
            {
                m_Disconnecting = true;
                if (IsConnected)
                {
                    m_CancellationToken?.Cancel();
                    m_Client.Abort();
                }

                m_Client.Dispose();
                m_Client = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="p_URI">Socket endpoint</param>
        public void Connect(string p_URI)
        {
            lock (m_LockObject)
            {
                Dispose();

                m_Disconnecting = false;

                if (m_Client is null)
                {
                    ChatPlexSDK.Logger.Debug($"[CP_SDK.Network][WebSocketClient.Connect] Connecting to {p_URI}");

                    m_URI               = p_URI;
                    m_CancellationToken = new CancellationTokenSource();

                    Task.Run(async () =>
                    {
                        await Task.Yield();

                        try
                        {
                            m_Client    = new System.Net.WebSockets.ClientWebSocket();
                            m_StartTime = DateTime.UtcNow;

                            try
                            {
                                await m_Client.ConnectAsync(new Uri(p_URI), m_CancellationToken.Token).ConfigureAwait(false);
                            }
                            catch (System.Exception)
                            {

                            }

                            if (m_Client != null && m_Client.State == System.Net.WebSockets.WebSocketState.Open)
                            {
                                Client_OnOpen(this);

                                try
                                {
                                    var l_ReceiveArraySegment = new ArraySegment<byte>(m_ReceiveBuffer);

                                    var l_Message = "";
                                    while (m_Client.State == System.Net.WebSockets.WebSocketState.Open)
                                    {
                                        var l_Received = await m_Client.ReceiveAsync(l_ReceiveArraySegment, CancellationToken.None).ConfigureAwait(false);

                                        l_Message += Encoding.UTF8.GetString(m_ReceiveBuffer, l_ReceiveArraySegment.Offset, l_Received.Count);

                                        if (l_Received.EndOfMessage)
                                        {
                                            Client_OnMessageReceived(this, l_Message);
                                            l_Message = string.Empty;
                                        }
                                    }
                                }
                                catch (System.Exception l_Exception)
                                {
                                    if (!m_Disconnecting)
                                    {
                                        ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebSocketClient.Connect] An exception occurred in WebSocket while reading {m_URI}");
                                        ChatPlexSDK.Logger.Error(l_Exception);
                                    }
                                }

                                Client_OnClose(this);
                            }
                            else if (!m_Disconnecting)
                                Client_OnError(this);
                            else
                                Client_OnClose(this);
                        }
                        catch (TaskCanceledException)
                        {
                            ChatPlexSDK.Logger.Info("[CP_SDK.Network][WebSocketClient.Connect] WebSocket client task was cancelled");
                        }
                        catch (Exception l_Exception)
                        {
                            ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebSocketClient.Connect] An exception occurred in WebSocket while connecting to {m_URI}");
                            ChatPlexSDK.Logger.Error(l_Exception);

                            OnError?.Invoke();

                            if (!m_Disconnecting)
                                TryHandleReconnect();
                        }
                    }, m_CancellationToken.Token).ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Disconnect the client
        /// </summary>
        public void Disconnect()
        {
            lock (m_LockObject)
            {
                m_Disconnecting = true;
                ChatPlexSDK.Logger.Info("[CP_SDK.Network][WebSocketClient.Disconnect] Disconnecting");
                Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="p_Message">Message to send</param>
        public async void SendMessage(string p_Message)
        {
            if (!IsConnected)
            {
                ChatPlexSDK.Logger.Debug("[CP_SDK.Network][WebSocketClient.SendMessage] WebSocket not connected, can't send message! " + m_Client.State);
                return;
            }

            await m_SendSemaphoreSlim.WaitAsync().ConfigureAwait(false);
            try
            {
#if DEBUG
                /// Only log this in debug builds, since it can potentially contain sensitive auth data
                ChatPlexSDK.Logger.Debug($"Sending {p_Message}");
#endif

                var l_Writen    = Encoding.UTF8.GetBytes(p_Message, 0, p_Message.Length, m_SendBuffer, 0);
                var l_Segment   = new ArraySegment<byte>(m_SendBuffer, 0, l_Writen);

                await m_Client.SendAsync(l_Segment, System.Net.WebSockets.WebSocketMessageType.Text, true, m_CancellationToken.Token).ConfigureAwait(false);
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebSocketClient.SendMessage] An exception occurred while trying to send message to {m_URI}");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
            finally
            {
                m_SendSemaphoreSlim.Release();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public async void TryHandleReconnect()
        {
            var l_DiffTime = DateTime.UtcNow - m_StartTime;
            ChatPlexSDK.Logger.Info($"Connection was closed after " + string.Format("{0:00}:{1:00}:{2:00}", l_DiffTime.Hours, l_DiffTime.Minutes, l_DiffTime.Seconds) + ".");

            if (!m_ReconnectLock.Wait(0))
                return;

            m_CancellationToken.Cancel();
            m_CancellationToken = new CancellationTokenSource();

            m_Client.Abort();
            m_Client.Dispose();
            m_Client = null;

            if (AutoReconnect && !m_CancellationToken.IsCancellationRequested && !m_Disconnecting)
            {
                ChatPlexSDK.Logger.Info($"[CP_SDK.Network][WebSocketClient.TryHandleReconnect] Trying to reconnect to {m_URI} in {(int)TimeSpan.FromMilliseconds(ReconnectDelay).TotalSeconds} sec");

                try
                {
                    await Task.Delay(ReconnectDelay, m_CancellationToken.Token).ConfigureAwait(false);
                    if (m_Disconnecting)
                        return;
                    Connect(m_URI);

                    ReconnectDelay *= 2;

                    if (ReconnectDelay > 60000)
                        ReconnectDelay = 60000;
                }
                catch (TaskCanceledException)
                {

                }
            }

            m_ReconnectLock.Release();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On client connected
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        private void Client_OnOpen(object p_Sender)
        {
            ChatPlexSDK.Logger.Debug($"[CP_SDK.Network][WebSocketClient.Client_OnOpen] Connection to {m_URI} opened successfully!");
            OnOpen?.Invoke();
        }
        /// <summary>
        /// On client closed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        private void Client_OnClose(object p_Sender)
        {
            ChatPlexSDK.Logger.Debug($"[CP_SDK.Network][WebSocketClient.Client_OnClose] WebSocket connection to {m_URI} was closed");
            OnClose?.Invoke();

            if (!m_Disconnecting)
                TryHandleReconnect();
        }
        /// <summary>
        /// On error
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        private void Client_OnError(object p_Sender)
        {
            ChatPlexSDK.Logger.Error($"[CP_SDK.Network][WebSocketClient.Client_OnError] An error occurred in WebSocket while connected to {m_URI}");

            OnError?.Invoke();

            if (!m_Disconnecting)
                TryHandleReconnect();
        }
        /// <summary>
        /// On message received
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Message">Event data</param>
        private void Client_OnMessageReceived(object p_Sender, string p_Message)
        {
#if DEBUG
            /// Only log this in debug builds, since it can potentially contain sensitive auth data
            ChatPlexSDK.Logger.Debug($"[CP_SDK.Network][WebSocketClient.Client_OnMessageReceived] Message received from {m_URI}: {p_Message}");
#endif
            OnMessageReceived?.Invoke(p_Message);
        }
    }
}
