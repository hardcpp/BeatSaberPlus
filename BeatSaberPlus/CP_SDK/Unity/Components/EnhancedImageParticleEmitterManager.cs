using System.Collections.Generic;
using UnityEngine;

namespace CP_SDK.Unity.Components
{
    /// <summary>
    /// Emitter instance manager
    /// </summary>
    public class EnhancedImageParticleEmitterManager : MonoBehaviour
    {
        /// <summary>
        /// Pool
        /// </summary>
        private Pool.ObjectPool<EnhancedImageParticleEmitterGroup> m_GroupPool;
        /// <summary>
        /// Preview group
        /// </summary>
        private EnhancedImageParticleEmitterGroup m_PreviewGroup = null;
        /// <summary>
        /// Update queue
        /// </summary>
        private List<EnhancedImageParticleEmitterGroup> m_UpdateQueue = new List<EnhancedImageParticleEmitterGroup>(100);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Base size
        /// </summary>
        public float Size = 0.4f;
        /// <summary>
        /// Base speed
        /// </summary>
        public float Speed = 3f;
        /// <summary>
        /// Physics frame delay
        /// </summary>
        public int Delay = 8;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Configure this manager
        /// </summary>
        /// <param name="p_PoolSize">Pool size</param>
        /// <param name="p_Configs">Emitters configs</param>
        /// <param name="p_PreviewMaterial">Preview material</param>
        /// <param name="p_Size">Base size</param>
        /// <param name="p_Speed">Base speed</param>
        /// <param name="p_Delay">Delay</param>
        public void Configure(int p_PoolSize, List<EnhancedImageParticleEmitter.EmitterConfig> p_Configs, Material p_PreviewMaterial, float p_Size, float p_Speed, int p_Delay)
        {
            Clear();

            if (m_GroupPool != null)
                m_GroupPool.Clear();

            Size    = p_Size;
            Speed   = p_Speed;
            Delay   = p_Delay;

            var l_Material = EnhancedImageParticleMaterialProvider.GetMaterial();

            m_GroupPool = new Pool.ObjectPool<EnhancedImageParticleEmitterGroup>(
                createFunc: () =>
                {
                    var l_Group = new GameObject("Group").AddComponent<EnhancedImageParticleEmitterGroup>();
                    l_Group.transform.SetParent(transform);
                    l_Group.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    l_Group.Manager = this;
                    l_Group.Setup(p_Configs, EnhancedImageParticleSystemProvider.GetParticleSystem());
                    l_Group.SetupMaterial(l_Material, EnhancedImageParticleMaterialProvider.GetMaterialTexture(l_Material), p_PreviewMaterial);
                    l_Group.gameObject.SetActive(false);

                    return l_Group;
                },
                actionOnGet: (x) =>
                {
                    x.gameObject.SetActive(true);
                },
                actionOnRelease: (x) =>
                {
                    x.StopAllCoroutines();
                    x.gameObject.SetActive(false);
                },
                actionOnDestroy: (x) =>
                {
                    GameObject.Destroy(x.gameObject);
                },
                collectionCheck: false,
                defaultCapacity: p_PoolSize,
                maxSize: p_PoolSize
            );

            if (m_PreviewGroup)
                GameObject.Destroy(m_PreviewGroup.gameObject);

            m_PreviewGroup = new GameObject("Group").AddComponent<EnhancedImageParticleEmitterGroup>();
            m_PreviewGroup.transform.SetParent(transform);
            m_PreviewGroup.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            m_PreviewGroup.Manager = this;
            m_PreviewGroup.Setup(p_Configs, EnhancedImageParticleSystemProvider.GetParticleSystem());
            m_PreviewGroup.SetupMaterial(l_Material, EnhancedImageParticleMaterialProvider.GetMaterialTexture(l_Material), p_PreviewMaterial);
            m_PreviewGroup.gameObject.SetActive(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Emit
        /// </summary>
        /// <param name="p_Image">Source image</param>
        /// <param name="p_Count">Count</param>
        public void Emit(EnhancedImage p_Image, uint p_Count)
        {
            var l_Group = GetOrCreateGroup(p_Image);
            if (!l_Group.Registered)
            {
                l_Group.Manager     = this;
                l_Group.Registered  = true;
                l_Group.Roll        = Delay;

                m_UpdateQueue.Add(l_Group);
            }

            l_Group.Emit(p_Count);
        }
        /// <summary>
        /// Set preview enabled
        /// </summary>
        /// <param name="p_Enabled">Is enabled?</param>
        /// <param name="p_Focus">Focussed emitter</param>
        public void SetPreview(bool p_Enabled, EnhancedImageParticleEmitter.EmitterConfig p_Focus)
        {
            m_PreviewGroup.gameObject.SetActive(p_Enabled);
            m_PreviewGroup.SetPreview(p_Enabled, p_Focus);
        }
        /// <summary>
        /// Update emitters from config
        /// </summary>
        public void UpdateFromConfig()
        {
            m_PreviewGroup.UpdateFromConfig();

            for (int l_I = 0; l_I < m_UpdateQueue.Count; ++l_I)
                m_UpdateQueue[l_I].UpdateFromConfig();
        }
        /// <summary>
        /// Clear all active emitters
        /// </summary>
        public void Clear()
        {
            for (int l_I = 0; l_I < m_UpdateQueue.Count; ++l_I)
            {
                var l_Group = m_UpdateQueue[l_I];
                l_Group.StopAllCoroutines();
                l_Group.Registered = false;
                l_Group.Clear();

                m_GroupPool.Release(l_Group);
            }

            m_UpdateQueue.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get or create emitter group
        /// </summary>
        /// <param name="p_Image">Target image</param>
        /// <returns></returns>
        private EnhancedImageParticleEmitterGroup GetOrCreateGroup(EnhancedImage p_Image)
        {
            for (int l_I = 0; l_I < m_UpdateQueue.Count; ++l_I)
            {
                if (m_UpdateQueue[l_I].TargetImage != p_Image)
                    continue;

                return m_UpdateQueue[l_I];
            }

            var l_NewGroup = m_GroupPool.Get();
            l_NewGroup.UpdateTargetImage(p_Image);

            return l_NewGroup;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On physic frame
        /// </summary>
        private void FixedUpdate()
        {
            for (int l_I = 0; l_I < m_UpdateQueue.Count; ++l_I)
            {
                var l_Group         = m_UpdateQueue[l_I];
                var l_ShouldDelete  = !l_Group.Registered;

                if (!l_ShouldDelete && --l_Group.Roll == 0)
                {
                    l_Group.OnPhysicFrame();
                    l_Group.Roll = Delay;
                }

                if (l_ShouldDelete)
                {
                    l_Group.StopAllCoroutines();
                    l_Group.Registered = false;

                    m_GroupPool.Release(l_Group);
                    m_UpdateQueue.RemoveAt(l_I);

                    l_I--;
                    continue;
                }
            }
        }
    }
}
