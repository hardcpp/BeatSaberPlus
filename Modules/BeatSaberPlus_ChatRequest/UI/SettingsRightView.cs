using CP_SDK.XUI;

namespace BeatSaberPlus_ChatRequest.UI
{
    /// <summary>
    /// Chat request settings filters right screen
    /// </summary>
    internal sealed class SettingsRightView : CP_SDK.UI.ViewController<SettingsRightView>
    {
        private XUIToggle m_NoBeatSageToggle;
        private XUIToggle m_NPSMinToggle;
        private XUIToggle m_NPSMaxToggle;
        private XUIToggle m_NJSMinToggle;
        private XUIToggle m_NJSMaxToggle;
        private XUIToggle m_DurationMaxToggle;
        private XUIToggle m_VoteMinToggle;
        private XUIToggle m_DateMinToggle;
        private XUIToggle m_DateMaxToggle;
        private XUISlider m_NPSMin;
        private XUISlider m_NPSMax;
        private XUISlider m_NJSMin;
        private XUISlider m_NJSMax;
        private XUISlider m_DurationMax;
        private XUISlider m_VoteMin;
        private XUISlider m_DateMin;
        private XUISlider m_DateMax;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Should prevent changes
        /// </summary>
        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Filters"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("No BeatSage"),
                        XUIText.Make("NPS min"),
                        XUIText.Make("NPS max"),
                        XUIText.Make("NJS min"),
                        XUIText.Make("NJS max"),
                        XUIText.Make("Duration max"),
                        XUIText.Make("Vote min"),
                        XUIText.Make("Upload date min"),
                        XUIText.Make("Upload date max")
                    )
                    .SetSpacing(2)
                    .SetPadding(2)
                    .SetWidth(30)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)),

                    XUIVLayout.Make(
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.NoBeatSage ).Bind(ref m_NoBeatSageToggle ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.NPSMin     ).Bind(ref m_NPSMinToggle     ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.NPSMax     ).Bind(ref m_NPSMaxToggle     ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.NJSMin     ).Bind(ref m_NJSMinToggle     ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.NJSMax     ).Bind(ref m_NJSMaxToggle     ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.DurationMax).Bind(ref m_DurationMaxToggle),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.VoteMin    ).Bind(ref m_VoteMinToggle    ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.DateMin    ).Bind(ref m_DateMinToggle    ),
                        XUIToggle.Make().SetValue(CRConfig.Instance.Filters.DateMax    ).Bind(ref m_DateMaxToggle    )
                    )
                    .SetSpacing(2)
                    .SetPadding(2)
                    .SetWidth(20)
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged(_ => OnValueChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Discard all BeatSage maps"),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.NPSMinV)
                            .Bind(ref m_NPSMin),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.NPSMaxV)
                            .Bind(ref m_NPSMax),
                        XUISlider.Make()
                            .SetMinValue(1f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.NJSMinV)
                            .Bind(ref m_NJSMin),
                        XUISlider.Make()
                            .SetMinValue(1f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.NJSMaxV)
                            .Bind(ref m_NJSMax),
                        XUISlider.Make()
                            .SetMinValue(1f).SetMaxValue( 60f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.DurationMaxV)
                            .Bind(ref m_DurationMax),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(  1f).SetIncrements(0.01f)
                            .SetValue(CRConfig.Instance.Filters.VoteMinV)
                            .Bind(ref m_VoteMin),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.DateMinV)
                            .Bind(ref m_DateMin),
                        XUISlider.Make()
                            .SetMinValue(0f).SetMaxValue(100f).SetIncrements(   1f).SetInteger(true)
                            .SetValue(CRConfig.Instance.Filters.DateMaxV)
                            .Bind(ref m_DateMax)
                    )
                    .SetSpacing(2)
                    .SetPadding(2)
                    .SetWidth(65)
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnValueChanged()))
                )
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);

            m_DurationMax.SetFormatter(CP_SDK.UI.ValueFormatters.Minutes);
            m_VoteMin.SetFormatter(CP_SDK.UI.ValueFormatters.Percentage);
            m_DateMin.SetFormatter(CP_SDK.UI.ValueFormatters.DateMonthFrom2018Short);
            m_DateMax.SetFormatter(CP_SDK.UI.ValueFormatters.DateMonthFrom2018Short);

            OnValueChanged();
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

            /// Update interactable
            m_NPSMin.SetInteractable(       m_NPSMinToggle.Element.GetValue());
            m_NPSMax.SetInteractable(       m_NPSMaxToggle.Element.GetValue());
            m_NJSMin.SetInteractable(       m_NJSMinToggle.Element.GetValue());
            m_NJSMax.SetInteractable(       m_NJSMaxToggle.Element.GetValue());
            m_DurationMax.SetInteractable(  m_DurationMaxToggle.Element.GetValue());
            m_VoteMin.SetInteractable(      m_VoteMinToggle.Element.GetValue());
            m_DateMin.SetInteractable(      m_DateMinToggle.Element.GetValue());
            m_DateMax.SetInteractable(      m_DateMaxToggle.Element.GetValue());

            /// Left
            CRConfig.Instance.Filters.NoBeatSage   = m_NoBeatSageToggle.Element.GetValue();
            CRConfig.Instance.Filters.NPSMin       = m_NPSMinToggle.Element.GetValue();
            CRConfig.Instance.Filters.NPSMax       = m_NPSMaxToggle.Element.GetValue();
            CRConfig.Instance.Filters.NJSMin       = m_NJSMinToggle.Element.GetValue();
            CRConfig.Instance.Filters.NJSMax       = m_NJSMaxToggle.Element.GetValue();
            CRConfig.Instance.Filters.DurationMax  = m_DurationMaxToggle.Element.GetValue();
            CRConfig.Instance.Filters.VoteMin      = m_VoteMinToggle.Element.GetValue();
            CRConfig.Instance.Filters.DateMin      = m_DateMinToggle.Element.GetValue();
            CRConfig.Instance.Filters.DateMax      = m_DateMaxToggle.Element.GetValue();

            /// Right
            CRConfig.Instance.Filters.NPSMinV       = (int)m_NPSMin.Element.GetValue();
            CRConfig.Instance.Filters.NPSMaxV       = (int)m_NPSMax.Element.GetValue();
            CRConfig.Instance.Filters.NJSMinV       = (int)m_NJSMin.Element.GetValue();
            CRConfig.Instance.Filters.NJSMaxV       = (int)m_NJSMax.Element.GetValue();
            CRConfig.Instance.Filters.DurationMaxV  = (int)m_DurationMax.Element.GetValue();
            CRConfig.Instance.Filters.VoteMinV      = m_VoteMin.Element.GetValue();
            CRConfig.Instance.Filters.DateMinV      = (int)m_DateMin.Element.GetValue();
            CRConfig.Instance.Filters.DateMaxV      = (int)m_DateMax.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Left
            m_NoBeatSageToggle.SetValue(    CRConfig.Instance.Filters.NoBeatSage);
            m_NPSMinToggle.SetValue(        CRConfig.Instance.Filters.NPSMin);
            m_NPSMaxToggle.SetValue(        CRConfig.Instance.Filters.NPSMax);
            m_NJSMinToggle.SetValue(        CRConfig.Instance.Filters.NJSMin);
            m_NJSMaxToggle.SetValue(        CRConfig.Instance.Filters.NJSMax);
            m_DurationMaxToggle.SetValue(   CRConfig.Instance.Filters.DurationMax);
            m_VoteMinToggle.SetValue(       CRConfig.Instance.Filters.VoteMin);
            m_DateMinToggle.SetValue(       CRConfig.Instance.Filters.DateMin);
            m_DateMaxToggle.SetValue(       CRConfig.Instance.Filters.DateMax);

            /// Right
            m_NPSMin.SetValue(      CRConfig.Instance.Filters.NPSMinV);
            m_NPSMax.SetValue(      CRConfig.Instance.Filters.NPSMaxV);
            m_NJSMin.SetValue(      CRConfig.Instance.Filters.NJSMinV);
            m_NJSMax.SetValue(      CRConfig.Instance.Filters.NJSMaxV);
            m_DurationMax.SetValue( CRConfig.Instance.Filters.DurationMaxV);
            m_VoteMin.SetValue(     CRConfig.Instance.Filters.VoteMinV);
            m_DateMin.SetValue(     CRConfig.Instance.Filters.DateMinV);
            m_DateMax.SetValue(     CRConfig.Instance.Filters.DateMaxV);

            m_PreventChanges = false;

            OnValueChanged();
        }
    }
}
