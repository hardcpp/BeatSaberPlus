using System;
using System.Collections.Generic;

namespace CP_SDK.Pool
{
    /// <summary>
    /// A stack based Pool.IObjectPool_1.
    /// </summary>
    public class MTObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        /// <summary>
        /// Total size
        /// </summary>
        public int CountAll { get; private set; }
        /// <summary>
        /// Active elements
        /// </summary>
        public int CountActive => CountAll - CountInactive;
        /// <summary>
        /// Released element
        /// </summary>
        public int CountInactive => m_Stack.Count;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private readonly Stack<T> m_Stack;
        private readonly Func<T> m_CreateFunc;
        private readonly Action<T> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly int m_MaxSize;
        private bool m_CollectionCheck;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public MTObjectPool(Func<T> createFunc,             Action<T> actionOnGet = null,   Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null,
                            bool collectionCheck = true,    int defaultCapacity = 10,       int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));

            m_Stack             = new Stack<T>(defaultCapacity);
            m_CreateFunc        = createFunc;
            m_MaxSize           = maxSize;
            m_ActionOnGet       = actionOnGet;
            m_ActionOnRelease   = actionOnRelease;
            m_ActionOnDestroy   = actionOnDestroy;
            m_CollectionCheck   = collectionCheck;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
            => Clear();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Simple get
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T l_Result;

            lock (m_Stack)
            {
                if (m_Stack.Count == 0)
                {
                    l_Result = m_CreateFunc();
                    CountAll++;
                }
                else
                    l_Result = m_Stack.Pop();
            }

            m_ActionOnGet?.Invoke(l_Result);
            return l_Result;
        }
        /// <summary>
        /// Managed object get
        /// </summary>
        /// <param name="p_Element">Result value</param>
        /// <returns></returns>
        public PooledObject<T> Get(out T p_Element)
            => new PooledObject<T>(p_Element = Get(), this);
        /// <summary>
        /// Release an element
        /// </summary>
        /// <param name="p_Element">Element to release</param>
        public void Release(T p_Element)
        {
            lock (m_Stack)
            {
                if (m_CollectionCheck && m_Stack.Count > 0 && m_Stack.Contains(p_Element))
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

                m_ActionOnRelease?.Invoke(p_Element);

                if (CountInactive < m_MaxSize)
                    m_Stack.Push(p_Element);
                else
                    m_ActionOnDestroy?.Invoke(p_Element);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear the object pool
        /// </summary>
        public void Clear()
        {
            lock (m_Stack)
            {
                if (m_ActionOnDestroy != null)
                {
                    foreach (T l_Current in m_Stack)
                        m_ActionOnDestroy(l_Current);
                }

                m_Stack.Clear();
            }
            CountAll = 0;
        }
    }
}