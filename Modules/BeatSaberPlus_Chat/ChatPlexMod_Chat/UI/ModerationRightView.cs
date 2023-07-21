using CP_SDK.XUI;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Moderation right view
    /// </summary>
    internal sealed class ModerationRightView : CP_SDK.UI.ViewController<ModerationRightView>
    {
        private XUIVVList m_List = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<Data.ChatUserListItem>  m_Items         = new List<Data.ChatUserListItem>();
        private Data.ChatUserListItem        m_SelectedItem  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public Data.ChatUserListItem SelectedItem => m_SelectedItem;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Channel Active Users (Last 40 ones)"),

                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(CP_SDK.UI.Data.ListCellPrefabs<CP_SDK.UI.Data.TextListCell>.Get())
                        .OnListItemSelected(OnListItemSelect)
                        .Bind(ref m_List)
                )
                .SetHeight(55)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("TimeOut(10 min)").OnClick(OnTimeOutButton),
                    XUIPrimaryButton.Make("Ban").OnClick(OnBanButton),
                    XUIPrimaryButton.Make("Mod").OnClick(OnModButton),
                    XUIPrimaryButton.Make("UnMod").OnClick(OnUnModButton)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
            => Refresh();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh event list
        /// </summary>
        internal void Refresh()
        {
            m_Items.Clear();
            for (var l_I = 0; l_I < Chat.Instance.LastChatUsers.Count; ++l_I)
                m_Items.Add(new Data.ChatUserListItem(Chat.Instance.LastChatUsers[l_I].Item1, Chat.Instance.LastChatUsers[l_I].Item2));
            m_Items.Sort((x, y) => x.User.DisplayName.CompareTo(y.User.DisplayName));

            m_List.SetListItems(m_Items);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On user selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnListItemSelect(CP_SDK.UI.Data.IListItem p_SelectedItem)
            => m_SelectedItem = (Data.ChatUserListItem)p_SelectedItem;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// TimeOut an user
        /// </summary>
        private void OnTimeOutButton()
        {
            if (!EnsureItemSelected() || !EnsurePermissions())
                return;

            ShowConfirmationModal($"Do you really want to <b>TimeOut</b> user\n{m_SelectedItem.User.DisplayName}?", (x) => {
                if (!x)
                    return;

                foreach (var l_Current in CP_SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is CP_SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/timeout {m_SelectedItem.User.UserName}");
                }
            });
        }
        /// <summary>
        /// Ban an user
        /// </summary>
        private void OnBanButton()
        {
            if (!EnsureItemSelected() || !EnsurePermissions())
                return;

            ShowConfirmationModal($"Do you really want to <b>Ban</b> user\n{m_SelectedItem.User.DisplayName}?", (x) => {
                if (!x)
                    return;

                foreach (var l_Current in CP_SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is CP_SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/ban {m_SelectedItem.User.UserName}");
                }
            });
        }
        /// <summary>
        /// Mod an user
        /// </summary>
        private void OnModButton()
        {
            if (!EnsureItemSelected() || !EnsurePermissions())
                return;

            ShowConfirmationModal($"Do you really want to <b>Mod</b> user\n{m_SelectedItem.User.DisplayName}?", (x) => {
                if (!x)
                    return;

                foreach (var l_Current in CP_SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is CP_SDK.Chat.Services.Twitch.TwitchService)
                        m_SelectedItem.Service.SendTextMessage(l_Current.Item2, $"/mod {m_SelectedItem.User.UserName}");
                }
            });
        }
        /// <summary>
        /// UnMod an user
        /// </summary>
        private void OnUnModButton()
        {
            if (!EnsureItemSelected() || !EnsurePermissions())
                return;

            ShowConfirmationModal($"Do you really want to <b>UnMod</b> user\n{m_SelectedItem.User.DisplayName}?", (x) => {
                if (!x)
                    return;

                foreach (var l_Current in CP_SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is CP_SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/unmod {m_SelectedItem.User.UserName}");
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Ensure that an shortcut is selected
        /// </summary>
        /// <returns></returns>
        private bool EnsureItemSelected()
        {
            if (m_SelectedItem == null)
            {
                ShowMessageModal("Please select an user first!");
                return false;
            }

            return true;
        }
        /// <summary>
        /// Ensure permissions
        /// </summary>
        /// <returns></returns>
        private bool EnsurePermissions()
        {
            if (!(m_SelectedItem.Service is CP_SDK.Chat.Services.Twitch.TwitchService))
            {
                ShowMessageModal("Only twitch is supported at the moment!");
                return false;
            }

            return true;
        }
    }
}
