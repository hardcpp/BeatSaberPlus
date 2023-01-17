using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Emote particle system provider
    /// </summary>
    public static class EnhancedImageParticleSystemProvider
    {
        /// <summary>
        /// Template particle system
        /// </summary>
        private static GameObject m_ParticleSystem = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get particle system
        /// </summary>
        /// <returns></returns>
        public static GameObject GetParticleSystem()
        {
            if (!m_ParticleSystem)
                Create();

            return m_ParticleSystem;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create template
        /// </summary>
        private static void Create()
        {
            m_ParticleSystem = new GameObject("[CP_SDK.Unity.EnhancedImageParticleSystemProvider.ParticleSystem]");
            GameObject.DontDestroyOnLoad(m_ParticleSystem);

            var l_PS    = m_ParticleSystem.AddComponent<ParticleSystem>();
            var l_PSR   = m_ParticleSystem.GetComponent<ParticleSystemRenderer>();

            l_PS.Stop();

            var l_Main = l_PS.main;
            l_Main.duration             = 1.0f;
            l_Main.loop                 = true;
            l_Main.startDelay           = 0;
            l_Main.startLifetime        = 5;
            l_Main.startSpeed           = 3;
            l_Main.startSize            = 0.4f;
            l_Main.startColor           = Color.white;
            l_Main.gravityModifier      = 0f;
            l_Main.simulationSpace      = ParticleSystemSimulationSpace.World;
            l_Main.playOnAwake          = false;
            l_Main.emitterVelocityMode  = ParticleSystemEmitterVelocityMode.Transform;
            l_Main.maxParticles         = 200;
            l_Main.prewarm              = true;

            var l_Emission = l_PS.emission;
            l_Emission.enabled          = false;
            l_Emission.rateOverTime     = 1;
            l_Emission.rateOverDistance = 0;
            l_Emission.burstCount       = 1;
            l_Emission.SetBurst(0, new ParticleSystem.Burst()
            {
                time            = 0,
                count           = 1,
                cycleCount      = 1,
                repeatInterval  = 0.010f,
                probability     = 1f
            });

            var l_Shape = l_PS.shape;
            l_Shape.shapeType       = ParticleSystemShapeType.Box;
            l_Shape.position        = Vector3.zero;
            l_Shape.rotation        = new Vector3(90f, 0f, 0f);
            l_Shape.scale           = new Vector3(10f, 1.5f, 2f);
            l_Shape.angle           = 25f;
            l_Shape.length          = 5;
            l_Shape.boxThickness    = Vector3.zero;
            l_Shape.radiusThickness = 1f;

            var l_UVModule = l_PS.textureSheetAnimation;
            l_UVModule.enabled = false;

            var l_ColorOT = l_PS.colorOverLifetime;
            l_ColorOT.enabled = true;
            l_ColorOT.color = new ParticleSystem.MinMaxGradient(new Gradient()
            {
                alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey() { time = 0.00f, alpha = 0f},
                    new GradientAlphaKey() { time = 0.05f, alpha = 1f},
                    new GradientAlphaKey() { time = 0.75f, alpha = 1f},
                    new GradientAlphaKey() { time = 1.00f, alpha = 0f}
                },
                colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey() { time = 0.00f, color = Color.white },
                    new GradientColorKey() { time = 1.00f, color = Color.white }
                }
            });

            var l_TextureSheetAnimation = l_PS.textureSheetAnimation;
            l_TextureSheetAnimation.enabled     = false;
            l_TextureSheetAnimation.mode        = ParticleSystemAnimationMode.Sprites;
            l_TextureSheetAnimation.timeMode    = ParticleSystemAnimationTimeMode.Lifetime;

            l_PSR.renderMode = ParticleSystemRenderMode.VerticalBillboard;
            l_PSR.normalDirection   = 1f;
            l_PSR.material          = EnhancedImageParticleMaterialProvider.GetMaterial();
            l_PSR.minParticleSize   = 0.0f;
            l_PSR.maxParticleSize   = 0.5f;
            l_PSR.receiveShadows    = false;
            l_PSR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            l_PS.Play();
        }
    }
}
