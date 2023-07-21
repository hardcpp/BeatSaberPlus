using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.Unity.Components
{
    /// <summary>
    /// Emitter group
    /// </summary>
    public class EnhancedImageParticleEmitterGroup : MonoBehaviour
    {
        /// <summary>
        /// Manager instance
        /// </summary>
        internal EnhancedImageParticleEmitterManager Manager;
        /// <summary>
        /// Is registered in Emitter group manager?
        /// </summary>
        internal bool Registered = false;
        /// <summary>
        /// Physics frames rolling counter
        /// </summary>
        internal int Roll;
        /// <summary>
        /// Target image
        /// </summary>
        internal EnhancedImage TargetImage;
        /// <summary>
        /// Timeout in seconds
        /// </summary>
        internal float TimeOut { get; private set; } = 15.0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Particle material
        /// </summary>
        private Material m_Material = null;
        /// <summary>
        /// Preview material
        /// </summary>
        private Material m_PreviewMaterial = null;
        /// <summary>
        /// Emission queue count
        /// </summary>
        private uint m_EmitQueue = 0;
        /// <summary>
        /// Timeout coroutine
        /// </summary>
        private Coroutine m_Coroutine = null;
        /// <summary>
        /// Is preview enabled
        /// </summary>
        private bool m_PreviewEnabled = false;
        /// <summary>
        /// Last focused emitter
        /// </summary>
        private EnhancedImageParticleEmitter.EmitterConfig m_FocussedConfig = null;
        /// <summary>
        /// Childs emitters
        /// </summary>
        private EnhancedImageParticleEmitter[] m_Emitters = new EnhancedImageParticleEmitter[] { };

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component awake
        /// </summary>
        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);

            transform.localPosition = Vector3.zero;
            transform.localScale    = Vector3.one;
            transform.localRotation = Quaternion.identity;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup from config
        /// </summary>
        /// <param name="p_Configs">Config emitters</param>
        /// <param name="p_ParticleSystemTemplate">Template</param>
        internal void Setup(List<EnhancedImageParticleEmitter.EmitterConfig> p_Configs, GameObject p_ParticleSystemTemplate)
        {
            /// Clear childs
            while (transform.childCount > 0)
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);

            for (var l_I = 0; l_I < p_Configs.Count; ++l_I)
            {
                if (!p_Configs[l_I].Enabled)
                    continue;

                var l_Instance = GameObject.Instantiate(p_ParticleSystemTemplate, transform).AddComponent<EnhancedImageParticleEmitter>();
                l_Instance.Group                    = this;
                l_Instance.Config                   = p_Configs[l_I];
                l_Instance.PreviewMaterialTemplate  = m_PreviewMaterial;
                l_Instance.Awake();
                if (m_Material) l_Instance.PSR.material = m_Material;
                l_Instance.UpdateFromConfig();
            }

            m_Emitters = GetComponentsInChildren<EnhancedImageParticleEmitter>();

            SetPreview(m_PreviewEnabled, m_FocussedConfig);
        }
        /// <summary>
        /// Setup material
        /// </summary>
        /// <param name="p_TemplateMaterial">Base material for cloning</param>
        /// <param name="p_Texture">Particle texture</param>
        /// <param name="p_PreviewMaterial">Material for preview</param>
        internal void SetupMaterial(Material p_TemplateMaterial, Texture p_Texture, Material p_PreviewMaterial)
        {
            if (!m_Material)
                m_Material = new Material(p_TemplateMaterial);

            m_PreviewMaterial = p_PreviewMaterial;

            EnhancedImageParticleMaterialProvider.SetMaterialTexture(m_Material, p_Texture);

            for (var l_I = 0; l_I < m_Emitters.Length; ++l_I)
            {
                m_Emitters[l_I].PSR.material            = m_Material;
                m_Emitters[l_I].PreviewMaterialTemplate = p_PreviewMaterial;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update from config
        /// </summary>
        internal void UpdateFromConfig()
        {
            for (var l_I = 0; l_I < m_Emitters.Length; ++l_I)
                m_Emitters[l_I].UpdateFromConfig();
        }
        /// <summary>
        /// Update target image
        /// </summary>
        /// <param name="p_TargetImage">New target image</param>
        internal void UpdateTargetImage(EnhancedImage p_TargetImage)
        {
            TargetImage = p_TargetImage;

            var l_AspectRatio = (float)p_TargetImage.Width / (float)p_TargetImage.Height;

            /// Sorta working animated emotes
            if (p_TargetImage.AnimControllerData != null)
            {
                for (int l_EmitterI = 0; l_EmitterI < m_Emitters.Length; ++l_EmitterI)
                {
                    var l_Current               = m_Emitters[l_EmitterI];
                    var l_TextureSheetAnimation = l_Current.PS.textureSheetAnimation;

                    l_Current.UpdateFromConfig();

                    /// Clear old sprites
                    while (l_TextureSheetAnimation.spriteCount > 0)
                        l_TextureSheetAnimation.RemoveSprite(0);

                    var l_SpriteCount   = p_TargetImage.AnimControllerData.Frames.Length;
                    var l_TimeForEmote  = 0.0f;
                    for (int l_I = 0; l_I < l_SpriteCount; ++l_I)
                    {
                        l_TextureSheetAnimation.AddSprite(p_TargetImage.AnimControllerData.Frames[l_I]);
                        l_TimeForEmote += p_TargetImage.AnimControllerData.Delays[l_I];
                    }

                    var l_AnimationCurve        = new AnimationCurve();
                    var l_TimeAccumulator       = 0.0f;
                    var l_SingleFramePercentage = 1.0f / (float)l_SpriteCount;
                    for (int l_FrameI = 0; l_FrameI < l_SpriteCount; ++l_FrameI)
                    {
                        l_AnimationCurve.AddKey(l_TimeAccumulator / l_TimeForEmote, ((float)l_FrameI) * l_SingleFramePercentage);
                        l_TimeAccumulator += p_TargetImage.AnimControllerData.Delays[l_FrameI];
                    }
                    l_AnimationCurve.AddKey(1.0f, 1.0f);

                    l_TextureSheetAnimation.enabled         = true;
                    l_TextureSheetAnimation.frameOverTime   = new ParticleSystem.MinMaxCurve(1f, l_AnimationCurve);

                    var l_CycleCount = (int)Mathf.Max(1.0f, (l_Current.LifeTime * 1000f) / l_TimeForEmote);
                    l_TextureSheetAnimation.cycleCount = l_CycleCount;

                    var l_PSMain = l_Current.PS.main;
                    l_PSMain.startLifetime = l_CycleCount * (l_TimeForEmote / 1000f);

                    /// Wide emote support
                    if (Mathf.Abs(1.0f - l_AspectRatio) > 0.1f)
                    {
                        var l_StartSize3D = new Vector3(
                                l_PSMain.startSize.constant * l_AspectRatio,
                                l_PSMain.startSize.constant,
                                l_PSMain.startSize.constant
                            );
                        l_PSMain.startSize3D = true;
                        l_PSMain.startSizeXMultiplier = l_StartSize3D.x;
                        l_PSMain.startSizeYMultiplier = l_StartSize3D.y;
                        l_PSMain.startSizeZMultiplier = l_StartSize3D.z;
                    }
                }
            }
            else
            {
                for (int l_EmitterI = 0; l_EmitterI < m_Emitters.Length; ++l_EmitterI)
                {
                    var l_Current = m_Emitters[l_EmitterI];

                    l_Current.UpdateFromConfig();

                    var l_TextureSheetAnimation = l_Current.PS.textureSheetAnimation;
                    l_TextureSheetAnimation.enabled = false;

                    var l_PSMain = l_Current.PS.main;
                    l_PSMain.startLifetime = l_Current.LifeTime;

                    /// Wide emote support
                    if (Mathf.Abs(1.0f - l_AspectRatio) > 0.1f)
                    {
                        var l_StartSize3D = new Vector3(
                                l_PSMain.startSize.constant * l_AspectRatio,
                                l_PSMain.startSize.constant,
                                l_PSMain.startSize.constant
                            );
                        l_PSMain.startSize3D = true;
                        l_PSMain.startSizeXMultiplier = l_StartSize3D.x;
                        l_PSMain.startSizeYMultiplier = l_StartSize3D.y;
                        l_PSMain.startSizeZMultiplier = l_StartSize3D.z;
                    }
                }
            }

            EnhancedImageParticleMaterialProvider.SetMaterialTexture(m_Material, p_TargetImage.Sprite.texture);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Emit specific amount of particles
        /// </summary>
        /// <param name="p_Amount">Amount to emit</param>
        internal void Emit(UInt32 p_Amount)
        {
            if (p_Amount <= 0)
                return;

            m_EmitQueue += p_Amount;

            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }
        }
        /// <summary>
        /// Clear all emotes queued
        /// </summary>
        internal void Clear()
        {
            m_EmitQueue = 0;

            for (var l_I = 0; l_I < m_Emitters.Length; ++l_I)
                m_Emitters[l_I].PS.Clear();
        }
        /// <summary>
        /// Set preview enabled
        /// </summary>
        /// <param name="p_Enabled">Enabled</param>
        /// <param name="p_FocusedConfig">Emitter to focus</param>
        internal void SetPreview(bool p_Enabled, EnhancedImageParticleEmitter.EmitterConfig p_FocusedConfig)
        {
            m_PreviewEnabled    = p_Enabled;
            m_FocussedConfig      = p_FocusedConfig;

            for (var l_I = 0; l_I < m_Emitters.Length; ++l_I)
            {
                var l_Color = m_Emitters[l_I].Config.Enabled
                    ?
                        (m_Emitters[l_I].Config == p_FocusedConfig ? new Color(0.00f, 1.00f, 0.00f) : new Color(1.00f, 1.00f, 1.00f))
                    :
                        (m_Emitters[l_I].Config == p_FocusedConfig ? new Color(0.00f, 0.50f, 0.00f) : new Color(0.50f, 0.50f, 0.50f))
                    ;

                m_Emitters[l_I].SetPreview(p_Enabled, l_Color);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On physic frame
        /// </summary>
        /// <returns></returns>
        internal void OnPhysicFrame()
        {
            if (m_EmitQueue <= 0)
            {
                if (m_Coroutine == null)
                    m_Coroutine = StartCoroutine(Coroutine_TimingOut());

                return;
            }
            else if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            m_EmitQueue--;

            for (var l_I = 0; l_I < m_Emitters.Length; ++l_I)
                m_Emitters[l_I].PS.Emit(1);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Timing out coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator Coroutine_TimingOut()
        {
            yield return new WaitForSeconds(TimeOut);
            m_Coroutine = null;

            Registered = false;
        }
    }
}
