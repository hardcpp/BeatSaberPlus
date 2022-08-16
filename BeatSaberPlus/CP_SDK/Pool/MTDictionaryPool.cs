using System.Collections.Generic;

namespace CP_SDK.Pool
{
    /// <summary>
    /// A version of Pool.CollectionPool_2 for Dictionaries.
    /// </summary>
    public class MTDictionaryPool<TKey, TValue> : MTCollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>>
    {

    }
}