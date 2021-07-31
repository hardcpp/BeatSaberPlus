﻿using Newtonsoft.Json;

namespace BeatSaberPlus.Modules.ChatIntegrations.Models.Events
{
    /// <summary>
    /// Chat points reward event model
    /// </summary>
    public class ChatPointsReward : Event
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string RewardID = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string Title = "";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal string Prompt = "Description";
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal int Cost = 50;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal bool RequireInput = false;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal int MaxPerStream = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal int MaxPerUserPerStream = 0;
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        internal int Cooldown = 0;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatPointsReward()
        {
            Name = "New chat points reward event";
            Title = "New reward " + SDK.Misc.Time.UnixTimeNow();
        }
    }
}