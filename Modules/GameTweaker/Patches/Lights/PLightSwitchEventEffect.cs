using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.Modules.GameTweaker.Patches.Lights
{
    /// <summary>
    /// LightSwitchEventEffect patch
    /// </summary>
    [HarmonyPatch]
    public class PLightSwitchEventEffect : LightSwitchEventEffect
    {
        /// <summary>
        /// Light init data
        /// </summary>
        private class InitialData
        {
            internal bool       StartLightOnStart;
            internal ColorSO    StartLightColor0;
            internal float      StartOffColorIntensity;
            internal float      StartHighlightValue;
            internal Color      StartAfterHighlightColor;
            internal Color      StartHighlightColor;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init data cache
        /// </summary>
        private static Dictionary<LightSwitchEventEffect, InitialData> m_InitialData = new Dictionary<LightSwitchEventEffect, InitialData>();
        /// <summary>
        /// Intensity cache
        /// </summary>
        private static float m_Intensity = 1.0f;
        /// <summary>
        /// Is faux static light
        /// </summary>
        private static bool m_FauxStaticLight = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Faux static light accessor
        /// </summary>
        internal static bool FauxStaticLight => m_FauxStaticLight;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix Start
        /// </summary>
        /// <param name="__instance">LightSwitchEventEffect instance</param>
//         [HarmonyPatch(typeof(LightSwitchEventEffect))]
//         [HarmonyPatch(nameof(LightSwitchEventEffect.Start))]
//         [HarmonyPrefix]
//         internal static void Prefix_Start(  ref LightSwitchEventEffect __instance,
//                                             ref bool ____lightOnStart,          ref ColorSO ____lightColor0,        ref float ____offColorIntensity,
//                                             ref float ____highlightValue,       ref Color ____afterHighlightColor,  ref Color ____highlightColor)
//         {
//             if (m_InitialData.ContainsKey(__instance))
//                 m_InitialData.Remove(__instance);
//
//             m_InitialData.Add(__instance, new InitialData()
//             {
//                 StartLightOnStart           = ____lightOnStart,
//                 StartLightColor0            = ____lightColor0,
//                 StartOffColorIntensity      = ____offColorIntensity,
//                 StartHighlightValue         = ____highlightValue,
//                 StartAfterHighlightColor    = ____afterHighlightColor,
//                 StartHighlightColor         = ____highlightColor
//             });
//         }
        /// <summary>
        /// Prefix OnDestroy
        /// </summary>
        /// <param name="__instance">LightSwitchEventEffect instance</param>
//         [HarmonyPatch(typeof(LightSwitchEventEffect))]
//         [HarmonyPatch(nameof(LightSwitchEventEffect.OnDestroy))]
//         [HarmonyPrefix]
//         internal static void Prefix_OnDestroy(ref LightSwitchEventEffect __instance)
//         {
//             if (m_InitialData.ContainsKey(__instance))
//                 m_InitialData.Remove(__instance);
//         }
        /// <summary>
        /// Prefix HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
        /// </summary>
//         [HarmonyPatch(typeof(LightSwitchEventEffect))]
//         [HarmonyPatch(nameof(LightSwitchEventEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger))]
//         [HarmonyPrefix]
//         internal static bool Prefix_HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger(BeatmapEventData beatmapEventData)
//         {
//             if (m_FauxStaticLight)
//             {
//                 /// Static light event
//                 if (beatmapEventData.value == 1 && (beatmapEventData.type == BeatmapEventType.Event0 || beatmapEventData.type == BeatmapEventType.Event4))
//                     return true; ///< Fallback to original method
//
//                 /// Skip original method
//                 return false;
//             }
//
//             /// Fallback to original method
//             return true;
//         }
        /// <summary>
        /// Prefix SetColor
        /// </summary>
        /// <param name="color">Input color</param>
        [HarmonyPatch(typeof(LightSwitchEventEffect))]
        [HarmonyPatch(nameof(LightSwitchEventEffect.SetColor))]
        [HarmonyPriority(Priority.High)]
        [HarmonyPrefix]
        internal static void Prefix_SetColor(ref UnityEngine.Color color)
        {
            if (m_Intensity == 1f)
                return;

            float l_Alpha = color.a;

            UnityEngine.Color.RGBToHSV(color, out var l_H, out var l_S, out var l_V);
            l_V *= m_Intensity;
            color = UnityEngine.Color.HSVToRGB(l_H, l_S, l_V);

            color.a = l_Alpha;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        internal static void SetFromConfig()
        {
            m_Intensity         = (Config.GameTweaker.Enabled && Config.GameTweaker.AddOverrideLightIntensityOption) ? Config.GameTweaker.OverrideLightIntensity : 1.0f;
            m_FauxStaticLight   = false;//Config.GameTweaker.Enabled && ((SDK.Game.Logic.LevelData?.Data?.playerSpecificSettings?.environmentEffectsFilterPreset ?? EnvironmentEffectsFilterPreset.AllEffects) == EnvironmentEffectsFilterPreset.NoEffects);
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_Intensity">New intensity</param>
        internal static void SetTempLightIntensity(float p_Intensity)
        {
            m_Intensity = p_Intensity;
        }
        /// <summary>
        /// Set faux static light
        /// </summary>
        /// <param name="p_Enabled">Is faux static light enabled?</param>
        internal static void SetTempFauxStaticLight(bool p_Enabled)
        {
            if (m_FauxStaticLight == p_Enabled)
                return;

            if (!m_FauxStaticLight && p_Enabled)
            {
                var l_EventA = new BeatmapEventData(0.0f, BeatmapEventType.Event0, 1, 1);
                var l_EventB = new BeatmapEventData(0.0f, BeatmapEventType.Event4, 1, 1);

                foreach (var l_KVP in m_InitialData)
                {
                    var l_Light = l_KVP.Key;
                    var l_Data  = l_KVP.Value;

                    if (l_Light == null || !l_Light)
                        continue;

                    l_Light.SetField<LightSwitchEventEffect, bool>(     "_lightOnStart",        l_Data.StartLightOnStart);
                    l_Light.SetField<LightSwitchEventEffect, ColorSO>(  "_lightColor0",         l_Data.StartLightColor0);
                    l_Light.SetField<LightSwitchEventEffect, float>(    "_offColorIntensity",   l_Data.StartOffColorIntensity);
                    l_Light.SetField<LightSwitchEventEffect, float>(    "_highlightValue",      l_Data.StartHighlightValue);
                    l_Light.SetField<LightSwitchEventEffect, Color>(    "_afterHighlightColor", l_Data.StartAfterHighlightColor);
                    l_Light.SetField<LightSwitchEventEffect, Color>(    "_highlightColor",      l_Data.StartHighlightColor);
                    l_Light.SetField<LightSwitchEventEffect, bool>(     "_initialized",         false);

                    /// Force update to restore initial state
                    //l_Light.Update();

                    /// Ingest static light initial event
                    l_Light.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger(l_EventA);
                    l_Light.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger(l_EventB);
                }

                m_FauxStaticLight = true;
            }
            else if (m_FauxStaticLight && !p_Enabled)
            {
                foreach (var l_KVP in m_InitialData)
                {
                    var l_Light = l_KVP.Key;
                    var l_Data  = l_KVP.Value;

                    if (l_Light == null || !l_Light)
                        continue;

                    l_Light.SetField<LightSwitchEventEffect, bool>(     "_lightOnStart",        l_Data.StartLightOnStart);
                    l_Light.SetField<LightSwitchEventEffect, ColorSO>(  "_lightColor0",         l_Data.StartLightColor0);
                    l_Light.SetField<LightSwitchEventEffect, float>(    "_offColorIntensity",   l_Data.StartOffColorIntensity);
                    l_Light.SetField<LightSwitchEventEffect, float>(    "_highlightValue",      l_Data.StartHighlightValue);
                    l_Light.SetField<LightSwitchEventEffect, Color>(    "_afterHighlightColor", l_Data.StartAfterHighlightColor);
                    l_Light.SetField<LightSwitchEventEffect, Color>(    "_highlightColor",      l_Data.StartHighlightColor);
                    l_Light.SetField<LightSwitchEventEffect, bool>(     "_initialized",         false);

                    /// Force update to restore initial state
                    //l_Light.Update();
                }

                m_FauxStaticLight = false;
            }
        }
    }
}
