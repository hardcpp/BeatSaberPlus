using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Persistent MonoBehaviour singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PersistentSingletonInput<T> : BaseInputModule where T : BaseInputModule
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static T m_Instance = default(T);
        /// <summary>
        /// Lock object
        /// </summary>
        private static object m_Lock = new object();
        /// <summary>
        /// Is application quitting
        /// </summary>
        private static bool m_ApplicationIsQuitting = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_ApplicationIsQuitting)
                {
                    ChatPlexSDK.Logger.Warning("[CP_SDK.Unity][PersistentSingletonInput<" + typeof(T).Name + ">.Instance] was destroyed on application quit.");
                    return default(T);
                }

                if (m_Instance != null)
                    return m_Instance;
                else
                {
                    lock (m_Lock)
                    {
                        if (m_Instance != null)
                            return m_Instance;

                        var l_Existing = FindObjectsOfType(typeof(T)) as T[];

                        if (l_Existing.Length == 1)
                            m_Instance = l_Existing[0];
                        else if (l_Existing.Length > 1)
                        {
                            ChatPlexSDK.Logger.Error("[CP_SDK.Unity][PersistentSingletonInput<" + typeof(T).Name + ">.Instance] Multiple instance this singleton was found");
                            return m_Instance;
                        }

                        if (m_Instance == null)
                        {
                            var l_Owner = FindObjectsOfType<EventSystem>().FirstOrDefault();
                            if (l_Owner)
                                m_Instance = l_Owner.gameObject.AddComponent<T>();
                            else
                            {
                                m_Instance = new GameObject("[CP_SDK.Unity][PersistentSingletonInput<" + typeof(T).Name + ">.Instance]").AddComponent<T>();
                                DontDestroyOnLoad(m_Instance.gameObject);
                            }
                        }

                        return m_Instance;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Touch the instance to warm up the singleton
        /// </summary>
        public static void TouchInstance()
        {
            int l_KeepAlive = Instance == null ? 1 : 0;
        }
    }
}
