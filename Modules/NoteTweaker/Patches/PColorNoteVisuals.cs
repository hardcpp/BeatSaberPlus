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
        /// <param name="____colorManager">ColorManager instance</param>
        /// <param name="____noteController">NoteController instance</param>
        /// <param name="____arrowMeshRenderers">MeshRenderer instance</param>
        /// <param name="____circleMeshRenderers">SpriteRenderer instance</param>
        internal static void Postfix(ref ColorManager ____colorManager,
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
                                l_CurrentBlock.materialPropertyBlock.SetColor(_colorId, l_Color.ColorWithAlpha(0.6f));
                                l_CurrentBlock.ApplyChanges();
                            }
                        }
                    }
                }
            }

            if (!m_Enabled)
                return;

            var l_CutDirection  = ____noteController.noteData.cutDirection;
            var l_DotEnabled    = l_CutDirection == NoteCutDirection.Any ? m_CircleEnabled : (m_CircleEnabled && m_CircleForceEnabled);

            var l_BaseColor = ____colorManager.ColorForType(l_ColorType);

            var l_ArrowColor = (m_OverrideArrowColors ? (l_ColorType == ColorType.ColorB ? m_RightArrowColor  : m_LeftArrowColor)  : l_BaseColor).ColorWithAlpha(m_ArrowAlpha);
            var l_DotColor   = (m_OverrideDotColors   ? (l_ColorType == ColorType.ColorB ? m_RightCircleColor : m_LeftCircleColor) : l_BaseColor).ColorWithAlpha(m_DotAlpha);

            if (m_BlockColorsEnabled)
                l_ArrowColor = (l_ColorType == ColorType.ColorA ? m_LeftBlockColor : m_RightBlockColor).ColorWithAlpha(0.6f);

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

            var l_CircleScale = l_CutDirection == NoteCutDirection.Any ? m_CircleScale : m_PrecisionCircleScale;
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
            m_Enabled               = NTConfig.Instance.Enabled;
            SetArrowScaleFromConfig();
            SetArrowColorsFromConfig();
            m_CircleEnabled         = NTConfig.Instance.Enabled ? NTConfig.Instance.DotAlpha != 0f       : true;
            m_CircleForceEnabled    = NTConfig.Instance.Enabled ? NTConfig.Instance.ShowDotsWithArrow    : false;
            SetDotScaleFromConfig();
            SetDotColorsFromConfig();
        }
        internal static void SetArrowScaleFromConfig()
        {
            m_ArrowScale        = (NTConfig.Instance.Enabled ? NTConfig.Instance.ArrowScale : 1.0f) * Vector3.one;
            m_ArrowGlowScale    = (NTConfig.Instance.Enabled ? NTConfig.Instance.ArrowScale : 1.0f) * new Vector3(0.6f, 0.3f, 0.6f);
        }
        internal static void SetArrowColorsFromConfig()
        {
            m_OverrideArrowColors   = NTConfig.Instance.Enabled ? NTConfig.Instance.OverrideDotColors : false;
            m_ArrowAlpha            = NTConfig.Instance.Enabled ? NTConfig.Instance.ArrowAlpha        : 0.6f;
            m_LeftArrowColor        = NTConfig.Instance.Enabled ? NTConfig.Instance.ArrowLColor       : new Color(0.659f, 0.125f, 0.125f, 1.000f);
            m_RightArrowColor       = NTConfig.Instance.Enabled ? NTConfig.Instance.ArrowRColor       : new Color(0.125f, 0.392f, 0.659f, 1.000f);
        }

        internal static void SetDotScaleFromConfig()
        {
            m_CircleScale           = (NTConfig.Instance.Enabled ? NTConfig.Instance.DotScale             : 1.0f) * new Vector3(0.5f, 0.5f, 0.5f);
            m_PrecisionCircleScale  = (NTConfig.Instance.Enabled ? NTConfig.Instance.PrecisionDotScale    : 1.0f) * new Vector3(0.5f, 0.5f, 0.5f);
        }
        internal static void SetDotColorsFromConfig()
        {
            m_OverrideDotColors = NTConfig.Instance.Enabled ? NTConfig.Instance.OverrideDotColors : false;
            m_DotAlpha          = NTConfig.Instance.Enabled ? NTConfig.Instance.DotAlpha          : 1f;
            m_LeftCircleColor   = NTConfig.Instance.Enabled ? NTConfig.Instance.DotLColor         : new Color(0.659f, 0.125f, 0.125f, 1.000f);
            m_RightCircleColor  = NTConfig.Instance.Enabled ? NTConfig.Instance.DotRColor         : new Color(0.125f, 0.392f, 0.659f, 1.000f);
        }

        public static void SetBlockColorOverride(bool p_Enabled, Color p_Left, Color p_Right)
        {
            m_BlockColorsEnabled    = p_Enabled;
            m_LeftBlockColor        = p_Left.ColorWithAlpha(1f);
            m_RightBlockColor       = p_Right.ColorWithAlpha(1f);
        }
    }
}
