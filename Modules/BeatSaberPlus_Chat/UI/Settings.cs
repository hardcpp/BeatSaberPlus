using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Stream chat settings view
    /// </summary>
    internal class Settings : BeatSaberPlus.SDK.UI.ResourceViewController<Settings>
    {
#pragma warning disable CS0649
        [UIComponent("chat-width")]
        public IncrementSetting m_ChatWidth;
        [UIComponent("chat-height")]
        public IncrementSetting m_ChatHeight;
        [UIComponent("chat-reverse")]
        private ToggleSetting m_ChatReverse;
        [UIComponent("chat-opacity")]
        private IncrementSetting m_ChatOpacity;
        [UIComponent("chat-fontsize")]
        private IncrementSetting m_ChatFontSize;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("chat-background")]
        private ColorSetting m_ChatBackgroundColor;
        [UIComponent("chat-highlight")]
        private ColorSetting m_ChatHighlightColor;
        [UIComponent("chat-accent")]
        private ColorSetting m_ChatAccentColor;
        [UIComponent("chat-text")]
        private ColorSetting m_ChatTextColor;
        [UIComponent("chat-ping")]
        private ColorSetting m_ChatPingColor;
#pragma warning restore CS0649

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
            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

            /// Left
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ChatWidth,         l_Event, null,                                                     CConfig.Instance.ChatSize.x,         true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ChatHeight,        l_Event, null,                                                     CConfig.Instance.ChatSize.y,         true);
            BeatSaberPlus.SDK.UI.ToggleSetting.Setup(m_ChatReverse,          l_Event,                                                           CConfig.Instance.ReverseChatOrder,   true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ChatOpacity,       l_Event, BeatSaberPlus.SDK.UI.BSMLSettingFormartter.Percentage,    CConfig.Instance.BackgroundColor.a,  true);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_ChatFontSize,      l_Event, null,                                                     CConfig.Instance.FontSize,           true);

            /// Right
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ChatBackgroundColor,   l_Event,        CConfig.Instance.BackgroundColor,    true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ChatHighlightColor,    l_Event,        CConfig.Instance.HighlightColor,     true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ChatAccentColor,       l_Event,        CConfig.Instance.AccentColor,        true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ChatTextColor,         l_Event,        CConfig.Instance.TextColor,          true);
            BeatSaberPlus.SDK.UI.ColorSetting.Setup(m_ChatPingColor,         l_Event,        CConfig.Instance.PingColor,          true);
        }
        /// <summary>
        /// On view deactivation
        /// </summary>
        protected override sealed void OnViewDeactivation()
        {
            CConfig.Instance.Save();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When settings are changed
        /// </summary>
        /// <param name="p_Value"></param>
        private void OnSettingChanged(object p_Value)
        {
            if (m_PreventChanges)
                return;

            /// Update config
            CConfig.Instance.ChatSize         = new Vector2((int)m_ChatWidth.Value, (int)m_ChatHeight.Value);
            CConfig.Instance.ReverseChatOrder = m_ChatReverse.Value;
            CConfig.Instance.FontSize         = m_ChatFontSize.Value;
            CConfig.Instance.BackgroundColor  = m_ChatBackgroundColor.CurrentColor.ColorWithAlpha(m_ChatOpacity.Value);
            CConfig.Instance.HighlightColor   = m_ChatHighlightColor.CurrentColor;
            CConfig.Instance.AccentColor      = m_ChatAccentColor.CurrentColor;
            CConfig.Instance.TextColor        = m_ChatTextColor.CurrentColor;
            CConfig.Instance.PingColor        = m_ChatPingColor.CurrentColor;

            /// Update floating view
            Chat.Instance.UpdateFloatingWindow(CP_SDK.ChatPlexSDK.ActiveGenericScene, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_PreventChanges = true;

            /// Set values
            m_ChatWidth.Value       = CConfig.Instance.ChatSize.x;
            m_ChatHeight.Value      = CConfig.Instance.ChatSize.y;
            m_ChatReverse.Value     = CConfig.Instance.ReverseChatOrder;
            m_ChatOpacity.Value     = CConfig.Instance.BackgroundColor.a;
            m_ChatFontSize.Value    = CConfig.Instance.FontSize;

            /// Set values
            m_ChatBackgroundColor.CurrentColor  = CConfig.Instance.BackgroundColor.ColorWithAlpha(1f);
            m_ChatHighlightColor.CurrentColor   = CConfig.Instance.HighlightColor.ColorWithAlpha(1f);
            m_ChatAccentColor.CurrentColor      = CConfig.Instance.AccentColor.ColorWithAlpha(1f);
            m_ChatTextColor.CurrentColor        = CConfig.Instance.TextColor.ColorWithAlpha(1f);
            m_ChatPingColor.CurrentColor        = CConfig.Instance.PingColor.ColorWithAlpha(1f);

            m_PreventChanges = false;
        }
    }
}
