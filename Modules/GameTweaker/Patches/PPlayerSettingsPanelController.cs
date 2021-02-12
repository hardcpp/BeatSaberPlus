using BeatSaberMarkupLanguage.Components.Settings;
using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.GameTweaker.Patches
{
    /// <summary>
    /// PlayerSettingsPanelController modifier
    /// </summary>
    [HarmonyPatch(typeof(PlayerSettingsPanelController))]
    [HarmonyPatch(nameof(PlayerSettingsPanelController.SetLayout), new Type[] { typeof(PlayerSettingsPanelController.PlayerSettingsPanelLayout) })]
    public class PPlayerSettingsPanelController : PlayerSettingsPanelController
    {
        private static Toggle                                m_LeftHandedToggle                         = null;
        private static Toggle                                m_StaticLightsToggle                       = null;
        private static Toggle                                m_ReduceDebrisToggle                       = null;
        private static Toggle                                m_NoTextsAndHudsToggle                     = null;
        private static Toggle                                m_AdvanceHudToggle                         = null;
        private static PlayerHeightSettingsController        m_PlayerHeightSettingsController           = null;
        private static Toggle                                m_AutomaticPlayerHeightToggle              = null;
        private static FormattedFloatListSettingsController  m_SfxVolumeSettingsController              = null;
        private static FormattedFloatListSettingsController  m_SaberTrailIntensitySettingsController    = null;
        private static NoteJumpStartBeatOffsetDropdown       m_NoteJumpStartBeatOffsetDropdown          = null;
        private static Toggle                                m_HideNoteSpawnEffectToggle                = null;
        private static Toggle                                m_AdaptiveSfxToggle                        = null;

        private static IncrementSetting m_OverrideLightsIntensityToggle            = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">PlayerSettingsPanelController instance</param>
        internal static void Prefix(ref PlayerSettingsPanelController __instance,
                                    ref Toggle                                  ____leftHandedToggle,
                                    ref Toggle                                  ____staticLightsToggle,
                                    ref Toggle                                  ____reduceDebrisToggle,
                                    ref Toggle                                  ____noTextsAndHudsToggle,
                                    ref Toggle                                  ____advanceHudToggle,
                                    ref PlayerHeightSettingsController          ____playerHeightSettingsController,
                                    ref Toggle                                  ____automaticPlayerHeightToggle,
                                    ref FormattedFloatListSettingsController    ____sfxVolumeSettingsController,
                                    ref FormattedFloatListSettingsController    ____saberTrailIntensitySettingsController,
                                    ref NoteJumpStartBeatOffsetDropdown         ____noteJumpStartBeatOffsetDropdown,
                                    ref Toggle                                  ____hideNoteSpawnEffectToggle,
                                    ref Toggle                                  ____adaptiveSfxToggle)
        {
            m_LeftHandedToggle                      = ____leftHandedToggle;
            m_StaticLightsToggle                    = ____staticLightsToggle;
            m_ReduceDebrisToggle                    = ____reduceDebrisToggle;
            m_NoTextsAndHudsToggle                  = ____noTextsAndHudsToggle;
            m_AdvanceHudToggle                      = ____advanceHudToggle;
            m_PlayerHeightSettingsController        = ____playerHeightSettingsController;
            m_AutomaticPlayerHeightToggle           = ____automaticPlayerHeightToggle;
            m_SfxVolumeSettingsController           = ____sfxVolumeSettingsController;
            m_SaberTrailIntensitySettingsController = ____saberTrailIntensitySettingsController;
            m_NoteJumpStartBeatOffsetDropdown       = ____noteJumpStartBeatOffsetDropdown;
            m_HideNoteSpawnEffectToggle             = ____hideNoteSpawnEffectToggle;
            m_AdaptiveSfxToggle                     = ____adaptiveSfxToggle;

            /// Apply
            if (Config.GameTweaker.Enabled)
                SetReorderEnabled(Config.GameTweaker.ReorderPlayerSettings, Config.GameTweaker.AddOverrideLightIntensityOption);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set if the effect is enabled
        /// </summary>
        /// <param name="p_Enabled">New state</param>
        internal static void SetReorderEnabled(bool p_Enabled, bool p_AddOverrideLightsIntensityOption)
        {
            if (m_StaticLightsToggle == null || m_NoteJumpStartBeatOffsetDropdown == null || m_SfxVolumeSettingsController == null)
                return;

            m_StaticLightsToggle.transform.parent.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

            if (p_AddOverrideLightsIntensityOption && (m_OverrideLightsIntensityToggle == null || !m_OverrideLightsIntensityToggle))
            {
                var l_Creator = new BeatSaberMarkupLanguage.Tags.Settings.IncrementSettingTag();
                var l_Clone = l_Creator.CreateObject(m_StaticLightsToggle.transform.parent.parent);
                l_Clone.GetComponentInChildren<TextMeshProUGUI>().text = "Override lights intensity";
                (l_Clone.transform as RectTransform).offsetMax = new Vector2(90, (l_Clone.transform as RectTransform).offsetMax.y);

                if (m_StaticLightsToggle.transform.parent.Find("Icon"))
                {
                    var l_Icon = GameObject.Instantiate(m_StaticLightsToggle.transform.parent.Find("Icon"), l_Clone.transform);
                    l_Icon.transform.SetAsFirstSibling();
                }

                var l_LabelTemplateRectTransform = m_StaticLightsToggle.transform.parent.GetChild(1).transform as RectTransform;
                (l_Clone.transform.GetChild(1).transform as RectTransform).offsetMin = l_LabelTemplateRectTransform.offsetMin;
                (l_Clone.transform.GetChild(1).transform as RectTransform).offsetMax = l_LabelTemplateRectTransform.offsetMax;

                (l_Clone.transform.GetChild(2).transform as RectTransform).offsetMin = new Vector2(-36f, 0f);
                (l_Clone.transform.GetChild(2).transform as RectTransform).offsetMax = Vector2.zero;

                m_OverrideLightsIntensityToggle = l_Clone.GetComponent<IncrementSetting>();
                m_OverrideLightsIntensityToggle.minValue    = 0;
                m_OverrideLightsIntensityToggle.maxValue    = 20;
                m_OverrideLightsIntensityToggle.increments  = 0.1f;

                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(null, typeof(PPlayerSettingsPanelController).GetMethod(nameof(OnOverrideLightIntensityChange), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                SDK.UI.IncrementSetting.Setup(m_OverrideLightsIntensityToggle, l_Event, SDK.UI.BSMLSettingFormartter.Percentage, Config.GameTweaker.OverrideLightIntensity, false);
            }
            else if (!p_AddOverrideLightsIntensityOption && m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle)
            {
                GameObject.DestroyImmediate(m_OverrideLightsIntensityToggle.gameObject);
                m_OverrideLightsIntensityToggle = null;
            }

            if (p_Enabled)
            {
                m_PlayerHeightSettingsController.transform.SetAsFirstSibling();
                m_AutomaticPlayerHeightToggle.transform.parent.SetAsFirstSibling();
                m_SfxVolumeSettingsController.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle) m_OverrideLightsIntensityToggle.transform.SetAsFirstSibling();
                m_StaticLightsToggle.transform.parent.SetAsFirstSibling();
                m_NoteJumpStartBeatOffsetDropdown.transform.parent.SetAsFirstSibling();
            }
            else
            {
                m_NoteJumpStartBeatOffsetDropdown.transform.parent.SetAsFirstSibling();
                m_HideNoteSpawnEffectToggle.transform.parent.SetAsFirstSibling();
                m_AdaptiveSfxToggle.transform.parent.SetAsFirstSibling();
                m_ReduceDebrisToggle.transform.parent.SetAsFirstSibling();
                m_AdvanceHudToggle.transform.parent.SetAsFirstSibling();
                m_NoTextsAndHudsToggle.transform.parent.SetAsFirstSibling();
                m_SaberTrailIntensitySettingsController.transform.parent.SetAsFirstSibling();
                if (m_OverrideLightsIntensityToggle != null && m_OverrideLightsIntensityToggle) m_OverrideLightsIntensityToggle.transform.SetAsFirstSibling();
                m_StaticLightsToggle.transform.parent.SetAsFirstSibling();
                m_LeftHandedToggle.transform.parent.SetAsFirstSibling();
                m_SfxVolumeSettingsController.transform.parent.SetAsFirstSibling();
                m_PlayerHeightSettingsController.transform.SetAsFirstSibling();
                m_AutomaticPlayerHeightToggle.transform.parent.SetAsFirstSibling();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On OverrideLightIntensity setting changes
        /// </summary>
        /// <param name="p_Value">New value</param>
        private static void OnOverrideLightIntensityChange(object p_Value)
        {
            Config.GameTweaker.OverrideLightIntensity = (float)p_Value;
        }
    }
}
