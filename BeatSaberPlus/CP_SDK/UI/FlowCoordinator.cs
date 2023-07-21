using CP_SDK.UI.Views;
using UnityEngine;

namespace CP_SDK.UI
{
    /// <summary>
    /// Flow coordinator base class
    /// </summary>
    /// <typeparam name="t_Base"></typeparam>
    public abstract class FlowCoordinator<t_Base> : IFlowCoordinator
        where t_Base : FlowCoordinator<t_Base>
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static t_Base m_Instance = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create flow coordinator
        /// </summary>
        /// <returns></returns>
        public static t_Base Instance()
        {
            if (!m_Instance)
            {
                m_Instance = new GameObject($"[CP_SDK.UI.FlowCoordinator<{typeof(t_Base).FullName}>]", typeof(t_Base)).GetComponent<t_Base>();
                GameObject.DontDestroyOnLoad(m_Instance.gameObject);
            }

            return m_Instance;
        }
        /// <summary>
        /// Destroy
        /// </summary>
        public static void Destroy()
        {
            if (!m_Instance)
                return;

            GameObject.Destroy(m_Instance.gameObject);
            m_Instance = null;
        }
    }
}
