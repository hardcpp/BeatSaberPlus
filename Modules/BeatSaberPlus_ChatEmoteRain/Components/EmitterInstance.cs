using UnityEngine;

namespace BeatSaberPlus_ChatEmoteRain.Components
{
    /// <summary>
    /// Emitter instance component
    /// </summary>
    internal class EmitterInstance : MonoBehaviour
    {
        /// <summary>
        /// Emitter config
        /// </summary>
        internal CERConfig._Emitter Emitter;
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
        internal void UpdateFromEmitter(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            if (Emitter == null)
                return;

            transform.localPosition = new Vector3(Emitter.PosX, Emitter.PosY, Emitter.PosZ);

            var l_Shape = PS.shape;
            l_Shape.rotation    = new Vector3(Emitter.RotX,     Emitter.RotY,   Emitter.RotZ);
            l_Shape.scale       = new Vector3(Emitter.SizeX,    Emitter.SizeY,  Emitter.SizeZ);

            /// Update preview if enabled
            if (m_Preview)
            {
                m_Preview.transform.localScale          = l_Shape.scale;
                m_Preview.transform.localEulerAngles    = l_Shape.rotation;
            }

            var l_IsMenu    = p_Scene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu;
            var l_Size      = (l_IsMenu ? CERConfig.Instance.MenuSize  : CERConfig.Instance.SongSize)  * Emitter.Size;
            var l_Speed     = (l_IsMenu ? CERConfig.Instance.MenuSpeed : CERConfig.Instance.SongSpeed) * Emitter.Speed;

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
