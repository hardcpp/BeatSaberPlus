using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Stream chat settings view
    /// </summary>
    internal class Settings : SDK.UI.ResourceViewController<Settings>
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
            SDK.UI.IncrementSetting.Setup(m_ChatWidth,         l_Event, null,                                   Config.Chat.ChatWidth,          true);
            SDK.UI.IncrementSetting.Setup(m_ChatHeight,        l_Event, null,                                   Config.Chat.ChatHeight,         true);
            SDK.UI.ToggleSetting.Setup(m_ChatReverse,          l_Event,                                         Config.Chat.ReverseChatOrder,   true);
            SDK.UI.IncrementSetting.Setup(m_ChatOpacity,       l_Event, SDK.UI.BSMLSettingFormartter.Percentage, Config.Chat.BackgroundA,        true);
            SDK.UI.IncrementSetting.Setup(m_ChatFontSize,      l_Event, null,                                   Config.Chat.FontSize,           true);

            /// Right
            SDK.UI.ColorSetting.Setup(m_ChatBackgroundColor,   l_Event,        Config.Chat.BackgroundColor,    true);
            SDK.UI.ColorSetting.Setup(m_ChatHighlightColor,    l_Event,        Config.Chat.HighlightColor,     true);
            SDK.UI.ColorSetting.Setup(m_ChatAccentColor,       l_Event,        Config.Chat.AccentColor,        true);
            SDK.UI.ColorSetting.Setup(m_ChatTextColor,         l_Event,        Config.Chat.TextColor,          true);
            SDK.UI.ColorSetting.Setup(m_ChatPingColor,         l_Event,        Config.Chat.PingColor,          true);
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
            Config.Chat.ChatWidth        = (int)m_ChatWidth.Value;
            Config.Chat.ChatHeight       = (int)m_ChatHeight.Value;
            Config.Chat.ReverseChatOrder = m_ChatReverse.Value;
            Config.Chat.FontSize         = m_ChatFontSize.Value;
            Config.Chat.BackgroundR      = m_ChatBackgroundColor.CurrentColor.r;
            Config.Chat.BackgroundG      = m_ChatBackgroundColor.CurrentColor.g;
            Config.Chat.BackgroundB      = m_ChatBackgroundColor.CurrentColor.b;
            Config.Chat.BackgroundA      = m_ChatOpacity.Value;
            Config.Chat.HighlightR       = m_ChatHighlightColor.CurrentColor.r;
            Config.Chat.HighlightG       = m_ChatHighlightColor.CurrentColor.g;
            Config.Chat.HighlightB       = m_ChatHighlightColor.CurrentColor.b;
            Config.Chat.AccentR          = m_ChatAccentColor.CurrentColor.r;
            Config.Chat.AccentG          = m_ChatAccentColor.CurrentColor.g;
            Config.Chat.AccentB          = m_ChatAccentColor.CurrentColor.b;
            Config.Chat.TextR            = m_ChatTextColor.CurrentColor.r;
            Config.Chat.TextG            = m_ChatTextColor.CurrentColor.g;
            Config.Chat.TextB            = m_ChatTextColor.CurrentColor.b;
            Config.Chat.PingR            = m_ChatPingColor.CurrentColor.r;
            Config.Chat.PingG            = m_ChatPingColor.CurrentColor.g;
            Config.Chat.PingB            = m_ChatPingColor.CurrentColor.b;

            /// Update floating view
            Chat.Instance.UpdateFloatingWindow(SDK.Game.Logic.ActiveScene, false);
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
            m_ChatWidth.Value       = Config.Chat.ChatWidth;
            m_ChatHeight.Value      = Config.Chat.ChatHeight;
            m_ChatReverse.Value     = Config.Chat.ReverseChatOrder;
            m_ChatOpacity.Value     = Config.Chat.BackgroundA;
            m_ChatFontSize.Value    = Config.Chat.FontSize;

            /// Set values
            m_ChatBackgroundColor.CurrentColor  = new Color(Config.Chat.BackgroundR,    Config.Chat.BackgroundG,    Config.Chat.BackgroundB, 1f);
            m_ChatHighlightColor.CurrentColor   = new Color(Config.Chat.HighlightR,     Config.Chat.HighlightG,     Config.Chat.HighlightB,  1f);
            m_ChatAccentColor.CurrentColor      = new Color(Config.Chat.AccentR,        Config.Chat.AccentG,        Config.Chat.AccentB,     1f);
            m_ChatTextColor.CurrentColor        = new Color(Config.Chat.TextR,          Config.Chat.TextG,          Config.Chat.TextB,       1f);
            m_ChatPingColor.CurrentColor        = new Color(Config.Chat.PingR,          Config.Chat.PingG,          Config.Chat.PingB,       1f);

            m_PreventChanges = false;
        }
    }
}
