using CP_SDK.Chat.SimpleJSON;
using System;
using System.Collections.Generic;

namespace CP_SDK.Chat.Models.Twitch
{
    /// <summary>
    /// Twitch PubSub topic listen response
    /// </summary>
    internal class PubSubTopicListenResponse
    {
        /// <summary>
        /// IF error exists, it will be here
        /// </summary>
        internal string Error { get; private set; }
        /// <summary>
        /// Unique communication token
        /// </summary>
        internal string Nonce { get; private set; }
        /// <summary>
        /// Whether or not successful
        /// </summary>
        internal bool Successful { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>Response model constructor.</summary>
        internal PubSubTopicListenResponse(string p_RawJSON)
        {
            JSONNode l_JSON = JSON.Parse(p_RawJSON);
            if (l_JSON.TryGetKey("error", out var val1)) { Error = val1.Value; }
            if (l_JSON.TryGetKey("nonce", out var val2)) { Nonce = val2.Value; }

            if (string.IsNullOrWhiteSpace(Error))
                Successful = true;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Twitch PubSub base message data
    /// </summary>
    internal abstract class PubSubMessageData
    {
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Twitch PubSub follow event
    /// </summary>
    internal class PubSubFollowing : PubSubMessageData
    {
        /// <summary>
        /// Following user display name.
        /// </summary>
        internal string DisplayName { get; private set; }
        /// <summary>
        /// Following user username.
        /// </summary>
        internal string Username { get; private set; }
        /// <summary>
        /// Following user user-id.
        /// </summary>
        internal string UserId { get; private set; }
        /// <summary>
        /// ID of the followed channel
        /// </summary>
        internal string FollowedChannelId { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Following constructor.
        /// </summary>
        /// <param name="p_RawJSON"></param>
        internal PubSubFollowing(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);
            if (l_JSON.TryGetKey("display_name", out l_Value)) { DisplayName   = l_Value.Value; }
            if (l_JSON.TryGetKey("username",     out l_Value)) { Username      = l_Value.Value; }
            if (l_JSON.TryGetKey("user_id",      out l_Value)) { UserId        = l_Value.Value; }
        }
    }

    /// <summary>
    /// Twitch PubSub subscription event
    /// </summary>
    internal class PubSubChannelSubscription : PubSubMessageData
    {
        /// <summary>
        /// Subscription plans
        /// </summary>
        internal enum PubSubSubscriptionPlan
        {
            NotSet,
            Prime,
            Tier1,
            Tier2,
            Tier3
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Generic pub sub message
        /// </summary>
        internal class PubSubSubMessage
        {
            /// <summary>
            /// Message
            /// </summary>
            internal string Message { get; private set; }
            /// <summary>
            /// Emotes
            /// </summary>
            internal List<Emote> Emotes { get; private set; } = new List<Emote>();

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="p_JSON">Input JSON</param>
            internal PubSubSubMessage(JSONNode p_JSON)
            {
                JSONNode l_Value;
                if (p_JSON.TryGetKey("message", out l_Value)) { Message = l_Value.Value; }
                if (p_JSON.TryGetKey("emotes",  out l_Value))
                {
                    foreach (var token in l_Value.AsArray)
                        Emotes.Add(new Emote(token.Value));
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            ////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// PubSub emote
            /// </summary>
            internal class Emote
            {
                /// <summary>
                /// Start index
                /// </summary>
                internal int Start { get; private set; }
                /// <summary>
                /// End index
                /// </summary>
                internal int End { get; private set; }
                /// <summary>
                /// Emote ID
                /// </summary>
                internal int Id { get; private set; }

                internal Emote(JSONNode p_JSON)
                {
                    JSONNode l_Value;
                    if (p_JSON.TryGetKey("start", out l_Value)) { Start = l_Value.AsInt; }
                    if (p_JSON.TryGetKey("start", out l_Value)) { End   = l_Value.AsInt; }
                    if (p_JSON.TryGetKey("start", out l_Value)) { Id    = l_Value.AsInt; }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Buyer user name
        /// </summary>
        internal string Username { get; private set; }
        /// <summary>
        /// Buyer display name
        /// </summary>
        internal string DisplayName { get; private set; }
        /// <summary>
        /// Target channel name
        /// </summary>
        internal string ChannelName { get; private set; }
        /// <summary>
        /// ID of the buyer
        /// </summary>
        internal string UserId { get; private set; }
        /// <summary>
        /// Target channel id
        /// </summary>
        internal string ChannelId { get; private set; }
        /// <summary>
        /// Time
        /// </summary>
        internal DateTime Time { get; private set; }
        /// <summary>
        /// Type of subscription
        /// </summary>
        internal PubSubSubscriptionPlan SubscriptionPlan { get; private set; }
        /// <summary>
        /// Name of the subscription plan
        /// </summary>
        internal string SubscriptionPlanName { get; private set; }
        /// <summary>
        /// How many month user was sub
        /// </summary>
        internal int CumulativeMonths { get; private set; }
        /// <summary>
        /// How many month user was sub in continue
        /// </summary>
        internal int StreakMonths { get; private set; }
        /// <summary>
        /// Context "sub", "resub", "subgift"
        /// </summary>
        internal string Context { get; private set; }
        /// <summary>
        /// Is a gifted sub
        /// </summary>
        internal bool IsGift { get; private set; }
        /// <summary>
        /// Recipient Id
        /// </summary>
        internal string RecipientId { get; private set; }
        /// <summary>
        /// Recipient user name
        /// </summary>
        internal string RecipientName { get; private set; }
        /// <summary>
        /// Recipient display name
        /// </summary>
        internal string RecipientDisplayName { get; private set; }
        /// <summary>
        /// Message for the sub
        /// </summary>
        internal PubSubSubMessage SubMessage { get; private set; }
        /// <summary>
        /// Purchased month
        /// </summary>
        internal int PurchasedMonthCount { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_RawJSON">Input JSON</param>
        internal PubSubChannelSubscription(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);

            if (l_JSON.TryGetKey("user_name",       out l_Value)) { Username    = l_Value.Value; }
            if (l_JSON.TryGetKey("display_name",    out l_Value)) { DisplayName = l_Value.Value; }
            if (l_JSON.TryGetKey("channel_name",    out l_Value)) { ChannelName = l_Value.Value; }
            if (l_JSON.TryGetKey("user_id",         out l_Value)) { UserId      = l_Value.Value; }
            if (l_JSON.TryGetKey("channel_id",      out l_Value)) { ChannelId   = l_Value.Value; }
            if (l_JSON.TryGetKey("time",            out l_Value)) { Time        = DateTimeStringToObject(l_Value.Value); }

            if (l_JSON.TryGetKey("sub_plan",        out l_Value))
            {
                switch (l_Value.Value.ToLower())
                {
                    case "prime": SubscriptionPlan = PubSubSubscriptionPlan.Prime; break;
                    case "1000":  SubscriptionPlan = PubSubSubscriptionPlan.Tier1; break;
                    case "2000":  SubscriptionPlan = PubSubSubscriptionPlan.Tier2; break;
                    case "3000":  SubscriptionPlan = PubSubSubscriptionPlan.Tier3; break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            if (l_JSON.TryGetKey("sub_plan_name",           out l_Value)) { SubscriptionPlanName    = l_Value.Value;             }
            if (l_JSON.TryGetKey("cumulative_months",       out l_Value)) { CumulativeMonths        = l_Value.AsInt;             }
            if (l_JSON.TryGetKey("streak_months",           out l_Value)) { StreakMonths            = l_Value.AsInt;             }
            if (l_JSON.TryGetKey("multi_month_duration",    out l_Value)) { PurchasedMonthCount     = l_Value.AsInt;             }
            if (l_JSON.TryGetKey("context",                 out l_Value)) { Context                 = l_Value.Value;             }
            if (l_JSON.TryGetKey("is_gift",                 out l_Value)) { IsGift                  = l_Value.AsBool;            }
            if (l_JSON.TryGetKey("sub_message",             out l_Value)) { SubMessage = new PubSubSubMessage(l_Value.Value);    }
            if (l_JSON.TryGetKey("recipient_id",            out l_Value)) { RecipientId             = l_Value.Value;             }
            if (l_JSON.TryGetKey("recipient_user_name",     out l_Value)) { RecipientName           = l_Value.Value;             }
            if (l_JSON.TryGetKey("recipient_display_name",  out l_Value)) { RecipientDisplayName    = l_Value.Value;             }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// String DateTime to DateTime
        /// </summary>
        /// <param name="p_Str">Input string</param>
        /// <returns></returns>
        private static DateTime DateTimeStringToObject(string p_Str)
        {
            return p_Str == null ? new DateTime() : Convert.ToDateTime(p_Str);
        }
    }

    /// <summary>
    /// Twitch PubSub bits event
    /// </summary>
    internal class PubSubChannelBitsEvents : PubSubMessageData
    {
        /// <summary>
        /// Information about a user’s new badge level, if the cheer was not anonymous and the user reached a new badge level with this cheer. Otherwise, null.
        /// </summary>
        internal string BadgeEntitlement { get; private set; }
        /// <summary>
        /// The amount of bits sent.
        /// </summary>
        internal int BitsUsed { get; private set; }
        /// <summary>
        /// Channel/User ID of where the bits were sent to.
        /// </summary>
        internal string ChannelId { get; private set; }
        /// <summary>
        /// Chat message that accompanied the bits.
        /// </summary>
        internal string ChatMessage { get; private set; }
        /// <summary>
        /// Context related to event.
        /// </summary>
        internal string Context { get; private set; }
        /// <summary>
        /// Whether or not the event was anonymous.
        /// </summary>
        internal bool IsAnonymous { get; private set; }
        /// <summary>
        /// Message ID.
        /// </summary>
        internal string MessageId { get; private set; }
        /// <summary>
        /// The type of object contained in the data field.
        /// </summary>
        internal string MessageType { get; private set; }
        /// <summary>
        /// Time stamp of the event.
        /// </summary>
        internal string Time { get; private set; }
        /// <summary>
        /// The total amount of bits the user has sent.
        /// </summary>
        internal int TotalBitsUsed { get; private set; }
        /// <summary>
        /// User ID of the sender.
        /// </summary>
        internal string UserId { get; private set; }
        /// <summary>
        /// Username of the sender.
        /// </summary>
        internal string Username { get; private set; }
        /// <summary>
        /// Message version
        /// </summary>
        internal string Version { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ChannelBitsEvent model constructor
        /// </summary>
        /// <param name="p_RawJSON">Input JSON</param>
        internal PubSubChannelBitsEvents(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);

            if (l_JSON.TryGetKey("data", out var l_Data))
            {

                if (l_Data.TryGetKey("badge_entitlement",   out l_Value)) { BadgeEntitlement    = l_Value.Value;  }
                if (l_Data.TryGetKey("bits_used",           out l_Value)) { BitsUsed            = l_Value.AsInt;  }
                if (l_Data.TryGetKey("channel_id",          out l_Value)) { ChannelId           = l_Value.Value;  }
                if (l_Data.TryGetKey("chat_message",        out l_Value)) { ChatMessage         = l_Value.Value;  }
                if (l_Data.TryGetKey("context",             out l_Value)) { Context             = l_Value.Value;  }
                if (l_Data.TryGetKey("is_anonymous",        out l_Value)) { IsAnonymous         = l_Value.AsBool; }
                if (l_Data.TryGetKey("message_id",          out l_Value)) { MessageId           = l_Value.Value;  }
                if (l_Data.TryGetKey("message_type",        out l_Value)) { MessageType         = l_Value.Value;  }
                if (l_Data.TryGetKey("time",                out l_Value)) { Time                = l_Value.Value;  }
                if (l_Data.TryGetKey("total_bits_used",     out l_Value)) { TotalBitsUsed       = l_Value.AsInt;  }
                if (l_Data.TryGetKey("user_id",             out l_Value)) { UserId              = l_Value.Value;  }
                if (l_Data.TryGetKey("user_name",           out l_Value)) { Username            = l_Value.Value;  }
                if (l_Data.TryGetKey("version",             out l_Value)) { Version             = l_Value.Value;  }
            }
        }
    }

    /// <summary>
    /// Twitch PubSub channel points event
    /// </summary>
    internal class PubSubChannelPointsEvents : PubSubMessageData
    {
        /// <summary>
        ///
        /// </summary>
        internal string Type { get; private set; }
        /// <summary>
        /// Time the pubsub message was sent
        /// </summary>
        internal DateTime Time { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string UserId { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string UserName { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string UserDisplayName { get; private set; }
        /// <summary>
        /// Target channel id
        /// </summary>
        internal string ChannelId { get; private set; }
        /// <summary>
        /// Timestamp in which a reward was redeemed
        /// </summary>
        internal DateTime RedeemedAt { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string RewardID { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string TransactionID { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string Title { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string Prompt { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal int Cost { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string Image { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string BackgroundColor { get; private set; }
        /// <summary>
        ///
        /// </summary>
        internal string UserInput { get; private set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ChannelPointsEvent model constructor
        /// </summary>
        /// <param name="p_RawJSON">Input JSON</param>
        internal PubSubChannelPointsEvents(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);

            if (l_JSON.TryGetKey("type", out     l_Value)) { Type = l_Value.Value; }
            if (l_JSON.TryGetKey("data", out var l_Data))
            {
                if (l_Data.TryGetKey("timestamp",   out     l_Value))     { Time = DateTimeStringToObject(l_Value.Value); }
                if (l_Data.TryGetKey("redemption",  out var l_Redemption))
                {
                    if (l_Redemption.TryGetKey("id",                out     l_Value)) { TransactionID       = l_Value.Value; }
                    if (l_Redemption.TryGetKey("user",              out var l_User))
                    {
                        if (l_User.TryGetKey("id",                  out     l_Value)) { UserId              = l_Value.Value; }
                        if (l_User.TryGetKey("login",               out     l_Value)) { UserName            = l_Value.Value; }
                        if (l_User.TryGetKey("display_name",        out     l_Value)) { UserDisplayName     = l_Value.Value; }
                    }
                    if (l_Redemption.TryGetKey("channel_id",        out     l_Value)) { ChannelId           = l_Value.Value; }
                    if (l_Redemption.TryGetKey("redeemed_at",       out     l_Value)) { RedeemedAt          = DateTimeStringToObject(l_Value.Value); }
                    if (l_Redemption.TryGetKey("reward",            out var l_Reward))
                    {
                        if (l_Reward.TryGetKey("id",                out     l_Value)) { RewardID           = l_Value.Value; }
                        if (l_Reward.TryGetKey("title",             out     l_Value)) { Title              = l_Value.Value; }
                        if (l_Reward.TryGetKey("prompt",            out     l_Value)) { Prompt             = l_Value.Value; }
                        if (l_Reward.TryGetKey("cost",              out     l_Value)) { Cost               = l_Value.AsInt; }

                        if (l_Reward.HasKey("image") && l_Reward.GetValueOrDefault("image", "").IsObject)
                        {
                            var l_Node = l_Reward.GetValueOrDefault("image", new JSONObject()).AsObject;
                            for (int l_I = l_Node.Count - 1; l_I >= 0; --l_I)
                            {
                                if (string.IsNullOrEmpty(l_Node[l_I].Value))
                                    continue;

                                Image = l_Node[l_I].Value;
                                break;
                            }
                        }
                        else
                        {
                            var l_Node = l_Reward.GetValueOrDefault("default_image", new JSONObject()).AsObject;
                            for (int l_I = l_Node.Count - 1; l_I >= 0; --l_I)
                            {
                                if (string.IsNullOrEmpty(l_Node[l_I].Value))
                                    continue;

                                Image = l_Node[l_I].Value;
                                break;
                            }
                        }

                        if (l_Reward.TryGetKey("background_color",  out     l_Value)) { BackgroundColor    = l_Value.Value; }
                    }
                    if (l_Redemption.TryGetKey("user_input",        out     l_Value)) { UserInput          = l_Value.Value; }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// String DateTime to DateTime
        /// </summary>
        /// <param name="p_Str">Input string</param>
        /// <returns></returns>
        private static DateTime DateTimeStringToObject(string p_Str)
        {
            return p_Str == null ? new DateTime() : Convert.ToDateTime(p_Str);
        }
    }

    /// <summary>
    /// Twitch PubSub VideoPlayback event
    /// </summary>
    internal class PubSubVideoPlayback : PubSubMessageData
    {
        /// <summary>
        /// Valid playback types.
        /// </summary>
        internal enum VideoPlaybackType
        {
            StreamUp,
            StreamDown,
            ViewCount
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Video playback type
        /// </summary>
        internal VideoPlaybackType Type { get; private set; }
        /// <summary>
        /// Server time stamp
        /// </summary>
        internal string ServerTime { get; private set; }
        /// <summary>
        /// Current delay (if one exists)
        /// </summary>
        internal int PlayDelay { get; private set; }
        /// <summary>
        /// Viewer count
        /// </summary>
        internal int Viewers { get; set; } = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// VideoPlayback constructor.
        /// </summary>
        /// <param name="p_RawJSON"></param>
        internal PubSubVideoPlayback(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);
            if (l_JSON.TryGetKey("type",                    out l_Value))
            {
                switch (l_Value.Value)
                {
                    case "stream-up":
                        Type = VideoPlaybackType.StreamUp;
                        if (l_JSON.TryGetKey("play_delay",  out l_Value)) { PlayDelay   = l_Value.AsInt; }
                        break;
                    case "stream-down":
                        Type = VideoPlaybackType.StreamDown;
                        break;
                    case "viewcount":
                        Type = VideoPlaybackType.ViewCount;
                        if (l_JSON.TryGetKey("viewers",     out l_Value)) { Viewers     = l_Value.AsInt; }
                        break;
                }

            }
            if (l_JSON.TryGetKey("server_time",             out l_Value)) { ServerTime  = l_Value.Value; }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Twitch PubSub message
    /// </summary>
    internal class PubSubMessage
    {
        /// <summary>
        /// Topic of the message
        /// </summary>
        internal string Topic { get; private set; }
        /// <summary>
        /// Message content
        /// </summary>
        internal readonly PubSubMessageData MessageData;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p_RawJSON">Input JSON</param>
        internal PubSubMessage(string p_RawJSON)
        {
            JSONNode l_Value;
            JSONNode l_JSON = JSON.Parse(p_RawJSON);

            if (l_JSON.TryGetKey("data", out var l_Data))
            {
                if (l_Data.TryGetKey("topic",       out l_Value)) { Topic = l_Value.Value; }
                if (l_Data.TryGetKey("message",     out l_Value))
                {
                    switch (Topic?.Split('.')[0])
                    {
                        case "channel-bits-events-v2":
                            MessageData = new PubSubChannelBitsEvents(l_Value.Value);
                            break;
                        case "channel-subscribe-events-v1":
                            MessageData = new PubSubChannelSubscription(l_Value.Value);
                            break;
                        case "channel-points-channel-v1":
                            MessageData = new PubSubChannelPointsEvents(l_Value.Value);
                            break;
                        case "following":
                            MessageData = new PubSubFollowing(l_Value.Value);
                            break;
                        case "video-playback":
                            MessageData = new PubSubVideoPlayback(l_Value.Value);
                            break;
                    }
                }
            }
        }
    }
}
