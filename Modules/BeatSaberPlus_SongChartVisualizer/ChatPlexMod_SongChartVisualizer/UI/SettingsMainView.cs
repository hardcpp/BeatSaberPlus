using CP_SDK.XUI;

namespace ChatPlexMod_SongChartVisualizer.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUIToggle m_AlignWithFloor;
        private XUIToggle m_ShowLockIcon;
        private XUIToggle m_ShowNPSLegend;
        private XUIToggle m_FollowEnvironementRotations;

        private XUIColorInput m_BackgroundColor;
        private XUIColorInput m_CursorColor;
        private XUIColorInput m_LineColor;
        private XUIColorInput m_LegendColor;
        private XUIColorInput m_DashColor;

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
            var l_Config = SCVConfig.Instance;

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Song Chart Visualizer | Settings"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Align with floor on move"),
                        XUIToggle.Make().SetValue(l_Config.AlignWithFloor).Bind(ref m_AlignWithFloor),

                        XUIText.Make("Show lock icon for movement"),
                        XUIToggle.Make().SetValue(l_Config.ShowLockIcon).Bind(ref m_ShowLockIcon)
                    )
                    .SetSpacing(1)
                    .SetWidth(40.0f)
                    .OnReady(x => x.HOrVLayoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Show NPS legend"),
                        XUIToggle.Make().SetValue(l_Config.ShowNPSLegend).Bind(ref m_ShowNPSLegend),

                        XUIText.Make("Follow environment rotations"),
                        XUIToggle.Make().SetValue(l_Config.FollowEnvironementRotation).Bind(ref m_FollowEnvironementRotations)
                    )
                    .SetSpacing(1)
                    .SetWidth(40.0f)
                    .OnReady(x => x.HOrVLayoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Background color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.BackgroundColor)
                            .Bind(ref m_BackgroundColor),

                        XUIText.Make("Position indicator color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.CursorColor)
                            .Bind(ref m_CursorColor),

                        XUIText.Make("Graph line color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.LineColor)
                            .Bind(ref m_LineColor),

                        XUIText.Make("Legend text color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.LegendColor)
                            .Bind(ref m_LegendColor),

                        XUIText.Make("Dash line color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.DashLineColor)
                            .Bind(ref m_DashColor)
                    )
                    .SetSpacing(1)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChanged()))
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
            SongChartVisualizer.Instance.SetPreview(true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            SongChartVisualizer.Instance.SetPreview(false);
            SCVConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        private void OnSettingChanged()
        {
            if (m_PreventChanges)
                return;

            var l_Config = SCVConfig.Instance;

            l_Config.AlignWithFloor             = m_AlignWithFloor.Element.GetValue();
            l_Config.ShowLockIcon               = m_ShowLockIcon.Element.GetValue();
            l_Config.ShowNPSLegend              = m_ShowNPSLegend.Element.GetValue();
            l_Config.FollowEnvironementRotation = m_FollowEnvironementRotations.Element.GetValue();

            SCVConfig.Instance.CursorColor      = m_BackgroundColor.Element.GetValue();
            SCVConfig.Instance.CursorColor      = m_CursorColor.Element.GetValue();
            SCVConfig.Instance.LineColor        = m_LineColor.Element.GetValue();
            SCVConfig.Instance.LegendColor      = m_LegendColor.Element.GetValue();
            SCVConfig.Instance.DashLineColor    = m_DashColor.Element.GetValue();

            m_LegendColor.SetInteractable(l_Config.ShowNPSLegend);
            m_DashColor.SetInteractable(l_Config.ShowNPSLegend);

            SCVConfig.Instance.Save();

            /// Refresh preview
            SongChartVisualizer.Instance.UpdateStyle();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void OnResetButton()
        {
            var l_Config = SCVConfig.Instance;

            m_PreventChanges = true;

            m_AlignWithFloor                .SetValue(l_Config.AlignWithFloor);
            m_ShowLockIcon                  .SetValue(l_Config.ShowLockIcon);
            m_ShowNPSLegend                 .SetValue(l_Config.ShowNPSLegend);
            m_FollowEnvironementRotations   .SetValue(l_Config.FollowEnvironementRotation);

            m_BackgroundColor   .SetValue(SCVConfig.Instance.CursorColor);
            m_CursorColor       .SetValue(SCVConfig.Instance.CursorColor);
            m_LineColor         .SetValue(SCVConfig.Instance.LineColor);
            m_LegendColor       .SetValue(SCVConfig.Instance.LegendColor);
            m_DashColor         .SetValue(SCVConfig.Instance.DashLineColor);

            m_PreventChanges = false;

            SongChartVisualizer.Instance.UpdateStyle();
        }
    }
}
