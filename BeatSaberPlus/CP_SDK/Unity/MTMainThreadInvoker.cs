#if CP_SDK_UNITY
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Main thread task system
    /// </summary>
    [DefaultExecutionOrder(30000)]
    public class MTMainThreadInvoker : MonoBehaviour
    {
        /// <summary>
        /// Max queue size
        /// </summary>
        const int MAX_QUEUE_SIZE = 1000;
        /// <summary>
        /// Self instance (singleton)
        /// </summary>
        static MTMainThreadInvoker m_Instance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queue class
        /// </summary>
        private class Queue
        {
            public Action[] Data = new Action[MAX_QUEUE_SIZE];
            public int WritePos = 0;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maximum single frame task execution time
        /// </summary>
        private static TimeSpan m_YieldAfterTime = TimeSpan.FromMilliseconds(.5);
        /// <summary>
        /// Stop watch instance
        /// </summary>
        private static Stopwatch m_StopWatch = new Stopwatch();
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
                m_Instance = new GameObject("[CP_SDK.Unity.MTMainThreadInvoker]").AddComponent<MTMainThreadInvoker>();
                DontDestroyOnLoad(m_Instance.gameObject);
            }
        }
        /// <summary>
        /// Stop
        /// </summary>
        internal static void Destroy()
        {
            if (!m_Instance)
                return;

            /// Clear queues
            m_Queues = new Queue[2]
            {
                new Queue(),
                new Queue()
            };

            GameObject.Destroy(m_Instance.gameObject);
            m_Instance = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a new action
        /// </summary>
        /// <param name="p_Action">Action to enqueue</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Enqueue(Action p_Action)
        {
            if (p_Action == null)
                return;

            lock (m_Queues)
            {
                var l_Queue = m_Queues[m_FrontQueue];
                if (l_Queue.WritePos >= MAX_QUEUE_SIZE)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][MTMainThreadInvoker.Enqueue] Too many actions pushed!");
                    return;
                }

                l_Queue.Data[l_Queue.WritePos++] = p_Action;
                m_Queued = true;
            }
        }

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

            m_StopWatch.Restart();

            do
            {
                try
                {
                    l_Queue.Data[l_I]();
                }
                catch (Exception l_Exception)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Unity][MTMainThreadInvoker.Update] Error:");
                    ChatPlexSDK.Logger.Error(l_Exception);
                }

                ++l_I;
            } while (l_I < l_Count && m_StopWatch.Elapsed < m_YieldAfterTime);

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
