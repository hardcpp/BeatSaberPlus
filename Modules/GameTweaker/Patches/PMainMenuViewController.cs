using HarmonyLib;
using System;
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// DidActivate
        /// </summary>
        /// <param name="__instance">MainMenuViewController instance</param>
        /// <param name="____beatmapEditorButton">BeatMap editor button instance</param>
        internal static void Postfix(ref MainMenuViewController __instance, ref Button ____beatmapEditorButton)
        {
            if (m_BeatMapEditorButton != ____beatmapEditorButton)
                m_BeatMapEditorButton = ____beatmapEditorButton;

            if (Config.GameTweaker.Enabled)
            {
                /// Apply
                SetBeatMapEditorButtonDisabled(Config.GameTweaker.DisableBeatMapEditorButtonOnMainMenu);
            }
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
    }
}
