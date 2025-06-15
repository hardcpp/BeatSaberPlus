using Newtonsoft.Json.Linq;
using System;

namespace BeatSaberPlus_ChatRequest.Models
{
    /// <summary>
    /// Song entry
    /// </summary>
    public class SongEntry : CP_SDK_BS.UI.Data.SongListItem
    {
        internal DateTime?      RequestTime     = null;
        internal string         RequesterName   = "";
        internal string         Message         = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serialize into a JObject
        /// </summary>
        /// <param name="p_SongEntry">Source</param>
        /// <returns></returns>
        internal static JObject Serialize(SongEntry p_SongEntry)
        {
            return new JObject
            {
                ["key"] = p_SongEntry.BeatSaver_Map?.id ?? p_SongEntry.GetLevelHash(),
                ["rqt"] = p_SongEntry.RequestTime.HasValue ? CP_SDK.Misc.Time.ToUnixTime(p_SongEntry.RequestTime.Value) : CP_SDK.Misc.Time.UnixTimeNow(),
                ["rqn"] = p_SongEntry.RequesterName,
                ["npr"] = p_SongEntry.TitlePrefix,
                ["msg"] = p_SongEntry.Message
            };
        }
        /// <summary>
        /// Deserialize from JObject
        /// </summary>
        /// <param name="p_JObject">Source</param>
        /// <returns></returns>
        internal static SongEntry Deserialize(JObject p_JObject)
        {
            var l_Key       = p_JObject["key"]?.Value<string>()     ?? "";
            var l_Time      = p_JObject["rqt"]?.Value<long>()       ?? CP_SDK.Misc.Time.UnixTimeNow();
            var l_Requester = p_JObject["rqn"]?.Value<string>()     ?? "";
            var l_Prefix    = p_JObject["npr"]?.Value<string>()     ?? "";
            var l_Message   = p_JObject["msg"]?.Value<string>()     ?? "";

            if (l_Key == "" && p_JObject.ContainsKey("id"))
                l_Key = p_JObject["id"].Value<int>().ToString("x");

            if (l_Key == "")
                return null;

            return new SongEntry()
            {
                BeatSaver_Map   = CP_SDK_BS.Game.BeatMapsClient.GetFromCacheByKey(l_Key) ?? CP_SDK_BS.Game.BeatMaps.MapDetail.PartialFromKey(l_Key),
                RequestTime     = CP_SDK.Misc.Time.FromUnixTime(l_Time),
                RequesterName   = l_Requester,
                TitlePrefix     = l_Prefix,
                Message         = l_Message
            };
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On show
        /// </summary>
        public override void OnShow()
        {
            Tooltip = "<b>Requested by</b> " + TitlePrefix + (TitlePrefix.Length != 0 ? " " : "") + RequesterName;

            if (RequestTime.HasValue)
            {
                var l_Elapsed = CP_SDK.Misc.Time.UnixTimeNow() - CP_SDK.Misc.Time.ToUnixTime(RequestTime.Value);
                if (l_Elapsed < (60 * 60))
                    Tooltip += "\n<align=\"center\">" + Math.Max(1, l_Elapsed / 60).ToString() + " minute(s) ago</align>";
                else if (l_Elapsed < (60 * 60 * 24))
                    Tooltip += "\n<align=\"center\">" + Math.Max(1, l_Elapsed / (60 * 60)).ToString() + " hour(s) ago</align>";
                else
                    Tooltip += "\n<align=\"center\">" + Math.Max(1, l_Elapsed / (60 * 60 * 24)).ToString() + " day(s) ago</align>";
            }

            if (!string.IsNullOrEmpty(Message))
                Tooltip += "\n" + Message;

            base.OnShow();
        }
    }
}
