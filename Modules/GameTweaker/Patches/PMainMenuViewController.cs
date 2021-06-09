﻿using HarmonyLib;
using System;
using System.Linq;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// MainMenuViewController
    /// </summary>
    [HarmonyPatch(typeof(MainMenuViewController))]
    [HarmonyPatch("DidActivate", new Type[] { typeof(bool), typeof(bool), typeof(bool) })]
    public class PMainMenuViewController : MainMenuViewController
    {
        /// <summary>
        /// BeatMap editor button
        /// </summary>
        private static Button m_BeatMapEditorButton = null;
        /// <summary>
        /// Random music pack promo banner
        /// </summary>
        private static MusicPackPromoBanner m_MusicPackPromoBanner = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// DidActivate
        /// </summary>
        /// <param name="____beatmapEditorButton">BeatMap editor button instance</param>
        /// <param name="____musicPackPromoBanner">Promo banner instance</param>
        internal static void Postfix(ref Button ____beatmapEditorButton, ref MusicPackPromoBanner ____musicPackPromoBanner)
        {
            if (m_BeatMapEditorButton != ____beatmapEditorButton)
                m_BeatMapEditorButton = ____beatmapEditorButton;

            if (m_MusicPackPromoBanner != ____musicPackPromoBanner)
                m_MusicPackPromoBanner = ____musicPackPromoBanner;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetBeatMapEditorButtonDisabled(Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
                SetRemovePackMusicPromoBanner(Config.GameTweaker.RemoveNewContentPromotional);

            }

            var l_Manager = UnityEngine.Resources.FindObjectsOfTypeAll<AnniversaryManager>().FirstOrDefault();
            if (l_Manager)
                l_Manager.gameObject.SetActive(!Config.GameTweaker.Enabled || !Config.GameTweaker.RemoveAnniversaryEvent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
        /// <summary>
        /// Set if Pack music promo banner should be hidden
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetRemovePackMusicPromoBanner(bool p_Enabled)
        {
            if (m_MusicPackPromoBanner == null || !m_MusicPackPromoBanner)
                return;

            m_MusicPackPromoBanner.gameObject.SetActive(!p_Enabled);
        }
    }
}
