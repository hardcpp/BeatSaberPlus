using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Moderation left screen
    /// </summary>
    internal sealed class ModerationLeftView : CP_SDK.UI.ViewController<ModerationLeftView>
    {
        private XUIText m_RoomStateText = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Channel Actions"),

                XUIVLayout.Make(
                    XUIVSpacer.Make(5f),

                    XUIText.Make("RoomState: Normal")
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .Bind(ref m_RoomStateText),

                    XUIVSpacer.Make(5f),

                    XUIPrimaryButton.Make("Toggle emote only mode",  OnToggleEmoteOnlyModeButton),
                    XUIPrimaryButton.Make("Toggle follower mode",    OnToggleFollowerModeButton),
                    XUIPrimaryButton.Make("Toggle slow mode",        OnToggleSlowModeButton),

                    XUIVSpacer.Make(5f),

                    XUISecondaryButton.Make("Manage shortcuts",      OnManageShortcutButton),

                    XUIVSpacer.Make(5f)
                )
                .SetWidth(60f)
                .SetPadding(0)
                .ForEachDirect<XUIPrimaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
                .ForEachDirect<XUISecondaryButton>(y =>
                {
                    y.SetHeight(8f);
                    y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                })
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
            => UpdateRoomState();

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

            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count != 0)
            {
                var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
                if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel)
                {
                    var l_TwitchChannel = l_Channel.Item2 as CP_SDK.Chat.Models.Twitch.TwitchChannel;

                    if (l_TwitchChannel.Roomstate.EmoteOnly)                l_States.Add("Emotes only");
                    if (l_TwitchChannel.Roomstate.FollowersOnly)            l_States.Add("Followers only");
                    if (l_TwitchChannel.Roomstate.SlowModeInterval != 0)    l_States.Add("Slow mode (" + l_TwitchChannel.Roomstate.SlowModeInterval + "s)");
                }
            }

            if (l_States.Count == 0)
                l_States.Add("Normal");

            m_RoomStateText.SetText("Room State: " + string.Join(", ", l_States.ToArray()));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for moderation permissions
        /// </summary>
        /// <returns></returns>
        private bool CheckForModeratorPermissions()
        {
            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count == 0)
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
        private void OnToggleEmoteOnlyModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as CP_SDK.Chat.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.EmoteOnly)
                    ShowConfirmationModal("Do you really want to <b>disable</b> emote only mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonlyoff"); });
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> emote only mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/emoteonly"); });
            }
        }
        /// <summary>
        /// Toggle follower mode
        /// </summary>
        private void OnToggleFollowerModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as CP_SDK.Chat.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.FollowersOnly)
                    ShowConfirmationModal("Do you really want to <b>disable</b> follower only mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/followersoff"); });
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> follower only mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/followers"); });
            }
        }
        /// <summary>
        /// Toggle slow mode
        /// </summary>
        private void OnToggleSlowModeButton()
        {
            if (!CheckForModeratorPermissions())
                return;

            var l_Channel = CP_SDK.Chat.Service.Multiplexer.Channels.First();
            if (l_Channel.Item2 is CP_SDK.Chat.Models.Twitch.TwitchChannel)
            {
                var l_TwitchChannel = l_Channel.Item2 as CP_SDK.Chat.Models.Twitch.TwitchChannel;

                if (l_TwitchChannel.Roomstate.SlowModeInterval != 0)
                    ShowConfirmationModal("Do you really want to <b>disable</b> slow mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/slowoff"); });
                else
                    ShowConfirmationModal("Do you really want to <b>enable</b> slow mode?", (x) => { if (x) l_Channel.Item1.SendTextMessage(l_Channel.Item2, "/slow"); });
            }
        }
        /// <summary>
        /// Manage shortcuts
        /// </summary>
        private void OnManageShortcutButton()
        {
            ModerationViewFlowCoordinator.Instance().SwitchToShortcuts();
        }
    }
}
