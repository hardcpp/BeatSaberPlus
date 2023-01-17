using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Persistant singleton class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PersistentSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>
        /// Instance of the singleton
        /// </summary>
        protected static T m_Instance;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Instance
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GameObject().AddComponent<T>();
                    m_Instance.gameObject.name = "[" + typeof(T).FullName + "]";
                    DontDestroyOnLoad(m_Instance.gameObject);
                }

                return m_Instance;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Touch instance
        /// </summary>
        public static void TouchInstance()
        {
            int placeholder = (Object)PersistentSingleton<T>.Instance == (Object)null ? 1 : 0;
        }
    }
}
