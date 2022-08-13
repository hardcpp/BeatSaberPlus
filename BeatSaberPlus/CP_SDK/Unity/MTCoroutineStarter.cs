#if CP_SDK_UNITY
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// MultiThreading coroutine starter
    /// </summary>
    public class MTCoroutineStarter : MonoBehaviour
    {
        /// <summary>
        /// Max queue size
        /// </summary>
        const int MAX_QUEUE_SIZE = 1000;
        /// <summary>
        /// Self instance (singleton)
        /// </summary>
        private static MTCoroutineStarter m_Instance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queue class
        /// </summary>
        private class Queue
        {
            public IEnumerator[] Data = new IEnumerator[MAX_QUEUE_SIZE];
            public int WritePos = 0;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queues instance
        /// </summary>
        private static Queue[] m_Queues = new Queue[2]
        {
            new Queue(),
            new Queue()
        };
        /// <summary>
        /// Have queued actions
        /// </summary>
        private static volatile bool m_Queued = false;
        /// <summary>
        /// Current front queue
        /// </summary>
        private static int m_FrontQueue = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unity GameObject initialize
        /// </summary>
        internal static void Initialize()
        {
            if (m_Instance == null)
            {
                m_Instance = new GameObject("[CP_SDK.Unity.MTCoroutineStarter]").AddComponent<MTCoroutineStarter>();
                DontDestroyOnLoad(m_Instance.gameObject);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a new coroutine from thread
        /// </summary>
        /// <param name="p_Coroutine">Coroutine to enqueue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnqueueFromThread(IEnumerator p_Coroutine)
        {
            if (p_Coroutine == null)
                return;

            lock (m_Queues)
            {
                var l_Queue = m_Queues[m_FrontQueue];
                if (l_Queue.WritePos >= MAX_QUEUE_SIZE)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][MTCoroutineStarter.Enqueue] Too many coroutines pushed!");
                    return;
                }

                l_Queue.Data[l_Queue.WritePos++] = p_Coroutine;
                m_Queued = true;
            }
        }
        /// <summary>
        /// Start coroutine
        /// </summary>
        /// <param name="p_Coroutine"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Coroutine Start(IEnumerator p_Coroutine)
            => m_Instance.StartCoroutine(p_Coroutine);
        /// <summary>
        /// Stop coroutine
        /// </summary>
        /// <param name="p_Coroutine"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Stop(Coroutine p_Coroutine)
            => m_Instance.StopCoroutine(p_Coroutine);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Unity GameObject update
        /// </summary>
        private void Update()
        {
            if (!m_Queued)
                return;

            var l_QueueToHandle     = m_FrontQueue;
            var l_NextFrontQueue    = (m_FrontQueue + 1) & 1;

            lock (m_Queues)
            {
                m_FrontQueue    = l_NextFrontQueue;
                m_Queued        = false;
            }

            var l_Queue = m_Queues[l_QueueToHandle];
            var l_Count = l_Queue.WritePos;
            var l_I     = 0;

            do
            {
                try
                {
                    m_Instance.StartCoroutine(l_Queue.Data[l_I]);
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                ++l_I;
            } while (l_I < l_Count);

            if (l_I < l_Count)
            {
                var l_ToCopy        = l_Count - l_I;
                var l_FrontQueue    = m_Queues[m_FrontQueue];

                lock (m_Queues)
                {
                    Array.Copy(l_Queue.Data, l_I, l_FrontQueue.Data, l_FrontQueue.WritePos, l_ToCopy);
                    l_FrontQueue.WritePos += l_ToCopy;
                    m_Queued = true;
                }
            }

            Array.Clear(l_Queue.Data, 0, l_Count);
            l_Queue.WritePos = 0;
        }
    }
}
#endif