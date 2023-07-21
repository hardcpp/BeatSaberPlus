using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Conditions
{
    public class User_Permissions
        : Interfaces.ICondition<User_Permissions, Models.Conditions.User_Permissions>
    {
        private XUIToggle m_Subscriber  = null;
        private XUIToggle m_VIP         = null;
        private XUIToggle m_Moderator   = null;
        private XUIToggle m_Notify      = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Check user permissions";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Is a subscriber?",
                    XUIToggle.Make()
                        .SetValue(Model.Subscriber).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Subscriber)
                ),

                Templates.SettingsHGroup("Is a VIP?",
                    XUIToggle.Make()
                        .SetValue(Model.VIP).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_VIP)
                ),

                Templates.SettingsHGroup("Is a moderator?",
                    XUIToggle.Make()
                        .SetValue(Model.Moderator).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Moderator)
                ),

                Templates.SettingsHGroup("Notify when no power?",
                    XUIToggle.Make()
                        .SetValue(Model.NotifyWhenNoPermission).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Notify)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Subscriber                = m_Subscriber.Element.GetValue();
            Model.VIP                       = m_VIP.Element.GetValue();
            Model.Moderator                 = m_Moderator.Element.GetValue();
            Model.NotifyWhenNoPermission    = m_Notify.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(Models.EventContext p_Context)
        {
            if (p_Context.User.IsBroadcaster)
                return true;

            var l_IsModerator   = p_Context.User.IsBroadcaster || p_Context.User.IsModerator;
            var l_IsSuscriber   = p_Context.User.IsSubscriber;
            var l_IsVIP         = p_Context.User.IsVip;

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
