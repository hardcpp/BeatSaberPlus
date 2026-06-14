using CP_SDK.Unity.Extensions;
using HarmonyLib;
using System;
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
        private static ColorManager m_ColorManager;

        private static bool     m_Enabled               = false;
        private static bool     m_BlockColorsEnabled    = false;
        private static Vector3  m_ArrowScale;
        private static Vector3  m_ArrowGlowScale;
        private static bool     m_OverrideArrowColors;
        private static float    m_ArrowAlpha;
        private static Color    m_LeftArrowColor;
        private static Color    m_RightArrowColor;
        private static bool     m_CircleEnabled;
        private static bool     m_CircleForceEnabled;
        private static Vector3  m_CircleScale;
        private static Vector3  m_BurstCircleScale;
        private static Vector3  m_PrecisionCircleScale;
        private static bool     m_OverrideDotColors;
        private static float    m_DotAlpha;
        private static Color    m_LeftCircleColor;
        private static Color    m_RightCircleColor;
        private static Color    m_LeftBlockColor;
        private static Color    m_RightBlockColor;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private class CachedColorNoteVisuals
        {
            public bool isBurstNote = false;

            public Transform[] arrowMeshRenderersTransforms = Array.Empty<Transform>();
            public Transform[] arrowGlowTransforms = Array.Empty<Transform>();
            public MaterialPropertyBlockController[] arrowGlowMaterialPropertyBlockControllers = Array.Empty<MaterialPropertyBlockController>();

            public Transform[] circleMeshRenderersTransforms = Array.Empty<Transform>();
            public MaterialPropertyBlockController[] circleMaterialPropertyBlockControllers = Array.Empty<MaterialPropertyBlockController>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static Dictionary<ColorNoteVisuals, CachedColorNoteVisuals> 
            _cache = new Dictionary<ColorNoteVisuals, CachedColorNoteVisuals>(100);

        private static List<MaterialPropertyBlockController>
            _componentsCache = new List<MaterialPropertyBlockController>(10);
        private static List<Transform> 
            _arrowGlowTransforms = new List<Transform>(10);
        private static List<MaterialPropertyBlockController> 
            _arrowGlowMaterialPropertyBlockControllers = new List<MaterialPropertyBlockController>(10);
        private static List<MaterialPropertyBlockController>
            _circleMaterialPropertyBlockControllers = new List<MaterialPropertyBlockController>(10);

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
                             ref MaterialPropertyBlockController[] ____materialPropertyBlockControllers)
        {
            // On the fly caching
            if (!_cache.TryGetValue(__instance, out var cached))
            {
                cached = new CachedColorNoteVisuals();
                cached.isBurstNote = (bool)__instance.GetComponent<BurstSliderGameNoteController>();

                // =====

                cached.arrowMeshRenderersTransforms = new Transform[____arrowMeshRenderers.Length];
                _arrowGlowTransforms.Clear();
                _arrowGlowMaterialPropertyBlockControllers.Clear();

                for (int i = 0; i < ____arrowMeshRenderers.Length; ++i)
                {
                    cached.arrowMeshRenderersTransforms[i] = ____arrowMeshRenderers[i].transform;

                    var glowTransform = ____arrowMeshRenderers[i].transform.parent.Find("NoteArrowGlow");
                    if (glowTransform)
                    {
                        _arrowGlowTransforms.Add(glowTransform);

                        _componentsCache.Clear();
                        glowTransform.GetComponents(_componentsCache);

                        if (_componentsCache.Count > 0)
                            _arrowGlowMaterialPropertyBlockControllers.AddRange(_componentsCache);
                    }
                }

                if (_arrowGlowTransforms.Count > 0)
                    cached.arrowGlowTransforms = _arrowGlowTransforms.ToArray();
                if (_arrowGlowMaterialPropertyBlockControllers.Count > 0)
                    cached.arrowGlowMaterialPropertyBlockControllers = _arrowGlowMaterialPropertyBlockControllers.ToArray();

                // =====

                cached.circleMeshRenderersTransforms = new Transform[____circleMeshRenderers.Length];
                _circleMaterialPropertyBlockControllers.Clear();
                for (int i = 0; i < ____circleMeshRenderers.Length; ++i)
                {
                    cached.circleMeshRenderersTransforms[i] = ____circleMeshRenderers[i].transform;

                    _componentsCache.Clear();
                    ____circleMeshRenderers[i].GetComponents(_componentsCache);

                    if (_componentsCache.Count > 0)
                        _circleMaterialPropertyBlockControllers.AddRange(_componentsCache);
                }

                if (_circleMaterialPropertyBlockControllers.Count > 0)
                    cached.circleMaterialPropertyBlockControllers = _circleMaterialPropertyBlockControllers.ToArray();

                // =====

                _cache.Add(__instance, cached);
            }

            var colorType = ____noteController.noteData.colorType;
            if (m_BlockColorsEnabled)
            {
                var blockColor = colorType == ColorType.ColorA ? m_LeftBlockColor : m_RightBlockColor;
                for (int i = 0; i < ____materialPropertyBlockControllers.Length; ++i)
                {
                    var block = ____materialPropertyBlockControllers[i];
                    block.materialPropertyBlock.SetColor(_colorId, blockColor);
                    block.ApplyChanges();
                }

                if (!m_Enabled)
                {
                    var newArrowGlowColor = ColorU.WithAlpha(blockColor, 0.6f);
                    for (int mbi = 0; mbi < cached.arrowGlowMaterialPropertyBlockControllers.Length; ++mbi)
                    {
                        var currentBlock = cached.arrowGlowMaterialPropertyBlockControllers[mbi];
                        currentBlock.materialPropertyBlock.SetColor(_colorId, newArrowGlowColor);
                        currentBlock.ApplyChanges();
                    }
                }
            }

            if (!m_Enabled)
                return;

            var cutDirection = ____noteController.noteData.cutDirection;
            var dotEnabled = cutDirection == NoteCutDirection.Any ? m_CircleEnabled : (m_CircleEnabled && m_CircleForceEnabled);
            var baseColor = ____colorManager.ColorForType(colorType);
            var isRight = colorType == ColorType.ColorB;

            // =====

            var arrowColor = ColorU.WithAlpha(m_OverrideArrowColors ? (isRight ? m_RightArrowColor : m_LeftArrowColor) : baseColor, m_ArrowAlpha);

            if (m_BlockColorsEnabled)
                arrowColor = ColorU.WithAlpha(isRight ? m_RightBlockColor : m_LeftBlockColor, 0.6f);

            for (int l_I = 0; l_I < cached.arrowMeshRenderersTransforms.Length; ++l_I)
                cached.arrowMeshRenderersTransforms[l_I].localScale = m_ArrowScale;

            for (int i = 0; i < cached.arrowGlowTransforms.Length; ++i)
                 cached.arrowGlowTransforms[i].localScale = m_ArrowGlowScale;

            for (int i = 0; i < cached.arrowGlowMaterialPropertyBlockControllers.Length; ++i)
            {
                var currentBlock = cached.arrowGlowMaterialPropertyBlockControllers[i];
                currentBlock.materialPropertyBlock.SetColor(_colorId, arrowColor);
                currentBlock.ApplyChanges();
            }

            // =====

            var dotColor = ColorU.WithAlpha(m_OverrideDotColors ? (isRight ? m_RightCircleColor : m_LeftCircleColor) : baseColor, m_DotAlpha);
            var circleScale = cached.isBurstNote ? m_BurstCircleScale : (cutDirection == NoteCutDirection.Any ? m_CircleScale : m_PrecisionCircleScale);

            for (int i = 0; i < cached.circleMeshRenderersTransforms.Length; ++i)
                cached.circleMeshRenderersTransforms[i].localScale = circleScale;

            for (int i = 0; i < ____circleMeshRenderers.Length; ++i)
                ____circleMeshRenderers[i].enabled = dotEnabled;
            
            for (int i = 0; i < cached.circleMaterialPropertyBlockControllers.Length; ++i)
            {
                var currentBlock = cached.circleMaterialPropertyBlockControllers[i];
                currentBlock.materialPropertyBlock.SetColor(_colorId, dotColor);
                currentBlock.ApplyChanges();
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

            if (p_OnSceneSwitch)
                _cache.Clear();
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static Color? GetColorForSaber(SaberType p_Type)
        {
            if (m_ColorManager != null)
                return m_ColorManager.ColorForSaberType(p_Type);

            return null;
        }
    }
}
