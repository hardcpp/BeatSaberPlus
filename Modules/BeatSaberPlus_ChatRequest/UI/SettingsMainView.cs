using CP_SDK.XUI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUISlider m_GeneralTab_UserRequest;
        private XUISlider m_GeneralTab_VIPBonusRequest;
        private XUISlider m_GeneralTab_SubscriberBonusRequest;
        private XUISlider m_GeneralTab_HistorySize;

        private XUIToggle m_GeneralTab_PlayPreviewMusic;
        private XUIToggle m_GeneralTab_BigCoverArt;
        private XUISlider m_GeneralTab_QueueSize;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIToggle m_CooldownTab_BSRCooldownPerUser;
        private XUISlider m_CooldownTab_BSRCooldown;

        private XUIToggle m_CooldownTab_QueueCooldownPerUser;
        private XUISlider m_CooldownTab_QueueCooldown;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat Request - Settings"),

                XUITabControl.Make(
                    ("General",     BuildGeneralTab()),
                    ("Commands",    BuildCommandsTab()),
                    ("Cooldown",    BuildCooldownTab())
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            CRConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build general tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildGeneralTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIText.Make("User max request"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.UserMaxRequest)
                        .Bind(ref m_GeneralTab_UserRequest),

                    XUIText.Make("VIP bonus request"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.VIPBonusRequest)
                        .Bind(ref m_GeneralTab_VIPBonusRequest),

                    XUIText.Make("Subscriber bonus request"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.SubscriberBonusRequest)
                        .Bind(ref m_GeneralTab_SubscriberBonusRequest),

                    XUIText.Make("History size"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(50f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.HistorySize)
                        .Bind(ref m_GeneralTab_HistorySize)
                )
                .SetSpacing(2)
                .SetPadding(2)
                .SetWidth(60)
                .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged())),

                XUIVLayout.Make(
                    XUIText.Make("Play preview music if downloaded"),
                    XUIToggle.Make()
                        .SetValue(CRConfig.Instance.PlayPreviewMusic)
                        .Bind(ref m_GeneralTab_PlayPreviewMusic),

                    XUIText.Make("Show cover art in big"),
                    XUIToggle.Make()
                        .SetValue(CRConfig.Instance.BigCoverArt)
                        .Bind(ref m_GeneralTab_BigCoverArt),

                    XUIText.Make("Queue command show count"),
                    XUISlider.Make()
                        .SetMinValue(1f).SetMaxValue(10f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.Commands.QueueCommandShowSize)
                        .Bind(ref m_GeneralTab_QueueSize)
                )
                .SetSpacing(2)
                .SetPadding(2)
                .SetWidth(60)
                .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged()))
                .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
            )
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained);
        }
        /// <summary>
        /// Build commands tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildCommandsTab()
        {
            var l_CConfig  = CRConfig.Instance.Commands;
            var l_Comamnds = new List<Data.CommandListItem>()
            {
                new Data.CommandListItem(l_CConfig.BSRCommand, () => l_CConfig.BSRCommandPermissions, (x) => l_CConfig.BSRCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BSRHelpCommand, () => l_CConfig.BSRHelpCommandPermissions, (x) => l_CConfig.BSRHelpCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.LinkCommand, () => l_CConfig.LinkCommandPermissions, (x) => l_CConfig.LinkCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.QueueCommand, () => l_CConfig.QueueCommandPermissions, (x) => l_CConfig.QueueCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.QueueStatusCommand, () => l_CConfig.QueueStatusCommandPermissions, (x) => l_CConfig.QueueStatusCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.WrongCommand, () => l_CConfig.WrongCommandCommandPermissions, (x) => l_CConfig.WrongCommandCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.ModAddCommand, () => l_CConfig.ModAddPermissions, (x) => l_CConfig.ModAddPermissions = x),
                new Data.CommandListItem(l_CConfig.OpenCommand, () => l_CConfig.OpenCommandPermissions, (x) => l_CConfig.OpenCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.CloseCommand, () => l_CConfig.CloseCommandPermissions, (x) => l_CConfig.CloseCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.SabotageCommand, () => l_CConfig.SabotageCloseCommandPermissions, (x) => l_CConfig.SabotageCloseCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.SongMessageCommand, () => l_CConfig.SongMessageCommandPermissions, (x) => l_CConfig.SongMessageCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.MoveToTopCommand, () => l_CConfig.MoveToTopCommandPermissions, (x) => l_CConfig.MoveToTopCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.AddToTopCommand, () => l_CConfig.AddToTopCommandPermissions, (x) => l_CConfig.AddToTopCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.RemoveCommand, () => l_CConfig.RemoveCommandPermissions, (x) => l_CConfig.RemoveCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BsrBanCommand, () => l_CConfig.BsrBanCommandPermissions, (x) => l_CConfig.BsrBanCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BsrUnbanCommand, () => l_CConfig.BsrUnbanCommandPermissions, (x) => l_CConfig.BsrUnbanCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BsrBanMapperCommand, () => l_CConfig.BsrBanMapperCommandPermissions, (x) => l_CConfig.BsrBanMapperCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BsrUnbanMapperCommand, () => l_CConfig.BsrUnbanMapperCommandPermissions, (x) => l_CConfig.BsrUnbanMapperCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.RemapCommand, () => l_CConfig.RemapCommandPermissions, (x) => l_CConfig.RemapCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.AllowlistCommand, () => l_CConfig.AllowlistCommandPermissions, (x) => l_CConfig.AllowlistCommandPermissions = x),
                new Data.CommandListItem(l_CConfig.BlocklistCommand, () => l_CConfig.BlocklistCommandPermissions, (x) => l_CConfig.BlocklistCommandPermissions = x)
            };

            return XUIVLayout.Make(
                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<Data.CommandListCell>.Get())
                        .SetListItems(l_Comamnds)
                )
                .SetHeight(60)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            )
            .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true);
        }
        /// <summary>
        /// Build the cooldown tab
        /// </summary>
        /// <returns></returns>
        private IXUIElement BuildCooldownTab()
        {
            return XUIHLayout.Make(
                XUIVLayout.Make(
                    XUIText.Make("BSR command cooldown per user"),
                    XUIToggle.Make()
                        .SetValue(CRConfig.Instance.Commands.BSRCommandCooldownPerUser)
                        .Bind(ref m_CooldownTab_BSRCooldownPerUser),

                    XUIText.Make("BSR command cooldown in seconds"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(60f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.Commands.BSRCommandCooldown)
                        .Bind(ref m_CooldownTab_BSRCooldown)
                )
                .SetSpacing(2)
                .SetPadding(2)
                .SetWidth(60)
                .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged())),

                XUIVLayout.Make(
                    XUIText.Make("Queue command cooldown per user"),
                    XUIToggle.Make()
                        .SetValue(CRConfig.Instance.Commands.QueueCommandCooldownPerUser)
                        .Bind(ref m_CooldownTab_QueueCooldownPerUser),

                    XUIText.Make("Queue command cooldown in seconds"),
                    XUISlider.Make()
                        .SetMinValue(0f).SetMaxValue(60f).SetIncrements(1f).SetInteger(true)
                        .SetValue(CRConfig.Instance.Commands.QueueCommandCooldown)
                        .Bind(ref m_CooldownTab_QueueCooldown)
                )
                .SetSpacing(2)
                .SetPadding(2)
                .SetWidth(60)
                .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged()))
                .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
            )
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnValueChanged()
        {
            if (m_PreventChanges)
                return;

            /// General tab
            CRConfig.Instance.UserMaxRequest                = (int)m_GeneralTab_UserRequest.Element.GetValue();
            CRConfig.Instance.VIPBonusRequest               = (int)m_GeneralTab_VIPBonusRequest.Element.GetValue();
            CRConfig.Instance.SubscriberBonusRequest        = (int)m_GeneralTab_SubscriberBonusRequest.Element.GetValue();
            CRConfig.Instance.HistorySize                   = (int)m_GeneralTab_HistorySize.Element.GetValue();

            CRConfig.Instance.PlayPreviewMusic              = m_GeneralTab_PlayPreviewMusic.Element.GetValue();
            CRConfig.Instance.BigCoverArt                   = m_GeneralTab_BigCoverArt.Element.GetValue();
            CRConfig.Instance.Commands.QueueCommandShowSize = (int)m_GeneralTab_QueueSize.Element.GetValue();

            /// Cooldown tab
            CRConfig.Instance.Commands.BSRCommandCooldownPerUser    = m_CooldownTab_BSRCooldownPerUser.Element.GetValue();
            CRConfig.Instance.Commands.BSRCommandCooldown           = (int)m_CooldownTab_BSRCooldown.Element.GetValue();

            CRConfig.Instance.Commands.QueueCommandCooldownPerUser  = m_CooldownTab_QueueCooldownPerUser.Element.GetValue();
            CRConfig.Instance.Commands.QueueCommandCooldown         = (int)m_CooldownTab_QueueCooldown.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// General tab
            m_GeneralTab_UserRequest.SetValue(CRConfig.Instance.UserMaxRequest);
            m_GeneralTab_VIPBonusRequest       .SetValue(CRConfig.Instance.VIPBonusRequest);
            m_GeneralTab_SubscriberBonusRequest.SetValue(CRConfig.Instance.SubscriberBonusRequest);
            m_GeneralTab_HistorySize           .SetValue(CRConfig.Instance.HistorySize);

            m_GeneralTab_PlayPreviewMusic      .SetValue(CRConfig.Instance.PlayPreviewMusic);
            m_GeneralTab_BigCoverArt           .SetValue(CRConfig.Instance.BigCoverArt);
            m_GeneralTab_QueueSize             .SetValue(CRConfig.Instance.Commands.QueueCommandShowSize);

            /// Cooldown tab
            m_CooldownTab_BSRCooldownPerUser.SetValue(CRConfig.Instance.Commands.BSRCommandCooldownPerUser);
            m_CooldownTab_BSRCooldown.SetValue(CRConfig.Instance.Commands.BSRCommandCooldown);

            m_CooldownTab_QueueCooldownPerUser.SetValue(CRConfig.Instance.Commands.QueueCommandCooldownPerUser);
            m_CooldownTab_QueueCooldown.SetValue(CRConfig.Instance.Commands.QueueCommandCooldown);

            m_PreventChanges = false;
        }
    }
}
