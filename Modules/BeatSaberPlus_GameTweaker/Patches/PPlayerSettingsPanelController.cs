using BeatSaberMarkupLanguage.Components.Settings;
using HarmonyLib;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_GameTweaker.Patches
{
    /// <summary>
    /// PlayerSettingsPanelController modifier
    /// </summary>
    [HarmonyPatch]
    public class PPlayerSettingsPanelController : PlayerSettingsPanelController
    {
        private static Toggle                                 m_LeftHandedToggle                                    = null;
        private static EnvironmentEffectsFilterPresetDropdown m_EnvironmentEffectsFilterDefaultPresetDropdown       = null;
        private static EnvironmentEffectsFilterPresetDropdown m_EnvironmentEffectsFilterExpertPlusPresetDropdown    = null;
        private static Toggle                                 m_ReduceDebrisToggle                                  = null;
        private static Toggle                                 m_NoTextsAndHudsToggle                                = null;
        private static Toggle                                 m_AdvanceHudToggle                                    = null;
        private static PlayerHeightSettingsController         m_PlayerHeightSettingsController                      = null;
        private static Toggle                                 m_AutomaticPlayerHeightToggle                         = null;
        private static FormattedFloatListSettingsController   m_SfxVolumeSettingsController                         = null;
        private static FormattedFloatListSettingsController   m_SaberTrailIntensitySettingsController               = null;
        private static Toggle                                 m_HideNoteSpawnEffectToggle                           = null;
        private static Toggle                                 m_AdaptiveSfxToggle                                   = null;
        private static NoteJumpDurationTypeSettingsDropdown   m_NoteJumpDurationTypeSettingsDropdown                = null;
        private static FormattedFloatListSettingsController   m_NoteJumpFixedDurationSettingsController             = null;
        private static NoteJumpStartBeatOffsetDropdown        m_NoteJumpStartBeatOffsetDropdown                     = null;

        private static IncrementSetting m_OverrideLightsIntensityToggle            = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        [HarmonyPatch(typeof(PlayerSettingsPanelController))]
        [HarmonyPatch(nameof(PlayerSettingsPanelController.SetLayout), new Type[] { typeof(PlayerSettingsPanelController.PlayerSettingsPanelLayout) })]
        [HarmonyPrefix]
        internal static void SetLayout_Prefix(  ref Toggle                                  ____leftHandedToggle,
                                                ref EnvironmentEffectsFilterPresetDropdown  ____environmentEffectsFilterDefaultPresetDropdown,
                                                ref EnvironmentEffectsFilterPresetDropdown  ____environmentEffectsFilterExpertPlusPresetDropdown,
                                                ref Toggle                                  ____reduceDebrisToggle,
                                                ref Toggle                                  ____noTextsAndHudsToggle,
                                                ref Toggle                                  ____advanceHudToggle,
                                                ref PlayerHeightSettingsController          ____playerHeightSettingsController,
                                                ref Toggle                                  ____automaticPlayerHeightToggle,
                                                ref FormattedFloatListSettingsController    ____sfxVolumeSettingsController,
                                                ref FormattedFloatListSettingsController    ____saberTrailIntensitySettingsController,
                                                ref NoteJumpStartBeatOffsetDropdown         ____noteJumpStartBeatOffsetDropdown,
                                                ref Toggle                                  ____hideNoteSpawnEffectToggle,
                                                ref Toggle                                  ____adaptiveSfxToggle,
                                                ref NoteJumpDurationTypeSettingsDropdown    ____noteJumpDurationTypeSettingsDropdown,
                                                ref FormattedFloatListSettingsController    ____noteJumpFixedDurationSettingsController)
        {
            m_LeftHandedToggle                                  = ____leftHandedToggle;
            m_EnvironmentEffectsFilterDefaultPresetDropdown     = ____environmentEffectsFilterDefaultPresetDropdown;
            m_EnvironmentEffectsFilterExpertPlusPresetDropdown  = ____environmentEffectsFilterExpertPlusPresetDropdown;
            m_ReduceDebrisToggle                                = ____reduceDebrisToggle;
            m_NoTextsAndHudsToggle                              = ____noTextsAndHudsToggle;
            m_AdvanceHudToggle                                  = ____advanceHudToggle;
            m_PlayerHeightSettingsController                    = ____playerHeightSettingsController;
            m_AutomaticPlayerHeightToggle                       = ____automaticPlayerHeightToggle;
            m_SfxVolumeSettingsController                       = ____sfxVolumeSettingsController;
            m_SaberTrailIntensitySettingsController             = ____saberTrailIntensitySettingsController;
            m_NoteJumpStartBeatOffsetDropdown                   = ____noteJumpStartBeatOffsetDropdown;
            m_HideNoteSpawnEffectToggle                         = ____hideNoteSpawnEffectToggle;
            m_AdaptiveSfxToggle                                 = ____adaptiveSfxToggle;
            m_NoteJumpDurationTypeSettingsDropdown              = ____noteJumpDurationTypeSettingsDropdown;
            m_NoteJumpFixedDurationSettingsController           = ____noteJumpFixedDurationSettingsController;

            try
            {
                /*
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,2
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,3
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,4
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,5
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,6
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,7
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,8
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 0,9
                    [ERROR @ 06:07:26 | BeatSaberPlus_GameTweaker] 1
                */

                var l_NewReactionTimeList = new List<float>();
                for (float l_Value = 0.35f; l_Value < 1f; l_Value += 0.01f)
                    l_NewReactionTimeList.Add(l_Value);

                if (m_NoteJumpFixedDurationSettingsController)
                {
                    m_NoteJumpFixedDurationSettingsController.values = l_NewReactionTimeList.ToArray();
                    typeof(FormattedFloatListSettingsController).GetMethod("GetInitValues", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(m_NoteJumpFixedDurationSettingsController, new object[] { (int)0, (int)0 });
                }
            }
            catch (System.Exception)
            {

            }

            /// Apply
            if (GTConfig.Instance.Enabled)
                SetReorderEnabled(GTConfig.Instance.ReorderPlayerSettings, GTConfig.Instance.AddOverrideLightIntensityOption);

            if (GTConfig.Instance.Enabled)
                SetLightsOptionMerging(GTConfig.Instance.MergeLightPressetOptions);
        }
        /// <summary>
        /// Refresh prefix
        /// </summary>
        [HarmonyPatch(typeof(PlayerSettingsPanelController))]
        [HarmonyPatch(nameof(PlayerSettingsPanelController.Refresh), new Type[] { })]
        [HarmonyPostfix]
        internal static void Refresh_Postfix()
        {
            if (GTConfig.Instance.MergeLightPressetOptions
                && m_EnvironmentEffectsFilterDefaultPresetDropdown
                && m_EnvironmentEffectsFilterExpertPlusPresetDropdown)
            {
                try
                {
                    m_EnvironmentEffectsFilterExpertPlusPresetDropdown.SelectCellWithValue(m_EnvironmentEffectsFilterDefaultPresetDropdown.GetSelectedItemValue());
                }
                catch(System.Exception)
                {

                }
            }
        }
        /// <summary>
        /// On NoteJumpFixedDurationSettingsController change
        /// </summary>
        [HarmonyPatch(typeof(FormattedFloatListSettingsController))]
        [HarmonyPatch("TextForValue", new Type[] { typeof(int) })]
        [HarmonyPostfix]
        internal static void NoteJumpFixedDurationSettingsController_TextForValue_Postfix(FormattedFloatListSettingsController __instance , ref string __result)
        {
            if (__instance == m_NoteJumpFixedDurationSettingsController)
                __result = string.Format("{0:0.00}", __instance.value).Replace(".", "").Replace(",", "").TrimStart('0') + "0ms";
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetReorderEnabled(bool p_Enabled, bool p_AddOverrideLightsIntensityOption)
        {
            if (m_EnvironmentEffectsFilterDefaultPresetDropdown == null || m_NoteJumpStartBeatOffsetDropdown == null || m_SfxVolumeSettingsController == null)
                return;

            m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

            if (p_AddOverrideLightsIntensityOption && (m_OverrideLightsIntensityToggle == null || !m_OverrideLightsIntensityToggle))
            {
                var l_Creator = new BeatSaberMarkupLanguage.Tags.Settings.IncrementSettingTag();
                var l_Clone = l_Creator.CreateObject(m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.parent);
                l_Clone.GetComponentInChildren<TextMeshProUGUI>().text = "Override lights intensity";
                (l_Clone.transform as RectTransform).offsetMax = new Vector2(90, (l_Clone.transform as RectTransform).offsetMax.y);

                if (m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.Find("Icon"))
                {
                    var l_Icon = GameObject.Instantiate(m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.Find("Icon"), l_Clone.transform);
                    l_Icon.transform.SetAsFirstSibling();
                }

                var l_LabelTemplateRectTransform = m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.GetChild(1).transform as RectTransform;
                (l_Clone.transform.GetChild(1).transform as RectTransform).offsetMin = l_LabelTemplateRectTransform.offsetMin;
                (l_Clone.transform.GetChild(1).transform as RectTransform).offsetMax = l_LabelTemplateRectTransform.offsetMax;

                (l_Clone.transform.GetChild(2).transform as RectTransform).offsetMin = new Vector2(-36f, 0f);
                (l_Clone.transform.GetChild(2).transform as RectTransform).offsetMax = Vector2.zero;

                m_OverrideLightsIntensityToggle = l_Clone.GetComponent<IncrementSetting>();
                m_OverrideLightsIntensityToggle.minValue    = 0;
                m_OverrideLightsIntensityToggle.maxValue    = 20;
                m_OverrideLightsIntensityToggle.increments  = 0.1f;

                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(null, typeof(PPlayerSettingsPanelController).GetMethod(nameof(OnOverrideLightIntensityChange), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_OverrideLightsIntensityToggle, l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage, GTConfig.Instance.OverrideLightIntensity, false);
            }
            else if (!p_AddOverrideLightsIntensityOption && m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle)
            {
                GameObject.DestroyImmediate(m_OverrideLightsIntensityToggle.gameObject);
                m_OverrideLightsIntensityToggle = null;
            }

            if (p_Enabled)
            {
                m_SaberTrailIntensitySettingsController.transform.parent.SetAsFirstSibling();
                m_HideNoteSpawnEffectToggle.transform.parent.SetAsFirstSibling();
                m_ReduceDebrisToggle.transform.parent.SetAsFirstSibling();
                m_AdaptiveSfxToggle.transform.parent.SetAsFirstSibling();
                m_SfxVolumeSettingsController.transform.parent.SetAsFirstSibling();
                m_LeftHandedToggle.transform.parent.SetAsFirstSibling();
                m_PlayerHeightSettingsController.transform.SetAsFirstSibling();
                m_AutomaticPlayerHeightToggle.transform.parent.SetAsFirstSibling();
                m_EnvironmentEffectsFilterExpertPlusPresetDropdown.transform.parent.SetAsFirstSibling();
                m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle) m_OverrideLightsIntensityToggle.transform.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_NoteJumpFixedDurationSettingsController.transform.SetAsFirstSibling();
                m_NoteJumpStartBeatOffsetDropdown.transform.parent.SetAsFirstSibling();
                m_NoteJumpDurationTypeSettingsDropdown.transform.SetAsFirstSibling();
            }
            else
            {
                m_HideNoteSpawnEffectToggle.transform.parent.SetAsFirstSibling();
                m_ReduceDebrisToggle.transform.parent.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_SaberTrailIntensitySettingsController.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle) m_OverrideLightsIntensityToggle.transform.SetAsFirstSibling();
                m_EnvironmentEffectsFilterExpertPlusPresetDropdown.transform.parent.SetAsFirstSibling();
                m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.SetAsFirstSibling();

                m_NoteJumpFixedDurationSettingsController.transform.SetAsFirstSibling();
                m_NoteJumpStartBeatOffsetDropdown.transform.parent.SetAsFirstSibling();
                m_NoteJumpDurationTypeSettingsDropdown.transform.SetAsFirstSibling();

                m_AdaptiveSfxToggle.transform.parent.SetAsFirstSibling();
                m_SfxVolumeSettingsController.transform.parent.SetAsFirstSibling();
                m_LeftHandedToggle.transform.parent.SetAsFirstSibling();
                m_PlayerHeightSettingsController.transform.SetAsFirstSibling();
                m_AutomaticPlayerHeightToggle.transform.parent.SetAsFirstSibling();
            }
        }
        /// <summary>
        /// Should merge the two lights options
        /// </summary>
        /// <param name="p_Enabled"></param>
        internal static void SetLightsOptionMerging(bool p_Enabled)
        {
            if (m_EnvironmentEffectsFilterDefaultPresetDropdown)
            {
                m_EnvironmentEffectsFilterDefaultPresetDropdown.didSelectCellWithIdxEvent -= OnLightSettingChanged;

                if (p_Enabled)
                    m_EnvironmentEffectsFilterDefaultPresetDropdown.didSelectCellWithIdxEvent += OnLightSettingChanged;
            }

            if (m_EnvironmentEffectsFilterExpertPlusPresetDropdown)
                m_EnvironmentEffectsFilterExpertPlusPresetDropdown.transform.parent.gameObject.SetActive(!p_Enabled);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On OverrideLightIntensity setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        private static void OnOverrideLightIntensityChange(object p_Value)
        {
            GTConfig.Instance.OverrideLightIntensity = (float)p_Value;
            GTConfig.Instance.Save();
        }
        /// <summary>
        /// On light preset change, replicate to the hidden one
        /// </summary>
        /// <param name="p_CellIndex"></param>
        private static void OnLightSettingChanged(int p_CellIndex, EnvironmentEffectsFilterPreset p_Setting)
        {
            if (!m_EnvironmentEffectsFilterExpertPlusPresetDropdown)
                return;

            m_EnvironmentEffectsFilterExpertPlusPresetDropdown.SelectCellWithValue(p_Setting);
        }
    }
}
