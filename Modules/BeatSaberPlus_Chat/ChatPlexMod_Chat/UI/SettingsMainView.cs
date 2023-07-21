using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Settings main view
    /// </summary>
    internal sealed class SettingsMainView : CP_SDK.UI.ViewController<SettingsMainView>
    {
        public  XUISlider       m_ChatWidth;
        public  XUISlider       m_ChatHeight;
        private XUISlider       m_ChatFontSize;
        private XUIToggle       m_ChatReverse;
        private XUIToggle       m_ChatPlatformOriginColor;

        private XUIColorInput   m_ChatBackgroundColor;
        private XUIColorInput   m_ChatHighlightColor;
        private XUIColorInput   m_ChatTextColor;
        private XUIColorInput   m_ChatPingColor;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private bool m_PreventChanges = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            var l_Config = CConfig.Instance;

            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Chat  | Settings"),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Width"),
                        XUISlider.Make().SetMinValue(80.0f).SetMaxValue(300.0f).SetIncrements(1.0f).SetInteger(true).SetValue(l_Config.ChatSize.x).Bind(ref m_ChatWidth),

                        XUIText.Make("Height"),
                        XUISlider.Make().SetMinValue(80.0f).SetMaxValue(300.0f).SetIncrements(1.0f).SetInteger(true).SetValue(l_Config.ChatSize.y).Bind(ref m_ChatHeight),

                        XUIText.Make("Font size"),
                        XUISlider.Make().SetMinValue(1.0f).SetMaxValue(10.0f).SetIncrements(0.1f).SetValue(l_Config.FontSize).Bind(ref m_ChatFontSize),

                        XUIText.Make("Reverse chat order"),
                        XUIToggle.Make().SetValue(l_Config.ReverseChatOrder).Bind(ref m_ChatReverse),

                        XUIText.Make("Show platform origin color"),
                        XUIToggle.Make().SetValue(l_Config.PlatformOriginColor).Bind(ref m_ChatPlatformOriginColor)
                    )
                    .SetSpacing(1)
                    .SetWidth(40.0f)
                    .OnReady(x => x.HOrVLayoutGroup.childAlignment = UnityEngine.TextAnchor.UpperCenter)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged((_) => OnSettingChanged()))
                    .ForEachDirect<XUIToggle>(x => x.OnValueChanged((_) => OnSettingChanged())),

                    XUIVLayout.Make(
                        XUIText.Make("Background color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.BackgroundColor)
                            .Bind(ref m_ChatBackgroundColor),

                        XUIText.Make("Highlight color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.HighlightColor)
                            .Bind(ref m_ChatHighlightColor),

                        XUIText.Make("Text color"),
                        XUIColorInput.Make()
                            .SetValue(l_Config.TextColor)
                            .Bind(ref m_ChatTextColor),

                        XUIText.Make("Ping color"),
                        XUIColorInput.Make()
                            .SetAlphaSupport(true).SetValue(l_Config.PingColor)
                            .Bind(ref m_ChatPingColor)
                    )
                    .SetSpacing(1)
                    .SetWidth(40.0f)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center))
                    .ForEachDirect<XUIColorInput>(x => x.OnValueChanged((_) => OnSettingChanged()))
                )
                .SetSpacing(10f)
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override void OnViewDeactivation()
        {
            CConfig.Instance.Save();
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

            var l_Config = CConfig.Instance;

            /// Update config
            l_Config.ChatSize               = new Vector2((int)m_ChatWidth.Element.GetValue(), (int)m_ChatHeight.Element.GetValue());
            l_Config.FontSize               = m_ChatFontSize.Element.GetValue();
            l_Config.ReverseChatOrder       = m_ChatReverse.Element.GetValue();
            l_Config.PlatformOriginColor    = m_ChatPlatformOriginColor.Element.GetValue();

            l_Config.BackgroundColor  = m_ChatBackgroundColor.Element.GetValue();
            l_Config.HighlightColor   = m_ChatHighlightColor.Element.GetValue();
            l_Config.TextColor        = m_ChatTextColor.Element.GetValue();
            l_Config.PingColor        = m_ChatPingColor.Element.GetValue();

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

            m_ChatWidth              .SetValue(l_Config.ChatSize.x);
            m_ChatHeight             .SetValue(l_Config.ChatSize.y);
            m_ChatReverse            .SetValue(l_Config.ReverseChatOrder);
            m_ChatPlatformOriginColor.SetValue(l_Config.PlatformOriginColor);
            m_ChatFontSize           .SetValue(l_Config.FontSize);

            m_ChatBackgroundColor   .SetValue(l_Config.BackgroundColor);
            m_ChatHighlightColor    .SetValue(l_Config.HighlightColor);
            m_ChatTextColor         .SetValue(l_Config.TextColor);
            m_ChatPingColor         .SetValue(l_Config.PingColor);

            m_PreventChanges = false;
        }
    }
}
