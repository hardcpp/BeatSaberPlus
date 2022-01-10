using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatEmoteRain.Components
{
    /// <summary>
    /// Emitter group
    /// </summary>
    internal class EmitterGroup : MonoBehaviour
    {
        /// <summary>
        /// Is registered in Emitter group manager?
        /// </summary>
        internal bool Registered = false;
        /// <summary>
        /// Physics frames rolling counter
        /// </summary>
        internal int Roll;
        /// <summary>
        /// Scene for the group
        /// </summary>
        internal SDK.Game.Logic.SceneType Scene;
        /// <summary>
        /// Timeout in seconds
        /// </summary>
        internal float TimeOut { get; private set; } = 15.0f;
        /// <summary>
        /// Childs emitters
        /// </summary>
        internal EmitterInstance[] Emitters = new EmitterInstance[] { };

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
        private UInt32 m_EmitQueue;
        /// <summary>
        /// Timeout coroutine
        /// </summary>
        private Coroutine m_Coroutine;
        /// <summary>
        /// Is preview enabled
        /// </summary>
        private bool m_PreviewEnabled = false;
        /// <summary>
        /// Last focused emitter
        /// </summary>
        private CERConfig._Emitter m_PreviewFocus = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component awake
        /// </summary>
        internal void Awake()
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
        /// <param name="p_Emitters">Config emitters</param>
        /// <param name="p_ParticleSystemTemplate">Template</param>
        internal void Setup(List<CERConfig._Emitter> p_Emitters, GameObject p_ParticleSystemTemplate)
        {
            /// Clear childs
            while (transform.childCount > 0)
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);

            Emitters = new EmitterInstance[p_Emitters.Count];
            for (var l_I = 0; l_I < p_Emitters.Count; ++l_I)
            {
                var l_Instance = GameObject.Instantiate(p_ParticleSystemTemplate, transform).AddComponent<EmitterInstance>();
                l_Instance.Emitter                  = p_Emitters[l_I];
                l_Instance.PreviewMaterialTemplate  = m_PreviewMaterial;
                l_Instance.Awake();
                if (m_Material) l_Instance.PSR.material = m_Material;
                l_Instance.UpdateFromEmitter(Scene);

                Emitters[l_I] = l_Instance;
            }

            SetPreview(m_PreviewEnabled, m_PreviewFocus);
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

            m_Material.mainTexture = p_Texture;

            for (var l_I = 0; l_I < Emitters.Length; ++l_I)
            {
                Emitters[l_I].PSR.material            = m_Material;
                Emitters[l_I].PreviewMaterialTemplate = p_PreviewMaterial;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update from config
        /// </summary>
        internal void UpdateEmitters()
        {
            for (var l_I = 0; l_I < Emitters.Length; ++l_I)
                Emitters[l_I].UpdateFromEmitter(Scene);
        }
        /// <summary>
        /// Update texture
        /// </summary>
        /// <param name="p_Texture"></param>
        internal void UpdateTexture(Texture2D p_Texture)
        {
            m_Material.mainTexture = p_Texture;
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

                EmitterGroupManager.instance.Register(this);
            }
        }
        /// <summary>
        /// Stop the system group
        /// </summary>
        internal void Stop()
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            m_EmitQueue = 0;
        }
        /// <summary>
        /// Set preview enabled
        /// </summary>
        /// <param name="p_Enabled">Enabled</param>
        /// <param name="p_Focus">Emitter to focus</param>
        internal void SetPreview(bool p_Enabled, CERConfig._Emitter p_Focus)
        {
            m_PreviewEnabled    = p_Enabled;
            m_PreviewFocus      = p_Focus;

            for (var l_I = 0; l_I < Emitters.Length; ++l_I)
            {
                var l_Color = Emitters[l_I].Emitter.Enabled
                    ?
                        (Emitters[l_I].Emitter == p_Focus ? new Color(0.00f, 1.00f, 0.00f) : new Color(1.00f, 1.00f, 1.00f))
                    :
                        (Emitters[l_I].Emitter == p_Focus ? new Color(0.00f, 0.50f, 0.00f) : new Color(0.50f, 0.50f, 0.50f))
                    ;

                Emitters[l_I].SetPreview(p_Enabled, l_Color);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On physic frame
        /// </summary>
        /// <returns></returns>
        internal bool OnPhysicFrame()
        {
            if (m_EmitQueue <= 0)
            {
                m_Coroutine = StartCoroutine(Coroutine_TimingOut());
                return false;
            }
            else if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            m_EmitQueue--;

            for (var l_I = 0; l_I < Emitters.Length; ++l_I)
                Emitters[l_I].PS.Emit(1);

            return true;
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

            if (ChatEmoteRain.Instance != null)
                ChatEmoteRain.Instance.UnregisterGroup(this);
        }
    }
}
