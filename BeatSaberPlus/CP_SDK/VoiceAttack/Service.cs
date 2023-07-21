using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CP_SDK.VoiceAttack
{
    /// <summary>
    /// VoiceAttack service holder
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Is the client running?
        /// </summary>
        private static bool m_Running = true;
        /// <summary>
        /// Client instance
        /// </summary>
        private static System.Net.Sockets.TcpClient m_Client = null;
        /// <summary>
        /// Client thread
        /// </summary>
        private static System.Threading.Thread m_Thread = null;
        /// <summary>
        /// Last ping time
        /// </summary>
        private static DateTime m_LastPing = DateTime.Now;
        /// <summary>
        /// Reference count
        /// </summary>
        private static int m_ReferenceCount = 0;
        /// <summary>
        /// Lock object
        /// </summary>
        private static object m_Object = new object();
        /// <summary>
        /// Receive header buffer
        /// </summary>
        private static byte[] m_ReceiveHeaderBuffer = new byte[sizeof(UInt32)];
        /// <summary>
        /// Receive data buffer
        /// </summary>
        private static byte[] m_ReceiveDataBuffer = new byte[4096];
        /// <summary>
        /// Send buffer
        /// </summary>
        private static byte[] m_SendBuffer = new byte[sizeof(UInt32) + 4096];

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On command executed (GUID, Name)
        /// </summary>
        public static event Action<string, string> OnCommandExecuted;

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
        /// Create
        /// </summary>
        private static void Create()
        {
            if (m_Client != null)
                return;

            m_Client = new System.Net.Sockets.TcpClient();
            m_Client.ReceiveTimeout = 2 * 1000;
            m_Client.SendTimeout    = 2 * 1000;

            m_Running = true;

            m_Thread = new System.Threading.Thread(ThreadFunction);
            m_Thread.Start();
        }
        /// <summary>
        /// Destroy
        /// </summary>
        private static void Destroy()
        {
            m_Running = false;

            if (m_Thread != null)
                m_Thread.Abort();

            m_Thread = null;

            if (m_Client == null)
                return;

            try
            {
                m_Client.Client.Close();
            }
            catch
            {

            }
            m_Client = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Thread function
        /// </summary>
        private static void ThreadFunction()
        {
            var l_ReceivedMessages = new List<string>();
            while (m_Running)
            {
                try
                {
                    if (m_Client.Connected && (DateTime.Now - m_LastPing).TotalSeconds >= 30)
                    {
                        ChatPlexSDK.Logger.Info("[CP_SDK.VoiceAttack][Service.ThreadFunction] Lost connection(timeout) to " + m_Client.Client.RemoteEndPoint.ToString());
                        m_Client.Close();
                        m_Client = new System.Net.Sockets.TcpClient();
                        m_Client.ReceiveTimeout = 2 * 1000;
                        m_Client.SendTimeout    = 2 * 1000;
                    }

                    if (!m_Client.Connected)
                    {
                        m_Client.Connect("127.0.0.1", 48841);
                        ChatPlexSDK.Logger.Info("[CP_SDK.VoiceAttack][Service.ThreadFunction] Connected to " + m_Client.Client.RemoteEndPoint.ToString());
                        m_LastPing = DateTime.Now;
                    }
                    else
                    {
                        if (!ReadMessages(l_ReceivedMessages))
                        {
                            /// Disconnect on next tick
                            m_LastPing = DateTime.MinValue;
                        }
                        else
                        {
                            for (var l_I = 0; l_I < l_ReceivedMessages.Count; ++l_I)
                            {
                                var l_Message = l_ReceivedMessages[l_I];
                                if (l_Message == "ping")
                                {
                                    m_LastPing = DateTime.Now;
                                    SendMessage("pong");
                                }
                                else if (l_Message.StartsWith("command_completed;"))
                                {
                                    var l_RightPart     = l_Message.Substring("command_completed;".Length);
                                    var l_GUID          = l_RightPart.Substring(0, l_RightPart.IndexOf(';'));
                                    var l_CommandName   = l_RightPart.Substring(l_RightPart.IndexOf(';') + 1);

                                    Unity.MTThreadInvoker.EnqueueOnThread(() => OnCommandExecuted?.Invoke(l_GUID, l_CommandName));
                                }
                            }
                        }
                    }

                    System.Threading.Thread.Sleep(100);
                }
                catch
                {
                    if (m_Client != null && !m_Client.Connected)
                    {
                        m_Client.Dispose();
                        m_Client = new System.Net.Sockets.TcpClient();
                        m_Client.ReceiveTimeout = 2 * 1000;
                        m_Client.SendTimeout    = 2 * 1000;
                    }

                    System.Threading.Thread.Sleep(30 * 1000);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="p_Message">Message</param>
        private static void SendMessage(string p_Message)
        {
            lock (m_SendBuffer)
            {
                try
                {
                    var l_SizeBytes = BitConverter.GetBytes((uint)p_Message.Length);
                    Array.Copy(l_SizeBytes, m_SendBuffer, l_SizeBytes.Length);
                    var l_TotalSize = l_SizeBytes.Length + System.Text.Encoding.UTF8.GetBytes(p_Message, 0, p_Message.Length, m_SendBuffer, l_SizeBytes.Length);

                    m_Client.GetStream().Write(m_SendBuffer, 0, l_TotalSize);
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// Read messages
        /// </summary>
        /// <param name="p_Results">Out messages</param>
        /// <returns></returns>
        private static bool ReadMessages(List<string> p_Results)
        {
            lock (m_ReceiveHeaderBuffer)
            {
                p_Results.Clear();

                try
                {
                    while (m_Client.Available >= sizeof(UInt32))
                    {
                        m_Client.GetStream().Read(m_ReceiveHeaderBuffer, 0, m_ReceiveHeaderBuffer.Length);

                        var l_SizeToRead = BitConverter.ToUInt32(m_ReceiveHeaderBuffer, 0);
                        if (l_SizeToRead > m_ReceiveDataBuffer.Length)
                            return false;

                        m_Client.GetStream().Read(m_ReceiveDataBuffer, 0, (int)l_SizeToRead);
                        p_Results.Add(System.Text.Encoding.UTF8.GetString(m_ReceiveDataBuffer));
                    }
                }
                catch
                {
                    return false;
                }

                return true;
            }
        }
    }
}
