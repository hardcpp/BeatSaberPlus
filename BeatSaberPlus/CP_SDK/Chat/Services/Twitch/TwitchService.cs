using CP_SDK.Chat.Interfaces;
using CP_SDK.Chat.Models.Twitch;
using CP_SDK.Chat.SimpleJSON;
using CP_SDK.Unity.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace CP_SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch service
    /// </summary>
    public class TwitchService : ChatServiceBase, IChatService
    {
        /// <summary>
        /// Twitch application ID
        /// </summary>
        internal const string TWITCH_CLIENT_ID = "23vjr9ec2cwoddv2fc3xfbx9nxv8vi";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The display name of the service(s)
        /// </summary>
        public string DisplayName { get; } = "Twitch";
        /// <summary>
        /// Side handle of each message color
        /// </summary>
        public Color AccentColor { get; } = ColorU.WithAlpha("#9147FF", 0.75f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Channels
        /// </summary>
        public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => m_Channels.Select(x => (this as IChatService, x.Value as IChatChannel)).ToList().AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// OAuth token
        /// </summary>
        public string OAuthToken => m_TokenChat;
        /// <summary>
        /// OAuth token API
        /// </summary>
        public string OAuthTokenAPI => m_TokenChannel;
        /// <summary>
        /// Helix API instance
        /// </summary>
        public TwitchHelix HelixAPI { get; private set; } = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Required token scopes
        /// </summary>
        public IReadOnlyList<string> RequiredTokenScopes = new List<string>()
        {
            "bits:read",
            "channel:manage:broadcast",
            "channel:manage:polls",
            "channel:manage:predictions",
            "channel:manage:redemptions",
            "channel:moderate",
            "channel:read:redemptions",
            "channel:read:hype_train",
            "channel:read:predictions",
            "channel:read:subscriptions",
            "chat:edit",
            "chat:read",
            "clips:edit",
            "whispers:edit",
            "whispers:read"
        }.AsReadOnly();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Message parser instance
        /// </summary>
        private TwitchMessageParser m_MessageParser;
        /// <summary>
        /// Data provider
        /// </summary>
        private TwitchDataProvider m_DataProvider;
        /// <summary>
        /// IRC socket
        /// </summary>
        private Network.WebSocketClient m_IRCWebSocket;
        /// <summary>
        /// PubSub socket
        /// </summary>
        private Network.WebSocketClient m_PubSubSocket;
        /// <summary>
        /// Random generator
        /// </summary>
        private System.Random m_Random;
        /// <summary>
        /// Is the service started
        /// </summary>
        private bool m_IsStarted = false;
        /// <summary>
        /// OAuth token
        /// </summary>
        private string m_TokenChat { get => string.IsNullOrEmpty(TwitchSettingsConfig.Instance.TokenChat) ? "" : TwitchSettingsConfig.Instance.TokenChat; }
        /// <summary>
        /// OAuth token for API
        /// </summary>
        private string m_TokenChannel { get => string.IsNullOrEmpty(TwitchSettingsConfig.Instance.TokenChannel) ? m_TokenChat : TwitchSettingsConfig.Instance.TokenChannel; }
        /// <summary>
        /// OAuth token cache
        /// </summary>
        private string m_TokenChatCache;
        /// <summary>
        /// OAuth token cache
        /// </summary>
        private string m_TokenChannelCache;
        /// <summary>
        /// Logged in user
        /// </summary>
        private TwitchUser m_LoggedInUser = null;
        /// <summary>
        /// Logged in user name
        /// </summary>
        private string m_LoggedInUsername;
        /// <summary>
        /// Joined channels
        /// </summary>
        private ConcurrentDictionary<string, TwitchChannel> m_Channels = new ConcurrentDictionary<string, TwitchChannel>();
        /// <summary>
        /// Joined topics
        /// </summary>
        private Dictionary<string, List<string>> m_ChannelTopics = new Dictionary<string, List<string>>();
        /// <summary>
        /// Process message queue task
        /// </summary>
        private Task m_ProcessQueuedMessagesTask = null;
        /// <summary>
        /// Update Helix task
        /// </summary>
        private Task m_UpdateHelixTask = null;
        /// <summary>
        /// Message receive lock
        /// </summary>
        private object m_MessageReceivedLock = new object(), m_InitLock = new object();
        /// <summary>
        /// Parsing buffer
        /// </summary>
        private List<TwitchMessage> m_MessageReceivedParsingBuffer = new List<TwitchMessage>(10);
        /// <summary>
        /// Send message queue
        /// </summary>
        private ConcurrentQueue<string> m_TextMessageQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// Current message count
        /// </summary>
        private int m_CurrentSentMessageCount = 0;
        /// <summary>
        /// Last reset time
        /// </summary>
        private DateTime m_LastResetTime = DateTime.UtcNow;
        /// <summary>
        /// Last IRC ping
        /// </summary>
        private DateTime m_LastIRCSubPing = DateTime.UtcNow;
        /// <summary>
        /// Last PubSub ping
        /// </summary>
        private DateTime m_LastPubSubPing = DateTime.UtcNow;
        /// <summary>
        /// Twitch users cache
        /// </summary>
        private ConcurrentDictionary<string, TwitchUser> m_TwitchUsers = new ConcurrentDictionary<string, TwitchUser>();
        /// <summary>
        /// Temp joined channels
        /// </summary>
        private ConcurrentDictionary<string, (string GroupIdentifier, string Prefix, bool CanSend)> m_TempChannels = new ConcurrentDictionary<string, (string GroupIdentifier, string Prefix, bool CanSend)>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public TwitchService()
        {
            TwitchSettingsConfig.Instance.Warmup();

            /// Init
            m_DataProvider  = new TwitchDataProvider();
            m_MessageParser = new TwitchMessageParser(this, m_DataProvider, new FrwTwemojiParser());
            m_Random        = new System.Random();
            HelixAPI        = new TwitchHelix();

            HelixAPI.OnTokenValidate += HelixAPI_OnTokenValidate;

            /// IRC web socket
            m_IRCWebSocket = new Network.WebSocketClient();
            m_IRCWebSocket.OnOpen               += IRCSocket_OnOpen;
            m_IRCWebSocket.OnClose              += IRCSocket_OnClose;
            m_IRCWebSocket.OnError              += IRCSocket_OnError;
            m_IRCWebSocket.OnMessageReceived    += IRCSocket_OnMessageReceived;

            /// PubSub socket
            m_PubSubSocket = new Network.WebSocketClient();
            m_PubSubSocket.OnOpen               += PubSubSocket_OnOpen;
            m_PubSubSocket.OnClose              += PubSubSocket_OnClose;
            m_PubSubSocket.OnError              += PubSubSocket_OnError;
            m_PubSubSocket.OnMessageReceived    += PubSubSocket_OnMessageReceived;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Start the service
        /// </summary>
        public void Start()
        {
            lock (m_InitLock)
            {
                if (!m_IsStarted)
                {
                    m_IsStarted = true;

                    m_ProcessQueuedMessagesTask = Task.Run(ProcessQueuedMessages);
                    m_UpdateHelixTask = Task.Run(UpdateHelix);

                    /// Cache OAuth token
                    m_TokenChatCache       = m_TokenChat;
                    m_TokenChannelCache    = m_TokenChannel;

                    HelixAPI.OnTokenChanged(m_TokenChannel);

                    /// Waiting HelixAPI_OnTokenValidate callback
                }
            }
        }
        /// <summary>
        /// Stop the service
        /// </summary>
        public void Stop()
        {
            if (!m_IsStarted)
                return;

            lock (m_InitLock)
            {
                m_IsStarted = false;

                if (m_ProcessQueuedMessagesTask != null && m_ProcessQueuedMessagesTask.Status == TaskStatus.Running)
                {
                    m_ProcessQueuedMessagesTask.Wait();
                    m_ProcessQueuedMessagesTask = null;
                }

                if (m_UpdateHelixTask != null && m_UpdateHelixTask.Status == TaskStatus.Running)
                {
                    m_UpdateHelixTask.Wait();
                    m_UpdateHelixTask = null;
                }

                m_PubSubSocket.Disconnect();
                m_IRCWebSocket.Disconnect();

                foreach (var l_Channel in m_Channels)
                {
                    m_DataProvider.TryReleaseChannelResources(l_Channel.Value);
                    ChatPlexSDK.Logger.Info($"Removed channel {l_Channel.Value.Id} from the channel list.");
                    m_OnLiveStatusUpdatedCallbacks?.InvokeAll(this, l_Channel.Value, false, 0);
                    m_OnLeaveRoomCallbacks?.InvokeAll(this, l_Channel.Value);
                }
                m_Channels.Clear();
                m_ChannelTopics.Clear();

                m_LoggedInUser      = null;
                m_LoggedInUsername  = null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Recache emotes
        /// </summary>
        public void RecacheEmotes()
        {
            OnCredentialsUpdated(true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Web page HTML content
        /// </summary>
        /// <returns></returns>
        public string WebPageHTMLForm()
        {
            var l_ChannelList = TwitchSettingsConfig.Instance.Channels;
            var l_Content     = System.Text.Encoding.UTF8.GetString(Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK.Chat.Services.Twitch.TwitchHTMLForm.html"));

            l_Content = l_Content.Replace("{TWITCH_CLIENTID}",          TWITCH_CLIENT_ID);
            l_Content = l_Content.Replace("{TWITCH_SCOPES}",            string.Join("+", RequiredTokenScopes));

            l_Content = l_Content.Replace("{TWITCH_TOKENCHAT}",         TwitchSettingsConfig.Instance.TokenChat);
            l_Content = l_Content.Replace("{TWITCH_SHOW_ADVANCED}",     !string.IsNullOrEmpty(TwitchSettingsConfig.Instance.TokenChannel) ? "checked" : "");
            l_Content = l_Content.Replace("{TWITCH_TOKENCHANNEL}",      TwitchSettingsConfig.Instance.TokenChannel);

            l_Content = l_Content.Replace("{TWITCH_CHANNEL1}",          l_ChannelList.Length >= 1                                 ? l_ChannelList[0].Name : "");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL2}",          l_ChannelList.Length >= 2                                 ? l_ChannelList[1].Name : "");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL3}",          l_ChannelList.Length >= 3                                 ? l_ChannelList[2].Name : "");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL4}",          l_ChannelList.Length >= 4                                 ? l_ChannelList[3].Name : "");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL5}",          l_ChannelList.Length >= 5                                 ? l_ChannelList[4].Name : "");

            l_Content = l_Content.Replace("{TWITCH_CHANNEL1_SEND}",     l_ChannelList.Length >= 1 ? (l_ChannelList[0].CanSendMessages ? "checked" : "") : "checked");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL2_SEND}",     l_ChannelList.Length >= 2 ? (l_ChannelList[1].CanSendMessages ? "checked" : "") : "checked");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL3_SEND}",     l_ChannelList.Length >= 3 ? (l_ChannelList[2].CanSendMessages ? "checked" : "") : "checked");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL4_SEND}",     l_ChannelList.Length >= 4 ? (l_ChannelList[3].CanSendMessages ? "checked" : "") : "checked");
            l_Content = l_Content.Replace("{TWITCH_CHANNEL5_SEND}",     l_ChannelList.Length >= 5 ? (l_ChannelList[4].CanSendMessages ? "checked" : "") : "checked");

            return l_Content;
        }
        /// <summary>
        /// Web page HTML content
        /// </summary>
        /// <returns></returns>
        public string WebPageHTML()
            => System.Text.Encoding.UTF8.GetString(Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK.Chat.Services.Twitch.TwitchHTML.html"));
        /// <summary>
        /// Web page javascript content
        /// </summary>
        /// <returns></returns>
        public string WebPageJS()
            => System.Text.Encoding.UTF8.GetString(Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK.Chat.Services.Twitch.TwitchJS.js"));
        /// <summary>
        /// Web page javascript content
        /// </summary>
        /// <returns></returns>
        public string WebPageJSValidate()
            => System.Text.Encoding.UTF8.GetString(Misc.Resources.FromRelPath(Assembly.GetExecutingAssembly(), "CP_SDK.Chat.Services.Twitch.TwitchJSValidate.js"));
        /// <summary>
        /// On web page post data
        /// </summary>
        /// <param name="p_PostData">Post data</param>
        public void WebPageOnPost(Dictionary<string, string> p_PostData)
        {
            var l_NewTwitchChannels = new List<(string, bool)>()
            {
                (string.Empty, false),
                (string.Empty, false),
                (string.Empty, false),
                (string.Empty, false),
                (string.Empty, false)
            };

            foreach (var l_Data in p_PostData)
            {
                switch (l_Data.Key)
                {
                    case "twitch_tokenchat":
                        var l_NewTwitchTokenChat = new string(CP_SDK_WebSocketSharp.Net.HttpUtility.UrlDecode(l_Data.Value.Trim()).Where(c => !char.IsControl(c)).ToArray());


                        TwitchSettingsConfig.Instance.TokenChat = l_NewTwitchTokenChat.StartsWith("oauth:")
                                                        ?
                                                            l_NewTwitchTokenChat
                                                        :
                                                            !string.IsNullOrEmpty(l_NewTwitchTokenChat)
                                                            ?
                                                                $"oauth:{l_NewTwitchTokenChat}"
                                                            :
                                                                ""
                                                        ;
                        break;

                    case "twitch_tokenchannel":
                        var l_NewTwitchTokenChannel = new string(CP_SDK_WebSocketSharp.Net.HttpUtility.UrlDecode(l_Data.Value.Trim()).Where(c => !char.IsControl(c)).ToArray());


                        TwitchSettingsConfig.Instance.TokenChannel = l_NewTwitchTokenChannel.StartsWith("oauth:")
                                                        ?
                                                            l_NewTwitchTokenChannel
                                                        :
                                                            !string.IsNullOrEmpty(l_NewTwitchTokenChannel)
                                                            ?
                                                                $"oauth:{l_NewTwitchTokenChannel}"
                                                            :
                                                                ""
                                                        ;
                        break;

                    case "twitch_channel1":
                    case "twitch_channel2":
                    case "twitch_channel3":
                    case "twitch_channel4":
                    case "twitch_channel5":
                        var l_Value = new string(l_Data.Value.ToLower().Trim().Where(c => !char.IsControl(c)).ToArray());

                        if (l_Data.Key == "twitch_channel1") l_NewTwitchChannels[0] = (l_Value, l_NewTwitchChannels[0].Item2);
                        if (l_Data.Key == "twitch_channel2") l_NewTwitchChannels[1] = (l_Value, l_NewTwitchChannels[1].Item2);
                        if (l_Data.Key == "twitch_channel3") l_NewTwitchChannels[2] = (l_Value, l_NewTwitchChannels[2].Item2);
                        if (l_Data.Key == "twitch_channel4") l_NewTwitchChannels[3] = (l_Value, l_NewTwitchChannels[3].Item2);
                        if (l_Data.Key == "twitch_channel5") l_NewTwitchChannels[4] = (l_Value, l_NewTwitchChannels[4].Item2);
                        break;

                    case "twitch_channel1send":
                    case "twitch_channel2send":
                    case "twitch_channel3send":
                    case "twitch_channel4send":
                    case "twitch_channel5send":
                        var l_PostValue = l_Data.Value.ToLower().Trim();
                        var l_Value2    = l_PostValue == "true" || l_PostValue == "on" || l_PostValue == "1";

                        if (l_Data.Key == "twitch_channel1send") l_NewTwitchChannels[0] = (l_NewTwitchChannels[0].Item1, l_Value2);
                        if (l_Data.Key == "twitch_channel2send") l_NewTwitchChannels[1] = (l_NewTwitchChannels[1].Item1, l_Value2);
                        if (l_Data.Key == "twitch_channel3send") l_NewTwitchChannels[2] = (l_NewTwitchChannels[2].Item1, l_Value2);
                        if (l_Data.Key == "twitch_channel4send") l_NewTwitchChannels[3] = (l_NewTwitchChannels[3].Item1, l_Value2);
                        if (l_Data.Key == "twitch_channel5send") l_NewTwitchChannels[4] = (l_NewTwitchChannels[4].Item1, l_Value2);
                        break;
                }
            }

            try
            {
                TwitchSettingsConfig.Instance.Channels = l_NewTwitchChannels
                    .Where(x => !string.IsNullOrEmpty(x.Item1))
                    .Select(x => new TwitchSettingsConfig._Channel() { Name = x.Item1, CanSendMessages = x.Item2 })
                    .Distinct()
                    .ToArray();

                TwitchSettingsConfig.Instance.Save();
                OnCredentialsUpdated(false);
            }
            catch (Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Services.Twitch][TwitchService.WebPageOnPost] An exception occurred while updating config");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings changed
        /// </summary>
        /// <param name="p_Credentials">New credential</param>
        internal void OnCredentialsUpdated(bool p_Force)
        {
            if (!m_IsStarted)
                return;

            /// Restart if OAuth token is different
            if (p_Force || (m_TokenChat != m_TokenChatCache || m_TokenChannel != m_TokenChannelCache))
            {
                m_TokenChatCache       = m_TokenChat;
                m_TokenChannelCache    = m_TokenChannel;

                Stop();
                Start();
            }
            /// Join / Leave missing channels
            else
            {
                var l_ChannelList = TwitchSettingsConfig.Instance.Channels;
                foreach (var l_ChannelToJoin in l_ChannelList)
                {
                    if (!m_Channels.ContainsKey(l_ChannelToJoin.Name))
                        JoinChannel(l_ChannelToJoin.Name);
                }

                if (l_ChannelList.Length == 0)
                    m_OnSystemMessageCallbacks?.InvokeAll(this, "<b><color=red>No channel configured, messages won't be displayed</color></b>");

                foreach (var l_Channel in m_Channels)
                {
                    var l_ChannelConfig = l_ChannelList.FirstOrDefault(x => x.Name == l_Channel.Key);

                    if (l_ChannelConfig == null)
                        PartChannel(l_Channel.Key);
                    else
                        l_Channel.Value.CanSendMessages = l_ChannelConfig.CanSendMessages;
                }
            }
        }
        /// <summary>
        /// On Helix token validate
        /// </summary>
        /// <param name="p_Valid">Is valid</param>
        /// <param name="p_Result">Result</param>
        private void HelixAPI_OnTokenValidate(bool p_Valid, Helix_TokenValidate p_Result, string p_TokenUserID)
        {
            if (p_Valid)
            {
                m_PubSubSocket.Connect("wss://pubsub-edge.twitch.tv:443");
                m_IRCWebSocket.Connect("wss://irc-ws.chat.twitch.tv:443");

                foreach (var l_Scope in RequiredTokenScopes)
                {
                    if (p_Result.scopes.Contains(l_Scope, StringComparer.InvariantCultureIgnoreCase))
                        continue;

                    m_OnSystemMessageCallbacks?.InvokeAll(this, $"<color=yellow>Your Twitch token is missing permission <b>{l_Scope}</b>, some features may not work, please update your token!");
                }
            }
            else
            {
                m_OnSystemMessageCallbacks?.InvokeAll(this, "<color=red><b>Your Twitch token is invalid/expired</b>, please update it to make the chat work!");
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process queued message
        /// </summary>
        /// <returns></returns>
        private async Task ProcessQueuedMessages()
        {
            await Task.Yield();

            while (m_IsStarted)
            {
                if ((DateTime.UtcNow - m_LastIRCSubPing).TotalSeconds >= 60)
                {
                    if (m_IRCWebSocket.IsConnected)
                    {
                        m_IRCWebSocket.SendMessage("PING");
#if DEBUG
                        m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] Sent Ping");
#endif
                    }
                    else
                        m_IRCWebSocket.TryHandleReconnect();

                    m_LastIRCSubPing = DateTime.UtcNow;
                }

                if ((DateTime.UtcNow - m_LastPubSubPing).TotalSeconds >= 60)
                {
                    if (m_PubSubSocket.IsConnected)
                        m_PubSubSocket.SendMessage("{ \"type\": \"PING\" }");

                    m_LastPubSubPing = DateTime.UtcNow;
                }

                if (m_CurrentSentMessageCount >= 20)
                {
                    float l_RemainingMilliseconds = (float)(30000 - (DateTime.UtcNow - m_LastResetTime).TotalMilliseconds);
                    if (l_RemainingMilliseconds > 0)
                    {
                        await Task.Delay((int)l_RemainingMilliseconds).ConfigureAwait(false);
                    }
                }

                if ((DateTime.UtcNow - m_LastResetTime).TotalSeconds >= 30)
                {
                    m_CurrentSentMessageCount = 0;
                    m_LastResetTime = DateTime.UtcNow;
                }

                if (m_TextMessageQueue.TryDequeue(out var l_Message))
                {
                    SendRawMessage(l_Message, true);
                    m_CurrentSentMessageCount++;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Update helix task
        /// </summary>
        /// <returns></returns>
        private async Task UpdateHelix()
        {
            /// Make compiler happy
            await Task.Yield();

            while (m_IsStarted)
            {
                HelixAPI.Update();

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Join channel
        /// </summary>
        /// <param name="p_Channel">Name of the channel</param>
        private void JoinChannel(string p_Channel)
        {
            ChatPlexSDK.Logger.Info($"Trying to join channel #{p_Channel}");
            SendRawMessage($"JOIN #{p_Channel.ToLower()}");
        }
        /// <summary>
        /// Leave channel
        /// </summary>
        /// <param name="p_Channel">Name of the channel</param>
        private void PartChannel(string p_Channel)
        {
            SendRawMessage($"PART #{p_Channel.ToLower()}");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sends a text message to the specified IChatChannel
        /// </summary>
        /// <param name="p_Channel">The chat channel to send the message to</param>
        /// <param name="p_Message">The text message to be sent</param>
        public void SendTextMessage(IChatChannel p_Channel, string p_Message)
        {
            if (!(p_Channel is TwitchChannel l_TwitchChannel) || !l_TwitchChannel.CanSendMessages || m_LoggedInUser == null)
                return;

            string l_MessageID  = System.Guid.NewGuid().ToString();
            string l_Message    = $"@id={l_MessageID} PRIVMSG #{p_Channel.Id} :{new string(p_Message.Where(c => !char.IsControl(c)).ToArray())}\r\n";

            m_TextMessageQueue.Enqueue(l_Message);
        }
        /// <summary>
        /// Sends a raw message to the Twitch server
        /// </summary>
        /// <param name="p_RawMessage">The raw message to send.</param>
        /// <param name="p_ForwardToSharedClients">
        /// Whether or not the message should also be sent to other clients in the assembly that implement StreamCore, or only to the Twitch server.<br/>
        /// This should only be set to true if the Twitch server would rebroadcast this message to other external clients as a response to the message.
        /// </param>
        private void SendRawMessage(string p_RawMessage, bool p_ForwardToSharedClients = false)
        {
            if (m_IRCWebSocket.IsConnected)
            {
                m_IRCWebSocket.SendMessage(p_RawMessage);

                if (p_ForwardToSharedClients)
                    IRCSocket_OnMessageReceived(p_RawMessage);
            }
            else
                ChatPlexSDK.Logger.Warning("WebSocket service is not connected!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedAndLive()
        {
            if (HelixAPI == null)
                return false;

            if (string.IsNullOrEmpty(HelixAPI.BroadcasterLogin))
                return false;

            var l_Channel = m_Channels.Where(x => x.Key.ToLower() == HelixAPI.BroadcasterLogin.ToLower()).Select(x => x.Value).SingleOrDefault();
            return l_Channel != null && l_Channel.Live;
        }
        /// <summary>
        /// Get primary channel name
        /// </summary>
        /// <returns></returns>
        public string PrimaryChannelName()
        {
            if (HelixAPI == null)
                return string.Empty;

            if (string.IsNullOrEmpty(HelixAPI.BroadcasterLogin))
                return string.Empty;

            return HelixAPI.BroadcasterLogin;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Join temp channel with group identifier
        /// </summary>
        /// <param name="p_GroupIdentifier">Group identifier</param>
        /// <param name="p_ChannelName">Name of the channel</param>
        /// <param name="p_Prefix">Messages prefix</param>
        /// <param name="p_CanSendMessage">Can send message</param>
        public void JoinTempChannel(string p_GroupIdentifier, string p_ChannelName, string p_Prefix, bool p_CanSendMessage)
        {
            if (m_Channels.Any(x => x.Key.ToLower() == p_ChannelName.ToLower())
                || m_TempChannels.Any(x => x.Key.ToLower() == p_ChannelName.ToLower()))
                return;

            m_TempChannels.TryAdd(p_ChannelName, (p_GroupIdentifier, p_Prefix, p_CanSendMessage));

            JoinChannel(p_ChannelName);
        }
        /// <summary>
        /// Leave temp channel
        /// </summary>
        /// <param name="p_ChannelName">Name of the channel</param>
        public void LeaveTempChannel(string p_ChannelName)
        {
            var l_ToLeave = m_TempChannels.Where(x => x.Key.ToLower() == p_ChannelName.ToLower()).ToList();

            for (var l_I = 0; l_I < l_ToLeave.Count; ++l_I)
            {
                PartChannel(l_ToLeave[l_I].Key);
                m_TempChannels.TryRemove(l_ToLeave[l_I].Key, out _);
            }
        }
        /// <summary>
        /// Is in temp channel
        /// </summary>
        /// <param name="p_ChannelName">Channel name</param>
        /// <returns></returns>
        public bool IsInTempChannel(string p_ChannelName)
            => m_TempChannels.Any(x => x.Key.ToLower() == p_ChannelName.ToLower());
        /// <summary>
        /// Leave all temp channel by group identifier
        /// </summary>
        /// <param name="p_GroupIdentifier"></param>
        public void LeaveAllTempChannel(string p_GroupIdentifier)
        {
            var l_ToLeave = m_TempChannels.Where(x => x.Value.GroupIdentifier == p_GroupIdentifier).Select(x => x.Key).ToList();

            for (var l_I = 0; l_I < l_ToLeave.Count; ++l_I)
            {
                PartChannel(l_ToLeave[l_I]);
                m_TempChannels.TryRemove(l_ToLeave[l_I], out _);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Subscribe topics for a channel
        /// </summary>
        /// <param name="p_Topics">Topics to listen</param>
        /// <param name="p_RoomID">ID of the channel</param>
        private void SubscribeTopics(string[] p_Topics, string p_RoomID, string p_ChannelName)
        {
            ChatPlexSDK.Logger.Warning("TwitchPubSub Send topics");

            var l_OAuth = m_TokenChannel;
            if (l_OAuth != null && l_OAuth.Contains("oauth:"))
                l_OAuth = l_OAuth.Replace("oauth:", "");

            var l_Topics = new JSONArray();

            lock (m_ChannelTopics)
            {
                if (!m_ChannelTopics.ContainsKey(p_RoomID))
                    m_ChannelTopics.Add(p_RoomID, new List<string>());

                foreach (var l_Topic in p_Topics)
                {
                    if (m_ChannelTopics[p_RoomID].Contains(l_Topic))
                        continue;

                    m_ChannelTopics[p_RoomID].Add(l_Topic);

                    if (l_Topic == "video-playback")
                        l_Topics.Add(new JSONString(l_Topic + "." + p_ChannelName));
                    else
                        l_Topics.Add(new JSONString(l_Topic + "." + p_RoomID));
                }
            }

            var l_JSONDataData = new JSONObject();
            l_JSONDataData.Add("topics", l_Topics);
            if (l_OAuth != null)
                l_JSONDataData.Add("auth_token", l_OAuth);

            var l_JSONData = new JSONObject();
            l_JSONData.Add("type", "LISTEN");
            l_JSONData.Add("data", l_JSONDataData);

            m_PubSubSocket.SendMessage(l_JSONData.ToString());
        }
        /// <summary>
        /// Resubscribe all topics for all channel
        /// </summary>
        private void ResubscribeChannelsTopics()
        {
            var l_OAuth = m_TokenChannel;
            if (l_OAuth != null && l_OAuth.Contains("oauth:"))
                l_OAuth = l_OAuth.Replace("oauth:", "");

            var l_Topics = new JSONArray();
            lock (m_ChannelTopics)
            {
                foreach (var l_CurrentChannel in m_ChannelTopics)
                {
                    foreach (var l_CurrentTopic in l_CurrentChannel.Value)
                    {
                        if (l_CurrentTopic == "video-playback")
                        {
                            var l_Channel = m_Channels.Select(x => x.Value).Where(x => x.Roomstate.RoomId == l_CurrentChannel.Key).FirstOrDefault();
                            if (l_Channel != null)
                                l_Topics.Add(new JSONString(l_CurrentTopic + "." + l_Channel.Name));
                        }
                        else
                            l_Topics.Add(new JSONString(l_CurrentTopic + "." + l_CurrentChannel.Key));
                    }
                }
            }

            /// Skip if no topics
            if (l_Topics.Count == 0)
                return;

            var l_JSONDataData = new JSONObject();
            l_JSONDataData.Add("topics", l_Topics);
            if (l_OAuth != null)
                l_JSONDataData.Add("auth_token", l_OAuth);

            var l_JSONData = new JSONObject();
            l_JSONData.Add("type", "LISTEN");
            l_JSONData.Add("data", l_JSONDataData);

            m_PubSubSocket.SendMessage(l_JSONData.ToString());
        }
        /// <summary>
        /// Unsubscribe all topics for a channel
        /// </summary>
        /// <param name="p_RoomID">ID of the channel</param>
        private void UnsubscribeTopics(string p_RoomID, string p_ChannelName)
        {
            var l_OAuth = m_TokenChannel;
            if (l_OAuth != null && l_OAuth.Contains("oauth:"))
                l_OAuth = l_OAuth.Replace("oauth:", "");

            var l_Topics = new JSONArray();

            lock (m_ChannelTopics)
            {
                if (!m_ChannelTopics.ContainsKey(p_RoomID))
                    m_ChannelTopics.Add(p_RoomID, new List<string>());

                foreach (var l_CurrentTopic in m_ChannelTopics[p_RoomID])
                {
                    if (l_CurrentTopic == "video-playback")
                        l_Topics.Add(new JSONString(l_CurrentTopic + "." + p_ChannelName));
                    else
                        l_Topics.Add(new JSONString(l_CurrentTopic + "." + p_RoomID));
                }

                m_ChannelTopics[p_RoomID].Clear();
            }

            var l_JSONDataData = new JSONObject();
            l_JSONDataData.Add("topics", l_Topics);
            if (l_OAuth != null)
                l_JSONDataData.Add("auth_token", l_OAuth);

            var l_JSONData = new JSONObject();
            l_JSONData.Add("type", "UNLISTEN");
            l_JSONData.Add("data", l_JSONDataData);

            m_PubSubSocket.SendMessage(l_JSONData.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch IRC socket open
        /// </summary>
        private void IRCSocket_OnOpen()
        {
            m_OnSystemMessageCallbacks?.InvokeAll(this, "Connected to Twitch");

            ChatPlexSDK.Logger.Info("Twitch connection opened");
            ChatPlexSDK.Logger.Info("Trying to login!");
            if (!string.IsNullOrEmpty(m_TokenChat))
                m_IRCWebSocket.SendMessage($"PASS {m_TokenChat}");

            m_IRCWebSocket.SendMessage($"NICK {ChatPlexSDK.ProductName}{m_Random.Next(10000, 1000000)}");
            m_IRCWebSocket.SendMessage("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
        }
        /// <summary>
        /// Twitch IRC socket close
        /// </summary>
        private void IRCSocket_OnClose()
        {
            ChatPlexSDK.Logger.Info("Twitch connection closed");

            m_OnSystemMessageCallbacks?.InvokeAll(this, "Disconnected from Twitch");
#if DEBUG
            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] IRCSocket_OnClose");
#endif
        }
        /// <summary>
        /// Twitch IRC socket error
        /// </summary>
        private void IRCSocket_OnError()
        {
            ChatPlexSDK.Logger.Error("An error occurred in Twitch connection");

            m_OnSystemMessageCallbacks?.InvokeAll(this, "Disconnected from Twitch, error");
#if DEBUG
            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] IRCSocket_OnError");
#endif
        }
        /// <summary>
        /// When a twitch IRC message is received
        /// </summary>
        /// <param name="p_RawMessage">Raw message</param>
        private void IRCSocket_OnMessageReceived(string p_RawMessage)
        {
            lock (m_MessageReceivedLock)
            {
                //System.IO.File.AppendAllText("out.txt", p_RawMessage + "\r\n");
                ///ChatPlexSDK.Logger.Info("RawMessage: " + p_RawMessage);
                m_MessageReceivedParsingBuffer.Clear();
                if (!m_MessageParser.ParseRawMessage(p_RawMessage, m_Channels, m_LoggedInUser, m_MessageReceivedParsingBuffer, m_LoggedInUsername))
                {
                    ChatPlexSDK.Logger.Error("Failed to parse: " + p_RawMessage);
                    return;
                }

                for (var l_MessageI = 0; l_MessageI < m_MessageReceivedParsingBuffer.Count; ++l_MessageI)
                {
                    var l_TwitchMessage = m_MessageReceivedParsingBuffer[l_MessageI];
                    var l_TwitchChannel = l_TwitchMessage.Channel as TwitchChannel;
                    var l_Sender        = l_TwitchMessage.Sender.AsTwitchUser();

                    switch (l_TwitchMessage.Type)
                    {
                        case "PING":
                            SendRawMessage("PONG :tmi.twitch.tv");
#if DEBUG
                            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] Received Ping");
#endif
                            continue;
#if DEBUG
                        case "PONG":
                            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] Received Pong");
                            continue;

                        case "RECONNECT":
                            m_OnSystemMessageCallbacks?.InvokeAll(this, "[Debug] Received Reconnect");
                            continue;
#endif

                        /// Successful login
                        case "376":
                            m_DataProvider.TryRequestGlobalResources(m_TokenChannel);
                            m_LoggedInUsername          = l_TwitchMessage.Channel.Id;
                            m_LoggedInUser              = GetTwitchUser(null, m_LoggedInUsername, null);
                            m_LoggedInUser.DisplayName  = m_LoggedInUsername;

                            /// This isn't a typo, when you first sign in your username is in the channel id.
                            ChatPlexSDK.Logger.Info($"Logged into Twitch as {m_LoggedInUsername}");
                            ChatPlexSDK.Logger.Info(l_TwitchMessage.Sender.Id);
                            m_OnLoginCallbacks?.InvokeAll(this);

                            var l_ChannelList = TwitchSettingsConfig.Instance.Channels;
                            foreach (var l_ChannelToJoin in l_ChannelList)
                                JoinChannel(l_ChannelToJoin.Name);

                            if (l_ChannelList.Length == 0)
                                m_OnSystemMessageCallbacks?.InvokeAll(this, "<b><color=red>No channel configured, messages won't be displayed</color></b>");

                            continue;

                        case "NOTICE":
                            switch (l_TwitchMessage.Message)
                            {
                                case "Login authentication failed":
                                case "Invalid NICK":
                                    m_IRCWebSocket.Disconnect();
                                    break;

                            }
                            goto case "PRIVMSG";

                        case "USERNOTICE":
                        case "PRIVMSG":
                            m_OnTextMessageReceivedCallbacks?.InvokeAll(this, l_TwitchMessage);

                            if (l_TwitchMessage.IsRaid)
                                m_OnChannelRaidCallbacks?.InvokeAll(this, l_TwitchChannel, l_Sender, l_TwitchMessage.RaidViewerCount);

                            continue;

                        case "JOIN":
                            ///ChatPlexSDK.Logger.Info($"{twitchMessage.Sender.Name} JOINED {twitchMessage.Channel.Id}. LoggedInuser: {LoggedInUser.Name}");
                            if (l_TwitchMessage.Sender.UserName == m_LoggedInUsername
                                && !m_Channels.ContainsKey(l_TwitchMessage.Channel.Id))
                            {
                                var l_CanSend       = true;
                                var l_ChannelConfig = TwitchSettingsConfig.Instance.Channels.FirstOrDefault(x => x.Name.ToLower() == l_TwitchMessage.Channel.Id.ToLower());

                                if (l_ChannelConfig != null)
                                    l_CanSend = l_ChannelConfig.CanSendMessages;
                                else
                                    l_CanSend = m_TempChannels.FirstOrDefault(x => x.Key.ToLower() == l_TwitchMessage.Channel.Id.ToLower()).Value.CanSend;

                                m_Channels[l_TwitchMessage.Channel.Id] = l_TwitchMessage.Channel.AsTwitchChannel();
                                m_Channels[l_TwitchMessage.Channel.Id].CanSendMessages = l_CanSend;

                                if (m_TempChannels.Any(x => x.Key.ToLower() == l_TwitchChannel.Name.ToLower()))
                                {
                                    m_Channels[l_TwitchMessage.Channel.Id].Prefix = m_TempChannels.FirstOrDefault(x => x.Key.ToLower() == l_TwitchMessage.Channel.Id.ToLower()).Value.Prefix;
                                    m_Channels[l_TwitchMessage.Channel.Id].IsTemp = true;
                                }

                                ChatPlexSDK.Logger.Info($"Added channel {l_TwitchMessage.Channel.Id} to the channel list.");

                                m_OnJoinRoomCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);
                            }
                            continue;

                        case "PART":
                            ///ChatPlexSDK.Logger.Info($"{twitchMessage.Sender.Name} PARTED {twitchMessage.Channel.Id}. LoggedInuser: {LoggedInUser.Name}");
                            if (l_TwitchMessage.Sender.UserName == m_LoggedInUsername
                                && m_Channels.TryRemove(l_TwitchMessage.Channel.Id, out var l_Channel))
                            {
                                m_DataProvider.TryReleaseChannelResources(l_TwitchMessage.Channel);
                                ChatPlexSDK.Logger.Info($"Removed channel {l_Channel.Id} from the channel list.");
                                m_OnLiveStatusUpdatedCallbacks?.InvokeAll(this, l_TwitchMessage.Channel, false, 0);
                                m_OnLeaveRoomCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);

                                if (!string.IsNullOrEmpty(m_TokenChannel) && !m_TempChannels.Any(x => x.Key.ToLower() == l_TwitchChannel.Name.ToLower()))
                                    UnsubscribeTopics(l_TwitchChannel.Roomstate.RoomId, l_TwitchChannel.Name);
                            }
                            continue;

                        case "ROOMSTATE":
                            {
                                var l_CanSend       = true;
                                var l_ChannelConfig = TwitchSettingsConfig.Instance.Channels.FirstOrDefault(x => x.Name.ToLower() == m_Channels[l_TwitchMessage.Channel.Id].Name.ToLower());

                                if (l_ChannelConfig != null)
                                    l_CanSend = l_ChannelConfig.CanSendMessages;
                                else
                                    l_CanSend = m_TempChannels.FirstOrDefault(x => x.Key.ToLower() == m_Channels[l_TwitchMessage.Channel.Id].Name.ToLower()).Value.CanSend;

                                m_Channels[l_TwitchMessage.Channel.Id] = l_TwitchMessage.Channel as TwitchChannel;
                                m_Channels[l_TwitchMessage.Channel.Id].CanSendMessages = l_CanSend;

                                if (m_TempChannels.Any(x => x.Key.ToLower() == l_TwitchChannel.Name.ToLower()))
                                {
                                    m_Channels[l_TwitchMessage.Channel.Id].Prefix = m_TempChannels.FirstOrDefault(x => x.Key.ToLower() == l_TwitchMessage.Channel.Id.ToLower()).Value.Prefix;
                                    m_Channels[l_TwitchMessage.Channel.Id].IsTemp = true;
                                }

                                m_DataProvider.TryRequestChannelResources(l_TwitchMessage.Channel, m_TokenChannel, (x) => m_OnChannelResourceDataCached?.InvokeAll(this, l_TwitchMessage.Channel, x));

                                m_OnRoomStateUpdatedCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);

                                if (!string.IsNullOrEmpty(m_TokenChannel) && !m_TempChannels.Any(x => x.Key.ToLower() == l_TwitchChannel.Name.ToLower()))
                                {
                                    SubscribeTopics(new string[] {
                                        "following",
                                        "channel-subscribe-events-v1",
                                        "channel-bits-events-v2",
                                        "channel-points-channel-v1",
                                        "video-playback"
                                    }, l_TwitchChannel.Roomstate.RoomId, l_TwitchChannel.Name);
                                }
                            }
                            continue;

                        case "GLOBALUSERSTATE":
                            if (l_Sender != null && m_LoggedInUser != null)
                            {
                                m_LoggedInUser.Id           = l_Sender.Id;
                                m_LoggedInUser.DisplayName  = l_Sender.DisplayName;
                                m_LoggedInUser.Color        = l_Sender.Color;

                                if (m_DataProvider.IsReady && !string.IsNullOrEmpty(m_LoggedInUser.Id) && !string.IsNullOrEmpty(m_LoggedInUser.DisplayName))
                                {
                                    m_DataProvider.TryGetUserDisplayName(m_LoggedInUser.Id, m_LoggedInUser.DisplayName, out var l_PaintedName);

                                    m_LoggedInUser.PaintedName      = l_PaintedName;
                                    m_LoggedInUser._FancyNameReady  = true;
                                }
                            }
                            continue;

                        case "CLEARCHAT":
                            m_OnChatClearedCallbacks?.InvokeAll(this, l_TwitchMessage.TargetUserId);
                            continue;

                        case "CLEARMSG":
                            if (!string.IsNullOrEmpty(l_TwitchMessage.TargetMsgId))
                                m_OnMessageClearedCallbacks?.InvokeAll(this, l_TwitchMessage.TargetMsgId);

                            continue;

                        ///case "MODE":
                        ///case "NAMES":
                        ///case "HOSTTARGET":
                        ///case "RECONNECT":
                        ///    ChatPlexSDK.Logger.Info($"No handler exists for type {twitchMessage.Type}. {rawMessage}");
                        ///    continue;
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Twitch PubSub socket open
        /// </summary>
        private void PubSubSocket_OnOpen()
        {
            ChatPlexSDK.Logger.Info("TwitchPubSub connection opened");
            ResubscribeChannelsTopics();
        }
        /// <summary>
        /// Twitch PubSub socket close
        /// </summary>
        private void PubSubSocket_OnClose()
        {
            ChatPlexSDK.Logger.Info("TwitchPubSub connection closed");
        }
        /// <summary>
        /// Twitch PubSub socket error
        /// </summary>
        private void PubSubSocket_OnError()
        {
            ChatPlexSDK.Logger.Error("An error occurred in TwitchPubSub connection");
        }
        /// <summary>
        /// When a twitch PubSub message is received
        /// </summary>
        /// <param name="p_RawMessage">Raw message</param>
        private void PubSubSocket_OnMessageReceived(string p_RawMessage)
        {
            lock (m_MessageReceivedLock)
            {
                ///ChatPlexSDK.Logger.Warning("TwitchPubSub " + p_RawMessage);

                var l_MessageType = "";
                if (SimpleJSON.JSON.Parse(p_RawMessage).TryGetKey("type", out var l_TypeValue))
                    l_MessageType = l_TypeValue.Value;

                switch (l_MessageType?.ToLower())
                {
                    case "response":
                        var resp = new PubSubTopicListenResponse(p_RawMessage);
                        ChatPlexSDK.Logger.Warning("TwitchPubSub joined topic result " + resp.Successful);
                        break;

                    case "message":
                        ///ChatPlexSDK.Logger.Warning("TwitchPubSub " + SimpleJSON.JSON.Parse(p_RawMessage));
                        var l_Message = new PubSubMessage(p_RawMessage);
                        switch (l_Message.Topic.Split('.')[0])
                        {
                            case "channel-subscribe-events-v1":
                                var l_SubscriptionMessage = l_Message.MessageData as PubSubChannelSubscription;

                                var l_SubscriptionUser      = GetTwitchUser(l_SubscriptionMessage.UserId, l_SubscriptionMessage.Username, l_SubscriptionMessage.DisplayName);
                                var l_SubscriptionChannel   = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.Roomstate.RoomId == l_SubscriptionMessage.ChannelId);
                                var l_SubscriptionEvent     = new TwitchSubscriptionEvent()
                                {
                                    DisplayName             = l_SubscriptionMessage.DisplayName,
                                    SubPlan                 = l_SubscriptionMessage.SubscriptionPlan.ToString(),
                                    IsGift                  = l_SubscriptionMessage.IsGift,
                                    RecipientDisplayName    = l_SubscriptionMessage.RecipientDisplayName,
                                    PurchasedMonthCount     = System.Math.Max(1, l_SubscriptionMessage.PurchasedMonthCount)
                                };

                                m_OnChannelSubscriptionCallbacks?.InvokeAll(this, l_SubscriptionChannel, l_SubscriptionUser, l_SubscriptionEvent);
                                return;

                            case "channel-bits-events-v2":
                                var l_ChannelBitsMessage = l_Message.MessageData as PubSubChannelBitsEvents;

                                var l_BitsUser = GetTwitchUser(
                                    !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.UserId   : null,
                                    !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.Username : "AnAnonymousCheerer",
                                    !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.Username : "AnAnonymousCheerer");
                                var l_BitsChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.Roomstate.RoomId == l_ChannelBitsMessage.ChannelId);

                                m_OnChannelBitsCallbacks?.InvokeAll(this, l_BitsChannel, l_BitsUser, l_ChannelBitsMessage.BitsUsed);
                                break;

                            case "channel-points-channel-v1":
                                var l_ChannelPointsMessage = l_Message.MessageData as PubSubChannelPointsEvents;

                                var l_PointsUser    = GetTwitchUser(l_ChannelPointsMessage.UserId, l_ChannelPointsMessage.UserName, l_ChannelPointsMessage.UserDisplayName);
                                var l_PointsChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.Roomstate.RoomId == l_ChannelPointsMessage.ChannelId);
                                var l_PointsEvent   = new TwitchChannelPointEvent()
                                {
                                    RewardID        = l_ChannelPointsMessage.RewardID,
                                    TransactionID   = l_ChannelPointsMessage.TransactionID,
                                    Title           = l_ChannelPointsMessage.Title,
                                    Cost            = l_ChannelPointsMessage.Cost,
                                    Prompt          = l_ChannelPointsMessage.Prompt,
                                    UserInput       = l_ChannelPointsMessage.UserInput,
                                    Image           = l_ChannelPointsMessage.Image,
                                    BackgroundColor = l_ChannelPointsMessage.BackgroundColor
                                };

                                m_OnChannelPointsCallbacks?.InvokeAll(this, l_PointsChannel, l_PointsUser, l_PointsEvent);
                                break;

                            case "following":
                                var l_FollowMessage = l_Message.MessageData as PubSubFollowing;
                                l_FollowMessage.FollowedChannelId = l_Message.Topic.Split('.')[1];

                                var l_FollowUser    = GetTwitchUser(l_FollowMessage.UserId, l_FollowMessage.Username, l_FollowMessage.DisplayName);
                                var l_FollowChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.Roomstate.RoomId == l_FollowMessage.FollowedChannelId);

                                if (l_FollowUser != null && !l_FollowUser._HadFollowed)
                                {
                                    l_FollowUser._HadFollowed = true;
                                    m_OnChannelFollowCallbacks?.InvokeAll(this, l_FollowChannel, l_FollowUser);
                                }

                                break;

                            case "video-playback":
                                var l_VideoPlaybackMessage  = l_Message.MessageData as PubSubVideoPlayback;
                                var l_Channel               = m_Channels.Where(x => x.Key.ToLower() == l_Message.Topic.Split('.')[1].ToLower()).Select(x => x.Value).SingleOrDefault();

                                l_Channel.Live          = l_VideoPlaybackMessage.Type == PubSubVideoPlayback.VideoPlaybackType.StreamUp || l_VideoPlaybackMessage.Viewers != 0;
                                l_Channel.ViewerCount   = l_VideoPlaybackMessage.Viewers;

                                if (l_Channel != null)
                                    m_OnLiveStatusUpdatedCallbacks?.InvokeAll(this, l_Channel, l_Channel.Live, l_Channel.ViewerCount);

                                break;

                        }
                        break;

                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get twitch user by username
        /// </summary>
        /// <param name="p_UserName">Username</param>
        /// <returns></returns>
        internal TwitchUser GetTwitchUser(string p_UserID, string p_UserName, string p_DisplayName, string p_Color = null)
        {
            if (m_TwitchUsers.TryGetValue(p_UserName, out var l_User))
            {


                if (m_DataProvider.IsReady && !l_User._FancyNameReady && !string.IsNullOrEmpty(l_User.Id) && !string.IsNullOrEmpty(l_User.DisplayName))
                {
                    m_DataProvider.TryGetUserDisplayName(l_User.Id, l_User.DisplayName, out var l_PaintedName);

                    l_User.PaintedName      = l_PaintedName;
                    l_User._FancyNameReady  = true;
                }

                return l_User;
            }

            l_User = new TwitchUser()
            {
                Id          = p_UserID ?? string.Empty,
                UserName    = p_UserName,
                DisplayName = p_DisplayName ?? p_UserName,
                PaintedName = p_DisplayName ?? p_UserName,
                Color       = string.IsNullOrEmpty(p_Color) ? ChatUtils.GetNameColor(p_UserName) : p_Color,
            };

            if (m_DataProvider.IsReady && !string.IsNullOrEmpty(p_UserID) && !string.IsNullOrEmpty(p_DisplayName))
            {
                m_DataProvider.TryGetUserDisplayName(p_UserID, p_DisplayName, out var l_PaintedName);

                l_User.PaintedName      = l_PaintedName;
                l_User._FancyNameReady  = true;
            }

            if (!string.IsNullOrEmpty(p_UserName))
                m_TwitchUsers.TryAdd(p_UserName, l_User);

            return l_User;
        }
    }
}
