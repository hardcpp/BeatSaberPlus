using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.SDK.Misc
{
    /*
       Code from https://github.com/brian91292/EnhancedStreamChat-v3

       MIT License

       Copyright (c) 2020 brian91292

       Permission is hereby granted, free of charge, to any person obtaining a copy
       of this software and associated documentation files (the "Software"), to deal
       in the Software without restriction, including without limitation the rights
       to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
       copies of the Software, and to permit persons to whom the Software is
       furnished to do so, subject to the following conditions:

       The above copyright notice and this permission notice shall be included in all
       copies or substantial portions of the Software.

       THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
       IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
       FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
       AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
       LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
       OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
       SOFTWARE.
    */

    /// <summary>
    /// A dynamic pool of unity components of type T, that recycles old objects when possible, and allocates new objects when required.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IDisposable where T : Component
    {
        /// <summary>
        /// Ready objects
        /// </summary>
        private Queue<T> m_FreeObjects;
        /// <summary>
        /// On free action
        /// </summary>
        private Action<T> m_OnFree;
        /// <summary>
        /// Constructor function
        /// </summary>
        private Func<T> m_Constructor;
        /// <summary>
        /// Mutex
        /// </summary>
        private object m_Lock = new object();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ObjectPool constructor function, used to setup the initial pool size and callbacks.
        /// </summary>
        /// <param name="p_InitialCount">The number of components of type T to allocate right away.</param>
        /// <param name="p_Constructor">Object contructor</param>
        /// <param name="p_OnFree">The callback function to be called everytime ObjectPool.Free() is called</param>
        public ObjectPool(int p_InitialCount = 0, Func<T> p_Constructor = null, Action<T> p_OnFree = null)
        {
            this.m_Constructor    = p_Constructor;
            this.m_OnFree         = p_OnFree;
            this.m_FreeObjects    = new Queue<T>();

            while (p_InitialCount-- > 0)
                m_FreeObjects.Enqueue(InternalAlloc());
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~ObjectPool()
        {
            Dispose();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="p_Immediate">Immediate ?</param>
        public void Dispose(bool p_Immediate)
        {
            foreach (T l_Current in m_FreeObjects)
            {
                if (p_Immediate)
                    UnityEngine.Object.DestroyImmediate(l_Current.gameObject);
                else
                    UnityEngine.Object.Destroy(l_Current.gameObject);
            }

            m_FreeObjects.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Allocates a component of type T from a pre-allocated pool, or instantiates a new one if required.
        /// </summary>
        /// <returns></returns>
        public T Alloc()
        {
            T l_Object = null;

            lock (m_Lock)
            {
                if (m_FreeObjects.Count > 0)
                    l_Object = m_FreeObjects.Dequeue();
            }

            if (!l_Object)
                l_Object = InternalAlloc();

            return l_Object;
        }
        /// <summary>
        /// Inserts a component of type T into the stack of free objects. Note: the component does *not* need to be allocated using ObjectPool.Alloc() to be freed with this function!
        /// </summary>
        /// <param name="p_Object"></param>
        public void Free(T p_Object)
        {
            if (p_Object == null)
                return;

            lock (m_Lock)
                m_FreeObjects.Enqueue(p_Object);

            m_OnFree?.Invoke(p_Object);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Allocate a new element
        /// </summary>
        /// <returns>New object</returns>
        private T InternalAlloc()
        {
            T l_NewObject = (m_Constructor is null) ? new GameObject().AddComponent<T>() : m_Constructor.Invoke();

            return l_NewObject;
        }
    }
}
