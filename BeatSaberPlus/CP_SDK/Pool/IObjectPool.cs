namespace CP_SDK.Pool
{
    /// <summary>
    /// Object pool interface
    /// </summary>
    /// <typeparam name="T">Pooled object type</typeparam>
    public interface IObjectPool<T> where T : class
    {
        /// <summary>
        /// Released element
        /// </summary>
        int CountInactive { get; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Simple get
        /// </summary>
        /// <returns></returns>
        T Get();
        /// <summary>
        /// Managed object get
        /// </summary>
        /// <param name="p_Element">Result value</param>
        /// <returns></returns>
        PooledObject<T> Get(out T p_Element);
        /// <summary>
        /// Release an element
        /// </summary>
        /// <param name="p_Element">Element to release</param>
        void Release(T p_Element);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clear the object pool
        /// </summary>
        void Clear();
    }
}