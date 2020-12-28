using BeatSaberMarkupLanguage.Attributes;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Moderation left screen
    /// </summary>
    internal class ModerationLeft : SDK.UI.ResourceViewController<ModerationLeft>
    {
#pragma warning disable CS0649
        /// <summary>
        /// Room state text
        /// </summary>
        [UIComponent("RoomStateText")]
        private TextMeshProUGUI m_RoomStateText = null;
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            UpdateRoomState();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update room state
        /// </summary>
        internal void UpdateRoomState()
        {
            if (!UICreated)
                return;

            var l_States = new List<string>();

            if (SDK.Chat.Service.Multiplexer.Channels.Count != 0)
            {
                var l_Channel = SDK.Chat.Service.Multiplexer.Channels.First();
                if (l_Channel.Item2 is BeatSaberPlusChatCore.Models.Twitch.TwitchChannel)
                {
                    var l_TwitchChannel = l_Channel.Item2 as BeatSaberPlusChatCore.Models.Twitch.TwitchChannel;

                    if (l_TwitchChannel.Roomstate.EmoteOnly)
                        l_States.Add("Emotes only");
                    if (l_TwitchChannel.Roomstate.FollowersOnly)
                        l_States.Add("Followers only");
                    if (l_TwitchChannel.Roomstate.SlowModeInterval != 0)
                        l_States.Add("Slow mode (" + l_TwitchChannel.Roomstate.SlowModeInterval + "s)");
                }
            }

            if (l_States.Count == 0)
                l_States.Add("Normal");

            m_RoomStateText.text = "Room State: " + string.Join(", ", l_States.ToArray());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for moderation permissions
        /// </summary>
        /// <returns></returns>
        private bool CheckForModeratorPermissions()
        {
            if (SDK.Chat.Service.Multiplexer.Channels.Count == 0)
            {
                ShowMessageModal("You are not connected to a chat channel!");
                return false;
            }

            /// todo

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Toggle emote only
        /// </summary>
        [UIAction("click-toggle-emote-only-mode-btn-pressed")]
        private void OnToggleEmoteOnlyModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is BeatSaberPlusChatCore.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as BeatSaberPlusChatCore.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.EmoteOnly)
                    ShowConfirmationModal("Do you really want to <b>disable</b> emote only mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonlyoff"));
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> emote only mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonly"));
            }
        }
        /// <summary>
        /// Toggle follower mode
        /// </summary>
        [UIAction("click-toggle-follower-mode-btn-pressed")]
        private void OnToggleFollowerModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is BeatSaberPlusChatCore.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as BeatSaberPlusChatCore.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.FollowersOnly)
                    ShowConfirmationModal("Do you really want to <b>disable</b> follower only mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/followersoff"));
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> follower only mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/followers"));
            }
        }
        /// <summary>
        /// Toggle slow mode
        /// </summary>
        [UIAction("click-toggle-slow-mode-btn-pressed")]
        private void OnToggleSlowModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is BeatSaberPlusChatCore.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as BeatSaberPlusChatCore.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.SlowModeInterval != 0)
                    ShowConfirmationModal("Do you really want to <b>disable</b> slow mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/slowoff"));
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> slow mode?", () => l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/slow"));
            }
        }
    }
}
