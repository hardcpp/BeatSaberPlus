using Newtonsoft.Json;
using System;

namespace BeatSaberPlus_SongOverlay.Models
{
    [Serializable]
    public class Event
    {
        [JsonProperty] internal string _type = "event";
        [JsonProperty] internal string _event = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal MapInfo mapInfoChanged = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal string gameStateChanged = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal Score scoreEvent = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal float? pauseTime = null;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        internal float? resumeTime = null;

        internal void FeedEvent()
        {
            if (mapInfoChanged != null)     _event = "mapInfo";
            if (gameStateChanged != null)   _event = "gameState";
            if (scoreEvent != null)         _event = "score";
            if (pauseTime.HasValue)         _event = "pause";
            if (resumeTime.HasValue)        _event = "resume";
        }
    }
}
