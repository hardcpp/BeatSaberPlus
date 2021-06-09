using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Concurrent;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Conditions
{
    public class Misc_Cooldown : Interfaces.ICondition<Misc_Cooldown, Models.Conditions.Misc_Cooldown>
    {
        public override string Description => "Add a cooldown on your event";

#pragma warning disable CS0414
        [UIComponent("CooldownSlider")]
        private SliderSetting m_CooldownSlider = null;
        [UIComponent("PerUserToggle")]
        private ToggleSetting m_PerUserToggle = null;
        [UIComponent("NotifyUserToggle")]
        private ToggleSetting m_NotifyUserToggle = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.SliderSetting.Setup(m_CooldownSlider,    l_Event, SDK.UI.BSMLSettingFormartter.Time, Model.CooldownTime, true, true, new Vector2(0.51f, 0f), new Vector2(0.93f, 1f));
            SDK.UI.ToggleSetting.Setup(m_PerUserToggle,     l_Event,                                    Model.PerUser,      false);
            SDK.UI.ToggleSetting.Setup(m_NotifyUserToggle,  l_Event,                                    Model.NotifyUser,   false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.CooldownTime  = (uint)m_CooldownSlider.slider.value;
            Model.PerUser       = m_PerUserToggle.Value;
            Model.NotifyUser    = m_NotifyUserToggle.Value;
        }

        private long m_LastTime = 0;
        private ConcurrentDictionary<string, long> m_Cooldowns = new ConcurrentDictionary<string, long>();

        public override bool Eval(Models.EventContext p_Context)
        {
            if (Model.PerUser)
            {
                if (p_Context.User == null)
                    return true;

                if (m_Cooldowns.TryGetValue(p_Context.User.UserName, out var l_LastTime))
                {
                    var l_Remaining = (l_LastTime + Model.CooldownTime) - SDK.Misc.Time.UnixTimeNow();
                    if (l_Remaining > 0)
                    {
                        if (Model.NotifyUser && p_Context.ChatService != null && p_Context.Channel != null)
                            p_Context.ChatService.SendTextMessage(p_Context.Channel, BuildFailedMessage(p_Context.User, l_Remaining));
                        return false;
                    }

                    m_Cooldowns.TryUpdate(p_Context.User.UserName, SDK.Misc.Time.UnixTimeNow(), l_LastTime);
                }
                else
                    m_Cooldowns.TryAdd(p_Context.User.UserName, SDK.Misc.Time.UnixTimeNow());
            }
            else
            {
                var l_Remaining = (m_LastTime + Model.CooldownTime) - SDK.Misc.Time.UnixTimeNow();
                if (l_Remaining > 0)
                {
                    if (Model.NotifyUser && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                        p_Context.ChatService.SendTextMessage(p_Context.Channel, BuildFailedMessage(p_Context.User, l_Remaining));
                    return false;
                }

                m_LastTime = SDK.Misc.Time.UnixTimeNow();
            }

            return true;
        }

        private string BuildFailedMessage(BeatSaberPlus.SDK.Chat.Interfaces.IChatUser p_User, long p_Remaining)
        {
            var l_Minutes = p_Remaining / 60;
            var l_Seconds = p_Remaining - (l_Minutes * 60);

            if (l_Minutes != 0)
                return $"! @{p_User.DisplayName} command is on cooldown, {l_Minutes}m{l_Seconds}s remaining!";

            return $"! @{p_User.DisplayName} command is on cooldown, {l_Seconds}s remaining!";
        }
    }
}
