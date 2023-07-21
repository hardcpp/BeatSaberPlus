using CP_SDK.XUI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Main main view controller
    /// </summary>
    public sealed class MainMainView : ViewController<MainMainView>
    {
        /// <summary>
        /// Module buttons
        /// </summary>
        private Dictionary<IModuleBase, Components.CPOrSButton> m_ModulesButton = new Dictionary<IModuleBase, Components.CPOrSButton>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Modules = ChatPlexSDK.GetModules().Where(x => x.Type == EIModuleBaseType.Integrated);
            var l_Buttons = new List<IXUIElement>();
            foreach (var l_Module in l_Modules)
            {
                l_Buttons.Add(
                    XUISecondaryButton.Make(l_Module.FancyName, () =>
                    {
                        var l_Items = l_Module.GetSettingsViewControllers();
                        FlowCoordinators.MainFlowCoordinator.Instance().ChangeViewControllers(l_Items.Item1, l_Items.Item2, l_Items.Item3);
                    })
                    .SetFontSize(3.44f)
                    .SetWidth(40f)
                    .SetHeight(7f)
                    .SetTooltip(l_Module.Description)
                    .OnReady(x => m_ModulesButton.Add(l_Module, x))
                );
            }

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Modules"),

                XUIGLayout.Make(
                    l_Buttons.ToArray()
                )
                .SetCellSize(new Vector2(40, 9))
                .SetChildAlign(TextAnchor.UpperCenter)
                .SetConstraint(GridLayoutGroup.Constraint.FixedColumnCount)
                .SetConstraintCount(3)
                .SetSpacing(new Vector2(2, 0))
                .SetWidth(124)
                .SetHeight(55),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Settings", OnSettingsPressed)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            /// Refresh button states
            foreach (var l_Current in m_ModulesButton)
                l_Current.Value.SetInteractable(l_Current.Key.IsEnabled);

            /// Show welcome message
#if DEBUG
            if (true)
#else
            if (CPConfig.Instance.FirstRun)
#endif
            {
                ShowMessageModal($"<color=yellow><b>Welcome to {ChatPlexSDK.ProductName}!</b></color>\nBy default most modules are disabled, you can enable/disable them\nany time by clicking the <b>Settings</b> button below");
                CPConfig.Instance.FirstRun = false;
                CPConfig.Instance.Save();
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Go to settings
        /// </summary>
        private void OnSettingsPressed()
        {
            FlowCoordinators.MainFlowCoordinator.Instance().SwitchToSettingsView();
        }
    }
}
