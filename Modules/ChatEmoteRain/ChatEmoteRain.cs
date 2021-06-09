using BeatSaberMarkupLanguage;
using BeatSaberPlus.SDK.Chat.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatEmoteRain
{

    using PrefabPair = ValueTuple<Dictionary<string, Components.TimeoutScript>, UnityEngine.GameObject>;

    /// <summary>
    /// Chat Emote Rain instance
    /// </summary>
    internal class ChatEmoteRain : SDK.ModuleBase<ChatEmoteRain>
    {
        /// <summary>
        /// No emote SUBRAIN default ID
        /// </summary>
        private static string s_SUBRAIN_NO_EMOTE = "_BSPSubRain_$DEFAULT$_";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module type
        /// </summary>
        public override SDK.IModuleBaseType Type => SDK.IModuleBaseType.Integrated;
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
        public override bool IsEnabled { get => Config.ChatEmoteRain.Enabled; set => Config.ChatEmoteRain.Enabled = value; }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override SDK.IModuleBaseActivationType ActivationType => SDK.IModuleBaseActivationType.OnMenuSceneLoaded;

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
        /// Asset bundle
        /// </summary>
        private AssetBundle m_AssetBundle = null;
        /// <summary>
        /// Particles systems
        /// </summary>
        private Dictionary<SDK.Game.Logic.SceneType, PrefabPair> m_ParticleSystems = new Dictionary<SDK.Game.Logic.SceneType, PrefabPair>();
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
        private Dictionary<string, SDK.Unity.EnhancedImage> m_SubRainTextures = new Dictionary<string, SDK.Unity.EnhancedImage>();
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
            SDK.Game.Logic.OnSceneChange += Game_OnSceneChange;

            /// Create CustomMenuSongs directory if not existing
            if (!Directory.Exists("CustomSubRain"))
                Directory.CreateDirectory("CustomSubRain");

            /// Load assets
            LoadAssets();

            /// Load SubRain files
            LoadSubRainFiles();

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                SDK.Chat.Service.Acquire();

                /// Run all services
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
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
                SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            /// Unload assets
            UnloadAssets();

            /// Unload SubRain emotes
            m_SubRainTextures.Clear();

            /// Unbind events
            SDK.Game.Logic.OnSceneChange -= Game_OnSceneChange;
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
        private void Game_OnSceneChange(SDK.Game.Logic.SceneType p_Scene)
        {
            if (m_TempDisable)
            {
                SendChatMessage($"Emotes rains are now enabled!");
                m_TempDisable = false;
            }

            foreach (var l_KVP in m_ParticleSystems)
            {
                if (l_KVP.Key == p_Scene)
                    continue;

                l_KVP.Value.Item1.Clear();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On settings changed
        /// </summary>
        internal void OnSettingsChanged()
        {
            foreach (var l_KVP in m_ParticleSystems)
            {
                if (l_KVP.Key != SDK.Game.Logic.ActiveScene)
                    continue;

                foreach (var l_System in l_KVP.Value.Item1.Values)
                {
                    if (l_System == null || !l_System)
                        continue;

                    var l_ParticleSystem = l_System.PS.main;

                    if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Menu)
                    {
                        l_ParticleSystem.startSize      = Config.ChatEmoteRain.MenuRainSize;
                        l_ParticleSystem.startSpeed     = Config.ChatEmoteRain.MenuFallSpeed;
                        l_ParticleSystem.startLifetime  = (8 / (Config.ChatEmoteRain.MenuFallSpeed - 1)) + 1;
                    }
                    if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
                    {
                        l_ParticleSystem.startSize      = Config.ChatEmoteRain.SongRainSize;
                        l_ParticleSystem.startSpeed     = Config.ChatEmoteRain.SongFallSpeed;
                        l_ParticleSystem.startLifetime  = (8 / (Config.ChatEmoteRain.SongFallSpeed - 1)) + 1;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load assets
        /// </summary>
        private void LoadAssets()
        {
            m_AssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("BeatSaberPlus.Modules.ChatEmoteRain.Resources.ChatEmoteRain.bundle"));

            m_ParticleSystems.Add(SDK.Game.Logic.SceneType.Menu, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemMenu Variant")
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[SDK.Game.Logic.SceneType.Menu].Item2 ? m_ParticleSystems[SDK.Game.Logic.SceneType.Menu].Item2.name : "null"));

            m_ParticleSystems.Add(SDK.Game.Logic.SceneType.Playing, new PrefabPair(
                new Dictionary<string, Components.TimeoutScript>(),
                m_AssetBundle.LoadAsset<GameObject>("ERParticleSystemPlaySpace Variant")
            ));
            Logger.Instance.Debug("Prefab at: " + (m_ParticleSystems[SDK.Game.Logic.SceneType.Playing].Item2 ? m_ParticleSystems[SDK.Game.Logic.SceneType.Playing].Item2.name : "null"));
        }
        /// <summary>
        /// Unload assets
        /// </summary>
        private void UnloadAssets()
        {
            if (m_AssetBundle == null)
                return;

            m_ParticleSystems.Clear();

            m_AssetBundle.Unload(true);
            m_AssetBundle = null;
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
        internal void LoadExternalEmote(string p_FileName, string p_ID, Action<SDK.Unity.EnhancedImage> p_Callback)
        {
            if (p_FileName.ToLower().EndsWith(".png"))
            {
                SDK.Unity.EnhancedImage.FromRawStatic(p_ID, File.ReadAllBytes(p_FileName), (p_Result) =>
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
                SDK.Unity.EnhancedImage.FromRawAnimated(
                    p_ID,
                    SDK.Animation.AnimationType.GIF,
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
                SDK.Unity.EnhancedImage.FromRawAnimated(
                    p_ID,
                    SDK.Animation.AnimationType.APNG,
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
        internal void StartSubRain()
        {
            if (!Config.ChatEmoteRain.SubRain)
                return;

            if (   (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Menu    && Config.ChatEmoteRain.MenuRain)
                || (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing && Config.ChatEmoteRain.SongRain))
            {
                var l_EmitCount = (uint)Config.ChatEmoteRain.SubRainEmoteCount;
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
        internal void UnregisterParticleSystem(string p_EmoteID, SDK.Game.Logic.SceneType p_Mode)
        {
            GameObject.Destroy(m_ParticleSystems[p_Mode].Item1[p_EmoteID].gameObject);
            m_ParticleSystems[p_Mode].Item1.Remove(p_EmoteID);
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
                            SendChatMessage($"@{p_Message.Sender.UserName} emotes rains are disabled until next scene change!");
                        else
                            SendChatMessage($"@{p_Message.Sender.UserName} emotes rains are now enabled!");
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                }
                else if (l_LMessage.StartsWith("!er rain "))
                {
                    if (HasPower(p_Message.Sender))
                    {
                        l_LMessage = l_LMessage.Substring("!er rain ".Length);
                        var l_Parts = l_LMessage.Split(' ');

                        if (p_Message.Emotes.Length == 0 || l_Parts.Length < 2 || !uint.TryParse(l_Parts[0], out var l_Count))
                        {
                            SendChatMessage($"@{p_Message.Sender.UserName} bad syntax, the command is \"!er rain #COUNT #EMOTES \"!");
                            return;
                        }

                        SDK.Unity.MainThreadInvoker.Enqueue(() => {
                            foreach (var l_Emote in p_Message.Emotes)
                                EnqueueEmote(l_Emote, l_Count);
                        });

                        SendChatMessage($"@{p_Message.Sender.UserName} Let em' rain!");
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!");
                }
            }

            if (m_TempDisable)
                return;

            if (p_Message.IsSystemMessage && Config.ChatEmoteRain.SubRain
                && (p_Message.Message.StartsWith("⭐") || p_Message.Message.StartsWith("👑")))
            {
                //Logger.Instance.Debug($"Received System Message: {p_Message.Message}; Should be Sub.");
                SDK.Unity.MainThreadInvoker.Enqueue(() => StartSubRain());
            }

            IChatEmote[] l_Emotes = Config.ChatEmoteRain.ComboMode ? FilterEmotesForCombo(p_Message) : p_Message.Emotes;
            if (l_Emotes.Length > 0)
            {
                SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    (from iChatEmote in l_Emotes
                     group iChatEmote by iChatEmote.Id into emoteGrouping
                     select new { emote = emoteGrouping.First(), count = (byte)emoteGrouping.Count() }
                    ).ToList().ForEach(x => EnqueueEmote(x.emote, x.count));
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
            if (   (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Menu    && Config.ChatEmoteRain.MenuRain)
                || (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing && Config.ChatEmoteRain.SongRain))
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
        public IEnumerator StartParticleSystem(string p_EmoteID, SDK.Unity.EnhancedImage p_Raw, uint p_Count)
        {
            PrefabPair l_PrefabPair = m_ParticleSystems[SDK.Game.Logic.ActiveScene];

            if (!l_PrefabPair.Item1.ContainsKey(p_EmoteID) || !l_PrefabPair.Item1[p_EmoteID])
            {
                SDK.Unity.EnhancedImage l_EnhancedImageInfo = p_Raw;

                if (p_Raw == null)
                    yield return new WaitUntil(() => SDK.Chat.ImageProvider.CachedImageInfo.TryGetValue(p_EmoteID, out l_EnhancedImageInfo) && SDK.Game.Logic.ActiveScene != SDK.Game.Logic.SceneType.None);

                /// If not enhanced info, we skip
                if (l_EnhancedImageInfo == null && p_EmoteID != s_SUBRAIN_NO_EMOTE)
                    yield break;

                var l_TimeoutScript     = GameObject.Instantiate(l_PrefabPair.Item2).GetComponent<Components.TimeoutScript>();
                l_TimeoutScript.key     = p_EmoteID;
                l_TimeoutScript.mode    = SDK.Game.Logic.ActiveScene;

                var l_ParticleSystem = l_TimeoutScript.PS.main;

                if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Menu)
                {
                    l_ParticleSystem.startSize      = Config.ChatEmoteRain.MenuRainSize;
                    l_ParticleSystem.startSpeed     = Config.ChatEmoteRain.MenuFallSpeed;
                    l_ParticleSystem.startLifetime  = (8 / (Config.ChatEmoteRain.MenuFallSpeed - 1)) + 1;

                }
                if (SDK.Game.Logic.ActiveScene == SDK.Game.Logic.SceneType.Playing)
                {
                    l_ParticleSystem.startSize      = Config.ChatEmoteRain.SongRainSize;
                    l_ParticleSystem.startSpeed     = Config.ChatEmoteRain.SongFallSpeed;
                    l_ParticleSystem.startLifetime  = (8 / (Config.ChatEmoteRain.SongFallSpeed - 1)) + 1;
                }

                l_ParticleSystem.simulationSpace    = ParticleSystemSimulationSpace.World;

                if (!l_PrefabPair.Item1.ContainsKey(p_EmoteID))
                    l_PrefabPair.Item1.Add(p_EmoteID, l_TimeoutScript);
                else
                    l_PrefabPair.Item1[p_EmoteID] = l_TimeoutScript;

                float l_AspectRatio = (float)l_EnhancedImageInfo.Width / (float)l_EnhancedImageInfo.Height;

                /// Wide emote support
                if (Mathf.Abs(1f - l_AspectRatio) > 0.1f)
                {
                    var l_StartSize3D = new Vector3(
                            l_ParticleSystem.startSize.constant * l_AspectRatio,
                            l_ParticleSystem.startSize.constant,
                            l_ParticleSystem.startSize.constant
                        );
                    l_ParticleSystem.startSize3D = true;
                    l_ParticleSystem.startSizeXMultiplier = l_StartSize3D.x;
                    l_ParticleSystem.startSizeYMultiplier = l_StartSize3D.y;
                    l_ParticleSystem.startSizeZMultiplier = l_StartSize3D.z;
                }

                /// Sorta working animated emotes
                if (p_EmoteID != s_SUBRAIN_NO_EMOTE && l_EnhancedImageInfo.AnimControllerData != null)
                {
                    var l_TextureSheetAnimation = l_TimeoutScript.PS.textureSheetAnimation;
                    l_TextureSheetAnimation.enabled     = true;
                    l_TextureSheetAnimation.mode        = ParticleSystemAnimationMode.Sprites;
                    l_TextureSheetAnimation.timeMode    = ParticleSystemAnimationTimeMode.Lifetime;

                    int     l_SpriteCount   = l_EnhancedImageInfo.AnimControllerData.sprites.Length;
                    float   l_TimeForEmote  = 0;
                    for (int l_I = 0; l_I < l_SpriteCount; ++l_I)
                    {
                        l_TextureSheetAnimation.AddSprite(l_EnhancedImageInfo.AnimControllerData.sprites[l_I]);
                        l_TimeForEmote += l_EnhancedImageInfo.AnimControllerData.delays[l_I];
                    }

                    AnimationCurve l_AnimationCurve = new AnimationCurve();

                    float l_TimeAccumulator         = 0f;
                    float l_SingleFramePercentage   = 1.0f / l_SpriteCount;
                    float l_TimeMult                = 1000f / l_TimeForEmote;

                    for (int l_FrameI = 0; l_FrameI < l_SpriteCount; ++l_FrameI)
                    {
                        l_AnimationCurve.AddKey(l_TimeAccumulator / l_TimeForEmote, ((float)l_FrameI) * l_SingleFramePercentage);
                        l_TimeAccumulator += l_EnhancedImageInfo.AnimControllerData.delays[l_FrameI];
                    }

                    l_AnimationCurve.AddKey(1f, 1f);

                    ///Logger.Instance.Error(p_EmoteID);
                    ///foreach (var l_Key in l_AnimationCurve.keys)
                    ///    Logger.Instance.Error("( " + l_Key.time + ", " + l_Key.value + ")");

                    int l_CycleCount = (int)(((l_TimeoutScript.TimeLimit * 1.5f) * 1000f) / l_TimeForEmote);
                    l_ParticleSystem.startLifetime = (l_CycleCount * l_TimeForEmote) / 1000f;
                    l_TextureSheetAnimation.cycleCount = l_CycleCount;

                    l_TextureSheetAnimation.frameOverTime = new ParticleSystem.MinMaxCurve(l_TimeForEmote > 1000f ? 1f : 1f / (l_TimeForEmote / 1000f), l_AnimationCurve);

                    ///Logger.Instance.Error("startLifetime " + l_TimeoutScript.PS.main.startLifetime.constant);
                    ///Logger.Instance.Error("l_TimeForEmote " + l_TimeForEmote);
                    ///Logger.Instance.Error("frameOverTime " + l_TextureSheetAnimation.frameOverTime.curveMultiplier);
                    ///Logger.Instance.Error("cycleCount " + l_TextureSheetAnimation.cycleCount + " " + l_CycleCount);
                }

                if (l_EnhancedImageInfo != null)
                    l_TimeoutScript.PSR.material.mainTexture = l_EnhancedImageInfo.Sprite.texture;

                l_TimeoutScript.Emit(p_Count);
            }
            else
                l_PrefabPair.Item1[p_EmoteID].Emit(p_Count);

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
            if(Config.ChatEmoteRain.ComboModeType == 1) // Trigger type: 0 = Emote; 1 = User
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
                        if (Environment.TickCount - m_ComboState2[e.Id].Item2 < Config.ChatEmoteRain.ComboTimer * 1000 &&
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

                        if(m_ComboState2[e.Id].Item1.Count >= Config.ChatEmoteRain.ComboCount)
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
                        if (Environment.TickCount - m_ComboState[l_CurrentEmote.Id].Item2 < Config.ChatEmoteRain.ComboTimer * 1000)
                            m_ComboState[l_CurrentEmote.Id] = new Tuple<int, int>(m_ComboState[l_CurrentEmote.Id].Item1 + 1, Environment.TickCount & int.MaxValue);
                        else
                        {
                            m_ComboState.Remove(l_CurrentEmote.Id);
                            m_ComboState.Add(l_CurrentEmote.Id, new Tuple<int, int>(1, Environment.TickCount & int.MaxValue));
                        }

                        if (m_ComboState[l_CurrentEmote.Id].Item1 >= Config.ChatEmoteRain.ComboCount)
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
        internal void SendChatMessage(string p_Message)
        {
            SDK.Chat.Service.BroadcastMessage("! " + p_Message);
        }
        /// <summary>
        /// Has privileges
        /// </summary>
        /// <param name="p_User">Source user</param>
        /// <returns></returns>
        private bool HasPower(IChatUser p_User)
        {
            if (p_User is SDK.Chat.Models.Twitch.TwitchUser l_TwitchUser)
            {
                return l_TwitchUser.IsBroadcaster
                    || (Config.ChatEmoteRain.ModeratorPower     && l_TwitchUser.IsModerator)
                    || (Config.ChatEmoteRain.VIPPower           && l_TwitchUser.IsVip)
                    || (Config.ChatEmoteRain.SubscriberPower    && l_TwitchUser.IsSubscriber);
            }

            return false;
        }
    }
}
