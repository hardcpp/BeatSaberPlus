﻿using HarmonyLib;
using System.Collections.Generic;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// MusicPackPromoBanner remover
    /// </summary>
    [HarmonyPatch(typeof(MusicPackPromoBanner))]
    [HarmonyPatch(nameof(MusicPackPromoBanner.Setup))]
    public class PMusicPackPromoBanner : MusicPackPromoBanner
    {
        /// <summary>
        /// MusicPackPromoBanner instance
        /// </summary>
        private static MusicPackPromoBanner m_Instance = null;
        /// <summary>
        /// Original states
        /// </summary>
        private static List<bool> m_OriginalStates = new List<bool>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">MusicPackPromoBanner instance</param>
        internal static void Postfix(ref MusicPackPromoBanner __instance)
        {
            if (__instance != m_Instance)
                m_OriginalStates.Clear();

            if (GTConfig.Instance.Enabled && m_OriginalStates.Count == 0)
            {
                /// Store instance
                m_Instance = __instance;

                /// Backup original states
                for (int l_I = 0; l_I < m_Instance.transform.childCount; ++l_I)
                    m_OriginalStates.Add(m_Instance.transform.GetChild(l_I).gameObject.activeSelf);

                /// Apply
                SetEnabled(GTConfig.Instance.MainMenu.RemoveNewContentPromotional);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if MusicPackPromoBanner should be hidden
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetEnabled(bool p_Enabled)
        {
            if (m_Instance == null || !m_Instance)
                return;

            for (int l_I = 0; l_I < m_Instance.transform.childCount; ++l_I)
                m_Instance.transform.GetChild(l_I).gameObject.SetActive(p_Enabled ? false : (l_I < m_OriginalStates.Count ? m_OriginalStates[l_I] : false));
        }
    }
}