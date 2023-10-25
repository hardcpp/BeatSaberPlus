using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// BombNoteController patch
    /// </summary>
    [HarmonyPatch(typeof(BombNoteController))]
    [HarmonyPatch(nameof(BombNoteController.Init))]
    public class PBombNoteController : BombNoteController
    {
        private static int      SIMPLE_COLOR_ID = Shader.PropertyToID("_SimpleColor");
        private static Color    DEFAULT_COLOR   = new Color(0.251f, 0.251f, 0.251f, 1.000f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static bool     m_Enabled           = false;
        private static bool     m_TempEnabled       = false;
        private static Color    m_Color             = DEFAULT_COLOR;
        private static Material m_SharedMaterial    = null;
        private static bool     m_ShouldRecolorize  = false;
        private static Vector3  m_Scale;
        private static float    m_InvScale;
        private static Vector3  m_TempScale;
        private static float    m_TempInvScale;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Postfix Init
        /// </summary>
        internal static void Postfix(ref BombNoteController __instance, ref CuttableBySaber ____cuttableBySaber)
        {
            if (!m_SharedMaterial)
                m_SharedMaterial = __instance.GetComponentInChildren<Renderer>().sharedMaterial;

            if (m_ShouldRecolorize && m_SharedMaterial)
            {
                m_SharedMaterial.SetColor(SIMPLE_COLOR_ID, m_Color);
                m_ShouldRecolorize = false;
            }

            if (!m_Enabled && !m_TempEnabled)
                return;

            __instance.gameObject.transform.localScale                  = m_TempEnabled ? m_TempScale       : m_Scale;
            ____cuttableBySaber.GetComponent<SphereCollider>().radius   = 0.18f * (m_TempEnabled ? m_TempInvScale : m_InvScale);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        /// <param name="p_OnSceneSwitch">Reset on scene switch</param>
        internal static void SetFromConfig(bool p_OnSceneSwitch)
        {
            var l_Profile   = NTConfig.Instance.GetActiveProfile();
            var l_BombScale = FilterScale(NTConfig.Instance.Enabled ? l_Profile.BombsScale : 1.0f);

            m_Enabled               = IsScaleAllowed() ? NTConfig.Instance.Enabled : false;
            m_ShouldRecolorize      = true;
            m_Color                 = (m_Enabled && l_Profile.BombsOverrideColor) ? l_Profile.BombsColor : DEFAULT_COLOR;
            m_Scale                 = (     l_BombScale) * Vector3.one;
            m_InvScale              =  1f / l_BombScale;

            if (p_OnSceneSwitch)
                m_TempEnabled = false;

            if (m_SharedMaterial)
            {
                if (CP_SDK_BS.Game.Logic.ActiveScene == CP_SDK_BS.Game.Logic.ESceneType.Playing)
                    m_SharedMaterial.SetColor(SIMPLE_COLOR_ID, m_Color);
                else
                    m_SharedMaterial.SetColor(SIMPLE_COLOR_ID, DEFAULT_COLOR);
            }
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_Enabled">Is it enabled</param>
        /// <param name="p_Scale">New scale</param>
        public static void SetTemp(bool p_Enabled, float p_Scale)
        {
            if (!IsScaleAllowed())
                return;

            p_Scale = FilterScale(p_Scale);

            m_TempEnabled       = p_Enabled;
            m_TempScale         =       (p_Scale) * Vector3.one;
            m_TempInvScale      =  1f / (p_Scale);
        }
        /// <summary>
        /// Bomb color override
        /// </summary>
        /// <param name="p_Enabled">Is override enabled?</param>
        /// <param name="p_NewColor">New color</param>
        public static void SetBombColorOverride(bool p_Enabled, Color p_NewColor)
        {
            m_ShouldRecolorize = true;

            if (p_Enabled)
                m_Color = p_NewColor;
            else
            {
                var l_Profile = NTConfig.Instance.GetActiveProfile();
                m_Color = (m_Enabled && l_Profile.BombsOverrideColor) ? l_Profile.BombsColor : DEFAULT_COLOR;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is note scaling enabled
        /// </summary>
        /// <returns></returns>
        private static bool IsScaleAllowed()
        {
            if ((CP_SDK_BS.Game.Logic.LevelData?.Data?.gameplayModifiers?.proMode ?? false)
             || (CP_SDK_BS.Game.Logic.LevelData?.Data?.gameplayModifiers?.smallCubes ?? false)
             || (CP_SDK_BS.Game.Logic.LevelData?.Data?.gameplayModifiers?.strictAngles ?? false))
                return false;

            return true;
        }
        /// <summary>
        /// Filter note scaling
        /// </summary>
        /// <param name="p_Scale">Input scale</param>
        /// <returns></returns>
        private static float FilterScale(float p_Scale)
        {
            p_Scale = Mathf.Max(p_Scale, 0.4f);
            p_Scale = Mathf.Min(p_Scale, 1.5f);

            return p_Scale;
        }
    }
}
