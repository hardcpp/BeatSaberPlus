using CP_SDK.Chat.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ChatPlexMod_ChatIntegrations
{
    /// <summary>
    /// ChatIntegrations instance
    /// </summary>
    public partial class ChatIntegrations : CP_SDK.ModuleBase<ChatIntegrations>
    {
        public static string s_DATABASE_FILE            => Path.Combine(CIConfig.Instance.DataLocation, "Database.json");
        public static string s_EXPORT_PATH              => Path.Combine(CIConfig.Instance.DataLocation, "Export/");
        public static string s_IMPORT_PATH              => Path.Combine(CIConfig.Instance.DataLocation, "Import/");
        public static string s_EMOTE_RAIN_ASSETS_PATH   => Path.Combine(CIConfig.Instance.DataLocation, "Assets/EmoteRain/");
        public static string s_SOUND_CLIPS_ASSETS_PATH  => Path.Combine(CIConfig.Instance.DataLocation, "Assets/SoundClips/");

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override CP_SDK.EIModuleBaseType             Type                => CP_SDK.EIModuleBaseType.Integrated;
        public override string                              Name                => "Chat Integrations";
        public override string                              Description         => "Create cool & tights integration with your chat!";
        public override string                              DocumentationURL    => "https://github.com/hardcpp/BeatSaberPlus/wiki#chat-integrations";
        public override bool                                UseChatFeatures     => true;
        public override bool                                IsEnabled           { get => CIConfig.Instance.Enabled; set { CIConfig.Instance.Enabled = value; CIConfig.Instance.Save(); } }
        public override CP_SDK.EIModuleBaseActivationType   ActivationType      => CP_SDK.EIModuleBaseActivationType.OnMenuSceneLoaded;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private static bool m_RegisteringInternalTypes = false;

        private static List<string> m_RegisteredEventTypes              = new List<string>();
        private static List<string> m_RegisteredGlobalConditionsTypes   = new List<string>();
        private static List<string> m_RegisteredGlobalActionsTypes      = new List<string>();

        private static Dictionary<string, Func<Interfaces.IEventBase>>      m_RegisteredEventFuncs              = new Dictionary<string, Func<Interfaces.IEventBase>>();
        private static Dictionary<string, Func<Interfaces.IConditionBase>>  m_RegisteredGlobalConditionsFuncs   = new Dictionary<string, Func<Interfaces.IConditionBase>>();
        private static Dictionary<string, Func<Interfaces.IActionBase>>     m_RegisteredGlobalActionsFuncs      = new Dictionary<string, Func<Interfaces.IActionBase>>();
        private static Dictionary<string, Func<Interfaces.IEventBase>>      m_RegisteredTemplates               = new Dictionary<string, Func<Interfaces.IEventBase>>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static IReadOnlyList<string> RegisteredEventTypes            = m_RegisteredEventTypes.AsReadOnly();
        public static IReadOnlyList<string> RegisteredGlobalConditionsTypes = m_RegisteredGlobalConditionsTypes.AsReadOnly();
        public static IReadOnlyList<string> RegisteredGlobalActionsTypes    = m_RegisteredGlobalActionsTypes.AsReadOnly();

        public static IReadOnlyDictionary<string, Func<Interfaces.IEventBase>> RegisteredTemplates = new ReadOnlyDictionary<string, Func<Interfaces.IEventBase>>(m_RegisteredTemplates);

        public static event Action OnModuleEnable;
        public static event Action OnModuleDisable;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private UI.SettingsLeftView     m_SettingsLeftView  = null;
        private UI.SettingsMainView     m_SettingsMainView  = null;
        private UI.SettingsRightView    m_SettingsRightView = null;

        private bool m_ChatCoreAcquired     = false;
        private bool m_OBSAcquired          = false;
        private bool m_VoiceAttackAcquired  = false;

        private List<Interfaces.IEventBase> m_Events = new List<Interfaces.IEventBase>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public IReadOnlyList<Interfaces.IEventBase> Events;

        public Action<IChatMessage>     OnBroadcasterChatMessage        = null;
        public Action<string, string>   OnVoiceAttackCommandExecuted    = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enable the Module
        /// </summary>
        protected override void OnEnable()
        {
            if (!Directory.Exists(s_EXPORT_PATH))               Directory.CreateDirectory(s_EXPORT_PATH);
            if (!Directory.Exists(s_IMPORT_PATH))               Directory.CreateDirectory(s_IMPORT_PATH);
            if (!Directory.Exists(s_EMOTE_RAIN_ASSETS_PATH))    Directory.CreateDirectory(s_EMOTE_RAIN_ASSETS_PATH);
            if (!Directory.Exists(s_SOUND_CLIPS_ASSETS_PATH))   Directory.CreateDirectory(s_SOUND_CLIPS_ASSETS_PATH);

            RegisterInternalTypes();

            /// Create read only list
            Events = m_Events.AsReadOnly();

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
                var l_Multiplexer = CP_SDK.Chat.Service.Multiplexer;
                l_Multiplexer.OnJoinChannel           += ChatCoreMutiplixer_OnJoinChannel;
                l_Multiplexer.OnChannelFollow         += ChatCoreMutiplixer_OnChannelFollow;
                l_Multiplexer.OnChannelBits           += ChatCoreMutiplixer_OnChannelBits;
                l_Multiplexer.OnChannelPoints         += ChatCoreMutiplixer_OnChannelPoints;
                l_Multiplexer.OnChannelRaid           += ChatCoreMutiplexer_OnChannelRaid;
                l_Multiplexer.OnChannelSubscription   += ChatCoreMutiplixer_OnChannelSubscription;
                l_Multiplexer.OnTextMessageReceived   += ChatCoreMutiplixer_OnTextMessageReceived;
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

            try { OnModuleEnable?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.OnEnable] Error:");
                Logger.Instance.Error(l_Exception);
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
        }
        /// <summary>
        /// Disable the Module
        /// </summary>
        protected override void OnDisable()
        {
            /// Fake disable events for integrations
            m_Events.ForEach(x =>
            {
                if (x.IsEnabled)
                    x.OnDisable();
            });

            /// Save events
            SaveDatabase();

            try { OnModuleDisable?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.OnDisable] Error:");
                Logger.Instance.Error(l_Exception);
            }

            /// Clear database
            m_Events.Clear();

            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsLeftView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsMainView);
            CP_SDK.UI.UISystem.DestroyUI(ref m_SettingsRightView);

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
                var l_Multiplexer = CP_SDK.Chat.Service.Multiplexer;
                l_Multiplexer.OnJoinChannel         -= ChatCoreMutiplixer_OnJoinChannel;
                l_Multiplexer.OnChannelFollow       -= ChatCoreMutiplixer_OnChannelFollow;
                l_Multiplexer.OnChannelBits         -= ChatCoreMutiplixer_OnChannelBits;
                l_Multiplexer.OnChannelPoints       -= ChatCoreMutiplixer_OnChannelPoints;
                l_Multiplexer.OnChannelRaid         -= ChatCoreMutiplexer_OnChannelRaid;
                l_Multiplexer.OnChannelSubscription -= ChatCoreMutiplixer_OnChannelSubscription;
                l_Multiplexer.OnTextMessageReceived -= ChatCoreMutiplixer_OnTextMessageReceived;

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
        protected override (CP_SDK.UI.IViewController, CP_SDK.UI.IViewController, CP_SDK.UI.IViewController) GetSettingsViewControllersImplementation()
        {
            if (m_SettingsLeftView == null)     m_SettingsLeftView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsLeftView>();
            if (m_SettingsMainView == null)     m_SettingsMainView  = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsMainView>();
            if (m_SettingsRightView == null)    m_SettingsRightView = CP_SDK.UI.UISystem.CreateViewController<UI.SettingsRightView>();

            return (m_SettingsMainView, m_SettingsLeftView, m_SettingsRightView);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create event by type name
        /// </summary>
        /// <param name="p_Type">Type name</param>
        /// <returns></returns>
        internal static Interfaces.IEventBase CreateEvent(string p_Type)
        {
            if (!m_RegisteredEventFuncs.TryGetValue(p_Type, out var l_Func))
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.CreateEvent] Type \"{p_Type}\" missing");
                return null;
            }

            return l_Func();
        }
        /// <summary>
        /// Add an event
        /// </summary>
        /// <param name="p_Event">Event instance</param>
        internal void AddEvent(Interfaces.IEventBase p_Event)
        {
            lock (m_Events)
            {
                m_Events.Add(p_Event);
                m_Events.Sort((x, y) => (x.GetTypeName() + !x.IsEnabled + x.GenericModel.Name).CompareTo(y.GetTypeName() + !y.IsEnabled + y.GenericModel.Name));
            }

            if (p_Event.IsEnabled)
                p_Event.OnEnable();

            SaveDatabase();
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
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Can't find event type\n\"{p_JSON.ToString()}\"");
                return null;
            }

            var l_EventType = GetPatchedTypeName(p_JSON["Type"].Value<string>());
            p_JSON["Type"] = l_EventType;

            /// Create instance
            var l_NewEvent = CreateEvent(l_EventType);
            if (l_NewEvent == null)
            {
                /// Todo backup this event to avoid loss
                p_Error = "Event type \"" + l_EventType.Split('.').LastOrDefault() + "\" not found";
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Missing event type \"{l_EventType}\"");
                return null;
            }

            /// Unserialize
            if (!l_NewEvent.Unserialize(p_JSON, out p_Error))
            {
                /// Todo backup this event to avoid loss
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.AddEventFromSerialized] Failed to unserialize event\n\"{p_JSON.ToString()}\" \"{p_Error}\"");
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
                m_Events.Sort((x, y) => (x.GetTypeName() + !x.IsEnabled + x.GenericModel.Name).CompareTo(y.GetTypeName() + !y.IsEnabled + y.GenericModel.Name));
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
                m_Events.Sort((x, y) => (x.GetTypeName() + !x.IsEnabled + x.GenericModel.Name).CompareTo(y.GetTypeName() + !y.IsEnabled + y.GenericModel.Name));
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
                var l_EventCount = m_Events.Count;
                for (var l_I = 0; l_I < l_EventCount; ++l_I)
                {
                    var l_Event = m_Events[l_I];
                    if (!l_Event.IsEnabled || !l_Event.Handle((Models.EventContext)p_Context.Clone()))
                        continue;

                    l_Event.GenericModel.UsageCount++;
                    l_Event.GenericModel.LastUsageDate = CP_SDK.Misc.Time.UnixTimeNow();
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

            p_Event.GenericModel.UsageCount++;
            p_Event.GenericModel.LastUsageDate = CP_SDK.Misc.Time.UnixTimeNow();

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create event by type name
        /// </summary>
        /// <param name="p_Type">Type name</param>
        /// <returns></returns>
        internal static Interfaces.IConditionBase CreateCondition(string p_Type)
        {
            if (!m_RegisteredGlobalConditionsFuncs.TryGetValue(p_Type, out var l_Func))
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.CreateCondition] Type \"{p_Type}\" missing");
                return null;
            }

            return l_Func();
        }
        /// <summary>
        /// Create event by type name
        /// </summary>
        /// <param name="p_Type">Type name</param>
        /// <returns></returns>
        internal static Interfaces.IActionBase CreateAction(string p_Type)
        {
            if (!m_RegisteredGlobalActionsFuncs.TryGetValue(p_Type, out var l_Func))
            {
                Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.CreateAction] Type \"{p_Type}\" missing");
                return null;
            }

            return l_Func();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Register internal types
        /// </summary>
        internal static void RegisterInternalTypes()
        {
            try
            {
                if (m_RegisteringInternalTypes)
                    return;

                m_RegisteringInternalTypes = true;

                RegisterEventType("ChatBits"          , () => new Events.ChatBits());
                RegisterEventType("ChatCommand"       , () => new Events.ChatCommand());
                RegisterEventType("ChatFollow"        , () => new Events.ChatFollow());
                RegisterEventType("ChatPointsReward"  , () => new Events.ChatPointsReward());
                RegisterEventType("ChatRaid"          , () => new Events.ChatRaid());
                RegisterEventType("ChatSubscription"  , () => new Events.ChatSubscription());
                RegisterEventType("Dummy"             , () => new Events.Dummy());
                RegisterEventType("VoiceAttackCommand", () => new Events.VoiceAttackCommand());
                /// todo GameStart
                /// todo GameStop

                Conditions.EventRegistration.Register();
                Conditions.MiscRegistration.Register();
                Conditions.OBSRegistration.Register();

                Actions.ChatRegistration.Register();
                Actions.EmoteRainRegistration.Register();
                Actions.EventRegistration.Register();
                Actions.MiscRegistration.Register();
                Actions.OBSRegistration.Register();
                Actions.TwitchRegistration.Register();

                RegisterTemplate("ChatPointReward : Countdown + Emote bomb", () =>
                {
                    var l_Event = new Events.ChatPointsReward();
                    l_Event.Model.Cooldown  = 30;
                    l_Event.Model.Cost      = 100;
                    l_Event.Model.Name      = "Countdown + Emote bomb (Template)";
                    l_Event.Model.Title     = "Emote bomb (Template)";

                    var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "Explosion in...";
                    l_Event.AddOnSuccessAction(l_MessageAction);
                    l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "3...";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    var l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                    l_DelayAction.Model.Delay = 1;
                    l_DelayAction.Model.PreventNextActionFailure = false;
                    l_Event.AddOnSuccessAction(l_DelayAction);

                    l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "2...";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                    l_DelayAction.Model.Delay = 1;
                    l_DelayAction.Model.PreventNextActionFailure = false;
                    l_Event.AddOnSuccessAction(l_DelayAction);

                    l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "1...";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                    l_EmoteBombAction.Model.EmoteKindCount = 25;
                    l_EmoteBombAction.Model.CountPerEmote = 40;
                    l_Event.AddOnSuccessAction(l_EmoteBombAction);

                    l_DelayAction = new Actions.Misc_Delay() { Event = l_Event, IsEnabled = true };
                    l_DelayAction.Model.Delay = 1;
                    l_DelayAction.Model.PreventNextActionFailure = false;
                    l_Event.AddOnSuccessAction(l_DelayAction);

                    l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "BOOM!";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    return l_Event;
                });
                RegisterTemplate("ChatBits : Thanks message + emote bomb", () =>
                {
                    var l_Event = new Events.ChatBits();
                    l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                    var l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                    l_CooldownCondition.Model.PerUser       = true;
                    l_CooldownCondition.Model.NotifyUser    = false;
                    l_CooldownCondition.Model.CooldownTime  = 20;
                    l_Event.Conditions.Add(l_CooldownCondition);

                    var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                    l_EmoteBombAction.Model.EmoteKindCount  = 10;
                    l_EmoteBombAction.Model.CountPerEmote   = 10;
                    l_Event.AddOnSuccessAction(l_EmoteBombAction);

                    var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "Thanks $UserName for the $Bits bits!";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    return l_Event;
                });
                RegisterTemplate("ChatSubscription : Thanks message + emote bomb", () =>
                {
                    var l_Event = new Events.ChatSubscription();
                    l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                    var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                    l_EmoteBombAction.Model.EmoteKindCount  = 10;
                    l_EmoteBombAction.Model.CountPerEmote   = 10;
                    l_Event.AddOnSuccessAction(l_EmoteBombAction);

                    var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "Thanks $UserName for the $MonthCount of $SubPlan!";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    return l_Event;
                });
                RegisterTemplate("ChatFollow : Thanks message + emote bomb", () =>
                {
                    var l_Event = new Events.ChatFollow();
                    l_Event.Model.Name = "Thanks message + emote bomb (Template)";

                    var l_CooldownCondition = new Conditions.Misc_Cooldown() { Event = l_Event, IsEnabled = true };
                    l_CooldownCondition.Model.PerUser       = true;
                    l_CooldownCondition.Model.NotifyUser    = false;
                    l_CooldownCondition.Model.CooldownTime  = 20 * 60;
                    l_Event.Conditions.Add(l_CooldownCondition);

                    var l_EmoteBombAction = new Actions.EmoteRain_EmoteBombRain() { Event = l_Event, IsEnabled = true };
                    l_EmoteBombAction.Model.EmoteKindCount  = 5;
                    l_EmoteBombAction.Model.CountPerEmote   = 5;
                    l_Event.AddOnSuccessAction(l_EmoteBombAction);

                    var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "Thanks $UserName for the follow!";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    return l_Event;
                });
                RegisterTemplate("ChatCommand : Discord command", () =>
                {
                    var l_Event = new Events.ChatCommand();
                    l_Event.Model.Name      = "Discord command (Template)";
                    l_Event.Model.Command   = "!discord";

                    var l_MessageAction = new Actions.Chat_SendMessage() { Event = l_Event, IsEnabled = true };
                    l_MessageAction.Model.BaseValue = "@$UserName join my amazing discord at https://discord.chatplex.org";
                    l_Event.AddOnSuccessAction(l_MessageAction);

                    return l_Event;
                });
            }
            catch (System.Exception l_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_ChatIntegrations][ChatIntegrations.RegisterInternalTypes] Error:");
                Logger.Instance.Error(l_Exception);
            }
        }
        /// <summary>
        /// Register event type
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Func">Create func</param>
        /// <param name="p_SilentFail">Should fail silently</param>
        public static void RegisterEventType(string p_Name, Func<Interfaces.IEventBase> p_Func, bool p_SilentFail = false)
        {
            RegisterInternalTypes();
            if (m_RegisteredEventTypes.Contains(p_Name))
            {
                if (!p_SilentFail)
                    Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.RegisterEventType] Type \"{p_Name}\" already registered");
                return;
            }

            m_RegisteredEventTypes.Add(p_Name);

            if (!m_RegisteredEventFuncs.ContainsKey(p_Name))
                m_RegisteredEventFuncs.Add(p_Name, p_Func);
        }
        /// <summary>
        /// Register global condition type
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Func">Create func</param>
        /// <param name="p_SilentFail">Should fail silently</param>
        /// <param name="p_NonGlobal">Is non global</param>
        public static void RegisterConditionType(string p_Name, Func<Interfaces.IConditionBase> p_Func, bool p_SilentFail = false, bool p_NonGlobal = false)
        {
            RegisterInternalTypes();
            if (!p_NonGlobal)
            {
                if (m_RegisteredGlobalConditionsTypes.Contains(p_Name))
                {
                    if (!p_SilentFail)
                        Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.RegisterGlobalConditionType] Type \"{p_Name}\" already registered");
                    return;
                }

                m_RegisteredGlobalConditionsTypes.Add(p_Name);
            }

            if (!m_RegisteredGlobalConditionsFuncs.ContainsKey(p_Name))
                m_RegisteredGlobalConditionsFuncs.Add(p_Name, p_Func);
        }
        /// <summary>
        /// Register global action type
        /// </summary>
        /// <param name="p_Name">Name</param>
        /// <param name="p_Func">Create func</param>
        /// <param name="p_SilentFail">Should fail silently</param>
        /// <param name="p_NonGlobal">Is non global</param>
        public static void RegisterActionType(string p_Name, Func<Interfaces.IActionBase> p_Func, bool p_SilentFail = false, bool p_NonGlobal = false)
        {
            RegisterInternalTypes();
            if (!p_NonGlobal)
            {
                if (m_RegisteredGlobalActionsTypes.Contains(p_Name))
                {
                    if (!p_SilentFail)
                        Logger.Instance.Error($"[ChatPlexMod_ChatIntegrations][ChatIntegrations.RegisterGlobalActionType] Type \"{p_Name}\" already registered");
                    return;
                }

                m_RegisteredGlobalActionsTypes.Add(p_Name);
            }

            if (!m_RegisteredGlobalActionsFuncs.ContainsKey(p_Name))
                m_RegisteredGlobalActionsFuncs.Add(p_Name, p_Func);
        }
        /// <summary>
        /// Register a template
        /// </summary>
        /// <param name="p_Name">Template name</param>
        /// <param name="p_Func">Func</param>
        public static void RegisterTemplate(string p_Name, Func<Interfaces.IEventBase> p_Func)
        {
            if (m_RegisteredTemplates.ContainsKey(p_Name))
                return;

            m_RegisteredTemplates.Add(p_Name, p_Func);
        }
    }
}
