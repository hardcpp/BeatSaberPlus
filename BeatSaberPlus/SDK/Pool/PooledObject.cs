using System;

namespace BeatSaberPlus.Pool
{
    /// <summary>
    /// Guarded pooled object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct PooledObject<T> : IDisposable where T : class
    {
        /// <summary>
        /// Guarded element
        /// </summary>
        private readonly T m_ToReturn;
        /// <summary>
        /// Origin pool
        /// </summary>
        private readonly IObjectPool<T> m_Pool;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Element">Element instance to guard</param>
        /// <param name="p_Pool">Source pool</param>
        internal PooledObject(T p_Element, IObjectPool<T> p_Pool)
        {
            m_ToReturn  = p_Element;
            m_Pool      = p_Pool;
        }
        /// <summary>
        /// Dispose the object
        /// </summary>
        void IDisposable.Dispose() =>
            m_Pool.Release(m_ToReturn);
    }
}