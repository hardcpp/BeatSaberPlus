using BeatSaberPlus.SDK.Chat.Interfaces;
using BeatSaberPlus.SDK.Chat.Models.Twitch;
using BeatSaberPlus.SDK.Chat.SimpleJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Twitch service
    /// </summary>
    public class TwitchService : ChatServiceBase, IChatService
    {
        /// <summary>
        /// BOOMBOX twitch application ID
        /// </summary>
        internal const string TWITCH_CLIENT_ID = "23vjr9ec2cwoddv2fc3xfbx9nxv8vi";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The display name of the service(s)
        /// </summary>
        public string DisplayName { get; } = "Twitch";

        /// <summary>
        /// Channels
        /// </summary>
        public ReadOnlyCollection<(IChatService, IChatChannel)> Channels => m_Channels.Select(x => (this as IChatService, x.Value)).ToList().AsReadOnly();

        /// <summary>
        /// OAuth token
        /// </summary>
        public string OAuthToken => m_OAuthToken;
        /// <summary>
        /// OAuth token API
        /// </summary>
        public string OAuthTokenAPI => m_OAuthTokenAPI;

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
        private Random m_Random;
        /// <summary>
        /// Is the service started
        /// </summary>
        private bool m_IsStarted = false;
        /// <summary>
        /// OAuth token
        /// </summary>
        private string m_OAuthToken { get => string.IsNullOrEmpty(AuthConfig.Twitch.OAuthToken) ? "" : AuthConfig.Twitch.OAuthToken; }
        /// <summary>
        /// OAuth token for API
        /// </summary>
        private string m_OAuthTokenAPI { get => string.IsNullOrEmpty(AuthConfig.Twitch.OAuthAPIToken) ? m_OAuthToken : AuthConfig.Twitch.OAuthAPIToken; }
        /// <summary>
        /// OAuth token cache
        /// </summary>
        private string m_OAuthTokenCache;
        /// <summary>
        /// Logged in user
        /// </summary>
        private TwitchUser m_LoggedInUser = null;
        /// <summary>
        /// Logged in user name
        /// </summary>
        private string m_LoggedInUsername;
        /// <summary>
        /// Logged in user ID
        /// </summary>
        private string m_LoggedInUserID = "";
        /// <summary>
        /// Joined channels
        /// </summary>
        private ConcurrentDictionary<string, IChatChannel> m_Channels = new ConcurrentDictionary<string, IChatChannel>();
        /// <summary>
        /// Joined topics
        /// </summary>
        private Dictionary<string, List<string>> m_ChannelTopics = new Dictionary<string, List<string>>();
        /// <summary>
        /// Process message queue task
        /// </summary>
        private Task m_ProcessQueuedMessagesTask = null;
        /// <summary>
        /// Message receive lock
        /// </summary>
        private object m_MessageReceivedLock = new object(), m_InitLock = new object();
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
        /// PubSub sent messages
        /// </summary>
        private ConcurrentDictionary<string, bool> m_SentMessageIDs = new ConcurrentDictionary<string, bool>();
        /// <summary>
        /// Last PubSub ping
        /// </summary>
        private DateTime m_LastPubSubPing = DateTime.UtcNow;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public TwitchService()
        {
            /// Init
            m_DataProvider  = new TwitchDataProvider();
            m_MessageParser = new TwitchMessageParser(m_DataProvider, new FrwTwemojiParser());
            m_Random        = new Random();

            /// Listen on configuration change
            //m_AuthManager.OnCredentialsUpdated += OnCredentialsUpdated;

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
        internal void Start()
        {
            lock (m_InitLock)
            {
                if (!m_IsStarted)
                {
                    m_IsStarted = true;

                    m_PubSubSocket.Connect("wss://pubsub-edge.twitch.tv:443");
                    m_IRCWebSocket.Connect("wss://irc-ws.chat.twitch.tv:443");

                    m_ProcessQueuedMessagesTask = Task.Run(ProcessQueuedMessages);

                    /// Cache OAuth token
                    m_OAuthTokenCache = m_OAuthToken;
                }
            }
        }
        /// <summary>
        /// Stop the service
        /// </summary>
        internal void Stop()
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

                m_PubSubSocket.Disconnect();
                m_IRCWebSocket.Disconnect();

                foreach (var l_Channel in m_Channels)
                {
                    m_DataProvider.TryReleaseChannelResources(l_Channel.Value);
                    Logger.Instance.Info($"Removed channel {l_Channel.Value.Id} from the channel list.");
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
        /// When settings changed
        /// </summary>
        /// <param name="p_Credentials">New credential</param>
        private void OnCredentialsUpdated()
        {
            if (!m_IsStarted)
                return;

            /// Restart if OAuth token is different
            if (m_OAuthToken != m_OAuthTokenCache)
            {
                Stop();
                Start();
            }
            /// Join / Leave missing channels
            else
            {
                var l_ChannelList = AuthConfig.Twitch.Channels.Split(',').ToList();
                foreach (var l_ChannelToJoin in l_ChannelList)
                {
                    if (!m_Channels.ContainsKey(l_ChannelToJoin))
                        JoinChannel(l_ChannelToJoin);
                }

                foreach (var l_Channel in m_Channels)
                {
                    if (!l_ChannelList.Contains(l_Channel.Key))
                        PartChannel(l_Channel.Key);
                }
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
            while (m_IsStarted)
            {
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
                        await Task.Delay((int)l_RemainingMilliseconds);
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

                Thread.Sleep(10);
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
            Logger.Instance.Info($"Trying to join channel #{p_Channel}");
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
            if (!(p_Channel is TwitchChannel))
                return;

            string l_MessageID  = System.Guid.NewGuid().ToString();
            string l_Message    = $"@id={l_MessageID} PRIVMSG #{p_Channel.Id} :{p_Message}";

            m_SentMessageIDs.TryAdd(l_MessageID, true);

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
                Logger.Instance.Warn("WebSocket service is not connected!");
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
            Logger.Instance.Warn("TwitchPubSub Send topics");

            var l_OAuth = m_OAuthToken;
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
            var l_OAuth = m_OAuthToken;
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
                            var l_Channel = m_Channels.Select(x => x.Value).Where(x => x.AsTwitchChannel().Roomstate.RoomId == l_CurrentChannel.Key).FirstOrDefault();
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
            var l_OAuth = m_OAuthToken;
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
            Logger.Instance.Info("Twitch connection opened");
            m_IRCWebSocket.SendMessage("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");

            Logger.Instance.Info("Trying to login!");
            if (!string.IsNullOrEmpty(m_OAuthToken))
                m_IRCWebSocket.SendMessage($"PASS {m_OAuthToken}");

            m_IRCWebSocket.SendMessage($"NICK BeatSaberPlus{m_Random.Next(10000, 1000000)}");
        }
        /// <summary>
        /// Twitch IRC socket close
        /// </summary>
        private void IRCSocket_OnClose()
        {
            Logger.Instance.Info("Twitch connection closed");
        }
        /// <summary>
        /// Twitch IRC socket error
        /// </summary>
        private void IRCSocket_OnError()
        {
            Logger.Instance.Error("An error occurred in Twitch connection");
        }
        /// <summary>
        /// When a twitch IRC message is received
        /// </summary>
        /// <param name="p_RawMessage">Raw message</param>
        private void IRCSocket_OnMessageReceived(string p_RawMessage)
        {
            lock (m_MessageReceivedLock)
            {
                ///Logger.Instance.Info("RawMessage: " + p_RawMessage);
                if (m_MessageParser.ParseRawMessage(p_RawMessage, m_Channels, m_LoggedInUser, out var l_ParsedMessages))
                {
                    foreach (TwitchMessage l_TwitchMessage in l_ParsedMessages)
                    {
                        if (m_SentMessageIDs.TryRemove(l_TwitchMessage.Id, out var _))
                            l_TwitchMessage.Sender = m_LoggedInUser;

                        var l_TwitchChannel = (l_TwitchMessage.Channel as TwitchChannel);
                        if (l_TwitchChannel.Roomstate == null)
                            l_TwitchChannel.Roomstate = m_Channels.TryGetValue(l_TwitchMessage.Channel.Id, out var l_Channel) ? (l_Channel as TwitchChannel).Roomstate : new TwitchRoomstate();

                        switch (l_TwitchMessage.Type)
                        {
                            case "PING":
                                SendRawMessage("PONG :tmi.twitch.tv");
                                continue;

                            /// Successful login
                            case "376":
                                m_DataProvider.TryRequestGlobalResources();
                                m_LoggedInUsername = l_TwitchMessage.Channel.Id;

                                /// This isn't a typo, when you first sign in your username is in the channel id.
                                Logger.Instance.Info($"Logged into Twitch as {m_LoggedInUsername}");

                                m_OnLoginCallbacks?.InvokeAll(this);

                                var l_ChannelList = AuthConfig.Twitch.Channels.Split(',').ToList();
                                foreach (var l_ChannelToJoin in l_ChannelList)
                                    JoinChannel(l_ChannelToJoin);

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
                                continue;

                            case "JOIN":
                                ///Logger.Instance.Info($"{twitchMessage.Sender.Name} JOINED {twitchMessage.Channel.Id}. LoggedInuser: {LoggedInUser.Name}");
                                if (l_TwitchMessage.Sender.UserName == m_LoggedInUsername
                                    && !m_Channels.ContainsKey(l_TwitchMessage.Channel.Id))
                                {
                                    m_Channels[l_TwitchMessage.Channel.Id] = l_TwitchMessage.Channel.AsTwitchChannel();
                                    Logger.Instance.Info($"Added channel {l_TwitchMessage.Channel.Id} to the channel list.");
                                    m_OnJoinRoomCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);
                                }
                                continue;

                            case "PART":
                                ///Logger.Instance.Info($"{twitchMessage.Sender.Name} PARTED {twitchMessage.Channel.Id}. LoggedInuser: {LoggedInUser.Name}");
                                if (l_TwitchMessage.Sender.UserName == m_LoggedInUsername
                                    && m_Channels.TryRemove(l_TwitchMessage.Channel.Id, out var l_Channel))
                                {
                                    m_DataProvider.TryReleaseChannelResources(l_TwitchMessage.Channel);
                                    Logger.Instance.Info($"Removed channel {l_Channel.Id} from the channel list.");
                                    m_OnLeaveRoomCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);

                                    if (!string.IsNullOrEmpty(m_OAuthToken))
                                        UnsubscribeTopics(l_TwitchChannel.Roomstate.RoomId, l_TwitchChannel.Name);
                                }
                                continue;

                            case "ROOMSTATE":
                                m_Channels[l_TwitchMessage.Channel.Id] = l_TwitchMessage.Channel;
                                m_DataProvider.TryRequestChannelResources(l_TwitchMessage.Channel, (x) => m_OnChannelResourceDataCached?.InvokeAll(this, l_TwitchMessage.Channel, x));

                                m_OnRoomStateUpdatedCallbacks?.InvokeAll(this, l_TwitchMessage.Channel);

                                if (!string.IsNullOrEmpty(m_OAuthToken))
                                {
                                    SubscribeTopics(new string[] {
                                        "following",
                                        "channel-subscribe-events-v1",
                                        "channel-bits-events-v2",
                                        "channel-points-channel-v1",
                                        "video-playback"
                                    }, l_TwitchChannel.Roomstate.RoomId, l_TwitchChannel.Name);
                                }
                                continue;

                            case "USERSTATE":
                            case "GLOBALUSERSTATE":
                                m_LoggedInUser = l_TwitchMessage.Sender.AsTwitchUser();

                                if (string.IsNullOrEmpty(m_LoggedInUser.DisplayName))
                                    m_LoggedInUser.DisplayName = m_LoggedInUsername;

                                if (l_TwitchMessage.Type == "GLOBALUSERSTATE")
                                    m_LoggedInUserID = m_LoggedInUser.Id;
                                else
                                    m_LoggedInUser.Id = m_LoggedInUserID;

                                continue;

                            case "CLEARCHAT":
                                l_TwitchMessage.Metadata.TryGetValue("target-user-id", out var l_TargetUser);
                                m_OnChatClearedCallbacks?.InvokeAll(this, l_TargetUser);
                                continue;

                            case "CLEARMSG":
                                if (l_TwitchMessage.Metadata.TryGetValue("target-msg-id", out var l_TargetMessage))
                                    m_OnMessageClearedCallbacks?.InvokeAll(this, l_TargetMessage);

                                continue;

                            ///case "MODE":
                            ///case "NAMES":
                            ///case "HOSTTARGET":
                            ///case "RECONNECT":
                            ///    Logger.Instance.Info($"No handler exists for type {twitchMessage.Type}. {rawMessage}");
                            ///    continue;
                        }
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
            Logger.Instance.Info("TwitchPubSub connection opened");
            ResubscribeChannelsTopics();
        }
        /// <summary>
        /// Twitch PubSub socket close
        /// </summary>
        private void PubSubSocket_OnClose()
        {
            Logger.Instance.Info("TwitchPubSub connection closed");
        }
        /// <summary>
        /// Twitch PubSub socket error
        /// </summary>
        private void PubSubSocket_OnError()
        {
            Logger.Instance.Error("An error occurred in TwitchPubSub connection");
        }
        /// <summary>
        /// When a twitch PubSub message is received
        /// </summary>
        /// <param name="p_RawMessage">Raw message</param>
        private void PubSubSocket_OnMessageReceived(string p_RawMessage)
        {
            lock (m_MessageReceivedLock)
            {
                ///Logger.Instance.Warning("TwitchPubSub " + p_RawMessage);

                var l_MessageType = "";
                if (SimpleJSON.JSON.Parse(p_RawMessage).TryGetKey("type", out var l_TypeValue))
                    l_MessageType = l_TypeValue.Value;

                switch (l_MessageType?.ToLower())
                {
                    case "response":
                        var resp = new PubSubTopicListenResponse(p_RawMessage);
                        Logger.Instance.Warn("TwitchPubSub joined topic result " + resp.Successful);
                        break;

                    case "message":
                        ///Logger.Instance.Warning("TwitchPubSub " + SimpleJSON.JSON.Parse(p_RawMessage));
                        var l_Message = new PubSubMessage(p_RawMessage);
                        switch (l_Message.Topic.Split('.')[0])
                        {
                            case "channel-subscribe-events-v1":
                                var l_SubscriptionMessage = l_Message.MessageData as PubSubChannelSubscription;

                                var l_SubscriptionUser = new TwitchUser()
                                {
                                    Id              = l_SubscriptionMessage.UserId,
                                    UserName        = l_SubscriptionMessage.Username,
                                    DisplayName     = l_SubscriptionMessage.DisplayName,
                                    Color           = "#FFFFFFFF",
                                    Badges          = new IChatBadge[] { },
                                    IsBroadcaster   = false,
                                    IsModerator     = false,
                                    IsSubscriber    = false,
                                    IsTurbo         = false,
                                    IsVip           = false
                                };
                                var l_SubscriptionChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.AsTwitchChannel().Roomstate.RoomId == l_SubscriptionMessage.ChannelId);
                                var l_SubscriptionEvent = new TwitchSubscriptionEvent()
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

                                var l_BitsUser = new TwitchUser()
                                {
                                    Id              = !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.UserId   : "AnAnonymousCheerer",
                                    UserName        = !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.Username : "AnAnonymousCheerer",
                                    DisplayName     = !l_ChannelBitsMessage.IsAnonymous ? l_ChannelBitsMessage.Username : "AnAnonymousCheerer",
                                    Color           = "#FFFFFFFF",
                                    Badges          = new IChatBadge[] { },
                                    IsBroadcaster   = false,
                                    IsModerator     = false,
                                    IsSubscriber    = false,
                                    IsTurbo         = false,
                                    IsVip           = false
                                };
                                var l_BitsChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.AsTwitchChannel().Roomstate.RoomId == l_ChannelBitsMessage.ChannelId);

                                m_OnChannelBitsCallbacks?.InvokeAll(this, l_BitsChannel, l_BitsUser, l_ChannelBitsMessage.BitsUsed);
                                break;

                            case "channel-points-channel-v1":
                                var l_ChannelPointsMessage = l_Message.MessageData as PubSubChannelPointsEvents;

                                var l_PointsUser    = new TwitchUser()
                                {
                                    Id              = l_ChannelPointsMessage.UserId,
                                    UserName        = l_ChannelPointsMessage.UserName,
                                    DisplayName     = l_ChannelPointsMessage.UserDisplayName,
                                    Color           = "#FFFFFFFF",
                                    Badges          = new IChatBadge[] { },
                                    IsBroadcaster   = l_ChannelPointsMessage.UserId == m_LoggedInUser.Id,
                                    IsModerator     = false,
                                    IsSubscriber    = false,
                                    IsTurbo         = false,
                                    IsVip           = false
                                };
                                var l_PointsChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.AsTwitchChannel().Roomstate.RoomId == l_ChannelPointsMessage.ChannelId);
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

                                var l_FollowUser = new TwitchUser()
                                {
                                    Id              = l_FollowMessage.UserId,
                                    UserName        = l_FollowMessage.Username,
                                    DisplayName     = l_FollowMessage.DisplayName,
                                    Color           = "#FFFFFFFF",
                                    Badges          = new IChatBadge[] { },
                                    IsBroadcaster   = false,
                                    IsModerator     = false,
                                    IsSubscriber    = false,
                                    IsTurbo         = false,
                                    IsVip           = false
                                };
                                var l_FollowChannel = m_Channels.Select(x => x.Value).FirstOrDefault(x => x.AsTwitchChannel().Roomstate.RoomId == l_FollowMessage.FollowedChannelId);

                                m_OnChannelFollowCallbacks?.InvokeAll(this, l_FollowChannel, l_FollowUser);
                                break;

                            case "video-playback":
                                var l_VideoPlaybackMessage  = l_Message.MessageData as PubSubVideoPlayback;
                                var l_Channel               = m_Channels.Where(x => x.Key.ToLower() == l_Message.Topic.Split('.')[1].ToLower()).Select(x => x.Value).SingleOrDefault();

                                if (l_Channel != null)
                                    m_OnRoomVideoPlaybackUpdatedCallbacks?.InvokeAll(this, l_Channel, l_VideoPlaybackMessage.Type == PubSubVideoPlayback.VideoPlaybackType.StreamUp || l_VideoPlaybackMessage.Viewers != 0, l_VideoPlaybackMessage.Viewers);

                                break;

                        }
                        break;

                }
            }
        }
    }
}
