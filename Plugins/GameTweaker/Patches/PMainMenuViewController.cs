using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.GameTweaker.Patches
{
    /// <summary>
    /// MainMenuViewController
    /// </summary>
    [HarmonyPatch(typeof(MainMenuViewController))]
    [HarmonyPatch("DidActivate", new Type[] { typeof(bool), typeof(bool), typeof(bool) })]
    public class PMainMenuViewController : MainMenuViewController
    {
        /// <summary>
        /// MainMenuViewController instance
        /// </summary>
        private static MainMenuViewController m_MainMenuViewController = null;
        /// <summary>
        /// BeatMap editor button
        /// </summary>
        private static Button m_BeatMapEditorButton = null;
        /// <summary>
        /// Was added to hierarchy
        /// </summary>
        private static bool m_AddedToHierarchy = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// DidActivate
        /// </summary>
        /// <param name="__instance">MainMenuViewController instance</param>
        /// <param name="____beatmapEditorButton">BeatMap editor button instance</param>
        internal static void Postfix(ref MainMenuViewController __instance, ref Button ____beatmapEditorButton)
        {
            if (m_MainMenuViewController != __instance)
            {
                m_MainMenuViewController = __instance;
                m_MainMenuViewController.didFinishEvent += (_, __) => SetShowPlayerStatistics(false);
            }

            if (m_BeatMapEditorButton != ____beatmapEditorButton)
                m_BeatMapEditorButton = ____beatmapEditorButton;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetShowPlayerStatistics(Config.GameTweaker.ShowPlayerStatisticsOnMainMenu);
                /// Apply
                SetBeatMapEditorButtonDisabled(Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set we should show player statistics
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetShowPlayerStatistics(bool p_Enabled)
        {
            if (   ( p_Enabled &&  m_AddedToHierarchy)
                || (!p_Enabled && !m_AddedToHierarchy))
                return;

            if (m_MainMenuViewController == null || !m_MainMenuViewController)
                return;

            var l_MainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().FirstOrDefault();
            if (l_MainFlowCoordinator == null || !l_MainFlowCoordinator)
                return;

            var l_PlayerStatisticsViewController = Resources.FindObjectsOfTypeAll<PlayerStatisticsViewController>().FirstOrDefault();

            var l_Method = l_MainFlowCoordinator.GetType().GetMethod("SetRightScreenViewController", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (p_Enabled && l_Method != null)
            {
                l_Method.Invoke(l_MainFlowCoordinator, new object[] { l_PlayerStatisticsViewController, AnimationType.None });
                m_AddedToHierarchy = true;
            }
            else if (!p_Enabled && l_Method != null)
            {
                l_Method.Invoke(l_MainFlowCoordinator, new object[] { null, AnimationType.None });
                m_AddedToHierarchy = false;
            }
        }
        /// <summary>
        /// Set BeatMap editor button disabled
        /// </summary>
        /// <param name="p_Disabled">Is disabled</param>
        internal static void SetBeatMapEditorButtonDisabled(bool p_Disabled)
        {
            if (m_BeatMapEditorButton == null || !m_BeatMapEditorButton)
                return;

            m_BeatMapEditorButton.interactable = !p_Disabled;
        }
    }
}
