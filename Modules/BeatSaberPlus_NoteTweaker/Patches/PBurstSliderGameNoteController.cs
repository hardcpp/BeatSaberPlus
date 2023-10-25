using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// GameNoteController Patch
    /// </summary>
    [HarmonyPatch(typeof(BurstSliderGameNoteController))]
    [HarmonyPatch(nameof(BurstSliderGameNoteController.Init))]
    public class PBurstSliderGameNoteController : GameNoteController
    {
        private static bool m_Enabled = false;
        private static bool m_TempEnabled = false;
        private static Vector3 m_NoteScale;
        private static Vector3 m_NoteInvScale;
        private static Vector3 m_TempNoteScale;
        private static Vector3 m_TempNoteInvScale;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">GameNoteController instance</param>
        /// <param name="____bigCuttableBySaber">BoxCuttableBySaber instance</param>
        /// <param name="____smallCuttableBySaber">BoxCuttableBySaber instance</param>
        internal static void Postfix(ref BurstSliderGameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref BoxCuttableBySaber[] ____smallCuttableBySaberList)
        {
            if (!m_Enabled && !m_TempEnabled)
                return;

            __instance.gameObject.transform.localScale = m_TempEnabled ? m_TempNoteScale : m_NoteScale;

            for (int l_I = 0; l_I < ____bigCuttableBySaberList.Length; ++l_I)
                ____bigCuttableBySaberList[l_I].transform.localScale = m_TempEnabled ? m_TempNoteInvScale : m_NoteInvScale;

            for (int l_I = 0; l_I < ____smallCuttableBySaberList.Length; ++l_I)
                ____smallCuttableBySaberList[l_I].transform.localScale = m_TempEnabled ? m_TempNoteInvScale : m_NoteInvScale;
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
            var l_NoteScale = FilterScale(NTConfig.Instance.Enabled ? l_Profile.NotesScale : 1.0f);

            m_Enabled               = IsScaleAllowed() ? NTConfig.Instance.Enabled : false;
            m_NoteScale             = (     l_NoteScale) * Vector3.one;
            m_NoteInvScale          = (1f / l_NoteScale) * Vector3.one;

            if (p_OnSceneSwitch)
                m_TempEnabled = false;
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

            m_TempEnabled           = p_Enabled;
            m_TempNoteScale         =       (p_Scale) * Vector3.one;
            m_TempNoteInvScale      = (1f / (p_Scale))* Vector3.one;
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
