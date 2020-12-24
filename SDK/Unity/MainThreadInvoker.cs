using IPA.Utilities.Async;
using System;
using System.Threading;

namespace BeatSaberPlus.SDK.Unity
{
    /// <summary>
    /// Main thread task system
    /// </summary>
    public class MainThreadInvoker
    {
        /// <summary>
        /// Cancellation token
        /// </summary>
        private static CancellationTokenSource m_CancellationToken = new CancellationTokenSource();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancel all pending action and clear queue
        /// </summary>
        public static void ClearQueue()
        {
            m_CancellationToken.Cancel();
            m_CancellationToken = new CancellationTokenSource();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a new action
        /// </summary>
        /// <param name="p_Action">Action to enqueue</param>
        public static void Enqueue(Action p_Action)
        {
            if (p_Action == null)
                return;

            UnityMainThreadTaskScheduler.Factory.StartNew(p_Action, m_CancellationToken.Token);
        }
    }
}
