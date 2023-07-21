using CP_SDK.UI.Data;
using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Dropdown modal
    /// </summary>
    public sealed class Dropdown : IModal
    {
        private XUIVVList m_List = null;

        private Action<string> m_Callback = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_List != null)
                return;

            Templates.ModalRectLayout(
                XUIHLayout.Make(
                    XUIVVList.Make()
                        .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                        .Bind(ref m_List)
                )
                .SetHeight(50)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true, UISystem.ListBGColor)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).SetWidth(30f)
                )
                .SetPadding(0)
            )
            .SetWidth(80.0f).SetHeight(55.0f)
            .BuildUI(transform);
        }
        /// <summary>
        /// On modal close
        /// </summary>
        public override void OnClose()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="p_Options"></param>
        /// <param name="p_Selected"></param>
        /// <param name="p_Callback"></param>
        public void Init(List<string> p_Options, string p_Selected, Action<string> p_Callback)
        {
            m_List.OnListItemSelected(OnListItemSelect, false);

            var l_Items = p_Options.Select(x => new TextListItem(x)).ToList();
            m_List.SetListItems(l_Items);

            if (p_Selected != null && p_Options.IndexOf(p_Selected) != -1)
                m_List.SetSelectedListItem(l_Items[p_Options.IndexOf(p_Selected)]);

            m_List.OnListItemSelected(OnListItemSelect, true);

            m_Callback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel button
        /// </summary>
        private void OnCancelButton()
        {
            VController.CloseModal(this);
        }
        /// <summary>
        /// On list item selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected list item</param>
        private void OnListItemSelect(IListItem p_SelectedItem)
        {
            VController.CloseModal(this);

            var l_SelectedItem = m_List.Element.GetSelectedItem() as TextListItem;
            if (l_SelectedItem == null)
                return;

            try { m_Callback?.Invoke(l_SelectedItem.Text); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Dropdown.OnListItemSelect] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
