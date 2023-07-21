using CP_SDK.Chat.Interfaces;

namespace ChatPlexMod_ChatIntegrations
{
    /// <summary>
    /// ChatIntegrations instance
    /// </summary>
    public partial class ChatIntegrations
    {
        /// <summary>
        /// On channel join
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        private void ChatCoreMutiplixer_OnJoinChannel(IChatService p_ChatService, IChatChannel p_Channel)
        {
            if (p_ChatService is CP_SDK.Chat.Services.Twitch.TwitchService)
            {
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                {
                    m_Events.ForEach(x =>
                    {
                        /// Re enabled channel points reward on join
                        if (x.IsEnabled)
                            x.OnEnable();
                    });
                });
            }
        }
        /// <summary>
        /// On channel follow
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        private void ChatCoreMutiplixer_OnChannelFollow(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatFollow, ChatService = p_ChatService, Channel = p_Channel, User = p_User });
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Used bits</param>
        private void ChatCoreMutiplixer_OnChannelBits(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatBits, ChatService = p_ChatService, Channel = p_Channel, User = p_User, BitsEvent = p_BitsUsed });
        /// <summary>
        /// On channel points
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelPoints(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatChannelPointEvent p_Event)
        {
            try
            {
                HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatPointsReward, ChatService = p_ChatService, Channel = p_Channel, User = p_User, PointsEvent = p_Event });
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_ChatIntegrations][ChatIntegration.ChatCoreMutiplixer_OnChannelPoints] Error :");
                Logger.Instance.Error(p_Exception);
            }
        }
        /// <summary>
        /// On channel raid
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplexer_OnChannelRaid(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_Event)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatRaid, ChatService = p_ChatService, Channel = p_Channel, User = p_User, RaidEvent = p_Event });
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelSubscription(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatSubscription, ChatService = p_ChatService, Channel = p_Channel, User = p_User, SubscriptionEvent = p_Event });
        /// <summary>
        /// On text message received
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Message">ID of the message</param>
        private void ChatCoreMutiplixer_OnTextMessageReceived(IChatService p_ChatService, IChatMessage p_Message)
        {
            try
            {
                if (p_Message.Sender.IsBroadcaster && OnBroadcasterChatMessage != null)
                    CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => OnBroadcasterChatMessage?.Invoke(p_Message));
                else
                    HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.ChatMessage, ChatService = p_ChatService, Channel = p_Message.Channel, User = p_Message.Sender, Message = p_Message });
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance.Error("[ChatPlexMod_ChatIntegrations][ChatIntegration.ChatCoreMutiplixer_OnTextMessageReceived] Error :");
                Logger.Instance.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On VoiceAttack command executed
        /// </summary>
        /// <param name="p_GUID">Command GUID</param>
        /// <param name="p_Name">Command Name</param>
        private void VoiceAttack_OnCommandExecuted(string p_GUID, string p_Name)
        {
            if (OnVoiceAttackCommandExecuted != null)
                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => OnVoiceAttackCommandExecuted?.Invoke(p_GUID, p_Name));
            else
                CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() => HandleEvents(new Models.EventContext() { Type = Interfaces.ETriggerType.VoiceAttackCommand, VoiceAttackCommandGUID = p_GUID, VoiceAttackCommandName = p_GUID }));
        }
    }
}
