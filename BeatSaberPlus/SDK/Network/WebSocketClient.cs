using System;
using System.Threading;
using System.Threading.Tasks;
using BSP_WebSocketSharp;

namespace BeatSaberPlus.SDK.Network
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
        private WebSocket m_Client;
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
        /// Reconnect lock semaphore
        /// </summary>
        private SemaphoreSlim m_ReconnectLock = new SemaphoreSlim(1, 1);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is connected
        /// </summary>
        public bool IsConnected => !(m_Client is null) && (m_Client.ReadyState == WebSocketState.Open || m_Client.ReadyState == WebSocketState.Connecting);
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
            if (!(m_Client is null))
            {
                if (IsConnected)
                {
                    m_CancellationToken?.Cancel();
                    m_Client.Close();
                }

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

                if (m_Client is null)
                {
                    Logger.Instance.Debug($"[SDK.Network][WebSocketClient.Connect] Connecting to {p_URI}");

                    m_URI               = p_URI;
                    m_CancellationToken = new CancellationTokenSource();

                    Task.Run(() =>
                    {
                        try
                        {
                            m_Client = new WebSocket(p_URI);
                            m_Client.OnOpen     += Client_OnOpen;
                            m_Client.OnClose    += Client_OnClose;
                            m_Client.OnError    += Client_OnError;
                            m_Client.OnMessage  += Client_OnMessageReceived;

                            m_StartTime = DateTime.UtcNow;

                            m_Client.ConnectAsync();
                        }
                        catch (TaskCanceledException)
                        {
                            Logger.Instance.Info("[SDK.Network][WebSocketClient.Connect] WebSocket client task was cancelled");
                        }
                        catch (Exception l_Exception)
                        {
                            Logger.Instance.Error($"[SDK.Network][WebSocketClient.Connect] An exception occurred in WebSocket while connecting to {m_URI}");
                            Logger.Instance.Error(l_Exception);

                            OnError?.Invoke();

                            TryHandleReconnect();
                        }
                    }, m_CancellationToken.Token);
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
                Logger.Instance.Info("[SDK.Network][WebSocketClient.Disconnect] Disconnecting");
                Dispose();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="p_Message">Message to send</param>
        public void SendMessage(string p_Message)
        {
            try
            {
                if (IsConnected)
                {
#if DEBUG
                    /// Only log this in debug builds, since it can potentially contain sensitive auth data
                    Logger.Instance.Debug($"Sending {p_Message}");
#endif
                    m_Client.Send(p_Message);
                }
                else
                {
                    Logger.Instance.Debug("[SDK.Network][WebSocketClient.SendMessage] WebSocket not connected, can't send message!");
                }
            }
            catch (Exception l_Exception)
            {
                Logger.Instance.Error($"[SDK.Network][WebSocketClient.SendMessage] An exception occurred while trying to send message to {m_URI}");
                Logger.Instance.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private async void TryHandleReconnect()
        {
            var l_DiffTime = DateTime.UtcNow - m_StartTime;
            Logger.Instance.Info($"Connection was closed after " + string.Format("{0:00}:{1:00}:{2:00}", Math.Floor(l_DiffTime.TotalHours), l_DiffTime.Minutes, l_DiffTime.Seconds) + ".");

            if (!m_ReconnectLock.Wait(0))
                return;

            m_Client.OnOpen     -= Client_OnOpen;
            m_Client.OnClose    -= Client_OnClose;
            m_Client.OnError    -= Client_OnError;
            m_Client.OnMessage  -= Client_OnMessageReceived;
            m_Client = null;

            if (AutoReconnect && !m_CancellationToken.IsCancellationRequested)
            {
                Logger.Instance.Info($"[SDK.Network][WebSocketClient.TryHandleReconnect] Trying to reconnect to {m_URI} in {(int)TimeSpan.FromMilliseconds(ReconnectDelay).TotalSeconds} sec");

                try
                {
                    await Task.Delay(ReconnectDelay, m_CancellationToken.Token);
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
        /// <param name="p_Event">Event data</param>
        private void Client_OnOpen(object p_Sender, EventArgs p_Event)
        {
            Logger.Instance.Debug($"[SDK.Network][WebSocketClient.Client_OnOpen] Connection to {m_URI} opened successfully!");
            OnOpen?.Invoke();
        }
        /// <summary>
        /// On client closed
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void Client_OnClose(object p_Sender, CloseEventArgs p_Event)
        {
            Logger.Instance.Debug($"[SDK.Network][WebSocketClient.Client_OnClose] WebSocket connection to {m_URI} was closed : " + p_Event);
            OnClose?.Invoke();

            TryHandleReconnect();
        }
        /// <summary>
        /// On error
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void Client_OnError(object p_Sender, ErrorEventArgs p_Event)
        {
            Logger.Instance.Error($"[SDK.Network][WebSocketClient.Client_OnError] An error occurred in WebSocket while connected to {m_URI}");
            Logger.Instance.Error(p_Event.Exception);

            OnError?.Invoke();

            TryHandleReconnect();
        }
        /// <summary>
        /// On message received
        /// </summary>
        /// <param name="p_Sender">Event sender</param>
        /// <param name="p_Event">Event data</param>
        private void Client_OnMessageReceived(object p_Sender, MessageEventArgs p_Event)
        {
#if DEBUG
            /// Only log this in debug builds, since it can potentially contain sensitive auth data
            Logger.Instance.Debug($"[SDK.Network][WebSocketClient.Client_OnMessageReceived] Message received from {m_URI}: {p_Event.Data}");
#endif
            OnMessageReceived?.Invoke(p_Event.Data);
        }
    }
}
