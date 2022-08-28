using Newtonsoft.Json;
using UnityEngine;

namespace CP_SDK.Unity.Components
{
    /// <summary>
    /// Emitter instance component
    /// </summary>
    public class EnhancedImageParticleEmitter : MonoBehaviour
    {
        public class EmitterConfig
        {
            [JsonProperty] public bool Enabled = true;
            [JsonProperty] public string Name = "New emitter";

            [JsonProperty] public float Size = 1f;
            [JsonProperty] public float Speed = 1f;

            [JsonProperty] public float PosX = 0.00f;
            [JsonProperty] public float PosY = 11.00f;
            [JsonProperty] public float PosZ = 3.50f;
            [JsonProperty] public float RotX = 90.00f;
            [JsonProperty] public float RotY = 0.00f;
            [JsonProperty] public float RotZ = 0.00f;
            [JsonProperty] public float SizeX = 10.00f;
            [JsonProperty] public float SizeY = 1.25f;
            [JsonProperty] public float SizeZ = 2.00f;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Group
        /// </summary>
        internal EnhancedImageParticleEmitterGroup Group = null;
        /// <summary>
        /// Emitter config
        /// </summary>
        internal EmitterConfig Config = null;
        /// <summary>
        /// Particle system
        /// </summary>
        internal ParticleSystem PS = null;
        /// <summary>
        /// Particle system renderer
        /// </summary>
        internal ParticleSystemRenderer PSR = null;
        /// <summary>
        /// Preview material template
        /// </summary>
        internal Material PreviewMaterialTemplate = null;
        /// <summary>
        /// Life time
        /// </summary>
        internal float LifeTime = 0f;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Preview object
        /// </summary>
        private GameObject m_Preview = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component start
        /// </summary>
        internal void Awake()
        {
            PS    = gameObject.GetComponent<ParticleSystem>();
            PSR   = gameObject.GetComponent<ParticleSystemRenderer>();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update from emitter config
        /// </summary>
        internal void UpdateFromConfig()
        {
            if (Config == null)
                return;

            transform.localPosition = new Vector3(Config.PosX, Config.PosY, Config.PosZ);

            var l_Shape = PS.shape;
            l_Shape.rotation    = new Vector3(Config.RotX,     Config.RotY,   Config.RotZ);
            l_Shape.scale       = new Vector3(Config.SizeX,    Config.SizeY,  Config.SizeZ);

            /// Update preview if enabled
            if (m_Preview)
            {
                m_Preview.transform.localScale          = l_Shape.scale;
                m_Preview.transform.localEulerAngles    = l_Shape.rotation;
            }

            var l_Size  = Group.Manager.Size  * Config.Size;
            var l_Speed = Group.Manager.Speed * Config.Speed;

            var l_Main = PS.main;
            l_Main.startSize3D      = false;
            l_Main.startSize        = l_Size;
            l_Main.startSpeed       = l_Speed;
            l_Main.startLifetime    = Mathf.Min((8 / (Mathf.Max(l_Speed - 1, 0.01f))) + 1, 7f);

            LifeTime = l_Main.startLifetime.constant;
        }
        /// <summary>
        /// Set preview enabled
        /// </summary>
        /// <param name="p_Enabled">Enabled</param>
        /// <param name="p_Color">Color</param>
        internal void SetPreview(bool p_Enabled, Color p_Color)
        {
            if (!p_Enabled)
            {
                if (m_Preview)
                    GameObject.DestroyImmediate(m_Preview);

                var l_Emission = PS.emission;
                l_Emission.enabled = false;
                PS.Stop();

                m_Preview = null;
            }
            else
            {
                if (!m_Preview)
                {
                    var l_Shape = PS.shape;

                    m_Preview = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    m_Preview.transform.SetParent(transform, false);
                    m_Preview.transform.localPosition       = Vector3.zero;
                    m_Preview.transform.localScale          = l_Shape.scale;
                    m_Preview.transform.localEulerAngles    = l_Shape.rotation;

                    GameObject.DestroyImmediate(m_Preview.GetComponent<BoxCollider>());

                    var l_Material = new Material(PreviewMaterialTemplate);
                    m_Preview.GetComponent<MeshRenderer>().material = l_Material;
                }

                var l_Emission = PS.emission;
                l_Emission.enabled = true;

                float l_Factor = Mathf.Pow(2, -2);
                m_Preview.GetComponent<MeshRenderer>().sharedMaterial.color =
                    new Color(p_Color.r * l_Factor, p_Color.g * l_Factor, p_Color.b * l_Factor, 0.35f);

                PS.Play();
            }
        }
    }
}
