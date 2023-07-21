using CP_SDK;
using CP_SDK.UI.Data;
using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace ChatPlexMod_ChatIntegrations.UI.Modals
{
    /// <summary>
    /// Add Condition/Action modal
    /// </summary>
    internal sealed class AddXModal : CP_SDK.UI.IModal
    {
        internal class CategoryListItem : TextListItem
        {
            public List<TypeListItem> Types;
            internal CategoryListItem(string p_Category, int p_Order, List<TypeListItem> p_Types)
                : base($"<color=yellow>{p_Order}</color> - {p_Category}")
            {
                Types = p_Types;
            }
        }
        internal class TypeListItem : TextListItem
        {
            public string TypeName;
            internal TypeListItem(string p_TypeName, int p_Order)
                : base($"<color=yellow>{p_Order}</color> - ")
            {
                TypeName = p_TypeName;
                Text += "<alpha=#80>"      + p_TypeName.Substring(0, p_TypeName.IndexOf("_"))
                     +  "<alpha=#FF>::<b>" + p_TypeName.Substring(p_TypeName.IndexOf("_") + 1) + "</b>";
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUIVVList m_CategoryList    = null;
        private XUIVVList m_TypeList        = null;

        private Action<string> m_Callback;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_CategoryList != null)
                return;

            Templates.ModalRectLayout(
                XUIHLayout.Make(
                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                            .OnListItemSelected(OnCategorySelected)
                            .Bind(ref m_CategoryList)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetWidth(40.0f).SetHeight(50.0f)
                    .SetBackground(true, CP_SDK.UI.UISystem.ListBGColor)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true),

                    XUIHLayout.Make(
                        XUIVVList.Make()
                            .SetListCellPrefab(ListCellPrefabs<TextListCell>.Get())
                            .Bind(ref m_TypeList)
                    )
                    .SetSpacing(0).SetPadding(0)
                    .SetWidth(80.0f).SetHeight(50.0f)
                    .SetBackground(true, CP_SDK.UI.UISystem.ListBGColor)
                    .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
                ),
                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).SetWidth(30f),
                    XUIPrimaryButton.Make("Create", OnCreateButton).SetWidth(30f)
                )
                .SetPadding(0)
            )
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
        /// <param name="p_Availables">Availables types</param>
        /// <param name="p_Callback">Callback</param>
        public void Init(IReadOnlyList<string> p_Availables, Action<string> p_Callback)
        {
            var l_Availables    = p_Availables;
            var l_PerCategory   = new Dictionary<string, List<string>>();

            for (var l_I = 0; l_I < l_Availables.Count; ++l_I)
            {
                var l_Type      = l_Availables[l_I];
                var l_Category  = "Others";

                if (l_Type.Contains("_"))
                    l_Category = l_Type.Substring(0, l_Type.IndexOf("_"));

                if (!l_PerCategory.ContainsKey(l_Category))
                    l_PerCategory.Add(l_Category, new List<string>() { l_Type });
                else
                    l_PerCategory[l_Category].Add(l_Type);
            }

            l_PerCategory = l_PerCategory.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var l_Order = 0;
            var l_Items = new List<CategoryListItem>();
            foreach (var l_KVP in l_PerCategory)
            {
                l_KVP.Value.Sort((x, y) => x.CompareTo(y));

                var l_Types    = new List<TypeListItem>();
                var l_SubOrder = 0;
                for (var l_TI = 0; l_TI < l_KVP.Value.Count; ++l_TI)
                    l_Types.Add(new TypeListItem(l_KVP.Value[l_TI], ++l_SubOrder));

                l_Items.Add(new CategoryListItem(l_KVP.Key, ++l_Order, l_Types));
            }

            m_CategoryList.SetListItems(l_Items);
            m_CategoryList.SetSelectedListItem(l_Items.FirstOrDefault());

            m_Callback = p_Callback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On category selected
        /// </summary>
        /// <param name="p_SelectedItem">Selected item</param>
        private void OnCategorySelected(IListItem p_SelectedItem)
        {
            var l_SelectedCategory = p_SelectedItem as CategoryListItem;
            if (l_SelectedCategory == null)
            {
                m_TypeList.SetListItems(null);
                return;
            }

            m_TypeList.SetListItems(l_SelectedCategory.Types);
            m_TypeList.SetSelectedListItem(l_SelectedCategory.Types.FirstOrDefault());
        }
        /// <summary>
        /// On cancel button
        /// </summary>
        private void OnCancelButton()
        {
            VController.CloseModal(this);
        }
        /// <summary>
        /// On create button
        /// </summary>
        private void OnCreateButton()
        {
            var l_SelectedType = m_TypeList.Element.GetSelectedItem() as TypeListItem;
            if (l_SelectedType == null)
            {
                VController.ShowMessageModal("Please select a type first!");
                return;
            }

            VController.CloseModal(this);

            try { m_Callback?.Invoke(l_SelectedType.TypeName); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[ChatPlexMod_ChatIntegrations.UI][AddXModal.OnCreateButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
