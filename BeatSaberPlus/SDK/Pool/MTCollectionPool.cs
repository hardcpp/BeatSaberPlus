using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.Pool
{
    /// <summary>
    /// A Collection such as List, HashSet, Dictionary etc can be pooled and reused by using a CollectionPool.
    /// </summary>
    public class MTCollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
    {
        /// <summary>
        /// Static collection
        /// </summary>
        internal static readonly MTObjectPool<TCollection> s_Pool = new MTObjectPool<TCollection>(() => new TCollection(), actionOnRelease: (x => x.Clear()));

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Simple get
        /// </summary>
        /// <returns></returns>
        public static TCollection Get()
            => s_Pool.Get();
        /// <summary>
        /// Managed object get
        /// </summary>
        /// <param name="p_Element">Result value</param>
        /// <returns></returns>
        public static PooledObject<TCollection> Get(out TCollection p_Element)
            => s_Pool.Get(out p_Element);
        /// <summary>
        /// Release an element
        /// </summary>
        /// <param name="p_Element">Element to release</param>
        public static void Release(TCollection p_Element)
            => s_Pool.Release(p_Element);
    }
}