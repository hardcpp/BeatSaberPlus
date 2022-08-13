using BeatSaberMarkupLanguage;
using CP_SDK.Chat.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace BeatSaberPlus_ChatIntegrations
{
    /// <summary>
    /// ChatIntegrations instance
    /// </summary>
    public partial class ChatIntegrations : BeatSaberPlus.SDK.BSPModuleBase<ChatIntegrations>
    {
        /// <summary>
        /// Old database file
        /// </summary>
        private static string s_OLD_DATABASE_FILE = "UserData/BeatSaberPlus/ChatIntegrations.json";
        /// <summary>
        /// Database file path
        /// </summary>
        private static string s_DATABASE_FILE { get => Path.Combine(CIConfig.Instance.DataLocation, "Database.json"); }
        /// <summary>
        /// Export folder
        /// </summary>
        public static string s_EXPORT_PATH { get => Path.Combine(CIConfig.Instance.DataLocation, "Export/"); }
        /// <summary>
        /// Import folder
        /// </summary>
        public static string s_IMPORT_PATH { get => Path.Combine(CIConfig.Instance.DataLocation, "Import/"); }
        /// <summary>
        /// EmoteRain assets folder
        /// </summary>
        public static string s_EMOTE_RAIN_ASSETS_PATH { get => Path.Combine(CIConfig.Instance.DataLocation, "Assets/EmoteRain/"); }
        /// <summary>
        /// Sound clips assets folder
        /// </summary>
        public static string s_SOUND_CLIPS_ASSETS_PATH { get => Path.Combine(CIConfig.Instance.DataLocation, "Assets/SoundClips/"); }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch client ID for BeatSaberPlus
        /// </summary>
        public static string s_BEATSABERPLUS_CLIENT_ID = "23vjr9ec2cwoddv2fc3xfbx9nxv8vi";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Module type
        /// </summary>
        public override CP_SDK.EIModuleBaseType Type => CP_SDK.EIModuleBaseType.Integrated;
        /// <summary>
        /// Name of the Module
        /// </summary>
        public override string Name => "Chat Integrations";
        /// <summary>
        /// Description of the Module
        /// </summary>
        public override string Description => "Create cool & tights integration with your chat!";
        /// <summary>
        /// Is the Module using chat features
        /// </summary>
        public override bool UseChatFeatures => true;
        /// <summary>
        /// Is enabled
        /// </summary>
        public override bool IsEnabled { get => CIConfig.Instance.Enabled; set { CIConfig.Instance.Enabled = value; CIConfig.Instance.Save(); } }
        /// <summary>
        /// Activation kind
        /// </summary>
        public override CP_SDK.EIModuleBaseActivationType ActivationType => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static List<Interfaces.IEventBase> m_RegisteredEventTypes = new List<Interfaces.IEventBase>()
        {
            new Events.ChatBits(),
            new Events.ChatCommand(),
            new Events.ChatFollow(),
            new Events.ChatPointsReward(),
            new Events.ChatRaid(),
            new Events.ChatSubscription(),
            new Events.Dummy(),
            new Events.LevelEnded(),
            new Events.LevelPaused(),
            new Events.LevelResumed(),
            new Events.LevelStarted(),
            new Events.VoiceAttackCommand()
            /// todo GameStart
            /// todo GameStop
        };
        /// <summary>
        /// Registered event types
        /// </summary>
        public static IReadOnlyList<Interfaces.IEventBase> RegisteredEventTypes = m_RegisteredEventTypes.AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings view
        /// </summary>
        private UI.Settings m_SettingsView = null;
        /// <summary>
        /// Settings left view
        /// </summary>
        private UI.SettingsLeft m_SettingsLeftView = null;
        /// <summary>
        /// Settings right view
        /// </summary>
        private UI.SettingsRight m_SettingsRightView = null;
        /// <summary>
        /// Chat core instance
        /// </summary>
        private bool m_ChatCoreAcquired = false;
        /// <summary>
        /// OBS instance
        /// </summary>
        private bool m_OBSAcquired = false;
        /// <summary>
        /// Voice attack instance
        /// </summary>
        private bool m_VoiceAttackAcquired = false;
        /// <summary>
        /// Events
        /// </summary>
        private List<Interfaces.IEventBase> m_Events = new List<Interfaces.IEventBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event list
        /// </summary>
        public IReadOnlyList<Interfaces.IEventBase> Events;
        /// <summary>
        /// On broadcaster message
        /// </summary>
        public Action<IChatMessage> OnBroadcasterChatMessage = null;
        /// <summary>
        /// On voice attack command executed
        /// </summary>
        public Action<string, string> OnVoiceAttackCommandExecuted = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            if (!Directory.Exists(s_EXPORT_PATH))
                Directory.CreateDirectory(s_EXPORT_PATH);

            if (!Directory.Exists(s_IMPORT_PATH))
                Directory.CreateDirectory(s_IMPORT_PATH);

            if (!Directory.Exists(s_EMOTE_RAIN_ASSETS_PATH))
                Directory.CreateDirectory(s_EMOTE_RAIN_ASSETS_PATH);

            if (!Directory.Exists(s_SOUND_CLIPS_ASSETS_PATH))
                Directory.CreateDirectory(s_SOUND_CLIPS_ASSETS_PATH);

            /// Create read only list
            RegisteredEventTypes    = m_RegisteredEventTypes.AsReadOnly();
            Events                  = new ReadOnlyCollection<Interfaces.IEventBase>(m_Events);

            /// Load database
            if (!LoadDatabase())
            {
                /// todo : create basic samples
            }

            if (!m_ChatCoreAcquired)
            {
                /// Init chat core
                m_ChatCoreAcquired = true;
                CP_SDK.Chat.Service.Acquire();

                /// Run all services
                CP_SDK.Chat.Service.Multiplexer.OnJoinChannel           += ChatCoreMutiplixer_OnJoinChannel;
                CP_SDK.Chat.Service.Multiplexer.OnChannelFollow         += ChatCoreMutiplixer_OnChannelFollow;
                CP_SDK.Chat.Service.Multiplexer.OnChannelBits           += ChatCoreMutiplixer_OnChannelBits;
                CP_SDK.Chat.Service.Multiplexer.OnChannelPoints         += ChatCoreMutiplixer_OnChannelPoints;
                CP_SDK.Chat.Service.Multiplexer.OnChannelRaid           += ChatCoreMutiplexer_OnChannelRaid;
                CP_SDK.Chat.Service.Multiplexer.OnChannelSubscription   += ChatCoreMutiplixer_OnChannelSubscription;
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived   += ChatCoreMutiplixer_OnTextMessageReceived;
            }

            if (!m_OBSAcquired)
            {
                /// Init OBS
                m_OBSAcquired = true;
                CP_SDK.OBS.Service.Acquire();
            }

            if (!m_VoiceAttackAcquired)
            {
                /// Init voice attack
                m_VoiceAttackAcquired = true;
                CP_SDK.VoiceAttack.Service.Acquire();

                /// Run all services
                CP_SDK.VoiceAttack.Service.OnCommandExecuted += VoiceAttack_OnCommandExecuted;
            }

            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count != 0)
            {
                var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
                if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel l_TwitchChannel)
                {
                    m_Events.ForEach(x =>
                    {
                        /// Re enabled channel points reward on join
                        if (x.IsEnabled)
                            x.OnEnable();
                    });
                }
            }

            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted += Game_OnLevelStarted;
            BeatSaberPlus.SDK.Game.Logic.OnLevelEnded   += Game_OnLevelEnded;
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            BeatSaberPlus.SDK.Game.Logic.OnLevelStarted -= Game_OnLevelStarted;
            BeatSaberPlus.SDK.Game.Logic.OnLevelEnded   -= Game_OnLevelEnded;

            /// Fake disable events for integrations
            m_Events.ForEach(x =>
            {
                if (x.IsEnabled)
                    x.OnDisable();
            });

            /// Save events
            SaveDatabase();

            /// Clear database
            m_Events.Clear();

            /// Un-init voice attack
            if (m_VoiceAttackAcquired)
            {
                /// Unbind services
                CP_SDK.VoiceAttack.Service.OnCommandExecuted -= VoiceAttack_OnCommandExecuted;

                /// Stop all voice attack services
                CP_SDK.VoiceAttack.Service.Release();
                m_VoiceAttackAcquired = false;
            }

            if (m_OBSAcquired)
            {
                /// Release OBS service
                CP_SDK.OBS.Service.Release();
                m_OBSAcquired = false;
            }

            /// Un-init chat core
            if (m_ChatCoreAcquired)
            {
                /// Unbind services
                CP_SDK.Chat.Service.Multiplexer.OnJoinChannel         -= ChatCoreMutiplixer_OnJoinChannel;
                CP_SDK.Chat.Service.Multiplexer.OnChannelFollow       -= ChatCoreMutiplixer_OnChannelFollow;
                CP_SDK.Chat.Service.Multiplexer.OnChannelBits         -= ChatCoreMutiplixer_OnChannelBits;
                CP_SDK.Chat.Service.Multiplexer.OnChannelPoints       -= ChatCoreMutiplixer_OnChannelPoints;
                CP_SDK.Chat.Service.Multiplexer.OnChannelRaid         -= ChatCoreMutiplexer_OnChannelRaid;
                CP_SDK.Chat.Service.Multiplexer.OnChannelSubscription -= ChatCoreMutiplixer_OnChannelSubscription;
                CP_SDK.Chat.Service.Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

                /// Stop all chat services
                CP_SDK.Chat.Service.Release();
                m_ChatCoreAcquired = false;
            }
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
            if (m_SettingsLeftView == null)
                m_SettingsLeftView = BeatSaberUI.CreateViewController<UI.SettingsLeft>();
            if (m_SettingsRightView == null)
                m_SettingsRightView = BeatSaberUI.CreateViewController<UI.SettingsRight>();

            return (m_SettingsView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register custom event type
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        public static void RegisterCustomEventType<TEvent>() where TEvent : Interfaces.IEventBase, new()
        {
            m_RegisteredEventTypes.Add(new TEvent());
            RegisteredEventTypes = m_RegisteredEventTypes.AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add an event
        /// </summary>
        /// <param name="p_Event">Event instance</param>
        internal void AddEvent(Interfaces.IEventBase p_Event)
        {
            lock (m_Events)
            {
                m_Events.Add(p_Event);
                m_Events = m_Events.OrderBy((x) => x.GetTypeNameShort() + !x.IsEnabled + x.GenericModel.Name).ToList();
                Events = new ReadOnlyCollection<Interfaces.IEventBase>(m_Events);
            }

            if (p_Event.IsEnabled)
                p_Event.OnEnable();
        }
        /// <summary>
        /// Add an event
        /// </summary>
        /// <param name="p_JSON">Event</param>
        /// <param name="p_IsImport">Is an import</param>
        /// <param name="p_IsClone">Is a clone</param>
        internal Interfaces.IEventBase AddEventFromSerialized(JObject p_JSON, bool p_IsImport, bool p_IsClone, out string p_Error)
        {
            if (!p_JSON.ContainsKey("Type"))
            {
                p_Error = "Event doesn't have a valid type";
                Logger.Instance?.Error($"[Modules.ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Can't find event type\n\"{p_JSON.ToString()}\"");
                return null;
            }

            var l_EventType = p_JSON["Type"].Value<string>();

            if (l_EventType.StartsWith("BeatSaberPlus.Modules.ChatIntegrations."))
            {
                l_EventType = l_EventType.Replace("BeatSaberPlus.Modules.ChatIntegrations.", "BeatSaberPlus_ChatIntegrations.");
                p_JSON["Type"] = l_EventType;
            }

            var l_MatchingType  = m_RegisteredEventTypes.Where(x => x.GetTypeName() == l_EventType).FirstOrDefault();

            if (l_MatchingType == null)
            {
                /// Todo backup this event to avoid loss
                p_Error = "Event type \"" + l_EventType.Split('.').LastOrDefault() + "\" not found";
                Logger.Instance?.Error($"[Modules.ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Missing event type \"{l_EventType}\"");
                return null;
            }

            /// Create instance
            var l_NewEvent = Activator.CreateInstance(l_MatchingType.GetType()) as Interfaces.IEventBase;

            /// Unserialize event
            if (!l_NewEvent.Unserialize(p_JSON, out p_Error))
            {
                /// Todo backup this event to avoid loss
                Logger.Instance?.Error($"[Modules.ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Failed to unserialize event\n\"{p_JSON.ToString()}\" \"{p_Error}\"");
                return null;
            }

            if (p_IsImport || p_IsClone)
            {
                l_NewEvent.OnImportOrClone(p_IsImport, p_IsClone);

                if (l_NewEvent.IsEnabled)
                    l_NewEvent.OnEnable();
            }

            /// Avoid GUID conflict
            if (GetEventByGUID(l_NewEvent.GenericModel.GUID) != null)
                l_NewEvent.GenericModel.GUID = Guid.NewGuid().ToString();

            lock (m_Events)
            {
                m_Events.Add(l_NewEvent);
                m_Events    = m_Events.OrderBy((x) => x.GetTypeNameShort() + !x.IsEnabled + x.GenericModel.Name).ToList();
                Events      = new ReadOnlyCollection<Interfaces.IEventBase>(m_Events);
            }

            p_Error = "";
            return l_NewEvent;
        }
        /// <summary>
        /// Get event by name
        /// </summary>
        /// <param name="p_GUID">Event name</param>
        /// <returns></returns>
        public Interfaces.IEventBase GetEventByName(string p_Name)
        {
            lock (m_Events)
            {
                return m_Events.Where(x => x.GenericModel.Name == p_Name).FirstOrDefault();
            }
        }
        /// <summary>
        /// Get event by GUID
        /// </summary>
        /// <param name="p_GUID">Event GUID</param>
        /// <returns></returns>
        public Interfaces.IEventBase GetEventByGUID(string p_GUID)
        {
            lock (m_Events)
            {
                return m_Events.Where(x => x.GenericModel.GUID == p_GUID).FirstOrDefault();
            }
        }
        /// <summary>
        /// Get events by type
        /// </summary>
        /// <param name="p_Type">Type</param>
        /// <returns></returns>
        public List<Interfaces.IEventBase> GetEventsByType(Type p_Type)
        {
            lock (m_Events)
            {
                return m_Events.Where(x => p_Type == null || p_Type.IsAssignableFrom(x.GetType())).ToList();
            }
        }
        /// <summary>
        /// Toggle an event
        /// </summary>
        /// <param name="p_Event">Event instance</param>
        internal void ToggleEvent(Interfaces.IEventBase p_Event)
        {
            p_Event.IsEnabled = !p_Event.IsEnabled;
            if (p_Event.IsEnabled)
                p_Event.OnEnable();
            else
                p_Event.OnDisable();

            lock (m_Events)
            {
                m_Events = m_Events.OrderBy((x) => x.GetTypeNameShort() + !x.IsEnabled + x.GenericModel.Name).ToList();
                Events = new ReadOnlyCollection<Interfaces.IEventBase>(m_Events);
            }
        }
        /// <summary>
        /// Delete an event
        /// </summary>
        /// <param name="p_Event">Event instance</param>
        internal void DeleteEvent(Interfaces.IEventBase p_Event)
        {
            p_Event.OnDelete();

            lock (m_Events)
            {
                m_Events.Remove(p_Event);
                Events = new ReadOnlyCollection<Interfaces.IEventBase>(m_Events);
            }
        }
        /// <summary>
        /// Handle events
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public void HandleEvents(Models.EventContext p_Context)
        {
            lock (m_Events)
            {
                foreach (var l_Event in m_Events)
                {
                    if (!l_Event.IsEnabled || !l_Event.Handle((Models.EventContext)p_Context.Clone()))
                        continue;

                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                    {
                        l_Event.GenericModel.UsageCount++;
                        l_Event.GenericModel.LastUsageDate = CP_SDK.Misc.Time.UnixTimeNow();
                    });
                }
            }
        }
        /// <summary>
        /// Execute event
        /// </summary>
        /// <param name="p_Event">Event to execute</param>
        /// <param name="p_Context">Execution context</param>
        /// <returns></returns>
        public bool ExecuteEvent(Interfaces.IEventBase p_Event, Models.EventContext p_Context)
        {
            if (p_Event == null)
                return false;

            if (!p_Event.IsEnabled || !p_Event.Handle((Models.EventContext)p_Context.Clone()))
                return false;

            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
            {
                p_Event.GenericModel.UsageCount++;
                p_Event.GenericModel.LastUsageDate = CP_SDK.Misc.Time.UnixTimeNow();
            });

            return true;
        }
    }
}
