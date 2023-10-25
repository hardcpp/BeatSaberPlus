using CP_SDK.Unity.Extensions;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus_NoteTweaker.Patches
{
    /// <summary>
    /// ColorNoteVisuals patch
    /// </summary>
    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch(nameof(ColorNoteVisuals.HandleNoteControllerDidInit))]
    public class PColorNoteVisuals : ColorNoteVisuals
    {
        private static bool m_Enabled = false;
        private static bool m_BlockColorsEnabled = false;
        private static Vector3 m_ArrowScale;
        private static Vector3 m_ArrowGlowScale;
        private static bool m_OverrideArrowColors;
        private static float m_ArrowAlpha;
        private static Color m_LeftArrowColor;
        private static Color m_RightArrowColor;
        private static bool m_CircleEnabled;
        private static bool m_CircleForceEnabled;
        private static Vector3 m_CircleScale;
        private static Vector3 m_BurstCircleScale;
        private static Vector3 m_PrecisionCircleScale;
        private static bool m_OverrideDotColors;
        private static float m_DotAlpha;
        private static Color m_LeftCircleColor;
        private static Color m_RightCircleColor;
        private static Color m_LeftBlockColor;
        private static Color m_RightBlockColor;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static List<MaterialPropertyBlockController> m_ComponentsCache = new List<MaterialPropertyBlockController>(10);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Prefix
        /// </summary>
        /// <param name="__instance">ColorVisual component instance</param>
        /// <param name="____colorManager">ColorManager instance</param>
        /// <param name="____noteController">NoteController instance</param>
        /// <param name="____arrowMeshRenderers">MeshRenderer instance</param>
        /// <param name="____circleMeshRenderers">SpriteRenderer instance</param>
        internal static void Postfix(ColorNoteVisuals __instance,
                                     ref ColorManager ____colorManager,
                                     ref NoteController ____noteController,
                                     ref MeshRenderer[] ____arrowMeshRenderers,
                                     ref MeshRenderer[] ____circleMeshRenderers,
                                     ref MaterialPropertyBlockController[]  ____materialPropertyBlockControllers)
        {
            var l_ColorType = ____noteController.noteData.colorType;

            if (m_BlockColorsEnabled)
            {
                var l_Color = l_ColorType == ColorType.ColorA ? m_LeftBlockColor : m_RightBlockColor;
                foreach (var l_Block in ____materialPropertyBlockControllers)
                {
                    l_Block.materialPropertyBlock.SetColor(_colorId, l_Color);
                    l_Block.ApplyChanges();
                }

                if (!m_Enabled)
                {
                    for (int l_I = 0; l_I < ____arrowMeshRenderers.Length; ++l_I)
                    {
                        var l_Glow = ____arrowMeshRenderers[l_I].transform.parent.Find("NoteArrowGlow");
                        if (l_Glow)
                        {
                            m_ComponentsCache.Clear();
                            l_Glow.GetComponents(m_ComponentsCache);
                            for (int l_MBI = 0; l_MBI < m_ComponentsCache.Count; ++l_MBI)
                            {
                                var l_CurrentBlock = m_ComponentsCache[l_MBI];
                                l_CurrentBlock.materialPropertyBlock.SetColor(_colorId, ColorU.WithAlpha(l_Color, 0.6f));
                                l_CurrentBlock.ApplyChanges();
                            }
                        }
                    }
                }
            }

            if (!m_Enabled)
                return;

            var l_IsBurstNote   = (bool)__instance.GetComponent<BurstSliderGameNoteController>();
            var l_CutDirection  = ____noteController.noteData.cutDirection;
            var l_DotEnabled    = l_CutDirection == NoteCutDirection.Any ? m_CircleEnabled : (m_CircleEnabled && m_CircleForceEnabled);

            var l_BaseColor = ____colorManager.ColorForType(l_ColorType);

            var l_ArrowColor = ColorU.WithAlpha(m_OverrideArrowColors ? (l_ColorType == ColorType.ColorB ? m_RightArrowColor  : m_LeftArrowColor)  : l_BaseColor, m_ArrowAlpha);
            var l_DotColor   = ColorU.WithAlpha(m_OverrideDotColors   ? (l_ColorType == ColorType.ColorB ? m_RightCircleColor : m_LeftCircleColor) : l_BaseColor, m_DotAlpha);

            if (m_BlockColorsEnabled)
                l_ArrowColor = ColorU.WithAlpha(l_ColorType == ColorType.ColorA ? m_LeftBlockColor : m_RightBlockColor, 0.6f);

            for (int l_I = 0; l_I < ____arrowMeshRenderers.Length; ++l_I)
            {
                var l_CurrentArrow = ____arrowMeshRenderers[l_I];
                l_CurrentArrow.gameObject.transform.localScale = m_ArrowScale;

                var l_Glow = l_CurrentArrow.transform.parent.Find("NoteArrowGlow");
                if (l_Glow)
                {
                    l_Glow.transform.localScale = m_ArrowGlowScale;

                    m_ComponentsCache.Clear();
                    l_Glow.GetComponents(m_ComponentsCache);
                    for (int l_MBI = 0; l_MBI < m_ComponentsCache.Count; ++l_MBI)
                    {
                        var l_CurrentBlock = m_ComponentsCache[l_MBI];
                        l_CurrentBlock.materialPropertyBlock.SetColor(_colorId, l_ArrowColor);
                        l_CurrentBlock.ApplyChanges();
                    }
                }
            }

            var l_CircleScale = l_IsBurstNote ? m_BurstCircleScale : (l_CutDirection == NoteCutDirection.Any ? m_CircleScale : m_PrecisionCircleScale);

            for (int l_I = 0; l_I < ____circleMeshRenderers.Length; ++l_I)
            {
                var l_CurrentRenderer = ____circleMeshRenderers[l_I];
                l_CurrentRenderer.enabled                            = l_DotEnabled;
                l_CurrentRenderer.gameObject.transform.localScale    = l_CircleScale;
                l_CurrentRenderer.material.color                     = l_DotColor;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set from configuration
        /// </summary>
        internal static void SetFromConfig(bool p_OnSceneSwitch)
        {
            var l_Profile = NTConfig.Instance.GetActiveProfile();

            m_Enabled               = NTConfig.Instance.Enabled;
            SetArrowScaleFromConfig(l_Profile);
            SetArrowColorsFromConfig(l_Profile);
            m_CircleEnabled         = NTConfig.Instance.Enabled ? l_Profile.DotsIntensity != 0f     : true;
            m_CircleForceEnabled    = NTConfig.Instance.Enabled ? l_Profile.NotesShowPrecisonDots   : false;
            SetDotScaleFromConfig(l_Profile);
            SetDotColorsFromConfig(l_Profile);
        }
        public static void SetBlockColorOverride(bool p_Enabled, Color p_Left, Color p_Right)
        {
            m_BlockColorsEnabled    = p_Enabled;
            m_LeftBlockColor        = ColorU.WithAlpha(p_Left, 1f);
            m_RightBlockColor       = ColorU.WithAlpha(p_Right, 1f);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static void SetArrowScaleFromConfig(NTConfig._Profile p_Profile)
        {
            m_ArrowScale        = (NTConfig.Instance.Enabled ? p_Profile.ArrowsScale : 1.0f) * Vector3.one;
            m_ArrowGlowScale    = (NTConfig.Instance.Enabled ? p_Profile.ArrowsScale : 1.0f) * new Vector3(0.6f, 0.3f, 0.6f);
        }
        internal static void SetArrowColorsFromConfig(NTConfig._Profile p_Profile)
        {
            m_OverrideArrowColors   = NTConfig.Instance.Enabled ? p_Profile.ArrowsOverrideColors    : false;
            m_ArrowAlpha            = NTConfig.Instance.Enabled ? p_Profile.ArrowsIntensity         : 0.6f;
            m_LeftArrowColor        = NTConfig.Instance.Enabled ? p_Profile.ArrowsLColor            : new Color(0.659f, 0.125f, 0.125f, 1.000f);
            m_RightArrowColor       = NTConfig.Instance.Enabled ? p_Profile.ArrowsRColor            : new Color(0.125f, 0.392f, 0.659f, 1.000f);
        }

        internal static void SetDotScaleFromConfig(NTConfig._Profile p_Profile)
        {
            m_CircleScale           = (NTConfig.Instance.Enabled ? p_Profile.DotsScale              : 1.0f) * new Vector3(0.5f, 0.5f, 0.5f);
            m_BurstCircleScale      = (NTConfig.Instance.Enabled ? p_Profile.BurstNotesDotsScale    : 1.0f) * new Vector3(0.1f, 0.1f, 0.1f);
            m_PrecisionCircleScale  = (NTConfig.Instance.Enabled ? p_Profile.NotesPrecisonDotsScale : 1.0f) * new Vector3(0.5f, 0.5f, 0.5f);
        }
        internal static void SetDotColorsFromConfig(NTConfig._Profile p_Profile)
        {
            m_OverrideDotColors = NTConfig.Instance.Enabled ? p_Profile.DotsOverrideColors  : false;
            m_DotAlpha          = NTConfig.Instance.Enabled ? p_Profile.DotsIntensity       : 1f;
            m_LeftCircleColor   = NTConfig.Instance.Enabled ? p_Profile.DotsLColor          : new Color(0.659f, 0.125f, 0.125f, 1.000f);
            m_RightCircleColor  = NTConfig.Instance.Enabled ? p_Profile.DotsRColor          : new Color(0.125f, 0.392f, 0.659f, 1.000f);
        }
    }
}
