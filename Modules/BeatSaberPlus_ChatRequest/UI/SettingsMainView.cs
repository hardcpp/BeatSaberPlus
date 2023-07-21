using CP_SDK.XUI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        private XUISlider m_UserRequest;
        private XUISlider m_VIPBonusRequest;
        private XUISlider m_SubscriberBonusRequest;
        private XUISlider m_HistorySize;

        private XUIToggle m_PlayPreviewMusic;
        private XUIToggle m_ModeratorPower;
        private XUISlider m_QueueSize;
        private XUISlider m_QueueCooldown;

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
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat Request - Settings"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("User max request"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.UserMaxRequest)
                            .Bind(ref m_UserRequest),

                        XUIText.Make("VIP bonus request"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.VIPBonusRequest)
                            .Bind(ref m_VIPBonusRequest),

                        XUIText.Make("Subscriber bonus request"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(20f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.SubscriberBonusRequest)
                            .Bind(ref m_SubscriberBonusRequest),

                        XUIText.Make("History size"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(50f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.HistorySize)
                            .Bind(ref m_HistorySize)

                    )
                    .SetSpacing(2)
                    .SetPadding(2)
                    .SetWidth(60)
                    .ForEachDirect<XUIText>(  x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Play preview music if downloaded"),
                        XUIToggle.Make()
                            .SetValue(CRConfig.Instance.PlayPreviewMusic)
                            .Bind(ref m_PlayPreviewMusic),

                        XUIText.Make("Give moderators power to manage queue"),
                        XUIToggle.Make()
                            .SetValue(CRConfig.Instance.ModeratorPower)
                            .Bind(ref m_ModeratorPower),

                        XUIText.Make("Queue command show count"),
                        XUISlider.Make()
                            .SetMinValue(1f).SetMaxValue(10f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.QueueCommandShowSize)
                            .Bind(ref m_QueueSize),

                        XUIText.Make("Queue command cooldown seconds"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(60f).SetIncrements(1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.QueueCommandCooldown)
                            .Bind(ref m_QueueCooldown)
                    )
                    .SetSpacing(2)
                    .SetPadding(2)
                    .SetWidth(60)
                    .ForEachDirect<XUIText>(  x => x.SetAlign(TMPro.TextAlignmentOptions.Midline))
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged()))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged()))
                )
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained),

                XUIVSpacer.Make(5f)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            CRConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnValueChanged()
        {
            if (m_PreventChanges)
                return;

            /// Left
            CRConfig.Instance.UserMaxRequest           = (int)m_UserRequest.Element.GetValue();
            CRConfig.Instance.VIPBonusRequest          = (int)m_VIPBonusRequest.Element.GetValue();
            CRConfig.Instance.SubscriberBonusRequest   = (int)m_SubscriberBonusRequest.Element.GetValue();
            CRConfig.Instance.HistorySize              = (int)m_HistorySize.Element.GetValue();

            /// Right
            CRConfig.Instance.PlayPreviewMusic         = m_PlayPreviewMusic.Element.GetValue();
            CRConfig.Instance.ModeratorPower           = m_ModeratorPower.Element.GetValue();
            CRConfig.Instance.QueueCommandShowSize     = (int)m_QueueSize.Element.GetValue();
            CRConfig.Instance.QueueCommandCooldown     = (int)m_QueueCooldown.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            m_UserRequest           .SetValue(CRConfig.Instance.UserMaxRequest);
            m_VIPBonusRequest       .SetValue(CRConfig.Instance.VIPBonusRequest);
            m_SubscriberBonusRequest.SetValue(CRConfig.Instance.SubscriberBonusRequest);
            m_HistorySize           .SetValue(CRConfig.Instance.HistorySize);

            m_PlayPreviewMusic      .SetValue(CRConfig.Instance.PlayPreviewMusic);
            m_ModeratorPower        .SetValue(CRConfig.Instance.ModeratorPower);
            m_QueueSize             .SetValue(CRConfig.Instance.QueueCommandShowSize);
            m_QueueCooldown         .SetValue(CRConfig.Instance.QueueCommandCooldown);

            m_PreventChanges = false;
        }
    }
}
