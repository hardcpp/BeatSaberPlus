using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberPlus.SDK.Chat.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Moderation right screen
    /// </summary>
    internal class ModerationRight : SDK.UI.ResourceViewController<ModerationRight>
    {
        /// <summary>
        /// User line per page
        /// </summary>
        private static int USER_PER_PAGE = 10;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0649
        [UIObject("Background")]
        private GameObject m_Background = null;
        [UIComponent("UsersUpButton")]
        private Button m_UsersUpButton = null;
        [UIObject("UsersList")]
        private GameObject m_UsersListView = null;
        private SDK.UI.DataSource.SimpleTextList m_UsersList = null;
        [UIComponent("UsersDownButton")]
        private Button m_UsersDownButton = null;
#pragma warning restore CS0649

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Complete user list
        /// </summary>
        private List<(IChatService, IChatUser)> m_Users = new List<(IChatService, IChatUser)>();
        /// <summary>
        /// Current user list page
        /// </summary>
        private int m_CurrentPage = 1;
        /// <summary>
        /// Total user page count
        /// </summary>
        private int m_PageCount = 1;
        /// <summary>
        /// Selected index
        /// </summary>
        private int m_SelectedIndex = -1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Update background color
            SDK.UI.Backgroundable.SetOpacity(m_Background, 0.5f);

            /// Scale down up & down button
            m_UsersUpButton.transform.localScale   = Vector3.one * 0.5f;
            m_UsersDownButton.transform.localScale = Vector3.one * 0.5f;

            /// Prepare user list
            var l_BSMLTableView = m_UsersListView.GetComponentInChildren<BSMLTableView>();
            l_BSMLTableView.SetDataSource(null, false);
            GameObject.DestroyImmediate(m_UsersListView.GetComponentInChildren<CustomListTableData>());
            m_UsersList = l_BSMLTableView.gameObject.AddComponent<SDK.UI.DataSource.SimpleTextList>();
            m_UsersList.TableViewInstance = l_BSMLTableView;
            l_BSMLTableView.SetDataSource(m_UsersList, false);

            /// Bind events
            m_UsersUpButton.onClick.AddListener(OnUsersPageUpPressed);
            l_BSMLTableView.didSelectCellWithIdxEvent += OnUserSelected;
            m_UsersDownButton.onClick.AddListener(OnUsersPageDownPressed);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            Refresh();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh event list
        /// </summary>
        internal void Refresh()
        {
            /// Clear previous scores
            ClearDisplayedData();

            /// Build full list
            m_Users = Chat.Instance.LastChatUsers;
            m_Users.Sort((x,y) => x.Item2.DisplayName.CompareTo(y.Item2.DisplayName));

            /// Compute page count
            m_PageCount = Mathf.CeilToInt((float)(m_Users.Count) / (float)(USER_PER_PAGE));

            /// Rebuild list
            RebuildList();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to previous user page
        /// </summary>
        private void OnUsersPageUpPressed()
        {
            /// Underflow check
            if (m_CurrentPage < 2)
                return;

            /// Decrement current page
            m_CurrentPage--;

            /// Clear previous users
            ClearDisplayedData();

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// On user selected
        /// </summary>
        /// <param name="p_TableView">TableView instance</param>
        /// <param name="p_UserRelIndex">Relative index</param>
        private void OnUserSelected(HMUI.TableView p_TableView, int p_UserRelIndex)
        {
            if (((m_CurrentPage - 1) * USER_PER_PAGE) + p_UserRelIndex < m_Users.Count)
                m_SelectedIndex = ((m_CurrentPage - 1) * USER_PER_PAGE) + p_UserRelIndex;
            else
                m_SelectedIndex = -1;
        }
        /// <summary>
        /// Go to next user page
        /// </summary>
        private void OnUsersPageDownPressed()
        {
            /// Increment current page
            m_CurrentPage++;

            /// Clear previous users
            ClearDisplayedData();

            /// Rebuild list
            RebuildList();
        }
        /// <summary>
        /// Rebuild list
        /// </summary>
        private void RebuildList()
        {
            if (!UICreated)
                return;

            /// Clear old entries
            ClearDisplayedData();

            /// Reset selection
            m_SelectedIndex = -1;

            /// Reset page
            m_CurrentPage = m_CurrentPage > m_PageCount ? 1 : m_CurrentPage;

            for (int l_I = (m_CurrentPage - 1) * USER_PER_PAGE; l_I < m_Users.Count && l_I < (m_CurrentPage * USER_PER_PAGE); ++l_I)
                m_UsersList.Data.Add(BuildLineString(m_Users[l_I]));

            /// Refresh
            m_UsersList.TableViewInstance.ReloadData();

            /// Update UI
            m_UsersUpButton.interactable    = m_CurrentPage > 1;
            m_UsersDownButton.interactable  = m_CurrentPage < m_PageCount;
        }
        /// <summary>
        /// Clear the user list
        /// </summary>
        private void ClearDisplayedData()
        {
            if (!UICreated)
                return;

            m_UsersList.TableViewInstance.ClearSelection();
            m_UsersList.Data.Clear();
            m_UsersList.TableViewInstance.ReloadData();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// TimeOut an user
        /// </summary>
        [UIAction("click-timeout-btn-pressed")]
        private void OnTimeOutButton()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an user first!");
                return;
            }

            ShowConfirmationModal($"Do you really want to <b>TimeOut</b> user\n{m_Users[m_SelectedIndex].Item2.DisplayName}?", () => {
                foreach (var l_Current in SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/timeout {m_Users[m_SelectedIndex].Item2.UserName}");
                }
            });
        }
        /// <summary>
        /// Ban an user
        /// </summary>
        [UIAction("click-ban-btn-pressed")]
        private void OnBanButton()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an user first!");
                return;
            }

            ShowConfirmationModal($"Do you really want to <b>Ban</b> user\n{m_Users[m_SelectedIndex].Item2.DisplayName}?", () => {
                foreach (var l_Current in SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/ban {m_Users[m_SelectedIndex].Item2.UserName}");
                }
            });
        }
        /// <summary>
        /// Mod an user
        /// </summary>
        [UIAction("click-mod-btn-pressed")]
        private void OnModButton()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an user first!");
                return;
            }

            ShowConfirmationModal($"Do you really want to <b>Mod</b> user\n{m_Users[m_SelectedIndex].Item2.DisplayName}?", () => {
                foreach (var l_Current in SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/mod {m_Users[m_SelectedIndex].Item2.UserName}");
                }
            });
        }
        /// <summary>
        /// UnMod an user
        /// </summary>
        [UIAction("click-unmod-btn-pressed")]
        private void OnUnModButton()
        {
            if (m_SelectedIndex == -1)
            {
                ShowMessageModal("Please select an user first!");
                return;
            }

            ShowConfirmationModal($"Do you really want to <b>UnMod</b> user\n{m_Users[m_SelectedIndex].Item2.DisplayName}?", () => {
                foreach (var l_Current in SDK.Chat.Service.Multiplexer.Channels)
                {
                    if (l_Current.Item1 is SDK.Chat.Services.Twitch.TwitchService)
                        l_Current.Item1.SendTextMessage(l_Current.Item2, $"/unmod {m_Users[m_SelectedIndex].Item2.UserName}");
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build user line
        /// </summary>
        /// <param name="p_Item">User entry</param>
        /// <returns>Built user line</returns>
        private (string, string) BuildLineString((IChatService, IChatUser) p_Item)
        {
            /// Result line
            string l_Text = "<align=\"left\">[" + p_Item.Item1.DisplayName + "] ";

            /// Handle request limits
            if (p_Item.Item2 is SDK.Chat.Models.Twitch.TwitchUser l_TwitchUser)
            {
                if (l_TwitchUser.IsModerator || l_TwitchUser.IsBroadcaster)
                    l_Text += "🗡 <color=yellow>";
                else if (l_TwitchUser.IsVip)
                    l_Text += "💎 <color=red>";
                else if (l_TwitchUser.IsSubscriber)
                    l_Text += "👑 <color=#008eff>";
            }

            l_Text += p_Item.Item2.DisplayName;

            return (l_Text, null);
        }
    }
}
