using System;
using System.Collections;
using System.Collections.Generic;

namespace CP_SDK.Misc
{
    /// <summary>
    /// A generic ring buffer with fixed capacity.
    /// </summary>
    /// <typeparam name="T">The type of data stored in the buffer</typeparam>
    public class RingBuffer<T> : IEnumerable<T>, IEnumerable, ICollection<T>, ICollection
    {
        /// <summary>
        /// Buffer
        /// </summary>
        private T[] m_Buffer;
        /// <summary>
        /// Head element
        /// </summary>
        private int m_Head = 0;
        /// <summary>
        /// Tail element
        /// </summary>
        private int m_Tail = 0;
        /// <summary>
        /// Ring size
        /// </summary>
        private int m_Size = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an object that can be used to synchronize access to the
        /// RingBuffer.
        /// </summary>
        public Object SyncRoot { get { return this; } }
        /// <summary>
        /// Gets a value indicating whether access to the RingBuffer is
        /// synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized { get { return false; } }
        /// <summary>
        /// Element count
        /// </summary>
        public int Count { get { return m_Size; } }
        /// <summary>
        /// Is read only
        /// </summary>
        public bool IsReadOnly { get { return false; } }
        /// <summary>
        /// The total number of elements the buffer can store (grows).
        /// </summary>
        public int Capacity { get { return m_Buffer.Length; } }
        /// <summary>
        /// The number of elements currently contained in the buffer.
        /// </summary>
        public int Size { get { return m_Size; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Default capacity is 4, default overflow behavior is false.
        /// </summary>
        public RingBuffer()
            : this(4)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_Capacity">Ring size</param>
        public RingBuffer(int p_Capacity)
        {
            m_Buffer = new T[p_Capacity];
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an item
        /// </summary>
        /// <param name="p_Item">Item to add</param>
        public void Add(T p_Item)
        {
            Put(p_Item);
        }
        /// <summary>
        /// Determines whether the RingBuffer contains a specific value.
        /// </summary>
        /// <param name="p_Item">The value to check the RingBuffer for.</param>
        /// <returns>True if the RingBuffer contains <paramref name="p_Item"/>, false if it does not.
        /// </returns>
        public bool Contains(T p_Item)
        {
            EqualityComparer<T> l_Comparer = EqualityComparer<T>.Default;
            int l_Index = m_Head;
            for (int l_I = 0; l_I < m_Size; l_I++, l_Index = (l_Index + 1) % Capacity)
            {
                if (l_Comparer.Equals(p_Item, m_Buffer[l_Index]))
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Removes <paramref name="p_Item"/> from the buffer.
        /// </summary>
        /// <param name="p_Item">Is to check</param>
        /// <returns>True if <paramref name="p_Item"/> was found and successfully removed. False if <paramref name="p_Item"/> was not found or there was a problem removing it from the RingBuffer.
        /// </returns>
        public bool Remove(T p_Item)
        {
            int l_Index         = m_Head;
            int l_RemoveIndex   = 0;
            bool l_FoundItem    = false;
            EqualityComparer<T> l_Comparer = EqualityComparer<T>.Default;

            for (int l_I = 0; l_I < m_Size; l_I++, l_Index = (l_Index + 1) % Capacity)
            {
                if (!l_Comparer.Equals(p_Item, m_Buffer[l_Index]))
                    continue;

                l_RemoveIndex = l_Index;
                l_FoundItem = true;
                break;
            }

            if (l_FoundItem)
            {
                T[] l_NewBuffer = new T[m_Size - 1];
                l_Index = m_Head;

                bool l_PastItem = false;
                for (int l_I = 0; l_I < m_Size - 1; l_I++, l_Index = (l_Index + 1) % Capacity)
                {
                    if (l_Index == l_RemoveIndex)
                        l_PastItem = true;

                    l_NewBuffer[l_Index] = m_Buffer[l_PastItem ? ((l_Index + 1) % Capacity) : l_Index];
                }

                m_Size--;
                m_Buffer = l_NewBuffer;

                return true;
            }

            return false;
        }
        /// <summary>
        /// Removes all items from the RingBuffer.
        /// </summary>
        public void Clear()
        {
            for (int l_I = 0; l_I < Capacity; l_I++)
                m_Buffer[l_I] = default(T);

            m_Head = 0;
            m_Tail = 0;
            m_Size = 0;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieve the next item from the buffer.
        /// </summary>
        /// <returns>The oldest item added to the buffer.</returns>
        public T Get()
        {
            if (m_Size == 0)
                throw new System.InvalidOperationException("Buffer is empty.");

            T l_Item = m_Buffer[m_Head];
            m_Head = (m_Head + 1) % Capacity;
            m_Size--;

            return l_Item;
        }
        /// <summary>
        /// Adds an item to the end of the buffer.
        /// </summary>
        /// <param name="p_Item">The item to be added.</param>
        public void Put(T p_Item)
        {
            /// If tail & head are equal and the buffer is not empty, assume that it will overflow and throw an exception.
            if (m_Tail == m_Head && m_Size != 0)
                AddToBuffer(p_Item, true);
            /// If the buffer will not overflow, just add the item.
            else
                AddToBuffer(p_Item, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            int l_Index = m_Head;
            for (int l_I = 0; l_I < m_Size; l_I++, l_Index = (l_Index + 1) % Capacity)
                yield return m_Buffer[l_Index];
        }
        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }
        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Copies the contents of the RingBuffer to <paramref name="p_Array"/> starting at <paramref name="p_ArrayIndex"/>.
        /// </summary>
        /// <param name="p_Array">The array to be copied to.</param>
        /// <param name="p_ArrayIndex">The index of <paramref name="p_Array"/> where the buffer should begin copying to.</param>
        public void CopyTo(T[] p_Array, int p_ArrayIndex)
        {
            int l_Index = m_Head;
            for (int l_I = 0; l_I < m_Size; l_I++, p_ArrayIndex++, l_Index = (l_Index + 1) % Capacity)
                p_Array[p_ArrayIndex] = m_Buffer[l_Index];
        }
        /// <summary>
        /// Copies the elements of the RingBuffer to <paramref name="p_Array"/>, starting at a particular Array <paramref name="p_ArrayIndex"/>.
        /// </summary>
        /// <param name="p_Array">The one-dimensional Array that is the destination of the elements copied from RingBuffer. The Array must have zero-based indexing.</param>
        /// <param name="p_ArrayIndex">The zero-based index in <paramref name="p_Array"/> at which copying begins.</param>
        void ICollection.CopyTo(Array p_Array, int p_ArrayIndex)
        {
            CopyTo((T[])p_Array, p_ArrayIndex);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add item to ring buffer
        /// </summary>
        /// <param name="p_Item">Item to add</param>
        /// <param name="p_Overflow">Is overflow ?</param>
        protected void AddToBuffer(T p_Item, bool p_Overflow)
        {
            if (p_Overflow)
                m_Head = (m_Head + 1) % Capacity;
            else
                m_Size++;

            m_Buffer[m_Tail] = p_Item;
            m_Tail = (m_Tail + 1) % Capacity;
        }
    }
}
