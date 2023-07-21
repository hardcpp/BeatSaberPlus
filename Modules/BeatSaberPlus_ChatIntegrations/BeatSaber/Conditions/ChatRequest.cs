using CP_SDK.XUI;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Conditions
{
    public class ChatRequest_QueueDuration
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<ChatRequest_QueueDuration, Models.Conditions.ChatRequest_QueueDuration>
    {
        private XUIDropdown m_Comparison        = null;
        private XUISlider   m_Duration          = null;
        private XUIToggle   m_SendMessageOnFail = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add conditions on chat request queue duration!";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Comparison",
                    XUIDropdown.Make()
                        .SetOptions(ChatPlexMod_ChatIntegrations.Enums.Comparison.S).SetValue(ChatPlexMod_ChatIntegrations.Enums.Comparison.ToStr(Model.Comparison))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Comparison)
                ),

                Templates.SettingsHGroup("Duration",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(10800.0f).SetIncrements(1.0f).SetInteger(true).SetFormatter(CP_SDK.UI.ValueFormatters.TimeShortBaseSeconds)
                        .SetValue(Model.Duration).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Duration)
                ),

                Templates.SettingsHGroup("Send chat message on fail",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessageOnFail)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessageOnFail)
                ),
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Comparison            = ChatPlexMod_ChatIntegrations.Enums.Comparison.ToEnum(m_Comparison.Element.GetValue());
            Model.Duration              = (uint)m_Duration.Element.GetValue();
            Model.SendChatMessageOnFail = m_SendMessageOnFail.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;
            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            if (ChatPlexMod_ChatIntegrations.Enums.Comparison.Evaluate(Model.Comparison, (int)l_ChatRequest.QueueDuration, (int)Model.Duration))
                return true;

            if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
            {
                if (Model.Comparison <= ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Equal)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too short");
                else
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too long");
            }

            return false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_QueueSize
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<ChatRequest_QueueSize, Models.Conditions.ChatRequest_QueueSize>
    {
        private XUIDropdown m_Comparison        = null;
        private XUISlider   m_Count             = null;
        private XUIToggle   m_SendMessageOnFail = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add conditions on chat request queue size!";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Comparison",
                    XUIDropdown.Make()
                        .SetOptions(ChatPlexMod_ChatIntegrations.Enums.Comparison.S).SetValue(ChatPlexMod_ChatIntegrations.Enums.Comparison.ToStr(Model.Comparison))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Comparison)
                ),

                Templates.SettingsHGroup("Count",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(200.0f).SetIncrements(1.0f).SetInteger(true).SetFormatter(CP_SDK.UI.ValueFormatters.TimeShortBaseSeconds)
                        .SetValue(Model.Count).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Count)
                ),

                Templates.SettingsHGroup("Send chat message on fail",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessageOnFail)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessageOnFail)
                ),
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////
        private void OnSettingChanged()
        {
            Model.Comparison            = ChatPlexMod_ChatIntegrations.Enums.Comparison.ToEnum(m_Comparison.Element.GetValue());
            Model.Count                 = (uint)m_Count.Element.GetValue();
            Model.SendChatMessageOnFail = m_SendMessageOnFail.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;
            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            if (ChatPlexMod_ChatIntegrations.Enums.Comparison.Evaluate(Model.Comparison, (int)l_ChatRequest.SongQueueCount, (int)Model.Count))
                return true;

            if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
            {
                if (Model.Comparison <= ChatPlexMod_ChatIntegrations.Enums.Comparison.E.Equal)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too small");
                else
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is too big");
            }

            return false;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_QueueStatus
        : ChatPlexMod_ChatIntegrations.Interfaces.ICondition<ChatRequest_QueueStatus, Models.Conditions.ChatRequest_QueueStatus>
    {
        private XUIDropdown m_Status            = null;
        private XUIToggle   m_SendMessageOnFail = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add conditions on chat request queue!";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Status",
                    XUIDropdown.Make()
                        .SetOptions(Enums.QueueStatus.S).SetValue(Enums.QueueStatus.ToStr(Model.Status))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Status)
                ),

                Templates.SettingsHGroup("Send chat message on fail",
                    XUIToggle.Make()
                        .SetValue(Model.SendChatMessageOnFail)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_SendMessageOnFail)
                ),
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatRequest module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Status                = Enums.QueueStatus.ToEnum(m_Status.Element.GetValue());
            Model.SendChatMessageOnFail = m_SendMessageOnFail.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(ChatPlexMod_ChatIntegrations.Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatRequest)
            {
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing!");
                return false;
            }

            var l_ChatRequest = BeatSaberPlus_ChatRequest.ChatRequest.Instance;
            if (l_ChatRequest == null || !l_ChatRequest.IsEnabled)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} chat request is not enabled");

                return false;
            }

            if (Model.Status == Enums.QueueStatus.E.Open && !l_ChatRequest.QueueOpen)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is closed");

                return false;
            }
            else if (Model.Status == Enums.QueueStatus.E.Closed && l_ChatRequest.QueueOpen)
            {
                if (Model.SendChatMessageOnFail && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                    p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} queue is open");

                return false;
            }

            return true;
        }
    }
}
