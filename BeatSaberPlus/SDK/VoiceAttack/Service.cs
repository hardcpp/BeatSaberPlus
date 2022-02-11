using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.VoiceAttack
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
            while (m_Running)
            {
                try
                {
                    if (m_Client.Connected && (DateTime.Now - m_LastPing).TotalSeconds >= 30)
                    {
                        Logger.Instance.Info("[SDK.VoiceAttack][Service.ThreadFunction] Lost connection(timeout) to " + m_Client.Client.RemoteEndPoint.ToString());
                        m_Client.Close();
                        m_Client = new System.Net.Sockets.TcpClient();
                        m_Client.ReceiveTimeout = 2 * 1000;
                        m_Client.SendTimeout    = 2 * 1000;
                    }

                    if (!m_Client.Connected)
                    {
                        m_Client.Connect("127.0.0.1", 48841);
                        Logger.Instance.Info("[SDK.VoiceAttack][Service.ThreadFunction] Connected to " + m_Client.Client.RemoteEndPoint.ToString());
                        m_LastPing = DateTime.Now;
                    }
                    else
                    {
                        if (!ReadMessages(m_Client, out var l_Messages))
                        {
                            /// Disconnect on next tick
                            m_LastPing = DateTime.MinValue;
                        }
                        else
                        {
                            foreach (var l_Message in l_Messages)
                            {
                                if (l_Message == "ping")
                                {
                                    m_LastPing = DateTime.Now;
                                    SendMessage(m_Client, "pong");
                                }
                                else if (l_Message.StartsWith("command_completed;"))
                                {
                                    var l_RightPart     = l_Message.Substring("command_completed;".Length);
                                    var l_GUID          = l_RightPart.Substring(0, l_RightPart.IndexOf(';'));
                                    var l_CommandName   = l_RightPart.Substring(l_RightPart.IndexOf(';') + 1);

                                    Task.Run(() => OnCommandExecuted?.Invoke(l_GUID, l_CommandName));
                                }
                            }
                        }
                    }

                    System.Threading.Thread.Sleep(100);
                }
                catch
                {

                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="p_Client">Socket</param>
        /// <param name="p_Message">Message</param>
        private static void SendMessage(System.Net.Sockets.TcpClient p_Client, string p_Message)
        {
            try
            {
                var l_StringBytes = System.Text.Encoding.UTF8.GetBytes(p_Message);
                var l_SizeBytes = BitConverter.GetBytes((uint)p_Message.Length);

                p_Client.GetStream().Write(l_SizeBytes,   0, l_SizeBytes.Length);
                p_Client.GetStream().Write(l_StringBytes, 0, l_StringBytes.Length);
            }
            catch
            {

            }
        }
        /// <summary>
        /// Read messages
        /// </summary>
        /// <param name="p_Client">Socket</param>
        /// <param name="p_Results">Out messages</param>
        /// <returns></returns>
        private static bool ReadMessages(System.Net.Sockets.TcpClient p_Client, out List<string> p_Results)
        {
            p_Results = new List<string>();

            try
            {
                while (p_Client.Available >= sizeof(UInt32))
                {
                    var l_SizeBytes = new byte[sizeof(UInt32)];
                    p_Client.GetStream().Read(l_SizeBytes, 0, l_SizeBytes.Length);

                    var l_SizeToRead = BitConverter.ToUInt32(l_SizeBytes, 0);

                    if (l_SizeToRead > 4000)
                        return false;

                    var l_MessageBytes = new byte[l_SizeToRead];

                    p_Client.GetStream().Read(l_MessageBytes, 0, l_MessageBytes.Length);

                    p_Results.Add(System.Text.Encoding.UTF8.GetString(l_MessageBytes));
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
