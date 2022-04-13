using HarmonyLib;
using UnityEngine;

namespace BeatSaberPlus_GameTweaker.Patches.Lights
{
    /// <summary>
    /// LightSwitchEventEffect patch
    /// </summary>
    [HarmonyPatch]
    public class PLightsPatches : LightWithIdManager
    {
        /// <summary>
        /// Intensity cache
        /// </summary>
        private static float m_Intensity = 1.0f;
        /// <summary>
        /// Is current scene valid
        /// </summary>
        private static bool m_SceneIsValid = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds))]
        [HarmonyPatch(nameof(BloomPrePassBackgroundColorsGradientTintColorWithLightIds.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorA(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(BloomPrePassBackgroundColorsGradientElementWithLightId))]
        [HarmonyPatch(nameof(BloomPrePassBackgroundColorsGradientElementWithLightId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorB(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(BloomPrePassBackgroundLightWithId))]
        [HarmonyPatch(nameof(BloomPrePassBackgroundLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorC(ref UnityEngine.Color newColor) => ImplSetColor(ref newColor);
        [HarmonyPatch(typeof(DirectionalLightWithId))]
        [HarmonyPatch(nameof(DirectionalLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorD(ref UnityEngine.Color color) => ImplSetColor(ref color);

        /// EnableRendererWithLightId

        [HarmonyPatch(typeof(InstancedMaterialLightWithId))]
        [HarmonyPatch(nameof(InstancedMaterialLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorF(ref UnityEngine.Color newColor) => ImplSetColor(ref newColor);
        [HarmonyPatch(typeof(MaterialLightWithId))]
        [HarmonyPatch(nameof(MaterialLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorG(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(ParticleSystemLightWithId))]
        [HarmonyPatch(nameof(ParticleSystemLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorH(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(SpriteLightWithId))]
        [HarmonyPatch(nameof(SpriteLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorI(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(TubeBloomPrePassLightWithId))]
        [HarmonyPatch(nameof(TubeBloomPrePassLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorJ(ref UnityEngine.Color color) => ImplSetColor(ref color);
        [HarmonyPatch(typeof(UnityLightWithId))]
        [HarmonyPatch(nameof(UnityLightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorK(ref UnityEngine.Color color) => ImplSetColor(ref color);

        [HarmonyPatch(typeof(LightWithIds.LightWithId))]
        [HarmonyPatch(nameof(LightWithIds.LightWithId.ColorWasSet))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorL(ref Color newColor) => ImplSetColor(ref newColor);
        [HarmonyPatch(typeof(LightmapLightWithIds))]
        [HarmonyPatch(nameof(LightmapLightWithIds.SetDataToShaders))]
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        internal static void Prefix_SetColorM(ref Color lightmapColor, ref Color probeColor)
        {
            ImplSetColor(ref lightmapColor);
            ImplSetColor(ref probeColor);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static void ImplSetColor(ref UnityEngine.Color color)
        {
            if (!m_SceneIsValid || m_Intensity == 1f)
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
        public static void SetFromConfig()
        {
            m_Intensity = (GTConfig.Instance.Enabled && GTConfig.Instance.PlayerOptions.OverrideLightIntensityOption) ? GTConfig.Instance.PlayerOptions.OverrideLightIntensity : 1.0f;
        }
        /// <summary>
        /// Set temp config
        /// </summary>
        /// <param name="p_Intensity">New intensity</param>
        public static void SetTempLightIntensity(float p_Intensity)
        {
            m_Intensity = p_Intensity;
        }

        internal static void SetIsValidScene(bool p_IsValid) => m_SceneIsValid = p_IsValid;
    }
}
