using CP_SDK.UI.Data;
using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Moderation shortcuts main view
    /// </summary>
    internal sealed class ModerationShortcutsMainView : CP_SDK.UI.ViewController<ModerationShortcutsMainView>
    {
        private XUIVVList m_List = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private List<TextListItem>  m_Items         = new List<TextListItem>();
        private TextListItem        m_SelectedItem  = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Shortcuts"),

                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
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
                    XUIPrimaryButton.Make("New").OnClick(OnNewButton),
                    XUIPrimaryButton.Make("Delete").OnClick(OnDeleteButton)
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
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            CConfig.Instance.ModerationKeys.Clear();
            for (var l_I = 0; l_I < m_Items.Count; ++l_I)
                CConfig.Instance.ModerationKeys.Add(m_Items[l_I].Text);

            CConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh list
        /// </summary>
        private void Refresh()
        {
            m_Items.Clear();
            for (var l_I = 0; l_I < CConfig.Instance.ModerationKeys.Count; ++l_I)
                m_Items.Add(new TextListItem(CConfig.Instance.ModerationKeys[l_I]));

            m_List.SetListItems(m_Items);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On item selected
        /// </summary>
        /// <param name="p_ListItem">Selected item</param>
        private void OnListItemSelect(IListItem p_ListItem)
            => m_SelectedItem = (TextListItem)p_ListItem;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// New shortcut button
        /// </summary>
        private void OnNewButton()
        {
            ShowKeyboardModal(string.Empty, (x) =>
            {
                if (string.IsNullOrEmpty(x))
                    return;

                CConfig.Instance.ModerationKeys.Add(x.Trim());
                Refresh();

                m_List.SetSelectedListItem(m_Items.LastOrDefault());
            });
        }
        /// <summary>
        /// Delete shortcut button
        /// </summary>
        private void OnDeleteButton()
        {
           if (!EnsureItemSelected())
               return;

           ShowConfirmationModal($"<color=red>Do you want to delete shortcut</color>\n\"{m_SelectedItem.Text}\"?", (x) =>
           {
               if (!x)
                   return;

               CConfig.Instance.ModerationKeys.Remove(m_SelectedItem.Text);
               m_Items.Remove(m_SelectedItem);
               m_List.RemoveListItem(m_SelectedItem);
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
                ShowMessageModal("Please select a shortcut first!");
                return false;
            }

            return true;
        }
    }
}
