using CP_SDK.Chat.Models.Twitch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CP_SDK.Chat.Services.Twitch
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
        internal const int POLL_UPDATE_INTERVAL = 10000;
        internal const int ACTIVE_POLL_UPDATE_INTERVAL = 2000;

        internal const int HYPETRAIN_UPDATE_INTERVAL = 10000;
        internal const int ACTIVE_HYPETRAIN_UPDATE_INTERVAL = 2000;

        internal const int PREDICTION_UPDATE_INTERVAL = 10000;
        internal const int ACTIVE_PREDICTION_UPDATE_INTERVAL = 2000;

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
        /// Broadcast login
        /// </summary>
        private string m_BroadcasterLogin = "";
        /// <summary>
        /// API Token
        /// </summary>
        private string m_APIToken = "";
        /// <summary>
        /// API token scopes
        /// </summary>
        private List<string> m_APITokenScopes = new List<string>();
        /// <summary>
        /// Last poll check date
        /// </summary>
        private DateTime m_LastPollCheckTime = DateTime.MinValue;
        /// <summary>
        /// Last poll
        /// </summary>
        private Helix_Poll m_LastPoll = null;
        /// <summary>
        /// Last hype train check date
        /// </summary>
        private DateTime m_LastHypeTrainCheckTime = DateTime.MinValue;
        /// <summary>
        /// Last hype train
        /// </summary>
        private Helix_HypeTrain m_LastHypeTrain = null;
        /// <summary>
        /// Last prediction check date
        /// </summary>
        private DateTime m_LastPredictionCheckTime = DateTime.MinValue;
        /// <summary>
        /// Last prediction
        /// </summary>
        private Helix_Prediction m_LastPrediction = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Client
        /// </summary>
        public Network.APIClient APIClient => m_APIClient;
        /// <summary>
        /// Broadcaster ID
        /// </summary>
        public string BroadcasterID => m_BroadcasterID;
        /// <summary>
        /// Broadcaster login
        /// </summary>
        public string BroadcasterLogin => m_BroadcasterLogin;

        /// <summary>
        /// On token validate
        /// </summary>
        public event Action<bool, Helix_TokenValidate, string> OnTokenValidate;
        /// <summary>
        /// On active poll changed
        /// </summary>
        public event Action<Helix_Poll> OnActivePollChanged;
        /// <summary>
        /// On active hype train changed
        /// </summary>
        public event Action<Helix_HypeTrain> OnActiveHypeTrainChanged;
        /// <summary>
        /// On active prediction changed
        /// </summary>
        public event Action<Helix_Prediction> OnActivePredictionChanged;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On token changed
        /// </summary>
        /// <param name="p_Token">New token</param>
        internal void OnTokenChanged(string p_Token)
        {
            try
            {
                if (!m_APIClient.InternalClient.DefaultRequestHeaders.Contains("client-id"))
                    m_APIClient.InternalClient.DefaultRequestHeaders.Add("client-id", TwitchService.TWITCH_CLIENT_ID);

                if (m_APIClient.InternalClient.DefaultRequestHeaders.Contains("Authorization"))
                    m_APIClient.InternalClient.DefaultRequestHeaders.Remove("Authorization");

                m_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization",   "Bearer " + p_Token.Replace("oauth:", ""));
            }
            catch
            {

            }

            m_APITokenScopes    = new List<string>();
            m_APIToken          = p_Token;

            ValidateToken();
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

            #region Poll
            int l_Interval = POLL_UPDATE_INTERVAL;
            if (m_LastPoll != null && (m_LastPoll.status == Helix_Poll.Status.ACTIVE || m_LastPoll.status == Helix_Poll.Status.COMPLETED || m_LastPoll.status == Helix_Poll.Status.TERMINATED))
                l_Interval = ACTIVE_POLL_UPDATE_INTERVAL;

            if ((DateTime.Now - m_LastPollCheckTime).TotalMilliseconds > l_Interval)
            {
                GetLastPoll((p_Status, p_Result) =>
                {
                    m_LastPollCheckTime = DateTime.Now;

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
                    else if (p_Status != TwitchHelixResult.OK)
                        m_LastPoll = null;
                });
            }
            #endregion

            #region Hype train
            l_Interval = HYPETRAIN_UPDATE_INTERVAL;
            if (m_LastHypeTrain != null && m_LastHypeTrain.event_data.expires_at > DateTime.UtcNow)
                l_Interval = ACTIVE_HYPETRAIN_UPDATE_INTERVAL;

            if ((DateTime.Now - m_LastHypeTrainCheckTime).TotalMilliseconds > l_Interval)
            {
                GetLastHypeTrain((p_Status, p_Result) =>
                {
                    m_LastHypeTrainCheckTime = DateTime.Now;

                    if (p_Status == TwitchHelixResult.OK
                        && (m_LastHypeTrain == null
                            || m_LastHypeTrain.id != p_Result.id
                            || m_LastHypeTrain.event_data.started_at != p_Result.event_data.started_at
                            || m_LastHypeTrain.event_data.level != p_Result.event_data.level
                            || m_LastHypeTrain.event_data.total != p_Result.event_data.total
                            )
                        )
                    {
                        m_LastHypeTrain = p_Result;
                        OnActiveHypeTrainChanged?.Invoke(p_Result);
                    }
                    else if (p_Status != TwitchHelixResult.OK)
                        m_LastHypeTrain = null;
                });
            }
            #endregion

            #region Prediction
            l_Interval = PREDICTION_UPDATE_INTERVAL;
            if (m_LastPrediction != null && m_LastPrediction.status == Helix_Prediction.Status.ACTIVE)
                l_Interval = ACTIVE_PREDICTION_UPDATE_INTERVAL;

            if ((DateTime.Now - m_LastPredictionCheckTime).TotalMilliseconds > l_Interval)
            {
                GetLastPrediction((p_Status, p_Result) =>
                {
                    m_LastPredictionCheckTime = DateTime.Now;

                    if (p_Status == TwitchHelixResult.OK
                        && (m_LastPrediction == null
                            || m_LastPrediction.id != p_Result.id
                            || m_LastPrediction.status != p_Result.status
                            || m_LastPrediction.outcomes.Sum(x => x.channel_points) != p_Result.outcomes.Sum(x => x.channel_points)
                            )
                        )
                    {
                        m_LastPrediction = p_Result;
                        OnActivePredictionChanged?.Invoke(p_Result);
                    }
                    else if (p_Status != TwitchHelixResult.OK)
                        m_LastPrediction = null;
                });
            }
            #endregion
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validate API Token
        /// </summary>
        private void ValidateToken()
        {
            var l_APIClient = new Network.APIClient("", TimeSpan.FromSeconds(10), false);
            try
            {
                l_APIClient.InternalClient.DefaultRequestHeaders.Add("Authorization", "OAuth " + m_APIToken.Replace("oauth:", ""));
            }
            catch
            {

            }

            l_APIClient.GetAsync("https://id.twitch.tv/oauth2/validate", CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
                }

                if (p_Result.Result != null && p_Result.Result.IsSuccessStatusCode)
                {
                    if (GetObjectFromJsonString<Helix_TokenValidate>(p_Result.Result.BodyString, out var l_Validate))
                    {
                        m_APITokenScopes    = new List<string>(l_Validate.scopes);
                        m_BroadcasterID     = l_Validate.user_id;
                        m_BroadcasterLogin  = l_Validate.login;


                        OnTokenValidate?.Invoke(true, l_Validate, l_Validate.user_id);
                    }
                    else
                        ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Failed to parse reply");
                }
                else
                {
                    m_APITokenScopes.Clear();
                    m_BroadcasterID     = string.Empty;
                    m_BroadcasterLogin  = string.Empty;

                    OnTokenValidate?.Invoke(false, null, string.Empty);
                }
            }).ConfigureAwait(false);
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
                ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Error validating data:");
                ChatPlexSDK.Logger.Error(l_Message);
                return;
            }

            p_Poll.broadcaster_id = m_BroadcasterID;
            var l_ContentStr = new StringContent(JsonConvert.SerializeObject(p_Poll), Encoding.UTF8, "application/json");

#if DEBUG
            ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Sending:");
            ChatPlexSDK.Logger.Debug(JsonConvert.SerializeObject(p_Poll, Formatting.Indented));
#endif

            m_APIClient.PostAsync("polls", l_ContentStr, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// Get active poll
        /// </summary>
        /// <param name="p_Callback">Result callback</param>
        public void GetLastPoll(Action<TwitchHelixResult, Helix_Poll> p_Callback)
        {
            if (!HasTokenPermission("channel:manage:polls"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            m_APIClient.GetAsync("polls?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
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
            ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Sending:");
            ChatPlexSDK.Logger.Debug(JsonConvert.SerializeObject(l_Content, Formatting.Indented));
#endif

            m_APIClient.PatchAsync("polls", l_ContentStr, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get last hype train
        /// </summary>
        /// <param name="p_Callback">Result callback</param>
        public void GetLastHypeTrain(Action<TwitchHelixResult, Helix_HypeTrain> p_Callback)
        {
            if (!HasTokenPermission("channel:read:hype_train"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            m_APIClient.GetAsync("hypetrain/events?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastHypeTrain] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastHypeTrain] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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
                    Helix_HypeTrain l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_HypeTrain>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get last prediction
        /// </summary>
        /// <param name="p_Callback">Result callback</param>
        public void GetLastPrediction(Action<TwitchHelixResult, Helix_Prediction> p_Callback)
        {
            if (!HasTokenPermission("channel:read:predictions"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            m_APIClient.GetAsync("predictions?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPrediction] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPrediction] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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
                    Helix_Prediction l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Prediction>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
        }
        /// <summary>
        /// End a prediction
        /// </summary>
        /// <param name="p_ID">Prediction ID</param>
        /// <param name="p_Status">New status</param>
        /// <param name="p_WinningOutcomeID">Winning outcome id if status is RESOLVED</param>
        /// <param name="p_Callback">Result callback</param>
        public void EndPrediction(string p_ID, Helix_Prediction.Status p_Status, string p_WinningOutcomeID, Action<TwitchHelixResult, Helix_Prediction> p_Callback)
        {
            if (!HasTokenPermission("channel:read:predictions"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            var l_Body = new JObject();
            l_Body["broadcaster_id"]    = m_BroadcasterID;
            l_Body["id"]                = p_ID;
            l_Body["status"]            = p_Status.ToString();

            if (p_Status == Helix_Prediction.Status.RESOLVED)
                l_Body["winning_outcome_id"] = p_WinningOutcomeID;

            var l_ContentStr = new StringContent(l_Body.ToString(Formatting.None), Encoding.UTF8, "application/json");

            m_APIClient.PatchAsync("predictions", l_ContentStr, CancellationToken.None, true).ContinueWith((p_Result) =>
            {
#if DEBUG
                if (p_Result.Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPrediction] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.Result.BodyString);
                }
#endif

                if (p_Result.Result != null && !p_Result.Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPrediction] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.Result.BodyString);
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
                    Helix_Prediction l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Prediction>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }).ConfigureAwait(false);
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
