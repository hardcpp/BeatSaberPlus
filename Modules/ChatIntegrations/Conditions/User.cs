using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Conditions
{
    public class User_Permissions : Interfaces.ICondition<User_Permissions, Models.Conditions.User_Permissions>
    {
        public override string Description => "Check user permissions";

#pragma warning disable CS0414
        [UIComponent("SubscriberToggle")]
        private ToggleSetting m_SubscriberToggle = null;
        [UIComponent("VIPToggle")]
        private ToggleSetting m_VIPToggle = null;
        [UIComponent("ModeratorToggle")]
        private ToggleSetting m_ModeratorToggle = null;
        [UIComponent("NotifyToggle")]
        private ToggleSetting m_NotifyToggle = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ToggleSetting.Setup(m_SubscriberToggle,  l_Event, Model.Subscriber,              false);
            SDK.UI.ToggleSetting.Setup(m_VIPToggle,         l_Event, Model.VIP,                     false);
            SDK.UI.ToggleSetting.Setup(m_ModeratorToggle,   l_Event, Model.Moderator,               false);
            SDK.UI.ToggleSetting.Setup(m_NotifyToggle,      l_Event, Model.NotifyWhenNoPermission,  false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Subscriber                = m_SubscriberToggle.Value;
            Model.VIP                       = m_VIPToggle.Value;
            Model.Moderator                 = m_ModeratorToggle.Value;
            Model.NotifyWhenNoPermission    = m_NotifyToggle.Value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            bool l_IsSuscriber  = false;
            bool l_IsVIP        = false;
            bool l_IsModerator  = p_Context.User.IsBroadcaster || p_Context.User.IsModerator;

            if (p_Context.User is SDK.Chat.Models.Twitch.TwitchUser l_TwitchUser)
            {
                l_IsSuscriber   = l_TwitchUser.IsSubscriber;
                l_IsVIP         = l_TwitchUser.IsVip;
            }

            if (Model.Subscriber && l_IsSuscriber)
                return true;

            if (Model.VIP && l_IsVIP)
                return true;

            if (Model.Moderator && l_IsModerator)
                return true;

            if (Model.NotifyWhenNoPermission && p_Context.ChatService != null && p_Context.Channel != null && p_Context.User != null)
                p_Context.ChatService.SendTextMessage(p_Context.Channel, $"! @{p_Context.User.DisplayName} You can't use this command!");

            return false;
        }
    }
}
