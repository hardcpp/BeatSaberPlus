using UnityEngine;

namespace CP_SDK.UI.Data
{
    /// <summary>
    /// List cell prefabs getter
    /// </summary>
    /// <typeparam name="t_ListCellType">List cell type</typeparam>
    public static class ListCellPrefabs<t_ListCellType>
        where t_ListCellType : IListCell
    {
        /// <summary>
        /// Prefab instance
        /// </summary>
        private static t_ListCellType m_Prefab;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get prefab
        /// </summary>
        /// <returns></returns>
        public static t_ListCellType Get()
        {
            if (m_Prefab)
                return m_Prefab;

            m_Prefab = new GameObject(typeof(t_ListCellType).Name + "ListCellPrefab", typeof(RectTransform), typeof(t_ListCellType)).GetComponent<t_ListCellType>();
            GameObject.DontDestroyOnLoad(m_Prefab.gameObject);
            m_Prefab.gameObject.SetActive(false);

            return m_Prefab;
        }
    }
}
