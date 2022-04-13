using HarmonyLib;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// Base game filter removing
    /// </summary>
    [HarmonyPatch(typeof(LevelSearchViewController))]
    [HarmonyPatch(nameof(LevelSearchViewController.Setup))]
    public class PLevelSearchViewController : LevelSearchViewController
    {
        /// <summary>
        /// Filter button
        /// </summary>
        static private GameObject m_FilterButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="____filterParamsText">Filter button text instance</param>
        internal static void Prefix(ref TextMeshProUGUI ____filterParamsText)
        {
            /// Cache filter button instance
            m_FilterButton = (____filterParamsText != null && ____filterParamsText) ? ____filterParamsText.transform.parent.gameObject : null;

            if (GTConfig.Instance.Enabled)
            {
                /// Apply
                SetRemoveBaseGameFilter(GTConfig.Instance.LevelSelection.RemoveBaseGameFilterButton);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if we remove base game filter
        /// </summary>
        /// <param name="p_Remove">New state</param>
        internal static void SetRemoveBaseGameFilter(bool p_Remove)
        {
            if (m_FilterButton == null || !m_FilterButton)
                return;

            m_FilterButton.SetActive(!p_Remove);
        }
    }
}
