using CP_SDK.XUI;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal sealed class SettingsRightView : CP_SDK.UI.ViewController<SettingsRightView>
    {
        private XUIToggle m_AlignWithFloor;
        private XUIToggle m_ShowLockIcon;
        private XUIToggle m_FollowEnvironementRotations;
        private XUIToggle m_ChatViewerCount;
        private XUIToggle m_ChatFitlerViewers;

        private XUIToggle m_FollowEvents;
        private XUIToggle m_SubscriptionEvents;
        private XUIToggle m_BitsCheering;
        private XUIToggle m_ChannelPoints;
        private XUIToggle m_ChatFilterBroadcaster;

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
            var l_Config = CConfig.Instance;

            Templates.FullRectLayout(
                Templates.TitleBar("Filters"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Align with floor on move"),
                        XUIToggle.Make().SetValue(l_Config.AlignWithFloor).Bind(ref m_AlignWithFloor),
                        XUIText.Make("Show lock icon for movement"),
                        XUIToggle.Make().SetValue(l_Config.ShowLockIcon).Bind(ref m_ShowLockIcon),
                        XUIText.Make("Follow environment rotations"),
                        XUIToggle.Make().SetValue(l_Config.FollowEnvironementRotation).Bind(ref m_FollowEnvironementRotations),
                        XUIText.Make("Show viewer count"),
                        XUIToggle.Make().SetValue(l_Config.ShowViewerCount).Bind(ref m_ChatViewerCount),
                        XUIText.Make("Filter viewers commands"),
                        XUIToggle.Make().SetValue(l_Config.FilterViewersCommands).Bind(ref m_ChatFitlerViewers)
                    )
                    .SetSpacing(1)
                    .SetPadding(2)
                    .SetWidth(50)
                    .ForEachDirect<XUIText>(  x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Show follow events"),
                        XUIToggle.Make().SetValue(l_Config.ShowFollowEvents).Bind(ref m_FollowEvents),
                        XUIText.Make("Show subscription events"),
                        XUIToggle.Make().SetValue(l_Config.ShowSubscriptionEvents).Bind(ref m_SubscriptionEvents),
                        XUIText.Make("Show bits cheering events"),
                        XUIToggle.Make().SetValue(l_Config.ShowBitsCheeringEvents).Bind(ref m_BitsCheering),
                        XUIText.Make("Show channel points event"),
                        XUIToggle.Make().SetValue(l_Config.ShowChannelPointsEvent).Bind(ref m_ChannelPoints),
                        XUIText.Make("Filter broadcaster commands"),
                        XUIToggle.Make().SetValue(l_Config.FilterBroadcasterCommands).Bind(ref m_ChatFilterBroadcaster)
                    )
                    .SetSpacing(1)
                    .SetPadding(2)
                    .SetWidth(50)
                    .ForEachDirect<XUIText>(  x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
                )
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        private void OnValueChanged()
        {
            if (m_PreventChanges)
                return;

            var l_Config = CConfig.Instance;

            l_Config.AlignWithFloor              = m_AlignWithFloor.Element.GetValue();
            l_Config.ShowLockIcon                = m_ShowLockIcon.Element.GetValue();
            l_Config.FollowEnvironementRotation  = m_FollowEnvironementRotations.Element.GetValue();
            l_Config.ShowViewerCount             = m_ChatViewerCount.Element.GetValue();
            l_Config.FilterViewersCommands       = m_ChatFitlerViewers.Element.GetValue();

            l_Config.ShowFollowEvents            = m_FollowEvents.Element.GetValue();
            l_Config.ShowSubscriptionEvents      = m_SubscriptionEvents.Element.GetValue();
            l_Config.ShowBitsCheeringEvents      = m_BitsCheering.Element.GetValue();
            l_Config.ShowChannelPointsEvent      = m_ChannelPoints.Element.GetValue();
            l_Config.FilterBroadcasterCommands   = m_ChatFilterBroadcaster.Element.GetValue();

            /// Update floating view
            Chat.Instance.UpdateFloatingPanels();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            var l_Config = CConfig.Instance;

            m_AlignWithFloor                .SetValue(l_Config.AlignWithFloor);
            m_ShowLockIcon                  .SetValue(l_Config.ShowLockIcon);
            m_FollowEnvironementRotations   .SetValue(l_Config.FollowEnvironementRotation);
            m_ChatViewerCount               .SetValue(l_Config.ShowViewerCount);
            m_ChatFitlerViewers             .SetValue(l_Config.FilterViewersCommands);

            m_FollowEvents                  .SetValue(l_Config.ShowFollowEvents);
            m_SubscriptionEvents            .SetValue(l_Config.ShowSubscriptionEvents);
            m_BitsCheering                  .SetValue(l_Config.ShowBitsCheeringEvents);
            m_ChannelPoints                 .SetValue(l_Config.ShowChannelPointsEvent);
            m_ChatFilterBroadcaster         .SetValue(l_Config.FilterBroadcasterCommands);

            m_PreventChanges = false;
        }
    }
}
