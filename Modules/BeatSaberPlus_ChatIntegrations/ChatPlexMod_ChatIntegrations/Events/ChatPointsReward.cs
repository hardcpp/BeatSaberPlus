using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.XUI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using TMPro;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Events
{
    /// <summary>
    /// Chat command event
    /// </summary>
    public class ChatPointsReward : Interfaces.IEvent<ChatPointsReward, Models.Events.ChatPointsReward>
    {
        public override IReadOnlyList<(EValueType, string)> ProvidedValues      { get; protected set; }
        public override IReadOnlyList<string>               AvailableConditions { get; protected set; }
        public override IReadOnlyList<string>               AvailableActions    { get; protected set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public ChatPointsReward()
        {
            /// Build provided values list
            ProvidedValues = new List<(Interfaces.EValueType, string)>()
            {
                (Interfaces.EValueType.Integer, "MessageNumber"),
                (Interfaces.EValueType.String,  "MessageContent"),
                (Interfaces.EValueType.String,  "UserName")
            }.AsReadOnly();

            /// Build possible list
            AvailableConditions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalConditionsTypes)
                .Union(GetCustomConditionTypes())
                .Distinct().ToList().AsReadOnly();

            /// Build possible list
            AvailableActions = new List<string>()
                .Union(ChatIntegrations.RegisteredGlobalActionsTypes)
                .Union(GetCustomActionTypes())
                .Distinct().ToList().AsReadOnly();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITextInput m_TitleInput;
        private XUITextInput m_PromptInput;
        private XUITextInput m_CostInput;

        private XUIToggle m_RequireInput         = XUIToggle.Make();
        private XUISlider m_Cooldown             = XUISlider.Make().SetInteger(true).SetMinValue(0f).SetMaxValue(1200f).SetIncrements(1.0f);
        private XUIToggle m_AutoFullfillRefund   = XUIToggle.Make();
        private XUISlider m_MaxPerStream         = XUISlider.Make().SetInteger(true).SetMinValue(0f).SetMaxValue(100f).SetIncrements(1.0f);
        private XUISlider m_MaxPerUserPerStream  = XUISlider.Make().SetInteger(true).SetMinValue(0f).SetMaxValue(100f).SetIncrements(1.0f);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public override sealed void BuildUI(Transform p_Parent)
        {
            Action<CP_SDK.UI.Components.CText> l_ControlsTextStyle  = (x) => x.SetStyle(FontStyles.Bold).SetColor(Color.yellow);
            Action<CP_SDK.UI.Components.CText> l_ControlsTextStyleC = (x) => x.SetStyle(FontStyles.Bold).SetColor(Color.yellow).SetAlign(TextAlignmentOptions.Center);

            XUIElements = new IXUIElement[]
            {
                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Title:").OnReady(l_ControlsTextStyle),
                        XUIText.Make("Prompt:").OnReady(l_ControlsTextStyle),
                        XUIText.Make("Cost:").OnReady(l_ControlsTextStyle)
                    )
                    .SetMinWidth(20f).SetWidth(20f)
                    .OnReady(x => x.VLayoutGroup.childAlignment = TextAnchor.UpperLeft),

                    XUIVLayout.Make(
                        XUITextInput.Make("Title...").SetValue(Model.Title).Bind(ref m_TitleInput),
                        XUITextInput.Make("Prompt...").SetValue(Model.Prompt).Bind(ref m_PromptInput),
                        XUITextInput.Make("Cost...").SetValue(Model.Cost.ToString()).Bind(ref m_CostInput)
                    )
                    .SetMinWidth(110f).SetWidth(110f)
                )
                .SetMinWidth(130f).SetWidth(130f)
                .SetSpacing(0).SetPadding(0)
                .SetBackground(true)
                .ForEachDirect<XUIVLayout>(x => x.SetSpacing(0)),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Require input").OnReady(l_ControlsTextStyleC),
                        m_RequireInput
                    ),
                    XUIVLayout.Make(
                        XUIText.Make("Redeem Cooldown").OnReady(l_ControlsTextStyleC),
                        m_Cooldown
                    ),
                    XUIVLayout.Make(
                        XUIText.Make("Auto fullfill/refund").OnReady(l_ControlsTextStyleC),
                        m_AutoFullfillRefund
                    )
                )
                .SetMinWidth(130f).SetWidth(130f)
                .SetPadding(0)
                .ForEachDirect<XUIVLayout>((x) => { x.SetMinWidth(42f).SetWidth(42f).SetSpacing(0f); }),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Max per stream").OnReady(l_ControlsTextStyleC),
                        m_MaxPerStream
                    ),
                    XUIVLayout.Make(
                        XUIVSpacer.Make(4f),
                        XUIPrimaryButton.Make("Update reward", CreateOrUpdateReward)
                    ),
                    XUIVLayout.Make(
                        XUIText.Make("Max per user per stream").OnReady(l_ControlsTextStyleC),
                        m_MaxPerUserPerStream
                    )
                )
                .SetMinWidth(130f).SetWidth(130f)
                .SetPadding(0)
                .ForEachDirect<XUIVLayout>((x) => { x.SetMinWidth(42f).SetWidth(42f).SetSpacing(0f); }),

                XUIVLayout.Make(
                    XUIText.Make("The mod will automatically create and update the reward on your twitch account\n" +
                                "The reward get disabled when you quit the game, and re-enabled when you start it")
                        .SetAlign(TMPro.TextAlignmentOptions.Midline)
                ).SetBackground(true),
            };

            BuildUIAuto(p_Parent);

            m_MaxPerStream.SetFormatter((x) => ((int)x) == 0 ? "Unlimited" : ((int)x).ToString());
            m_MaxPerUserPerStream.SetFormatter((x) => ((int)x) == 0 ? "Unlimited" : ((int)x).ToString());
            m_Cooldown.SetFormatter((x) => {
                int l_IntValue = (int)x;
                if (l_IntValue == 0) return "Unlimited";

                int l_Minutes = l_IntValue / 60;
                int l_Seconds = l_IntValue - (l_Minutes * 60);

                string l_Result = (l_Minutes != 0 ? l_Minutes : l_Seconds).ToString();

                if (l_Minutes != 0) return l_Result + "m " + l_Seconds + "s";
                return l_Result + "s";
            });

            m_RequireInput.         SetValue(Model.RequireInput);
            m_Cooldown.             SetValue(Model.Cooldown);
            m_AutoFullfillRefund.   SetValue(Model.AutoFullfillRefund);
            m_MaxPerStream.         SetValue(Model.MaxPerStream);
            m_MaxPerUserPerStream.  SetValue(Model.MaxPerUserPerStream);

            m_TitleInput.           OnValueChanged((x) => Model.Title  = x.Length > 45 ? x.Substring(0, 45) : x);
            m_PromptInput.          OnValueChanged((x) => Model.Prompt = x.Length > 45 ? x.Substring(0, 45) : x);
            m_CostInput.            OnValueChanged((x) =>
            {
                int l_NewValue = 0;
                if (!int.TryParse(x, out l_NewValue))
                    l_NewValue = Model.Cost;
                else
                    l_NewValue = Mathf.Clamp(l_NewValue, 0, 10000000);

                Model.Cost = l_NewValue;
            });
            m_RequireInput.         OnValueChanged((x) => Model.RequireInput        = x);
            m_Cooldown.             OnValueChanged((x) => Model.Cooldown            = (int)x);
            m_AutoFullfillRefund.   OnValueChanged((x) => Model.AutoFullfillRefund  = x);
            m_MaxPerStream.         OnValueChanged((x) => Model.MaxPerStream        = (int)x);
            m_MaxPerUserPerStream.  OnValueChanged((x) => Model.MaxPerUserPerStream = (int)x);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On import or clone
        /// </summary>
        /// <param name="p_IsImport">Is an import</param>
        /// <param name="p_IsClone">Is a clone</param>
        public override void OnImportOrClone(bool p_IsImport, bool p_IsClone)
        {
            base.OnImportOrClone(p_IsImport, p_IsClone);

            if (p_IsImport) Model.Title += " (Import)";
            if (p_IsClone)  Model.Title += " (Clone)";

            Model.RewardID = "";
        }
        /// <summary>
        /// When the event is enabled
        /// </summary>
        public override sealed void OnEnable() => CreateOrUpdateReward();
        /// <summary>
        /// When the event is successful
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public override void OnSuccess(Models.EventContext p_Context)
        {
            if (!Model.AutoFullfillRefund)
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id={l_TwitchHelix.BroadcasterID}&reward_id={Model.RewardID}&id={p_Context.PointsEvent.TransactionID}";
                var l_Content       = new JObject()
                {
                    ["status"] = "FULFILLED",
                };
                var l_ContentStr    = new StringContent(l_Content.ToString(), Encoding.UTF8);

                l_TwitchHelix.WebClient.PatchAsync(l_URL, l_ContentStr, "application/json", CancellationToken.None, null, true);
            }
        }
        /// <summary>
        /// When the event failed
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public override sealed void OnEventFailed(Models.EventContext p_Context)
        {
            if (!Model.AutoFullfillRefund)
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards/redemptions?broadcaster_id={l_TwitchHelix.BroadcasterID}&reward_id={Model.RewardID}&id={p_Context.PointsEvent.TransactionID}";
                var l_Content       = new JObject()
                {
                    ["status"] = "CANCELED",
                };
                var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8);

                l_TwitchHelix.WebClient.PatchAsync(l_URL, l_ContentStr, "application/json", CancellationToken.None, (x) =>
                {
                    if (p_Context.ChatService != null && p_Context.Channel != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.UserName} Event failed, your points were refunded!");
                }, true);
            }
        }
        /// <summary>
        /// When the event is disabled
        /// </summary>
        public override sealed void OnDisable()
        {
            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count == 0)
                return;

            if (!string.IsNullOrEmpty(Model.RewardID))
            {
                var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
                var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

                if (l_TwitchHelix != null)
                {
                    var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";
                    var l_Content       = new JObject()
                    {
                        ["is_enabled"] = false
                    };
                    var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8);

                    l_TwitchHelix.WebClientEx.PatchAsync(l_URL, l_ContentStr, "application/json", CancellationToken.None, null, true);
                }
            }
        }
        /// <summary>
        /// When the event is deleted
        /// </summary>
        public override sealed void OnDelete() => DeleteReward();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed bool CanBeExecuted(Models.EventContext p_Context)
        {
            /// Ensure that we have all data
            if (p_Context.Type != Interfaces.ETriggerType.ChatPointsReward || p_Context.ChatService == null || p_Context.Channel == null || p_Context.User == null || p_Context.PointsEvent == null)
                return false;

            return p_Context.PointsEvent.RewardID == Model.RewardID;
        }
        /// <summary>
        /// Build provided value dictionary
        /// </summary>
        /// <param name="p_Context">Event context</param>
        protected override sealed void BuildProvidedValues(Models.EventContext p_Context)
        {
            p_Context.AddValue(Interfaces.EValueType.String, "UserName",       (string)p_Context.User.DisplayName);
            p_Context.AddValue(Interfaces.EValueType.String, "MessageContent", (string)p_Context.PointsEvent.UserInput);

            if (!string.IsNullOrEmpty(p_Context.PointsEvent.UserInput) && Int64.TryParse(Regex.Match(p_Context.PointsEvent.UserInput, @"-?\d+").Value, out var l_Number))
                p_Context.AddValue(Interfaces.EValueType.Integer, "MessageNumber",  (Int64?)l_Number);
            else
                p_Context.AddValue(Interfaces.EValueType.Integer, "MessageNumber",  (Int64?)null);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create or update the reward on twitch
        /// </summary>
        private void CreateOrUpdateReward()
        {
            if (UI.SettingsMainView.Instance != null)
                UI.SettingsMainView.Instance.ShowLoadingModal();

            if (string.IsNullOrEmpty(Model.RewardID))
                CreateOrUpdateReward_Callback(null);
            else
            {
                var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
                var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;
                var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";

                if (l_TwitchHelix != null)
                    l_TwitchHelix.WebClient.GetAsync(l_URL, CancellationToken.None, CreateOrUpdateReward_Callback, true);
            }
        }
        /// <summary>
        /// Create or update the reward on twitch
        /// </summary>
        private void CreateOrUpdateReward_Callback(CP_SDK.Network.WebResponse p_GetReply)
        {
            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;
            var l_ShouldCreate  = p_GetReply == null || !p_GetReply.IsSuccessStatusCode;
            var l_URL           = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}";

            if (l_ShouldCreate)
            {
                Model.RewardID = string.Empty;
                ChatIntegrations.Instance?.SaveDatabase();
            }

            if (!l_ShouldCreate)
                l_URL += $"&id={Model.RewardID}";

            var l_Content = new JObject()
            {
                ["title"]                               = Model.Title,
                ["is_user_input_required"]              = Model.RequireInput,
                ["prompt"]                              = Model.Prompt,
                ["cost"]                                = Mathf.Max(1, Model.Cost),
                ["is_enabled"]                          = Model.Enabled,
                ["is_max_per_stream_enabled"]           = Model.MaxPerStream > 0,
                ["max_per_stream"]                      = Mathf.Max(1, Model.MaxPerStream),
                ["is_max_per_user_per_stream_enabled"]  = Model.MaxPerUserPerStream > 0,
                ["max_per_user_per_stream"]             = Mathf.Max(1, Model.MaxPerUserPerStream),
                ["is_global_cooldown_enabled"]          = Model.Cooldown > 0,
                ["global_cooldown_seconds"]             = Mathf.Max(1, Model.Cooldown)
            };

            var l_ContentStr = new StringContent(l_Content.ToString(), Encoding.UTF8);
            Action<CP_SDK.Network.WebResponse> l_Callback = (CP_SDK.Network.WebResponse p_SecondReply) =>
            {
                if (p_SecondReply != null)
                {
                    if (p_SecondReply.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (UI.SettingsMainView.Instance != null)
                        {
                            CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                            {
                                if (p_SecondReply.BodyString.Contains("CREATE_CUSTOM_REWARD_DUPLICATE_REWARD"))
                                    UI.SettingsMainView.Instance.ShowMessageModal("<color=red>Twitch error,\nA reward with the same name already exist on your channel\nPlease delete on twitch.tv any conflicting reward");
                                else
                                    UI.SettingsMainView.Instance.ShowMessageModal("<color=red>Twitch error,\n" + p_SecondReply.BodyString);

                                UI.SettingsMainView.Instance.CloseLoadingModal();
                            });
                        }
                        return;
                    }

                    if (l_ShouldCreate)
                    {
                        var l_Response = JObject.Parse(p_SecondReply.BodyString);
                        if (l_Response == null
                            ||   !l_Response.ContainsKey("data")
                            ||    l_Response["data"].Type               != JTokenType.Array
                            ||   (l_Response["data"] as JArray).Count   != 1
                            || !((l_Response["data"] as JArray)[0] as JObject).ContainsKey("id")
                            )
                        {
                            Logger.Instance.Error("[ChatIntegration][ChatPointReward.CreateOrUpdateReward] Error:");
                            Logger.Instance.Error(p_SecondReply.BodyString != null ? p_SecondReply.BodyString : "empty response");

                            if (UI.SettingsMainView.Instance != null)
                            {
                                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                                {
                                    UI.SettingsMainView.Instance.ShowMessageModal("<color=red>Internal error,\nplease contact BS+ support!");
                                    UI.SettingsMainView.Instance.CloseLoadingModal();
                                });
                            }
                            return;
                        }

                        Model.RewardID = l_Response["data"][0]["id"].Value<string>();
                        ChatIntegrations.Instance?.SaveDatabase();
                    }

                    if (UI.SettingsMainView.Instance != null)
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => UI.SettingsMainView.Instance.CloseLoadingModal());
                }
                else
                {
                    Logger.Instance.Error("[ChatIntegration][ChatPointReward.CreateOrUpdateReward] Error 2:");
                    Logger.Instance.Error("empty response");

                    if (UI.SettingsMainView.Instance != null)
                    {
                        CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() =>
                        {
                            UI.SettingsMainView.Instance.ShowMessageModal("<color=red>Internal error,\nplease contact BS+ support!");
                            UI.SettingsMainView.Instance.CloseLoadingModal();
                        });
                    }
                }
            };

            if (l_ShouldCreate)
                l_TwitchHelix.WebClient.PostAsync(l_URL, l_ContentStr, "application/json", CancellationToken.None, l_Callback, true);
            else
                l_TwitchHelix.WebClient.PatchAsync(l_URL, l_ContentStr, "application/json", CancellationToken.None, l_Callback, true);
        }
        /// <summary>
        /// Delete the reward on twitch
        /// </summary>
        private void DeleteReward()
        {
            if (string.IsNullOrEmpty(Model.RewardID))
                return;

            var l_TwitchService = CP_SDK.Chat.Service.Multiplexer.Services.FirstOrDefault(x => x is CP_SDK.Chat.Services.Twitch.TwitchService);
            var l_TwitchHelix   = l_TwitchService != null ? (l_TwitchService as CP_SDK.Chat.Services.Twitch.TwitchService).HelixAPI : null;

            if (l_TwitchHelix != null)
            {
                var l_URL = $"https://api.twitch.tv/helix/channel_points/custom_rewards?broadcaster_id={l_TwitchHelix.BroadcasterID}&id={Model.RewardID}";

                l_TwitchHelix.WebClient.DeleteAsync(l_URL, CancellationToken.None, null, true);
            }
        }
    }
}
