using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// BombNoteController patch
    /// </summary>
    [HarmonyPatch(typeof(BombNoteController))]
    [HarmonyPatch(nameof(BombNoteController.Init))]
    public class PBombController : ColorNoteVisuals
    {
        private static int      SIMPLE_COLOR_ID = Shader.PropertyToID("_SimpleColor");
        private static Color    DEFAULT_COLOR   = new Color(0.251f, 0.251f, 0.251f, 0.000f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static bool     m_Enabled           = false;
        private static Color    m_Color             = DEFAULT_COLOR;
        private static Material m_SharedMaterial    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Postfix Init
        /// </summary>
        internal static void Postfix(BombNoteController __instance)
        {
            if (!m_Enabled)
                return;

            if (!m_SharedMaterial)
            {
                m_SharedMaterial = __instance.GetComponentInChildren<Renderer>().sharedMaterial;
                m_SharedMaterial .SetColor(SIMPLE_COLOR_ID, m_Color);
            }

            m_Enabled = false;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        internal static void SetFromConfig(bool p_OnSceneSwitch)
        {
            m_Enabled   = true;
            m_Color     = NTConfig.Instance.Enabled ? (NTConfig.Instance.OverrideBombColor ? NTConfig.Instance.BombColor : DEFAULT_COLOR) : DEFAULT_COLOR;

            if (m_SharedMaterial)
            {
                if (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing)
                    m_SharedMaterial.SetColor(SIMPLE_COLOR_ID, m_Color);
                else
                    m_SharedMaterial.SetColor(SIMPLE_COLOR_ID, DEFAULT_COLOR);
            }
        }
    }
}
