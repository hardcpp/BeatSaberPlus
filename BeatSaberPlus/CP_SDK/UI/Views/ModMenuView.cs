using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Mod menu view controller
    /// </summary>
    public sealed class ModMenuView : ViewController<ModMenuView>
    {
        private Dictionary<ModButton, Components.CPOrSButton>   m_Buttons = new Dictionary<ModButton, Components.CPOrSButton>();
        private XUIGLayout                                      m_GLayout = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Mods"),

                XUIHLayout.Make(
                    XUIVScrollView.Make(
                        XUIGLayout.Make(

                        )
                        .SetCellSize(new Vector2(34, 9))
                        .SetChildAlign(TextAnchor.UpperLeft)
                        .SetConstraint(GridLayoutGroup.Constraint.FixedColumnCount)
                        .SetConstraintCount(3)
                        .SetSpacing(new Vector2(2, 0))
                        .Bind(ref m_GLayout)
                    )
                )
                .SetHeight(60)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true, null, true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
            )
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
            => Refresh();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh buttons
        /// </summary>
        internal void Refresh()
        {
            var l_ModButtons    = ModMenu.ModButtons;
            var l_Existing      = new List<ModButton>(m_Buttons.Keys);

            /// Deletion
            for (var l_I = 0; l_I < l_Existing.Count; ++l_I)
            {
                if (l_ModButtons.Contains(l_Existing[l_I]))
                    continue;

                var l_ModButton = l_Existing[l_I];
                GameObject.Destroy(m_Buttons[l_ModButton]);
                m_Buttons.Remove(l_ModButton);
            }

            /// Creation
            for (var l_I = 0; l_I < l_ModButtons.Count; ++l_I)
            {
                if (m_Buttons.ContainsKey(l_ModButtons[l_I]))
                    continue;

                var l_ModButton = l_ModButtons[l_I];
                var l_Button    = UISystem.PrimaryButtonFactory.Create("ModButton", m_GLayout.RTransform);
                l_Button.SetWidth(34.0f);
                l_Button.SetHeight(7.0f);
                l_Button.SetFontSize(3.44f);
                l_Button.SetOverflowMode(TMPro.TextOverflowModes.Ellipsis);
                l_Button.OnClick(() => l_ModButton.FireOnClick());

                m_Buttons.Add(l_ModButton, l_Button);
            }

            /// Sorting / Update
            for (var l_I = 0; l_I < l_ModButtons.Count; ++l_I)
            {
                var l_ModButton = l_ModButtons[l_I];
                var l_Button    = m_Buttons[l_ModButton];

                l_Button.RTransform.SetAsLastSibling();
                l_Button.SetText(l_ModButton.Text);
                l_Button.SetTooltip(l_ModButton.Tooltip);
                l_Button.SetInteractable(l_ModButton.Interactable);
            }
        }
    }
}
