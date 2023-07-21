using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Views
{
    /// <summary>
    /// Settings main view controller
    /// </summary>
    public sealed class SettingsMainView : ViewController<SettingsMainView>
    {
        /// <summary>
        /// Module setting
        /// </summary>
        private Dictionary<IModuleBase, Components.CToggle> m_ModulesToggles = new Dictionary<IModuleBase, Components.CToggle>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            var l_Modules = ChatPlexSDK.GetModules().Where(x => x.Type == EIModuleBaseType.Integrated);
            var l_Toggles = new List<IXUIElement>();

            foreach (var l_Module in l_Modules)
            {
                l_Toggles.Add(
                    XUIVLayout.Make(
                        XUIHLayout.Make(
                            XUIHLayout.Make(
                                XUIText.Make(l_Module.FancyName)
                                    .SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)
                            )
                            .SetWidth(42.5f)
                            .SetPadding(0)
                            .OnReady(x => x.HLayoutGroup.childForceExpandWidth = true),

                            XUIHLayout.Make(
                                XUIToggle.Make()
                                    .SetValue(l_Module.IsEnabled)
                                    .OnValueChanged((x) => OnModuleToggled(l_Module, x))
                                    .OnReady(x => m_ModulesToggles.Add(l_Module, x))
                            )
                            .SetPadding(0)
                        )
                        .SetPadding(0)
                        .SetSpacing(0),

                        XUIVLayout.Make(
                            XUIText.Make(l_Module.Description)
                                .SetColor(Color.gray)
                                .SetAlign(TMPro.TextAlignmentOptions.CaplineGeoAligned)
                                .SetFontSize(2.8f)
                                .SetOverflowMode(TMPro.TextOverflowModes.Ellipsis),
                            XUIPrimaryButton.Make("Documentation", () => OnDocumentationButton(l_Module))
                                .SetInteractable(!string.IsNullOrEmpty(l_Module.DocumentationURL))
                        )
                        .SetWidth(60f)
                        .SetHeight(10f)
                        .SetPadding(0)
                        .SetSpacing(0)
                        .OnReady(x => {
                            x.VLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                            x.VLayoutGroup.childForceExpandWidth = true;
                        })
                    )
                    .SetWidth(65f)
                    .SetPadding(1)
                    .SetSpacing(0)
                    .SetBackground(true)
                );
            }

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Modules"),

                XUIHLayout.Make(
                    XUIVScrollView.Make(
                        XUIGLayout.Make(
                            l_Toggles.ToArray()
                        )
                        .SetConstraint(GridLayoutGroup.Constraint.FixedColumnCount)
                        .SetConstraintCount(2)
                        .SetCellSize(new Vector2(65f, 18f))
                        .SetSpacing(new Vector2(2f, 0.0f))
                    )
                )
                .SetHeight(65)
                .SetSpacing(0)
                .SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = x.CSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                .OnReady(x => x.HOrVLayoutGroup.childForceExpandHeight = true)
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
            foreach (var l_Current in m_ModulesToggles)
                l_Current.Value.SetValue(l_Current.Key.IsEnabled, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On module toggled
        /// </summary>
        /// <param name="p_Module">Module instance</param>
        /// <param name="p_Enabled">Is enabled</param>
        private void OnModuleToggled(IModuleBase p_Module, bool p_Enabled)
        {
            try
            {
                p_Module.SetEnabled(p_Enabled);
                CheckChatTutorial(p_Module);
            }
            catch (Exception p_InitException)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Views][SettingsMainView.OnModuleToggled] Error on module \"{p_Module.Name}\" init");
                ChatPlexSDK.Logger.Error(p_InitException);
            }
        }
        /// <summary>
        /// On documentation button
        /// </summary>
        /// <param name="p_Module">Module instance</param>
        private void OnDocumentationButton(IModuleBase p_Module)
        {
            ShowMessageModal("URL opened in your web browser.");
            Application.OpenURL(p_Module.DocumentationURL);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Check for chat tutorial
        /// </summary>
        /// <param name="p_Plugin">Plugin instance</param>
        private void CheckChatTutorial(CP_SDK.IModuleBase p_Plugin)
        {
#if DEBUG
            if (p_Plugin.UseChatFeatures && true)
#else
            if (p_Plugin.UseChatFeatures && CPConfig.Instance.FirstChatCoreRun)
#endif
            {
                ShowMessageModal("Hey it's seems that this is the first time\nyou use a chat module!\n<b><color=yellow>The configuration page has been opened in your browser!</color></b>");

                Chat.Service.OpenWebConfiguration();

                CPConfig.Instance.FirstChatCoreRun = false;
                CPConfig.Instance.Save();
            }
        }
    }
}
