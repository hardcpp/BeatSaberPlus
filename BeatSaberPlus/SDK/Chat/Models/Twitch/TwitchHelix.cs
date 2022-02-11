using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace BeatSaberPlus.SDK.Chat.Models.Twitch
{
    [Serializable]
    public class Helix_TokenValidate
    {
        [JsonProperty]
        public string client_id = "";
        [JsonProperty]
        public string login = "";
        [JsonProperty]
        public List<string> scopes = new List<string>();
        [JsonProperty]
        public string user_id = "";
        [JsonProperty]
        public int expires_in = 0;
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////
    
    /*
    [Serializable]
    public class Helix_CreateReward
    {
        [JsonProperty]
        public string title = "";
        [JsonProperty]
        public int cost = 0;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string prompt = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_enabled = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string background_color = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_user_input_required = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_max_per_stream_enabled = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? max_per_stream = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_max_per_user_per_stream_enabled = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? max_per_user_per_stream = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? is_global_cooldown_enabled = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? global_cooldown_seconds = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? should_redemptions_skip_request_queue = null;

        public bool Validate(out string p_Message)
        {
            p_Message = "";
            if (string.IsNullOrEmpty(title) || title.Length > 60)
            {
                p_Message = "Title must be between 1 and 60 characters.";
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class Helix_Reward
    {
        [Serializable]
        public class MaxPerStream
        {
            [JsonProperty] public bool is_enabled;
            [JsonProperty] public int max_per_stream;
        }


        [JsonProperty] public string broadcaster_id;
        [JsonProperty] public string broadcaster_login;
        [JsonProperty] public string broadcaster_name;
        [JsonProperty] public string id;
        [JsonProperty] public string title;
        [JsonProperty] public string prompt;
        [JsonProperty] public int cost;
        // image
        // default_image
        [JsonProperty] public string background_color;
        [JsonProperty] public bool is_enabled;


        [JsonProperty] public bool is_user_input_required;

        [JsonProperty] public bool is_max_per_user_per_stream_enabled;
        [JsonProperty] public int max_per_user_per_stream;

        [JsonProperty] public bool is_global_cooldown_enabled;
        [JsonProperty] public int global_cooldown_seconds;

        [JsonProperty] public bool is_paused;
        [JsonProperty] public bool is_in_stock;
        [JsonProperty] public bool should_redemptions_skip_request_queue;
        [JsonProperty] public int redemptions_redeemed_current_stream;
        [JsonProperty] public string cooldown_expires_at;
    }
    */

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    [Serializable]
    public class Helix_CreatePoll
    {
        [Serializable]
        public class Choice
        {
            [JsonProperty]
            public string title = "";
        }

        [JsonProperty]
        public string broadcaster_id = "";
        [JsonProperty]
        public string title = "";
        [JsonProperty]
        public List<Choice> choices = new List<Choice>();
        [JsonProperty]
        public int duration = 15;
        [JsonProperty]
        public bool bits_voting_enabled = false;
        [JsonProperty]
        public int bits_per_vote = 0;
        [JsonProperty]
        public bool channel_points_voting_enabled = false;
        [JsonProperty]
        public int channel_points_per_vote = 0;

        public bool Validate(out string p_Message)
        {
            p_Message = "";
            if (string.IsNullOrEmpty(title) || title.Length > 60)
            {
                p_Message = "Title must be between 1 and 60 characters.";
                return false;
            }

            if (choices.Count < 2 || choices.Count > 5)
            {
                p_Message = "You need between 2 and 5 choices.";
                return false;
            }

            foreach (var l_Choice in choices)
            {
                if (string.IsNullOrEmpty(l_Choice.title) || l_Choice.title.Length > 60)
                {
                    p_Message = "Choice.Title must be between 1 and 60 characters.";
                    return false;
                }
            }

            if (duration < 15 || duration > 1800)
            {
                p_Message = "Duration must be between 15 and 1800.";
                return false;
            }

            if (bits_per_vote < 0 || bits_per_vote > 10000)
            {
                p_Message = "BitsPerVote must be between 0 and 10000.";
                return false;
            }

            if (channel_points_per_vote < 0 || channel_points_per_vote > 10000)
            {
                p_Message = "ChannelPointsPerVote must be between 0 and 1000000.";
                return false;
            }

            return true;
        }
    }

    [Serializable]
    public class Helix_Poll
    {
        public enum Status
        {
            ACTIVE,
            COMPLETED,
            TERMINATED,
            ARCHIVED,
            MODERATED,
            INVALID
        }

        [Serializable]
        public class Choice
        {
            [JsonProperty] public string id { get; protected set; } = "";
            [JsonProperty] public string title { get; protected set; } = "";
            [JsonProperty] public int votes { get; protected set; } = 0;
            [JsonProperty] public int channel_points_votes { get; protected set; } = 0;
            [JsonProperty] public int bits_votes { get; protected set; } = 0;
        }

        [JsonProperty] public string id { get; protected set; } = "";
        [JsonProperty] public string broadcaster_id { get; protected set; } = "";
        [JsonProperty] public string broadcaster_name { get; protected set; } = "";
        [JsonProperty] public string broadcaster_login { get; protected set; } = "";
        [JsonProperty] public string title { get; protected set; } = "";
        [JsonProperty] public List<Choice> choices { get; protected set; } = new List<Choice>();
        [JsonProperty] public bool bits_voting_enabled { get; protected set; } = false;
        [JsonProperty] public int bits_per_vote { get; protected set; } = 0;
        [JsonProperty] public bool channel_points_voting_enabled { get; protected set; } = false;
        [JsonProperty] public int channel_points_per_vote { get; protected set; } = 0;
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty] public Status status { get; protected set; } = Status.INVALID;
        [JsonProperty] public int duration { get; protected set; } = 15;
        [JsonProperty] public DateTime started_at { get; protected set; }
        [JsonProperty] public DateTime? ended_at { get; protected set; }
    }
}
