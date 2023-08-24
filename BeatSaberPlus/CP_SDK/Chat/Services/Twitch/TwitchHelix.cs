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
        /// Web Client
        /// </summary>
        private Network.WebClientUnity   m_WebClient     = new Network.WebClientUnity  ("https://api.twitch.tv/helix/", TimeSpan.FromSeconds(10), true);
        private Network.WebClientCore m_WebClientEx   = new Network.WebClientCore("https://api.twitch.tv/helix/", TimeSpan.FromSeconds(10), true);
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
        public Network.WebClientUnity WebClient      => m_WebClient;
        public Network.WebClientCore WebClientEx  => m_WebClientEx;
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
                m_WebClient.SetHeader("Client-Id",       TwitchService.TWITCH_CLIENT_ID);
                m_WebClient.SetHeader("Authorization",   "Bearer " + p_Token.Replace("oauth:", ""));
            }
            catch (System.Exception) { }

            try
            {
                m_WebClientEx.SetHeader("Client-Id",     TwitchService.TWITCH_CLIENT_ID);
                m_WebClientEx.SetHeader("Authorization", "Bearer " + p_Token.Replace("oauth:", ""));
            }
            catch (System.Exception) { }

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
            var l_WebClient = new Network.WebClientUnity("", TimeSpan.FromSeconds(10), true);
            try
            {
                l_WebClient.SetHeader("Authorization", "OAuth " + m_APIToken.Replace("oauth:", ""));
            }
            catch { }

            l_WebClient.GetAsync("https://id.twitch.tv/oauth2/validate", CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.ValidateToken] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result != null && p_Result.IsSuccessStatusCode)
                {
                    if (GetObjectFromJsonString<Helix_TokenValidate>(p_Result.BodyString, out var l_Validate))
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
            }, true);
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

            m_WebClient.PostAsync("polls", Network.WebContent.FromJson(p_Poll), CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreatePoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
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
            }, true);
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

            m_WebClient.GetAsync("polls?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
                    Helix_Poll l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Poll>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
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

            m_WebClient.PatchAsync("polls", Network.WebContent.FromJson(l_Content), CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPoll] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
                    Helix_Poll l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Poll>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
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

            m_WebClient.GetAsync("hypetrain/events?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastHypeTrain] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastHypeTrain] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
                    Helix_HypeTrain l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_HypeTrain>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
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

            m_WebClient.GetAsync("predictions?broadcaster_id=" + m_BroadcasterID + "&first=1", CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPrediction] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.GetLastPrediction] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
                    Helix_Prediction l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Prediction>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
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

            m_WebClient.PatchAsync("predictions", Network.WebContent.FromJson(l_Body), CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPrediction] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.EndPrediction] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject l_Reply = JObject.Parse(p_Result.BodyString);
                    Helix_Prediction l_HelixResult = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_Prediction>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create clip
        /// </summary>
        /// <param name="p_Callback">End callback</param>
        public void CreateClip(Action<TwitchHelixResult, Helix_CreateClip> p_Callback)
        {
            if (!HasTokenPermission("clips:edit"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission, null);
                return;
            }

            m_WebClient.PostAsync("clips?broadcaster_id="+ m_BroadcasterID, Network.WebContent.FromJson(new JObject()), CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreateClip] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreateClip] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest, null);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed, null);
                else if (p_Result.IsSuccessStatusCode)
                {
                    JObject             l_Reply         = JObject.Parse(p_Result.BodyString);
                    Helix_CreateClip    l_HelixResult   = null;

                    if (l_Reply.ContainsKey("data") && l_Reply["data"].Type == JTokenType.Array && (l_Reply["data"] as JArray).Count > 0)
                        GetObjectFromJsonString<Helix_CreateClip>((l_Reply["data"] as JArray)[0].ToString(), out l_HelixResult);

                    p_Callback?.Invoke(TwitchHelixResult.OK, l_HelixResult);
                }
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError, null);
            }, true);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create marker
        /// </summary>
        /// <param name="p_MarkerName">Marker description</param>
        /// <param name="p_Callback">End callback</param>
        public void CreateMarker(string p_MarkerName, Action<TwitchHelixResult> p_Callback)
        {
            if (!HasTokenPermission("channel:manage:broadcast"))
            {
                p_Callback?.Invoke(TwitchHelixResult.TokenMissingPermission);
                return;
            }

            var l_Content = new JObject()
            {
                ["user_id"]     = m_BroadcasterID,
                ["description"] = p_MarkerName.Length > 140 ? p_MarkerName.Substring(0, 140) : p_MarkerName
            };

            m_WebClient.PostAsync("streams/markers", Network.WebContent.FromJson(l_Content), CancellationToken.None, (p_Result) =>
            {
#if DEBUG
                if (p_Result != null)
                {
                    ChatPlexSDK.Logger.Debug("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreateMarker] Receiving:");
                    ChatPlexSDK.Logger.Debug(p_Result.BodyString);
                }
#endif

                if (p_Result != null && !p_Result.IsSuccessStatusCode)
                {
                    ChatPlexSDK.Logger.Error("[CP_SDK.Chat.Service.Twitch][TwitchHelix.CreateMarker] Failed with message:");
                    ChatPlexSDK.Logger.Error(p_Result.BodyString);
                }

                if (p_Result == null)
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    p_Callback?.Invoke(TwitchHelixResult.InvalidRequest);
                else if (p_Result.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    p_Callback?.Invoke(TwitchHelixResult.AuthorizationFailed);
                else if (p_Result.IsSuccessStatusCode)
                    p_Callback?.Invoke(TwitchHelixResult.OK);
                else
                    p_Callback?.Invoke(TwitchHelixResult.NetworkError);
            }, true);
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
