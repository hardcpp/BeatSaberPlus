using BeatSaberPlus.SDK.Chat.Interfaces;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations
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
            if (p_ChatService is BeatSaberPlus.SDK.Chat.Services.Twitch.TwitchService)
            {
                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
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
            => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.ChatFollow, ChatService = p_ChatService, Channel = p_Channel, User = p_User });
        /// <summary>
        /// On channel bits
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_BitsUsed">Used bits</param>
        private void ChatCoreMutiplixer_OnChannelBits(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, int p_BitsUsed)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.ChatBits, ChatService = p_ChatService, Channel = p_Channel, User = p_User, BitsEvent = p_BitsUsed });
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
                HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.ChatPointsReward, ChatService = p_ChatService, Channel = p_Channel, User = p_User, PointsEvent = p_Event });
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance?.Error("[Modules.ChatIntegrations][ChatIntegration.ChatCoreMutiplixer_OnChannelPoints] Error :");
                Logger.Instance?.Error(p_Exception);
            }
        }
        /// <summary>
        /// On channel subscription
        /// </summary>
        /// <param name="p_ChatService">Chat service</param>
        /// <param name="p_Channel">Channel instance</param>
        /// <param name="p_User">User instance</param>
        /// <param name="p_Event">Event</param>
        private void ChatCoreMutiplixer_OnChannelSubscription(IChatService p_ChatService, IChatChannel p_Channel, IChatUser p_User, IChatSubscriptionEvent p_Event)
            => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.ChatSubscription, ChatService = p_ChatService, Channel = p_Channel, User = p_User, SubscriptionEvent = p_Event });
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
                    BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() => OnBroadcasterChatMessage?.Invoke(p_Message));
                else
                    HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.ChatMessage, ChatService = p_ChatService, Channel = p_Message.Channel, User = p_Message.Sender, Message = p_Message });
            }
            catch (System.Exception p_Exception)
            {
                Logger.Instance?.Error("[Modules.ChatIntegrations][ChatIntegration.ChatCoreMutiplixer_OnTextMessageReceived] Error :");
                Logger.Instance?.Error(p_Exception);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On level started
        /// </summary>
        /// <param name="p_Data">Level data</param>
        private void Game_OnLevelStarted(BeatSaberPlus.SDK.Game.LevelData p_Data)
        {
            Task.Run(() => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.LevelStarted, LevelData  = p_Data }));

            SharedCoroutineStarter.instance.StartCoroutine(Game_FindPauseManager(p_Data));
        }
        private IEnumerator Game_FindPauseManager(BeatSaberPlus.SDK.Game.LevelData p_Data)
        {
            var l_PauseController = null as PauseController;
            yield return new WaitUntil(() => (l_PauseController = GameObject.FindObjectOfType<PauseController>()));

            if (l_PauseController)
            {
                l_PauseController.didPauseEvent += () =>
                {
                    Task.Run(() => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.LevelPaused, LevelData = p_Data }));
                };
                l_PauseController.didResumeEvent += () =>
                {
                    Task.Run(() => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.LevelResumed, LevelData = p_Data }));
                };
            }
        }
        /// <summary>
        /// On level ended
        /// </summary>
        /// <param name="p_Data">Completion data</param>
        private void Game_OnLevelEnded(BeatSaberPlus.SDK.Game.LevelCompletionData p_Data)
        {
            Task.Run(() => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.LevelEnded, LevelCompletionData = p_Data }));
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
                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() => OnVoiceAttackCommandExecuted?.Invoke(p_GUID, p_Name));
            else
                Task.Run(() => HandleEvents(new Models.EventContext() { Type = Interfaces.TriggerType.VoiceAttackCommand, VoiceAttackCommandGUID = p_GUID, VoiceAttackCommandName = p_GUID }));
        }

    }
}
