using CP_SDK.Unity.Extensions;
using HarmonyLib;
using Polyglot;
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

        private static GameObject                             m_OverrideLightsIntensitySetting                      = null;

        private static CP_SDK.UI.Components.CSlider m_CustomReactionTime;

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
                if (!m_CustomReactionTime)
                {
                    var l_NewReactionTimeList   = new List<float>();
                    for (float l_Value = 0.200f; l_Value <= 1.000f; l_Value += 0.001f)
                        l_NewReactionTimeList.Add(l_Value);

                    if (m_NoteJumpFixedDurationSettingsController)
                    {
                        m_NoteJumpFixedDurationSettingsController.values = l_NewReactionTimeList.ToArray();
                        m_NoteJumpFixedDurationSettingsController.GetInitValues(out var _, out var __);
                        m_NoteJumpFixedDurationSettingsController.transform.GetChild(2).transform.localScale = Vector3.zero;

                        m_CustomReactionTime = CP_SDK.UI.UISystem.SliderFactory.Create("", m_NoteJumpFixedDurationSettingsController.transform);
                        m_CustomReactionTime.RTransform.anchorMin = new Vector2(0.5f, 0.5f);
                        m_CustomReactionTime.RTransform.anchorMax = new Vector2(1.0f, 0.5f);
                        m_CustomReactionTime.RTransform.sizeDelta = new Vector2(0.0f, 5.0f);
                        m_CustomReactionTime.SetColor(ColorU.ToUnityColor("#404040"));
                        m_CustomReactionTime.SetMinValue(200);
                        m_CustomReactionTime.SetMaxValue(1000f);
                        m_CustomReactionTime.SetIncrements(1f);
                        m_CustomReactionTime.SetInteger(true);
                        m_CustomReactionTime.SetFormatter(CP_SDK.UI.ValueFormatters.MillisecondsShort);
                        m_CustomReactionTime.OnValueChanged((x) =>
                        {
                            m_NoteJumpFixedDurationSettingsController.SetValue(m_NoteJumpFixedDurationSettingsController.values[((int)x) - 200], true);
                        });
                        m_CustomReactionTime.SetValue((int)(m_NoteJumpFixedDurationSettingsController.value * 1000f), false);
                    }
                }
            }
            catch (System.Exception)
            {

            }

            /// Apply
            if (GTConfig.Instance.Enabled)
                SetReorderEnabled(GTConfig.Instance.PlayerOptions.ReorderPlayerSettings, GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption);

            if (GTConfig.Instance.Enabled)
                SetLightsOptionMerging(GTConfig.Instance.PlayerOptions.MergeLightPressetOptions);
        }
        /// <summary>
        /// Refresh prefix
        /// </summary>
        [HarmonyPatch(typeof(PlayerSettingsPanelController))]
        [HarmonyPatch(nameof(PlayerSettingsPanelController.Refresh), new Type[] { })]
        [HarmonyPostfix]
        internal static void Refresh_Postfix()
        {
            if (GTConfig.Instance.PlayerOptions.MergeLightPressetOptions
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

            if (p_AddOverrideLightsIntensityOption && (m_OverrideLightsIntensitySetting == null || !m_OverrideLightsIntensitySetting))
            {
                m_OverrideLightsIntensitySetting = GameObject.Instantiate(
                    m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.gameObject,
                    m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.parent
                );

                GameObject.DestroyImmediate(m_OverrideLightsIntensitySetting.transform.Find("SimpleTextDropDown").gameObject);

                m_OverrideLightsIntensitySetting.GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;
                m_OverrideLightsIntensitySetting.GetComponentInChildren<TextMeshProUGUI>().text = "Override lights intensity";

                var l_Slider = CP_SDK.UI.UISystem.SliderFactory.Create("", m_OverrideLightsIntensitySetting.transform);
                l_Slider.RTransform.anchorMin = new Vector2(0.5f, 0.5f);
                l_Slider.RTransform.anchorMax = new Vector2(1.0f, 0.5f);
                l_Slider.RTransform.sizeDelta = new Vector2(0.0f, 5.0f);
                l_Slider.SetColor(ColorU.ToUnityColor("#404040"));
                l_Slider.SetMinValue(0.0f);
                l_Slider.SetMaxValue(10.0f);
                l_Slider.SetIncrements(0.01f);
                l_Slider.SetFormatter(CP_SDK.UI.ValueFormatters.Percentage);
                l_Slider.SetValue(GTConfig.Instance.PlayerOptions.OverrideLightIntensity);
                l_Slider.OnValueChanged((x) =>
                {
                    GTConfig.Instance.PlayerOptions.OverrideLightIntensity = (float)x;
                    GTConfig.Instance.Save();
                });
            }
            else if (!p_AddOverrideLightsIntensityOption && m_OverrideLightsIntensitySetting != null && m_OverrideLightsIntensitySetting)
            {
                GameObject.DestroyImmediate(m_OverrideLightsIntensitySetting);
                m_OverrideLightsIntensitySetting = null;
            }

            if (p_Enabled)
            {
                m_SaberTrailIntensitySettingsController.transform.parent.SetAsFirstSibling();
                m_HideNoteSpawnEffectToggle.transform.parent.SetAsFirstSibling();
                m_ReduceDebrisToggle.transform.parent.SetAsFirstSibling();
                m_AdaptiveSfxToggle.transform.parent.SetAsFirstSibling();
                m_SfxVolumeSettingsController.transform.parent.SetAsFirstSibling();
                m_LeftHandedToggle.transform.parent.SetAsFirstSibling();
                m_AutomaticPlayerHeightToggle.transform.parent.SetAsFirstSibling();
                m_EnvironmentEffectsFilterExpertPlusPresetDropdown.transform.parent.SetAsFirstSibling();
                m_EnvironmentEffectsFilterDefaultPresetDropdown.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensitySetting != null && m_OverrideLightsIntensitySetting) m_OverrideLightsIntensitySetting.transform.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_NoteJumpFixedDurationSettingsController.transform.SetAsFirstSibling();
                m_NoteJumpStartBeatOffsetDropdown.transform.parent.SetAsFirstSibling();
                m_NoteJumpDurationTypeSettingsDropdown.transform.SetAsFirstSibling();
                m_PlayerHeightSettingsController.transform.SetAsFirstSibling();
            }
            else
            {
                m_HideNoteSpawnEffectToggle.transform.parent.SetAsFirstSibling();
                m_ReduceDebrisToggle.transform.parent.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_SaberTrailIntensitySettingsController.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensitySetting != null && m_OverrideLightsIntensitySetting) m_OverrideLightsIntensitySetting.transform.SetAsFirstSibling();
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
