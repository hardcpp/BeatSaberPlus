using System.Reflection;
using UnityEngine;

namespace CP_SDK.Unity
{
    /// <summary>
    /// Particle material provider
    /// </summary>
    public static class EnhancedImageParticleMaterialProvider
    {
        /// <summary>
        /// Material
        /// </summary>
        private static Material m_Material = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get material
        /// </summary>
        /// <returns></returns>
        public static Material GetMaterial()
        {
            if (!m_Material)
            {
                switch (ChatPlexSDK.RenderPipeline)
                {
                    case ChatPlexSDK.ERenderPipeline.BuiltIn:
                        m_Material = CreateBuiltInMaterial();
                        break;

                    case ChatPlexSDK.ERenderPipeline.URP:
                        m_Material = CreateURPMaterial();
                        break;

                }
            }

            return m_Material;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create BuiltIn material
        /// </summary>
        /// <returns></returns>
        private static Material CreateBuiltInMaterial()
        {
            var l_Material = new Material(Shader.Find("Particles/Standard Unlit"));
            l_Material.EnableKeyword("ETC1_EXTERNAL_ALPHA");
            l_Material.EnableKeyword("_ALPHABLEND_ON");
            l_Material.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            l_Material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            l_Material.SetOverrideTag("RenderType", "Transparent");
            l_Material.SetFloat("_BlendOp",                         0.00f);
            l_Material.SetFloat("_BumpScale",                       1.00f);
            l_Material.SetFloat("_CameraFadingEnabled",             0.00f);
            l_Material.SetFloat("_CameraFarFadeDistance",           2.00f);
            l_Material.SetFloat("_CameraNearFadeDistance",          1.00f);
            l_Material.SetFloat("_ColorMode",                       0.00f);
            l_Material.SetFloat("_Cull",                            2.00f);
            l_Material.SetFloat("_Cutoff",                          0.50f);
            l_Material.SetFloat("_DetailNormalMapScale",            1.00f);
            l_Material.SetFloat("_DistortionBlend",                 0.50f);
            l_Material.SetFloat("_DistortionEnabled",               0.00f);
            l_Material.SetFloat("_DistortionStrength",              1.00f);
            l_Material.SetFloat("_DistortionStrengthScaled",        0.00f);
            l_Material.SetFloat("_DstBlend",                       10.00f);
            l_Material.SetFloat("_EmissionEnabled",                 0.00f);
            l_Material.SetFloat("_EnableExternalAlpha",             0.00f);
            l_Material.SetFloat("_FlipbookMode",                    0.00f);
            l_Material.SetFloat("_GlossMapScale",                   1.00f);
            l_Material.SetFloat("_Glossiness",                      1.00f);
            l_Material.SetFloat("_GlossyReflections",               0.00f);
            l_Material.SetFloat("_InvFade",                         1.15f);
            l_Material.SetFloat("_LightingEnabled",                 0.00f);
            l_Material.SetFloat("_Metallic",                        0.00f);
            l_Material.SetFloat("_Mode",                            2.00f);
            l_Material.SetFloat("_OcclusionStrength",               1.00f);
            l_Material.SetFloat("_Parallax",                        0.02f);
            l_Material.SetFloat("_SmoothnessTextureChannel",        0.00f);
            l_Material.SetFloat("_SoftParticlesEnabled",            0.00f);
            l_Material.SetFloat("_SoftParticlesFarFadeDistance",    1.00f);
            l_Material.SetFloat("_SoftParticlesNearFadeDistance",   0.00f);
            l_Material.SetFloat("_SrcBlend",                        5.00f);
            l_Material.SetFloat("_UVSec",                           0.00f);
            l_Material.SetFloat("_ZWrite",                          0.00f);
            l_Material.renderQueue      = 3000;
            l_Material.enableInstancing = true;

            l_Material.mainTexture = Texture2DU.CreateFromRaw(
                Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.Heart.png")
            );

            return l_Material;
        }
        /// <summary>
        /// Create URP Material
        /// </summary>
        /// <returns></returns>
        private static Material CreateURPMaterial()
        {
            /// Todo find for Audiotrip
            var l_Material = null as Material;// new Material(m_PreviewMateralAssetBundle.LoadAsset<Shader>("ATPParticlesUnlit"));
            l_Material.EnableKeyword("_ALPHATEST_ON");
            l_Material.EnableKeyword("_COLOROVERLAY_ON");
            l_Material.EnableKeyword("_RECEIVE_SHADOWS_OFF");
            l_Material.SetOverrideTag("RenderType", "TransparentCutout");
            l_Material.SetFloat("_AlphaClip",                       1.00f);
            l_Material.SetFloat("_Blend",                           0.00f);
            l_Material.SetFloat("_BlendOp",                         0.00f);
            l_Material.SetFloat("_BumpScale",                       1.00f);
            l_Material.SetFloat("_CameraFadingEnabled",             0.00f);
            l_Material.SetFloat("_CameraFarFadeDistance",           2.00f);
            l_Material.SetFloat("_CameraNearFadeDistance",          1.00f);
            l_Material.SetFloat("_ColorMode",                       3.00f);
            l_Material.SetFloat("_Cull",                            2.00f);
            l_Material.SetFloat("_Cutoff",                          0.50f);
            l_Material.SetFloat("_DistortionBlend",                 0.50f);
            l_Material.SetFloat("_DistortionEnabled",               0.00f);
            l_Material.SetFloat("_DistortionStrength",              1.00f);
            l_Material.SetFloat("_DistortionStrengthScaled",        0.10f);
            l_Material.SetFloat("_DstBlend",                        0.00f);
            l_Material.SetFloat("_FlipbookBlending",                0.00f);
            l_Material.SetFloat("_FlipbookMode",                    0.00f);
            l_Material.SetFloat("_Glossiness",                      0.00f);
            l_Material.SetFloat("_Metallic",                        0.00f);
            l_Material.SetFloat("_Mode",                            0.00f);
            l_Material.SetFloat("_QueueOffset",                     0.00f);
            l_Material.SetFloat("_ReceiveShadows",                  1.00f);
            l_Material.SetFloat("_Smoothness",                      0.50f);
            l_Material.SetFloat("_SoftParticlesEnabled",            0.00f);
            l_Material.SetFloat("_SoftParticlesFarFadeDistance",    1.00f);
            l_Material.SetFloat("_SoftParticlesNearFadeDistance",   0.00f);
            l_Material.SetFloat("_SrcBlend",                        1.00f);
            l_Material.SetFloat("_Surface",                         0.00f);
            l_Material.SetFloat("_ZWrite",                          1.00f);
            l_Material.SetColor("_EmissionColor",                   new Color(0, 0, 0, 0));
            l_Material.SetColor("_BaseColor",                       new Color(1, 1, 1, 1));
            l_Material.renderQueue      = 3000;
            l_Material.enableInstancing = true;

            l_Material.mainTexture = Texture2DU.CreateFromRaw(
                Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK._Resources.Heart.png")
            );

            return l_Material;
        }
    }
}
