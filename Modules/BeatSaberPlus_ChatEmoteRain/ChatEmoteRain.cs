using BeatSaberMarkupLanguage;
using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatEmoteRain
{
    /// <summary>
    /// Chat Emote Rain instance
    /// </summary>
    public class ChatEmoteRain : BeatSaberPlus.SDK.ModuleBase<ChatEmoteRain>
    {
        /// <summary>
        /// No emote SUBRAIN default ID
        /// </summary>
        private static string s_SUBRAIN_NO_EMOTE = "_BSPSubRain_$DEFAULT$_";
        /// <summary>
        /// Warm-up size per scene
        /// </summary>
        private static int POOL_SIZE_PER_SCENE = 50;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module type
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseType Type => BeatSaberPlus.SDK.IModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Chat Emote Rain";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Make chat emotes rain in game!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => true;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => CERConfig.Instance.Enabled; set { CERConfig.Instance.Enabled = value; CERConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override BeatSaberPlus.SDK.IModuleBaseActivationType ActivationType => BeatSaberPlus.SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Emote rain view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Emote rain left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Emote rain right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;
        /// <summary>
        /// Preview material asset bundle
        /// </summary>
        private AssetBundle m_PreviewMateralAssetBundle = null;
        /// <summary>
        /// Preview material
        /// </summary>
        private Material m_PreviewMaterial;
        /// <summary>
        /// Template particle system
        /// </summary>
        private GameObject m_TemplateParticleSystem = null;
        /// <summary>
        /// Template material for particles
        /// </summary>
        private Material m_TemplateMaterial = null;
        /// <summary>
        /// Templates per scene
        /// </summary>
        private Dictionary<BeatSaberPlus.SDK.Game.Logic.SceneType, Components.EmitterGroup> m_Templates
            = new Dictionary<BeatSaberPlus.SDK.Game.Logic.SceneType, Components.EmitterGroup>();
        /// <summary>
        /// Active systems
        /// </summary>
        private List<(string, Components.EmitterGroup)> m_ActiveSystems
            = new List<(string, Components.EmitterGroup)>();
        /// <summary>
        /// Available systems
        /// </summary>
        private Dictionary<BeatSaberPlus.SDK.Game.Logic.SceneType, Queue<Components.EmitterGroup>> m_ReadySystems
            = new Dictionary<BeatSaberPlus.SDK.Game.Logic.SceneType, Queue<Components.EmitterGroup>>();
        /// <summary>
        /// Combo state Dictionary<EmoteID, Tuple<ComboCount, lastSeenTickCount>>
        /// </summary>
        private Dictionary<string, Tuple<int, int>> m_ComboState = new Dictionary<string, Tuple<int, int>>();
        /// <summary>
        /// Combo state Dictionary<EmoteID, Tuple<List<UserIDs>, lastSeenTickCount>>
        /// </summary>
        private Dictionary<string, Tuple<List<string>, int>> m_ComboState2 = new Dictionary<string, Tuple<List<string>, int>>();
        /// <summary>
        /// SubRain emotes
        /// </summary>
        private Dictionary<string, BeatSaberPlus.SDK.Unity.EnhancedImage> m_SubRainTextures = new Dictionary<string, BeatSaberPlus.SDK.Unity.EnhancedImage>();
        /// <summary>
        /// Temp disable
        /// </summary>
        private bool m_TempDisable = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            /// Bind events
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange += Game_OnSceneChange;

            /// Create CustomMenuSongs directory if not existing
            if (!Directory.Exists("CustomSubRain"))
                Directory.CreateDirectory("CustomSubRain");

            LoadAssets();
            CreateTemplate();
            UpdateTemplateFor(BeatSaberPlus.SDK.Game.Logic.SceneType.Menu);
            UpdateTemplateFor(BeatSaberPlus.SDK.Game.Logic.SceneType.Playing);

            /// Load SubRain files
            LoadSubRainFiles();

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                BeatSaberPlus.SDK.Chat.Service.Acquire();

                /// Run all services
                BeatSaberPlus.SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
            }
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                BeatSaberPlus.SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                BeatSaberPlus.SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            /// Unload assets
            UnloadAssets();
            DestroyTemplate();

            /// Unload SubRain emotes
            m_SubRainTextures.Clear();

            /// Unbind events
            BeatSaberPlus.SDK.Game.Logic.OnSceneChange -= Game_OnSceneChange;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get Module settings UI
        /// </summary>
        protected override (HMUI.ViewController, HMUI.ViewController, HMUI.ViewController) GetSettingsUIImplementation()
        {
            /// Create view if needed
            if (m_SettingsView == null)
                m_SettingsView = BeatSaberUI.CreateViewController<UI.Settings>();
            /// Create view if needed
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            /// Create view if needed
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            /// Change main view
            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On game scene change
        /// </summary>
        /// <param name="p_Scene">New scene</param>
        private void Game_OnSceneChange(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            if (m_TempDisable)
            {
                SendChatMessage($"Emotes rains are now enabled!", null, null);
                m_TempDisable = false;
            }

            for (int l_I = 0; l_I < m_ActiveSystems.Count; ++l_I)
            {
                var l_CurrentSystem = m_ActiveSystems[l_I].Item2;
                if (l_CurrentSystem.Scene == p_Scene)
                    continue;

                l_CurrentSystem.Stop();
                l_CurrentSystem.gameObject.SetActive(false);

                m_ActiveSystems.RemoveAt(l_I);
                l_I--;

                if (m_ReadySystems[l_CurrentSystem.Scene].Count < POOL_SIZE_PER_SCENE)
                    m_ReadySystems[l_CurrentSystem.Scene].Enqueue(l_CurrentSystem);
                else
                    GameObject.Destroy(l_CurrentSystem);

                continue;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On settings changed
        /// </summary>
        internal void OnSettingsChanged()
        {
            for (int l_I = 0; l_I < m_ActiveSystems.Count; ++l_I)
                m_ActiveSystems[l_I].Item2.UpdateEmitters();

            foreach (var l_KVP in m_ReadySystems)
            {
                foreach (var l_Group in l_KVP.Value)
                    l_Group.UpdateEmitters();
            }

            foreach (var l_KVP in m_Templates)
            {
                l_KVP.Value.UpdateEmitters();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create template
        /// </summary>
        private void CreateTemplate()
        {
            m_TemplateMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
            m_TemplateMaterial.EnableKeyword("ETC1_EXTERNAL_ALPHA");
            m_TemplateMaterial.EnableKeyword("_ALPHABLEND_ON");
            m_TemplateMaterial.EnableKeyword("_GLOSSYREFLECTIONS_OFF");
            m_TemplateMaterial.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");
            m_TemplateMaterial.SetOverrideTag("RenderType", "Transparent");
            m_TemplateMaterial.renderQueue = 3000;
            m_TemplateMaterial.SetFloat("_BlendOp",                         0f);
            m_TemplateMaterial.SetFloat("_BumpScale",                       1f);
            m_TemplateMaterial.SetFloat("_CameraFadingEnabled",             0f);
            m_TemplateMaterial.SetFloat("_CameraFarFadeDistance",           2f);
            m_TemplateMaterial.SetFloat("_CameraNearFadeDistance",          1f);
            m_TemplateMaterial.SetFloat("_ColorMode",                       0f);
            m_TemplateMaterial.SetFloat("_Cull",                            2f);
            m_TemplateMaterial.SetFloat("_Cutoff",                        0.5f);
            m_TemplateMaterial.SetFloat("_DetailNormalMapScale",            1f);
            m_TemplateMaterial.SetFloat("_DistortionBlend",               0.5f);
            m_TemplateMaterial.SetFloat("_DistortionEnabled",               0f);
            m_TemplateMaterial.SetFloat("_DistortionStrength",              1f);
            m_TemplateMaterial.SetFloat("_DistortionStrengthScaled",        0f);
            m_TemplateMaterial.SetFloat("_DstBlend",                       10f);
            m_TemplateMaterial.SetFloat("_EmissionEnabled",                 0f);
            m_TemplateMaterial.SetFloat("_EnableExternalAlpha",             0f);
            m_TemplateMaterial.SetFloat("_FlipbookMode",                    0f);
            m_TemplateMaterial.SetFloat("_GlossMapScale",                   1f);
            m_TemplateMaterial.SetFloat("_Glossiness",                      1f);
            m_TemplateMaterial.SetFloat("_GlossyReflections",               0f);
            m_TemplateMaterial.SetFloat("_InvFade",                      1.15f);
            m_TemplateMaterial.SetFloat("_LightingEnabled",                 0f);
            m_TemplateMaterial.SetFloat("_Metallic",                        0f);
            m_TemplateMaterial.SetFloat("_Mode",                            2f);
            m_TemplateMaterial.SetFloat("_OcclusionStrength",               1f);
            m_TemplateMaterial.SetFloat("_Parallax",                     0.02f);
            m_TemplateMaterial.SetFloat("_SmoothnessTextureChannel",        0f);
            m_TemplateMaterial.SetFloat("_SoftParticlesEnabled",            0f);
            m_TemplateMaterial.SetFloat("_SoftParticlesFarFadeDistance",    1f);
            m_TemplateMaterial.SetFloat("_SoftParticlesNearFadeDistance",   0f);
            m_TemplateMaterial.SetFloat("_SrcBlend",                        5f);
            m_TemplateMaterial.SetFloat("_UVSec",                           0f);
            m_TemplateMaterial.SetFloat("_ZWrite",                          0f);
            m_TemplateMaterial.enableInstancing = true;

            m_TemplateMaterial.mainTexture = BeatSaberPlus.SDK.Unity.Texture2D.CreateFromRaw(
                Utilities.GetResource(Assembly.GetExecutingAssembly(), "BeatSaberPlus_ChatEmoteRain.Resources.DefaultEmote.png")
            );

            m_TemplateParticleSystem = new GameObject("BSP_ChatEmoteRain_Template");
            GameObject.DontDestroyOnLoad(m_TemplateParticleSystem);

            var l_PS    = m_TemplateParticleSystem.AddComponent<ParticleSystem>();
            var l_PSR   = m_TemplateParticleSystem.GetComponent<ParticleSystemRenderer>();

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
            l_PSR.material          = m_TemplateMaterial;
            l_PSR.minParticleSize   = 0.0f;
            l_PSR.maxParticleSize   = 0.5f;
            l_PSR.receiveShadows    = false;
            l_PSR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        /// <summary>
        /// Destroy template
        /// </summary>
        private void DestroyTemplate()
        {
            if (m_TemplateParticleSystem != null)
                GameObject.Destroy(m_TemplateParticleSystem);

            m_TemplateMaterial = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update templates from config
        /// </summary>
        /// <param name="p_Scene"></param>
        internal void UpdateTemplateFor(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene)
        {
            if (!m_Templates.TryGetValue(p_Scene, out var l_Group))
            {
                l_Group = new GameObject("BSP_ChatEmoteRain_Group" + p_Scene.ToString()).AddComponent<Components.EmitterGroup>();
                m_Templates.Add(p_Scene, l_Group);
            }

            var l_Configs = p_Scene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu
                ?
                    CERConfig.Instance.MenuEmitters
                :
                    CERConfig.Instance.SongEmitters
                ;

            l_Group.Scene = p_Scene;
            l_Group.Setup(l_Configs, m_TemplateParticleSystem);
            l_Group.SetupMaterial(m_TemplateMaterial, m_TemplateMaterial.mainTexture, m_PreviewMaterial);

            if (!m_ReadySystems.ContainsKey(p_Scene))
                m_ReadySystems.Add(p_Scene, new Queue<Components.EmitterGroup>());

            if (m_ReadySystems[p_Scene].Count == 0)
            {
                for (int l_I = 0; l_I < POOL_SIZE_PER_SCENE; ++l_I)
                {
                    var l_Instance = new GameObject("BSP_ChatEmoteRain_Group" + p_Scene.ToString()).AddComponent<Components.EmitterGroup>();
                    l_Instance.Scene = p_Scene;
                    l_Instance.Setup(l_Configs, m_TemplateParticleSystem);
                    l_Instance.SetupMaterial(m_TemplateMaterial, m_TemplateMaterial.mainTexture, m_PreviewMaterial);
                    l_Instance.gameObject.SetActive(false);

                    m_ReadySystems[p_Scene].Enqueue(l_Instance);
                }
            }
            else
            {
                foreach (var l_Current in m_ReadySystems[p_Scene])
                    l_Current.Setup(l_Configs, m_TemplateParticleSystem);
            }

            foreach (var l_Current in m_ActiveSystems)
            {
                if (l_Current.Item2.Scene != p_Scene)
                    continue;

                l_Current.Item2.Setup(l_Configs, m_TemplateParticleSystem);
            }
        }
        internal void SetTemplatesPreview(BeatSaberPlus.SDK.Game.Logic.SceneType p_Scene, bool p_Enabled, CERConfig._Emitter p_Focus)
        {
            if (!m_Templates.TryGetValue(p_Scene, out var l_Group))
                return;

            l_Group.SetPreview(p_Enabled, p_Focus);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load assets
        /// </summary>
        private void LoadAssets()
        {
            m_PreviewMateralAssetBundle = AssetBundle.LoadFromStream(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("BeatSaberPlus_ChatEmoteRain.Resources.PreviewMaterial.bundle"));

            m_PreviewMaterial = m_PreviewMateralAssetBundle.LoadAsset<Material>("PreviewMaterial");

            /*
            m_AssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BeatSaberPlus_ChatEmoteRain.Resources.ChatEmoteRain.bundle"));

            var l_Prefab1 = m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemMenu Variant");
            m_ParticleSystems.Add(SDK.Game.Logic.SceneType.Menu, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                l_Prefab1
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[SDK.Game.Logic.SceneType.Menu].Item2 ? m_ParticleSystems[SDK.Game.Logic.SceneType.Menu].Item2.name : "null"));

            m_ParticleSystems.Add(SDK.Game.Logic.SceneType.Playing, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemPlaySpace Variant")
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[SDK.Game.Logic.SceneType.Playing].Item2 ? m_ParticleSystems[SDK.Game.Logic.SceneType.Playing].Item2.name : "null"));
            */


            /*
            var ee = new GameObject().AddComponent<Components.EmitterInstance>();
            GameObject.DontDestroyOnLoad(ee.gameObject);
            ee.transform.position = 5f * Vector3.up;
            ee.GetComponent<ParticleSystemRenderer>().material =
                l_Prefab1.GetComponent<ParticleSystemRenderer>().material;
            ee.SetPreview(true, Color.red, true);*/

        }
        /// <summary>
        /// Unload assets
        /// </summary>
        private void UnloadAssets()
        {
            if (m_PreviewMateralAssetBundle != null)
            {
                m_PreviewMateralAssetBundle.Unload(true);
                m_PreviewMateralAssetBundle = null;
            }
        }
        /// <summary>
        /// Load SubRain emotes
        /// </summary>
        internal void LoadSubRainFiles()
        {
            m_SubRainTextures.Clear();

            var l_Files = Directory.GetFiles("CustomSubRain",   "*.png")
                   .Union(Directory.GetFiles("CustomSubRain",   "*.gif"))
                   .Union(Directory.GetFiles("CustomSubRain",   "*.apng")).ToArray();

            foreach (string l_CurrentFile in l_Files)
            {
                string l_EmoteName = Path.GetFileNameWithoutExtension(l_CurrentFile);
                l_EmoteName = "_BSPSubRain_" + l_EmoteName.Substring(l_EmoteName.LastIndexOf('\\') + 1);

                LoadExternalEmote(l_CurrentFile, l_EmoteName, (p_Result) =>
                {
                    if (p_Result != null)
                        m_SubRainTextures.Add(l_EmoteName, p_Result);
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load external emote
        /// </summary>
        /// <param name="p_FileName">File name</param>
        /// <param name="p_ID">New emote id</param>
        /// <param name="p_Callback">Load callback</param>
        public void LoadExternalEmote(string p_FileName, string p_ID, Action<BeatSaberPlus.SDK.Unity.EnhancedImage> p_Callback)
        {
            if (p_FileName.ToLower().EndsWith(".png"))
            {
                BeatSaberPlus.SDK.Unity.EnhancedImage.FromRawStatic(p_ID, File.ReadAllBytes(p_FileName), (p_Result) =>
                {
                    if (p_Result != null)
                    {
                        p_Result.Sprite.texture.wrapMode = TextureWrapMode.Mirror;
                        p_Callback?.Invoke(p_Result);
                    }
                    else
                        Logger.Instance.Warn("Failed to load image " + p_FileName);
                });
            }
            else if (p_FileName.ToLower().EndsWith(".gif"))
            {
                BeatSaberPlus.SDK.Unity.EnhancedImage.FromRawAnimated(
                    p_ID,
                    BeatSaberPlus.SDK.Animation.AnimationType.GIF,
                    File.ReadAllBytes(p_FileName), (p_Result) =>
                    {
                        if (p_Result != null)
                            p_Callback?.Invoke(p_Result);
                        else
                            Logger.Instance.Warn("Failed to load image " + p_FileName);
                    });
            }
            else if (p_FileName.ToLower().EndsWith(".apng"))
            {
                BeatSaberPlus.SDK.Unity.EnhancedImage.FromRawAnimated(
                    p_ID,
                    BeatSaberPlus.SDK.Animation.AnimationType.APNG,
                    File.ReadAllBytes(p_FileName), (p_Result) =>
                    {
                        if (p_Result != null)
                            p_Callback?.Invoke(p_Result);
                        else
                            Logger.Instance.Warn("Failed to load image " + p_FileName);
                    });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start a SubRain
        /// </summary>
        public void StartSubRain()
        {
            if (!CERConfig.Instance.SubRain)
                return;

            if (   (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu    && CERConfig.Instance.EnableMenu)
                || (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing && CERConfig.Instance.EnableSong))
            {
                var l_EmitCount = (uint)CERConfig.Instance.SubRainEmoteCount;
                if (m_SubRainTextures.Count == 0)
                    SharedCoroutineStarter.instance.StartCoroutine(StartParticleSystem(s_SUBRAIN_NO_EMOTE, null, l_EmitCount));
                else
                {
                    foreach (var l_KVP in m_SubRainTextures)
                        SharedCoroutineStarter.instance.StartCoroutine(StartParticleSystem(l_KVP.Key, l_KVP.Value, l_EmitCount));
                }
            }
        }
        /// <summary>
        /// Unregister a particle system
        /// </summary>
        /// <param name="p_EmoteID">Emote ID</param>
        /// <param name="p_Mode">Game mode</param>
        internal void UnregisterGroup(Components.EmitterGroup p_Group)
        {
#if DEBUG
            Logger.Instance.Debug("[ChatEmoteRain] UnregisterGroup group " + p_Group.GetHashCode());
#endif

            p_Group.Stop();
            p_Group.gameObject.SetActive(false);

            for (int l_I = 0; l_I < m_ActiveSystems.Count; ++l_I)
            {
                var l_CurrentSystem = m_ActiveSystems[l_I].Item2;
                if (l_CurrentSystem != p_Group)
                    continue;

                m_ActiveSystems.RemoveAt(l_I);
                l_I--;

                if (m_ReadySystems[l_CurrentSystem.Scene].Count < POOL_SIZE_PER_SCENE)
                {
                    m_ReadySystems[l_CurrentSystem.Scene].Enqueue(l_CurrentSystem);
#if DEBUG
                    Logger.Instance.Debug("[ChatEmoteRain] Queuing back group " + p_Group.GetHashCode());
#endif
                }
                else
                {
                    GameObject.Destroy(l_CurrentSystem.gameObject);
#if DEBUG
                    Logger.Instance.Debug("[ChatEmoteRain] Destroying overflow group " + p_Group.GetHashCode());
#endif
                }

                continue;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_Service, IChatMessage p_Message)
        {
            if (!string.IsNullOrEmpty(p_Message.Message) && p_Message.Message.Length > 2 && p_Message.Message[0] == '!')
            {
                string l_LMessage = p_Message.Message.ToLower();
                if (l_LMessage.StartsWith("!er toggle"))
                {
                    if (HasPower(p_Message.Sender))
                    {
                        m_TempDisable = !m_TempDisable;

                        if (m_TempDisable)
                            SendChatMessage($"@{p_Message.Sender.UserName} emotes rains are disabled until next scene change!", p_Service, p_Message);
                        else
                            SendChatMessage($"@{p_Message.Sender.UserName} emotes rains are now enabled!", p_Service, p_Message);
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!", p_Service, p_Message);
                }
                else if (l_LMessage.StartsWith("!er rain "))
                {
                    if (HasPower(p_Message.Sender))
                    {
                        l_LMessage = l_LMessage.Substring("!er rain ".Length);
                        var l_Parts = l_LMessage.Split(' ');

                        if (p_Message.Emotes == null || p_Message.Emotes.Length == 0 || l_Parts.Length < 2 || !uint.TryParse(l_Parts[0], out var l_Count))
                        {
                            SendChatMessage($"@{p_Message.Sender.UserName} bad syntax, the command is \"!er rain #COUNT #EMOTES \"!", p_Service, p_Message);
                            return;
                        }

                        BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() => {
                            for (int l_EmoteI = 0; l_EmoteI < p_Message.Emotes.Length; ++l_EmoteI)
                                EnqueueEmote(p_Message.Emotes[l_EmoteI], l_Count);
                        });

                        SendChatMessage($"@{p_Message.Sender.UserName} Let em' rain!", p_Service, p_Message);
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!", p_Service, p_Message);
                }
                else if (l_LMessage.StartsWith("!er clear"))
                {
                    if (HasPower(p_Message.Sender))
                    {
                        BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                        {
                            var l_ActiveScene   = BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu ? BeatSaberPlus.SDK.Game.Logic.SceneType.Menu : BeatSaberPlus.SDK.Game.Logic.SceneType.Playing;
                            var l_EmitterGroup  = m_ActiveSystems.Where(x => x.Item2.Scene == l_ActiveScene).Select(x => x.Item2);

                            foreach (var l_Emitter in l_EmitterGroup)
                                l_Emitter.Clear();
                        });
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!", p_Service, p_Message);
                }
            }

            if (m_TempDisable)
                return;

            if (p_Message.IsSystemMessage && CERConfig.Instance.SubRain
                && (p_Message.Message.StartsWith("⭐") || p_Message.Message.StartsWith("👑")))
            {
                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() => StartSubRain());
            }

            IChatEmote[] l_Emotes = CERConfig.Instance.ComboMode ? FilterEmotesForCombo(p_Message) : p_Message.Emotes;
            if (l_Emotes != null && l_Emotes.Length > 0)
            {
                var l_EmotesToRain =
                                    (from iChatEmote in l_Emotes
                                      group iChatEmote by iChatEmote.Id into emoteGrouping
                                      select new { emote = emoteGrouping.First(), count = (byte)emoteGrouping.Count() }
                    ).ToArray();

                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    for (int l_I = 0; l_I < l_EmotesToRain.Length; ++l_I)
                        EnqueueEmote(l_EmotesToRain[l_I].emote, (uint)l_EmotesToRain[l_I].count);
                });
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enqueue a emote for display
        /// </summary>
        /// <param name="p_Emote">Emote to enqueue</param>
        /// <param name="p_Count">Display count</param>
        /// <returns></returns>
        private void EnqueueEmote(IChatEmote p_Emote, uint p_Count)
        {
            if (   (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu    && CERConfig.Instance.EnableMenu)
                || (BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Playing && CERConfig.Instance.EnableSong))
            {
                SharedCoroutineStarter.instance.StartCoroutine(StartParticleSystem(p_Emote.Id, null, p_Count));
            }
        }
        /// <summary>
        /// Start particle system or update existing one for an Emote
        /// </summary>
        /// <param name="p_EmoteID">ID of the emote</param>
        /// <param name="p_Count">Display count</param>
        /// <returns></returns>
        public IEnumerator StartParticleSystem(string p_EmoteID, BeatSaberPlus.SDK.Unity.EnhancedImage p_Raw, uint p_Count)
        {
            BeatSaberPlus.SDK.Unity.EnhancedImage l_EnhancedImageInfo = p_Raw;

            if (p_Raw == null)
                yield return new WaitUntil(() => BeatSaberPlus.SDK.Chat.ImageProvider.CachedImageInfo.TryGetValue(p_EmoteID, out l_EnhancedImageInfo) && BeatSaberPlus.SDK.Game.Logic.ActiveScene != BeatSaberPlus.SDK.Game.Logic.SceneType.None);

            /// If not enhanced info, we skip
            if (l_EnhancedImageInfo == null && p_EmoteID != s_SUBRAIN_NO_EMOTE)
                yield break;

            var l_ActiveScene   = BeatSaberPlus.SDK.Game.Logic.ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu ? BeatSaberPlus.SDK.Game.Logic.SceneType.Menu : BeatSaberPlus.SDK.Game.Logic.SceneType.Playing;
            var l_EmitterGroup  = m_ActiveSystems.Where(x => x.Item2.Scene == l_ActiveScene && x.Item1 == p_EmoteID).Select(x => x.Item2).FirstOrDefault();

            if (l_EmitterGroup == null || !l_EmitterGroup)
            {
                if (m_ReadySystems[l_ActiveScene].Count > 0)
                {
                    l_EmitterGroup = m_ReadySystems[l_ActiveScene].Dequeue();
                    l_EmitterGroup.gameObject.SetActive(true);

                    /// Reset animated images animator
                    for (int l_EmitterI = 0; l_EmitterI < l_EmitterGroup.Emitters.Length; ++l_EmitterI)
                    {
                        var l_Current = l_EmitterGroup.Emitters[l_EmitterI];
                        var l_TextureSheetAnimation = l_Current.PS.textureSheetAnimation;
                        l_TextureSheetAnimation.enabled = false;
                    }

                    m_ActiveSystems.Add((p_EmoteID, l_EmitterGroup));

                    Components.EmitterGroupManager.instance.Register(l_EmitterGroup);
                }
                else
                {
                    var l_Configs = l_ActiveScene == BeatSaberPlus.SDK.Game.Logic.SceneType.Menu
                        ?
                            CERConfig.Instance.MenuEmitters
                        :
                            CERConfig.Instance.SongEmitters
                        ;

                    l_EmitterGroup = new GameObject("BSP_ChatEmoteRain_Group" + l_ActiveScene.ToString()).AddComponent<Components.EmitterGroup>();
                    l_EmitterGroup.Scene = l_ActiveScene;
                    l_EmitterGroup.Setup(l_Configs, m_TemplateParticleSystem);
                    l_EmitterGroup.SetupMaterial(m_TemplateMaterial, m_TemplateMaterial.mainTexture, m_PreviewMaterial);

                    m_ActiveSystems.Add((p_EmoteID, l_EmitterGroup));

                    Components.EmitterGroupManager.instance.Register(l_EmitterGroup);
#if DEBUG
                    Logger.Instance.Debug("[ChatEmoteRain] Allocating overgrown group " + l_EmitterGroup.GetHashCode());
#endif
                }

                float l_AspectRatio = (float)l_EnhancedImageInfo.Width / (float)l_EnhancedImageInfo.Height;

                /// Sorta working animated emotes
                if (p_EmoteID != s_SUBRAIN_NO_EMOTE && l_EnhancedImageInfo.AnimControllerData != null)
                {
                    for (int l_EmitterI = 0; l_EmitterI < l_EmitterGroup.Emitters.Length; ++l_EmitterI)
                    {
                        var l_Current               = l_EmitterGroup.Emitters[l_EmitterI];
                        var l_TextureSheetAnimation = l_Current.PS.textureSheetAnimation;

                        /// Clear old sprites
                        while (l_TextureSheetAnimation.spriteCount > 0)
                            l_TextureSheetAnimation.RemoveSprite(0);

                        var l_SpriteCount   = l_EnhancedImageInfo.AnimControllerData.sprites.Length;
                        var l_TimeForEmote  = 0f;
                        for (int l_I = 0; l_I < l_SpriteCount; ++l_I)
                        {
                            l_TextureSheetAnimation.AddSprite(l_EnhancedImageInfo.AnimControllerData.sprites[l_I]);
                            l_TimeForEmote += l_EnhancedImageInfo.AnimControllerData.delays[l_I];
                        }

                        AnimationCurve l_AnimationCurve = new AnimationCurve();
                        float l_TimeAccumulator         = 0f;
                        float l_SingleFramePercentage   = 1.0f / (float)l_SpriteCount;
                        for (int l_FrameI = 0; l_FrameI < l_SpriteCount; ++l_FrameI)
                        {
                            l_AnimationCurve.AddKey(l_TimeAccumulator / l_TimeForEmote, ((float)l_FrameI) * l_SingleFramePercentage);
                            l_TimeAccumulator += l_EnhancedImageInfo.AnimControllerData.delays[l_FrameI];
                        }
                        l_AnimationCurve.AddKey(1f, 1f);

                        l_TextureSheetAnimation.enabled         = true;
                        l_TextureSheetAnimation.frameOverTime   = new ParticleSystem.MinMaxCurve(1f, l_AnimationCurve);

                        var l_CycleCount = (int)Mathf.Max(1f, (l_Current.LifeTime * 1000f) / l_TimeForEmote);
                        l_TextureSheetAnimation.cycleCount = l_CycleCount;

                        var l_PSMain = l_Current.PS.main;
                        l_PSMain.startLifetime = l_CycleCount * (l_TimeForEmote / 1000f);

                        /// Wide emote support
                        if (Mathf.Abs(1f - l_AspectRatio) > 0.1f)
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
                    for (int l_EmitterI = 0; l_EmitterI < l_EmitterGroup.Emitters.Length; ++l_EmitterI)
                    {
                        var l_Current = l_EmitterGroup.Emitters[l_EmitterI];

                        var l_TextureSheetAnimation = l_Current.PS.textureSheetAnimation;
                        l_TextureSheetAnimation.enabled = false;

                        var l_PSMain = l_Current.PS.main;
                        l_PSMain.startLifetime = l_Current.LifeTime;

                        /// Wide emote support
                        if (Mathf.Abs(1f - l_AspectRatio) > 0.1f)
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

                    l_EmitterGroup.UpdateEmitters();
                }

                l_EmitterGroup.UpdateTexture(l_EnhancedImageInfo.Sprite.texture);
            }

            l_EmitterGroup.Emit(p_Count);

            yield return null;
        }
        /// <summary>
        /// Filter emotes for combo
        /// </summary>
        /// <param name="p_Emotes">Emotes to filter</param>
        /// <returns></returns>
        private IChatEmote[] FilterEmotesForCombo(IChatMessage p_Message)
        {
            IChatEmote[] returner = null;
            if (CERConfig.Instance.ComboModeType == 1) // Trigger type: 0 = Emote; 1 = User
            {
                IChatEmote[] l_Emotes = p_Message.Emotes;
                string l_Sender = p_Message.Sender.Id;

                List<IChatEmote> l_FirstFiltering = new List<IChatEmote>();
                foreach(IChatEmote l_CurrentEmote in l_Emotes)
                {
                    if(l_FirstFiltering.Count(x => x.Id == l_CurrentEmote.Id) == 0)
                        l_FirstFiltering.Add(l_CurrentEmote);
                }

                List<IChatEmote> l_SecondFiltering = new List<IChatEmote>();
                foreach (IChatEmote e in l_FirstFiltering)
                {
                    if (m_ComboState2.ContainsKey(e.Id))
                    {
                        if (Environment.TickCount - m_ComboState2[e.Id].Item2 < CERConfig.Instance.ComboTimer * 1000 &&
                            !m_ComboState2[e.Id].Item1.Contains(l_Sender))
                        {
                            m_ComboState2[e.Id].Item1.Add(l_Sender);
                            m_ComboState2[e.Id] = new Tuple<List<string>, int>(m_ComboState2[e.Id].Item1, Environment.TickCount & int.MaxValue);
                        }
                        else
                        {
                            m_ComboState2.Remove(e.Id);
                            List<string> temp = new List<string>();
                            temp.Add(l_Sender);
                            m_ComboState2.Add(e.Id, new Tuple<List<string>, int>(temp, Environment.TickCount & int.MaxValue));
                        }

                        if(m_ComboState2[e.Id].Item1.Count >= CERConfig.Instance.ComboCount)
                        {
                            l_SecondFiltering.Add(e);
                        }
                    }
                    else
                    {
                        List<string> temp = new List<string>();
                        temp.Add(l_Sender);
                        m_ComboState2.Add(e.Id, new Tuple<List<string>, int>(temp, Environment.TickCount & int.MaxValue));
                    }
                }
                returner = l_SecondFiltering.ToArray();
            }
            else
            {
                IChatEmote[] l_Emotes = p_Message.Emotes;

                List<IChatEmote> l_FirstFiltering = new List<IChatEmote>();
                foreach (IChatEmote l_CurrentEmote in l_Emotes)
                {
                    if (l_FirstFiltering.Count(x => x.Id == l_CurrentEmote.Id) == 0)
                        l_FirstFiltering.Add(l_CurrentEmote);
                }

                List<IChatEmote> l_SecondFiltering = new List<IChatEmote>();
                foreach (IChatEmote l_CurrentEmote in l_FirstFiltering)
                {
                    if (m_ComboState.ContainsKey(l_CurrentEmote.Id))
                    {
                        if (Environment.TickCount - m_ComboState[l_CurrentEmote.Id].Item2 < CERConfig.Instance.ComboTimer * 1000)
                            m_ComboState[l_CurrentEmote.Id] = new Tuple<int, int>(m_ComboState[l_CurrentEmote.Id].Item1 + 1, Environment.TickCount & int.MaxValue);
                        else
                        {
                            m_ComboState.Remove(l_CurrentEmote.Id);
                            m_ComboState.Add(l_CurrentEmote.Id, new Tuple<int, int>(1, Environment.TickCount & int.MaxValue));
                        }

                        if (m_ComboState[l_CurrentEmote.Id].Item1 >= CERConfig.Instance.ComboCount)
                            l_SecondFiltering.Add(l_CurrentEmote);
                    }
                    else
                        m_ComboState.Add(l_CurrentEmote.Id, new Tuple<int, int>(1, Environment.TickCount & int.MaxValue));
                }

                returner = l_SecondFiltering.ToArray();
            }

            return returner;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Send message to chat
        /// </summary>
        /// <param name="p_Message">Messages to send</param>
        /// <param name="p_Service">Source channel</param>
        /// <param name="p_SourceMessage">Context message</param>
        internal void SendChatMessage(string p_Message, IChatService p_Service, IChatMessage p_SourceMessage)
        {
            if (p_Service == null && p_SourceMessage == null)
                BeatSaberPlus.SDK.Chat.Service.BroadcastMessage("! " + p_Message);
            else
                p_Service.SendTextMessage(p_SourceMessage.Channel, "! " + p_Message);
        }
        /// <summary>
        /// Has privileges
        /// </summary>
        /// <param name="p_User">Source user</param>
        /// <returns></returns>
        private bool HasPower(IChatUser p_User)
        {
            if (CERConfig.Instance.ChatCommands.UserPower)
                return true;

            return p_User.IsBroadcaster
                || (CERConfig.Instance.ChatCommands.ModeratorPower     && p_User.IsModerator)
                || (CERConfig.Instance.ChatCommands.VIPPower           && p_User.IsVip)
                || (CERConfig.Instance.ChatCommands.SubscriberPower    && p_User.IsSubscriber);
        }
    }
}
