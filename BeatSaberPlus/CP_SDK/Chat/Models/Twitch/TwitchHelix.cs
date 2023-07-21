using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace CP_SDK.Chat.Models.Twitch
{
    [Serializable]
    public class Helix_TokenValidate
    {
        [JsonProperty] public string        client_id   = "";
        [JsonProperty] public string        login       = "";
        [JsonProperty] public List<string>  scopes      = new List<string>();
        [JsonProperty] public string        user_id     = "";
        [JsonProperty] public int           expires_in  = 0;
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
    public class Helix_CreateClip
    {
        [JsonProperty] public string edit_url { get; protected set; }
        [JsonProperty] public string id { get; protected set; }
    }

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
            [JsonProperty] public string id { get; protected set; }
            [JsonProperty] public string title { get; protected set; }
            [JsonProperty] public int votes { get; protected set; }
            [JsonProperty] public int channel_points_votes { get; protected set; }
            [JsonProperty] public int bits_votes { get; protected set; }
        }

        [JsonProperty] public string id { get; protected set; }
        [JsonProperty] public string broadcaster_id { get; protected set; }
        [JsonProperty] public string broadcaster_name { get; protected set; }
        [JsonProperty] public string broadcaster_login { get; protected set; }
        [JsonProperty] public string title { get; protected set; }
        [JsonProperty] public List<Choice> choices { get; protected set; } = new List<Choice>();
        [JsonProperty] public bool bits_voting_enabled { get; protected set; }
        [JsonProperty] public int bits_per_vote { get; protected set; }
        [JsonProperty] public bool channel_points_voting_enabled { get; protected set; }
        [JsonProperty] public int channel_points_per_vote { get; protected set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty] public Status status { get; protected set; }
        [JsonProperty] public int duration { get; protected set; }
        [JsonProperty] public DateTime started_at { get; protected set; }
        [JsonProperty] public DateTime? ended_at { get; protected set; }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    [Serializable]
    public class Helix_HypeTrain
    {
        [Serializable]
        public class Event_Data
        {
            [Serializable]
            public class Contribution
            {
                [JsonProperty] public int total { get; protected set; }
                [JsonProperty] public string type { get; protected set; }
                [JsonProperty] public string user { get; protected set; }
            }

            [JsonProperty] public string broadcaster_id { get; protected set; }
            [JsonProperty] public DateTime cooldown_end_time { get; protected set; }
            [JsonProperty] public DateTime expires_at { get; protected set; }
            [JsonProperty] public int goal { get; protected set; }
            [JsonProperty] public string id { get; protected set; }
            [JsonProperty] public Contribution last_contribution { get; protected set; }
            [JsonProperty] public int level { get; protected set; }
            [JsonProperty] public DateTime started_at { get; protected set; }
            [JsonProperty] public List<Contribution> top_contributions { get; protected set; } = new List<Contribution>();
            [JsonProperty] public int total { get; protected set; }
        }

        [JsonProperty] public string id { get; protected set; }
        [JsonProperty] public string event_type { get; protected set; }
        [JsonProperty] public DateTime event_timestamp { get; protected set; }
        [JsonProperty] public string version { get; protected set; }
        [JsonProperty] public Event_Data event_data { get; protected set; }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    [Serializable]
    public class Helix_Prediction
    {
        public enum Status
        {
            ACTIVE,
            LOCKED,
            RESOLVED,
            CANCELED
        }
        public enum Color
        {
            BLUE,
            PINK
        }

        [Serializable]
        public class Outcome
        {
            [JsonProperty] public string id { get; protected set; }
            [JsonProperty] public string title { get; protected set; }
            [JsonProperty] public int users { get; protected set; }
            [JsonProperty] public int channel_points { get; protected set; }
            //[JsonProperty] public object top_predictors { get; protected set; }
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty] public Color color { get; protected set; }
        }

        [JsonProperty] public string id { get; protected set; }
        [JsonProperty] public string broadcaster_id { get; protected set; }
        [JsonProperty] public string broadcaster_name { get; protected set; }
        [JsonProperty] public string broadcaster_login { get; protected set; }
        [JsonProperty] public string title { get; protected set; }
        [JsonProperty] public string winning_outcome_id { get; protected set; }
        [JsonProperty] public List<Outcome> outcomes { get; protected set; } = new List<Outcome>();
        [JsonProperty] public int prediction_window { get; protected set; }
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty] public Status status { get; protected set; }
        [JsonProperty] public DateTime created_at { get; protected set; }
        [JsonProperty] public DateTime? ended_at { get; protected set; }
        [JsonProperty] public DateTime? locked_at { get; protected set; }
    }
}
