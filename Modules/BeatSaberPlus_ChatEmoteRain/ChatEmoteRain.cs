using BeatSaberMarkupLanguage;
using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

using EmitterConfig = CP_SDK.Unity.Components.EnhancedImageParticleEmitter.EmitterConfig;

namespace ChatPlexMod_ChatEmoteRain
{
    /// <summary>
    /// Chat Emote Rain instance
    /// </summary>
    public class ChatEmoteRain : BeatSaberPlus.SDK.BSPModuleBase<ChatEmoteRain>
    {
        /// <summary>
        /// Warm-up size per scene
        /// </summary>
        private static int POOL_SIZE_PER_SCENE = 50;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module type
        /// </summary>
        public override CP_SDK.EIModuleBaseType Type => CP_SDK.EIModuleBaseType.Integrated;
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
        public override CP_SDK.EIModuleBaseActivationType ActivationType => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

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
        /// Menu emitter manager
        /// </summary>
        private CP_SDK.Unity.Components.EnhancedImageParticleEmitterManager m_MenuManager;
        /// <summary>
        /// Playing emitter manager
        /// </summary>
        private CP_SDK.Unity.Components.EnhancedImageParticleEmitterManager m_PlayingManager;
        /// <summary>
        /// SubRain emotes
        /// </summary>
        private List<CP_SDK.Unity.EnhancedImage> m_SubRainTextures = new List<CP_SDK.Unity.EnhancedImage>();
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
            CP_SDK.ChatPlexSDK.OnGenericSceneChange += ChatPlexSDK_OnGenericSceneChange;

            /// Create CustomMenuSongs directory if not existing
            if (!Directory.Exists("CustomSubRain"))
                Directory.CreateDirectory("CustomSubRain");

            LoadAssets();

            m_MenuManager       = new GameObject("[ChatPlexMod_ChatEmoteRain.MenuManager]").AddComponent<CP_SDK.Unity.Components.EnhancedImageParticleEmitterManager>();
            m_PlayingManager    = new GameObject("[ChatPlexMod_ChatEmoteRain.PlayingManager]").AddComponent<CP_SDK.Unity.Components.EnhancedImageParticleEmitterManager>();

            GameObject.DontDestroyOnLoad(m_MenuManager.gameObject);
            GameObject.DontDestroyOnLoad(m_PlayingManager.gameObject);

            UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Menu);
            UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene.Playing);

            /// Load SubRain files
            LoadSubRainFiles();

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                CP_SDK.Chat.Service.Acquire();

                /// Run all services
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived += ChatCoreMutiplixer_OnTextMessageReceived;
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
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                CP_SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }

            if (m_MenuManager != null)      GameObject.DestroyImmediate(m_MenuManager.gameObject);
            if (m_PlayingManager != null)   GameObject.DestroyImmediate(m_PlayingManager.gameObject);

            /// Unload assets
            UnloadAssets();

            /// Unload SubRain emotes
            m_SubRainTextures.Clear();

            /// Unbind events
            CP_SDK.ChatPlexSDK.OnGenericSceneChange -= ChatPlexSDK_OnGenericSceneChange;
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
        private void ChatPlexSDK_OnGenericSceneChange(CP_SDK.ChatPlexSDK.EGenericScene p_Scene)
        {
            if (m_TempDisable)
            {
                SendChatMessage($"Emotes rains are now enabled!", null, null);
                m_TempDisable = false;
            }

            if (p_Scene != CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                m_MenuManager.Clear();

            if (p_Scene != CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                m_PlayingManager.Clear();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On settings changed
        /// </summary>
        internal void OnSettingsChanged()
        {
            m_MenuManager.UpdateFromConfig();
            m_PlayingManager.UpdateFromConfig();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update templates from config
        /// </summary>
        /// <param name="p_Scene"></param>
        internal void UpdateTemplateFor(CP_SDK.ChatPlexSDK.EGenericScene p_Scene)
        {
            if (p_Scene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                m_MenuManager.Configure(POOL_SIZE_PER_SCENE, CERConfig.Instance.MenuEmitters, m_PreviewMaterial, CERConfig.Instance.MenuSize, CERConfig.Instance.MenuSpeed, CERConfig.Instance.EmoteDelay);
            else if (p_Scene == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                m_PlayingManager.Configure(POOL_SIZE_PER_SCENE, CERConfig.Instance.SongEmitters, m_PreviewMaterial, CERConfig.Instance.SongSize, CERConfig.Instance.SongSpeed, CERConfig.Instance.EmoteDelay);
        }
        internal void SetTemplatesPreview(CP_SDK.ChatPlexSDK.EGenericScene p_Scene, bool p_Enabled, EmitterConfig p_Focus)
        {
            if (p_Scene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                m_MenuManager.SetPreview(p_Enabled, p_Focus);
            else if (p_Scene == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                m_PlayingManager.SetPreview(p_Enabled, p_Focus);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load assets
        /// </summary>
        private void LoadAssets()
        {
            m_PreviewMateralAssetBundle = AssetBundle.LoadFromMemory(CP_SDK.Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "Resources.PreviewMaterial.bundle"));

            m_PreviewMaterial = m_PreviewMateralAssetBundle.LoadAsset<Material>("PreviewMaterial");
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load SubRain emotes
        /// </summary>
        internal void LoadSubRainFiles()
        {
            m_SubRainTextures.Clear();

            var l_Files = Directory.GetFiles("CustomSubRain", "*.png")
                   .Union(Directory.GetFiles("CustomSubRain", "*.gif"))
                   .Union(Directory.GetFiles("CustomSubRain", "*.apng")).ToArray();

            foreach (string l_CurrentFile in l_Files)
            {
                string l_EmoteName = Path.GetFileNameWithoutExtension(l_CurrentFile);
                l_EmoteName = "$CPM$CER$SR$_" + l_EmoteName.Substring(l_EmoteName.LastIndexOf('\\') + 1);

                CP_SDK.Unity.EnhancedImage.FromFile(l_CurrentFile, l_EmoteName, (p_Result) =>
                {
                    if (p_Result != null)
                        lock (m_SubRainTextures) { m_SubRainTextures.Add(p_Result); }
                });
            }
        }
        /// <summary>
        /// Start a SubRain
        /// </summary>
        public void StartSubRain()
        {
            if (!CERConfig.Instance.SubRain)
                return;

            if (   (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu    && CERConfig.Instance.EnableMenu)
                || (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing && CERConfig.Instance.EnableSong))
            {
                var l_EmitCount = (uint)CERConfig.Instance.SubRainEmoteCount;
                lock (m_SubRainTextures)
                {
                    for (int l_I = 0; l_I < m_SubRainTextures.Count; ++l_I)
                    {
                        var l_Emote = m_SubRainTextures[l_I];
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => EmitEnhancedImage(l_Emote, l_EmitCount));
                    }
                }
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

                        for (int l_EmoteI = 0; l_EmoteI < p_Message.Emotes.Length; ++l_EmoteI)
                            EnqueueEmote(p_Message.Emotes[l_EmoteI], l_Count);

                        SendChatMessage($"@{p_Message.Sender.UserName} Let em' rain!", p_Service, p_Message);
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!", p_Service, p_Message);
                }
                else if (l_LMessage.StartsWith("!er clear"))
                {
                    if (HasPower(p_Message.Sender))
                    {
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                        {
                            if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                                m_MenuManager.Clear();
                            else if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                                m_PlayingManager.Clear();
                        });
                    }
                    else
                        SendChatMessage($"@{p_Message.Sender.UserName} You have no power here!", p_Service, p_Message);
                }
            }

            if (m_TempDisable)
                return;

            if (p_Message.IsSystemMessage && CERConfig.Instance.SubRain && (p_Message.Message.StartsWith("⭐") || p_Message.Message.StartsWith("👑")))
                StartSubRain();

            IChatEmote[] l_Emotes = p_Message.Emotes;
            if (l_Emotes != null && l_Emotes.Length > 0)
            {
                var l_EmotesToRain =
                                    (from iChatEmote in l_Emotes
                                      group iChatEmote by iChatEmote.Id into emoteGrouping
                                      select new { emote = emoteGrouping.First(), count = (byte)emoteGrouping.Count() }
                    ).ToArray();

                for (int l_I = 0; l_I < l_EmotesToRain.Length; ++l_I)
                    EnqueueEmote(l_EmotesToRain[l_I].emote, (uint)l_EmotesToRain[l_I].count);
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
            if (   (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu    && CERConfig.Instance.EnableMenu)
                || (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing && CERConfig.Instance.EnableSong))
            {
                if (!CP_SDK.Chat.ChatImageProvider.CachedImageInfo.TryGetValue(p_Emote.Id, out var l_EnhancedImageInfo))
                {
                    CP_SDK.Chat.ChatImageProvider.TryCacheSingleImage(EChatResourceCategory.Emote, p_Emote.Id, p_Emote.Uri, p_Emote.Animation, (l_Info) =>
                    {
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => EmitEnhancedImage(l_Info, p_Count));
                    });
                }
                else
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => EmitEnhancedImage(l_EnhancedImageInfo, p_Count));
            }
        }
        /// <summary>
        /// Start particle system or update existing one for an Emote
        /// </summary>
        /// <param name="p_EmoteID">ID of the emote</param>
        /// <param name="p_Count">Display count</param>
        /// <returns></returns>
        public void EmitEnhancedImage(CP_SDK.Unity.EnhancedImage p_EnhancedImage, uint p_Count)
        {
            if (p_EnhancedImage == null)
                return;

            if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu)
                m_MenuManager.Emit(p_EnhancedImage, p_Count);
            else if (CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing)
                m_PlayingManager.Emit(p_EnhancedImage, p_Count);
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
                CP_SDK.Chat.Service.BroadcastMessage("! " + p_Message);
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
