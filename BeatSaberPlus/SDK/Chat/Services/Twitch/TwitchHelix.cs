using BeatSaberPlus.SDK.Chat.Models.Twitch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BeatSaberPlus.SDK.Chat.Services.Twitch
{
    /// <summary>
    /// Global result enum
    /// </summary>
    public enum TwitchHelixResult
    {
        OK,
        InvalidRequest,
        AuthorizationFailed,
        NetworkError,
        TokenMissingPermission
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Twitch HelixAPI
    /// </summary>
    public class TwitchHelix
    {
        internal const int POLL_UPDATE_INTERVAL = 30 * 1000;
        internal const int ACTIVE_POLL_UPDATE_INTERVAL = 5 * 1000;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// API Client
        /// </summary>
        private Network.APIClient m_APIClient = new Network.APIClient("https://api.twitch.tv/helix/", TimeSpan.FromSeconds(10), true);
        /// <summary>
        /// Broadcaster ID
        /// </summary>
        private string m_BroadcasterID = "";
        /// <summary>
        /// API Token
        /// </summary>
        private string m_APIToken = "";
        /// <summary>
        /// API token scopes
        /// </summary>
        private List<string> m_APITokenScopes = new List<string>();
        /// <summary>
        /// Last polling date
        /// </summary>
        private DateTime m_LastPolling = DateTime.MinValue;
        /// <summary>
        /// Last poll
        /// </summary>
        private Helix_Poll m_LastPoll = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On token validate
        /// </summary>
        public event Action<bool, Helix_TokenValidate> OnTokenValidate;
        /// <summary>
        /// On active poll changed
        /// </summary>
        public event Action<Helix_Poll> OnActivePollChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On token changed
        /// </summary>
        /// <param name="p_Token">New token</param>
        internal void OnTokenChanged(string p_Token)
        {
            if (!m_APIClient.InternalClient.DefaultRequestHeaders.Contains("client-id"))
                m_APIClient.InternalClient.DefaultRequestHeaders.Add("client-id", TwitchService.TWITCH_CLIENT_ID);

            if (m_APIClient.InternalClient.DefaultRequestHeaders.Contains("Authorization"))
                m_APIClient.InternalClient.DefaultRequestHeaders.Remove("Authorization");

            m_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization",   "Bearer " + p_Token.Replace("oauth:", ""));

            m_APITokenScopes    = new List<string>();
            m_APIToken          = p_Token;

            ValidateToken();
        }
        /// <summary>
        /// On broadcaster changed
        /// </summary>
        /// <param name="p_BroadcasterID"></param>
        internal void OnBroadcasterIDChanged(string p_BroadcasterID)
        {
            m_BroadcasterID = p_BroadcasterID;

            /*
            CreatePoll(new Helix_CreatePoll()
            {
                title = "Hello from BS+",
                choices = new System.Collections.Generic.List<Helix_CreatePoll.Choice>()
                {
                    new Helix_CreatePoll.Choice()
                    {
                        title ="POG"
                    },

                    new Helix_CreatePoll.Choice()
                    {
                        title ="POG?"
                    }
                },
                duration = 60
            }, (x, y) => { EndPoll(y, Helix_Poll.Status.TERMINATED, (xx, yy) => { }); });

            */
           // GetActivePoll((x, y) => { if (y != null) EndPoll(y, Helix_Poll.Status.TERMINATED, (xx, yy) => { }); });

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update loop
        /// </summary>
        internal void Update()
        {
            if (string.IsNullOrEmpty(m_BroadcasterID))
                return;

            int l_Interval = POLL_UPDATE_INTERVAL;
            if (m_LastPoll != null && m_LastPoll.status == Helix_Poll.Status.ACTIVE)
                l_Interval = ACTIVE_POLL_UPDATE_INTERVAL;

            if ((DateTime.Now - m_LastPolling).TotalMilliseconds > l_Interval)
            {
                GetActivePoll((p_Status, p_Result) =>
                {
                    if (p_Status == TwitchHelixResult.OK
                        && (m_LastPoll == null
                            || m_LastPoll.id != p_Result.id
                            || m_LastPoll.status != p_Result.status
                            || m_LastPoll.choices.Sum(x => x.votes) != p_Result.choices.Sum(x => x.votes)
                            )
                        )
                    {
                        m_LastPoll = p_Result;
                        OnActivePollChanged?.Invoke(p_Result);
                    }
                    else
                        m_LastPoll = null;
                });

                m_LastPolling = DateTime.Now;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validate API Token
        /// </summary>
        private void ValidateToken()
        {
            var l_APIClient = new Network.APIClient("", TimeSpan.FromSeconds(10), false);
            l_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization", "OAuth " + m_APIToken.Replace("oauth:", ""));

            l_APIClient.GetAsync("https://id.twitch.tv/oauth2/validate", CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Receiving:");
                    Logger.Instance.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Failed with message:");
                    Logger.Instance.Error(p_Result.Result.BodyString);
                }

                if (p_Result.Result != null && p_Result.Result.IsSuccessStatusCode)
                {
                    if (GetObjectFromJsonString<Helix_TokenValidate>(p_Result.Result.BodyString, out var l_Validate))
                    {
                        m_APITokenScopes = new List<string>(l_Validate.scopes);
                        OnTokenValidate?.Invoke(true, l_Validate);
                    }
                    else
                        Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Failed to parse reply");
                }
                else
                    OnTokenValidate?.Invoke(false, null);
            });
        }
        /// <summary>
        /// Has API Token scope
        /// </summary>
        /// <param name="p_Scope">Scope to check</param>
        /// <returns></returns>
        public bool HasTokenPermission(string p_Scope)
        {
            if (m_APITokenScopes == null)
                return false;

            return m_APITokenScopes.Contains(p_Scope, StringComparer.InvariantCultureIgnoreCase);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a poll
        /// </summary>
        /// <param name="p_Poll">Poll to create</param>
        /// <param name="p_Callback">Result callback</param>
        public void CreatePoll(Helix_CreatePoll p_Poll, Action<TwitchHelixResult, Helix_Poll> p_Callback)
        {
            if (!HasTokenPermission("channel:manage:polls"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            if (!p_Poll.Validate(out var l_Message))
            {
                Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Error validating data:");
                Logger.Instance.Error(l_Message);
                return;
            }

            p_Poll.broadcaster_id = m_BroadcasterID;
            var l_ContentStr = new StringContent(JsonConvert.SerializeObject(p_Poll), Encoding.UTF8, "application/json");

#if DEBUG
            Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Sending:");
            Logger.Instance.Debug(JsonConvert.SerializeObject(p_Poll, Formatting.Indented));
#endif

            m_APIClient.PostAsync("polls", l_ContentStr, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Receiving:");
                    Logger.Instance.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Failed with message:");
                    Logger.Instance.Error(p_Result.Result.BodyString);
                }

                if (p_Result.Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.Result.BodyString);
                    Helix_Poll l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                    {
                        if (GetObjectFromJsonString<Helix_Poll>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult) && l_HelixResult.status == Helix_Poll.Status.ACTIVE)
                            m_LastPoll = l_HelixResult;
                    }

                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            });
        }
        /// <summary>
        /// Get active poll
        /// </summary>
        /// <param name="p_Callback">Result callback</param>
        public void GetActivePoll(Action<TwitchHelixResult, Helix_Poll> p_Callback)
        {
            if (!HasTokenPermission("channel:manage:polls"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            m_APIClient.GetAsync("polls?broadcaster_id=" + m_BroadcasterID, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.GetActivePoll] Receiving:");
                    Logger.Instance.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.GetActivePoll] Failed with message:");
                    Logger.Instance.Error(p_Result.Result.BodyString);
                }

                if (p_Result.Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.Result.BodyString);
                    Helix_Poll l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Poll>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            });
        }
        /// <summary>
        /// End a poll
        /// </summary>
        /// <param name="p_Poll">Poll to end</param>
        /// <param name="p_EndStatus">Ending status</param>
        /// <param name="p_Callback">End callback</param>
        public void EndPoll(Helix_Poll p_Poll, Helix_Poll.Status p_EndStatus, Action<TwitchHelixResult, Helix_Poll> p_Callback)
        {
            if (!HasTokenPermission("channel:manage:polls"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            var l_Content = new JObject()
            {
                ["broadcaster_id"]  = m_BroadcasterID,
                ["id"]              = p_Poll.id,
                ["status"]          = (p_EndStatus == Helix_Poll.Status.ARCHIVED ? p_EndStatus : Helix_Poll.Status.TERMINATED).ToString()
            };

            var l_ContentStr = new StringContent(JsonConvert.SerializeObject(l_Content), Encoding.UTF8, "application/json");

#if DEBUG
            Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Sending:");
            Logger.Instance.Debug(JsonConvert.SerializeObject(l_Content, Formatting.Indented));
#endif

            m_APIClient.PatchAsync("polls", l_ContentStr, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    Logger.Instance.Debug("[SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Receiving:");
                    Logger.Instance.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    Logger.Instance.Error("[SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Failed with message:");
                    Logger.Instance.Error(p_Result.Result.BodyString);
                }

                if (p_Result.Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.Result.BodyString);
                    Helix_Poll l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Poll>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get JObject from serialized JSON
        /// </summary>
        /// <param name="p_Serialized">Input</param>
        /// <param name="p_JObject">Result object</param>
        /// <returns></returns>
        private static bool GetObjectFromJsonString<T>(string p_Serialized, out T p_JObject)
            where T : class, new()
        {
            p_JObject = null;
            try
            {
                p_JObject = JsonConvert.DeserializeObject<T>(p_Serialized);
            }
            catch (Exception)
            {
                return false;
            }

            return p_JObject != null;
        }
    }
}
