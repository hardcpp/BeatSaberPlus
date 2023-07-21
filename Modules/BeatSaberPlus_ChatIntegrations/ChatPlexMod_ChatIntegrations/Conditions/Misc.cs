using CP_SDK.XUI;
using System.Collections.Concurrent;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Conditions
{
    internal class MiscRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterConditionType("Misc_Cooldown", () => new Misc_Cooldown());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_Cooldown
        : Interfaces.ICondition<Misc_Cooldown, Models.Conditions.Misc_Cooldown>
    {
        private XUISlider m_Cooldown    = null;
        private XUIToggle m_PerUser     = null;
        private XUIToggle m_NotifyUser  = null;

        private long                                m_LastTime  = 0;
        private ConcurrentDictionary<string, long>  m_Cooldowns = new ConcurrentDictionary<string, long>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add a cooldown on your event";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Cooldown",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(1200.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.CooldownTime).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Cooldown)
                ),

                Templates.SettingsHGroup("Use a per user cooldown instead of global",
                    XUIToggle.Make()
                        .SetValue(Model.PerUser).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_PerUser)
                ),

                Templates.SettingsHGroup("Notify user on cooldown",
                    XUIToggle.Make()
                        .SetValue(Model.NotifyUser).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_NotifyUser)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.CooldownTime  = (uint)m_Cooldown.Element.GetValue();
            Model.PerUser       = m_PerUser.Element.GetValue();
            Model.NotifyUser    = m_NotifyUser.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(Models.EventContext p_Context)
        {
            if (Model.PerUser)
            {
                if (p_Context.User == null)
                    return true;

                if (m_Cooldowns.TryGetValue(p_Context.User.UserName, out var l_LastTime))
                {
                    var l_Remaining = (l_LastTime + Model.CooldownTime) - CP_SDK.Misc.Time.UnixTimeNow();
                    if (l_Remaining > 0)
                    {
                        if (Model.NotifyUser && p_Context.ChatService != null && p_Context.Channel != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, BuildFailedMessage(p_Context.User, l_Remaining));
                        return false;
                    }

                    m_Cooldowns.TryUpdate(p_Context.User.UserName, CP_SDK.Misc.Time.UnixTimeNow(), l_LastTime);
                }
                else
                    m_Cooldowns.TryAdd(p_Context.User.UserName, CP_SDK.Misc.Time.UnixTimeNow());
            }
            else
            {
                var l_Remaining = (m_LastTime + Model.CooldownTime) - CP_SDK.Misc.Time.UnixTimeNow();
                if (l_Remaining > 0)
                {
                    if (Model.NotifyUser && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, BuildFailedMessage(p_Context.User, l_Remaining));
                    return false;
                }

                m_LastTime = CP_SDK.Misc.Time.UnixTimeNow();
            }

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private string BuildFailedMessage(CP_SDK.Chat.Interfaces.IChatUser p_User, long p_Remaining)
        {
            var l_Minutes = p_Remaining / 60;
            var l_Seconds = p_Remaining - (l_Minutes * 60);

            if (l_Minutes != 0)
                return $"! @{p_User.DisplayName} command is on cooldown, {l_Minutes}m{l_Seconds}s remaining!";

            return $"! @{p_User.DisplayName} command is on cooldown, {l_Seconds}s remaining!";
        }
    }
}
