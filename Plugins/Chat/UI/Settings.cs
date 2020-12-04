using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus.Plugins.Chat.UI
{
    /// <summary>
    /// Stream chat settings view
    /// </summary>
    internal class Settings : BSMLResourceViewController
    {
        /// <summary>
        /// BSML file name
        /// </summary>
        public override string ResourceName => string.Join(".", GetType().Namespace, GetType().Name);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// BSML parser parameters
        /// </summary>
        [UIParams]
        private BeatSaberMarkupLanguage.Parser.BSMLParserParams m_ParserParams = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("MessageModalText")]
        private TextMeshProUGUI m_MessageModalText = null;
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
        /// On activation
        /// </summary>
        /// <param name="p_FirstActivation">Is the first activation ?</param>
        /// <param name="p_AddedToHierarchy">Activation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidActivate(bool p_FirstActivation, bool p_AddedToHierarchy, bool p_ScreenSystemEnabling)
        {
            /// Forward event
            base.DidActivate(p_FirstActivation, p_AddedToHierarchy, p_ScreenSystemEnabling);

            /// If first activation, bind event
            if(p_FirstActivation)
            {
                var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic));

                /// Left
                BeatSaberPlus.Utils.GameUI.PrepareIncrementSetting(m_ChatWidth,         l_Event, null,                                              Config.Chat.ChatWidth,          true);
                BeatSaberPlus.Utils.GameUI.PrepareIncrementSetting(m_ChatHeight,        l_Event, null,                                              Config.Chat.ChatHeight,         true);
                BeatSaberPlus.Utils.GameUI.PrepareToggleSetting(m_ChatReverse,          l_Event,                                                    Config.Chat.ReverseChatOrder,   true);
                BeatSaberPlus.Utils.GameUI.PrepareIncrementSetting(m_ChatOpacity,       l_Event, BeatSaberPlus.Utils.GameUI.Formatter_Percentage,   Config.Chat.BackgroundA,        true);
                BeatSaberPlus.Utils.GameUI.PrepareIncrementSetting(m_ChatFontSize,      l_Event, null,                                              Config.Chat.FontSize,           true);

                /// Right
                BeatSaberPlus.Utils.GameUI.PrepareColorSetting(m_ChatBackgroundColor,   l_Event,        Config.Chat.BackgroundColor,    true);
                BeatSaberPlus.Utils.GameUI.PrepareColorSetting(m_ChatHighlightColor,    l_Event,        Config.Chat.HighlightColor,     true);
                BeatSaberPlus.Utils.GameUI.PrepareColorSetting(m_ChatAccentColor,       l_Event,        Config.Chat.AccentColor,        true);
                BeatSaberPlus.Utils.GameUI.PrepareColorSetting(m_ChatTextColor,         l_Event,        Config.Chat.TextColor,          true);
                BeatSaberPlus.Utils.GameUI.PrepareColorSetting(m_ChatPingColor,         l_Event,        Config.Chat.PingColor,          true);
            }
        }
        /// <summary>
        /// On deactivate
        /// </summary>
        /// <param name="p_RemovedFromHierarchy">Desactivation type</param>
        /// <param name="p_ScreenSystemEnabling">Is screen system enabled</param>
        protected override void DidDeactivate(bool p_RemovedFromHierarchy, bool p_ScreenSystemDisabling)
        {
            /// Forward event
            base.DidDeactivate(p_RemovedFromHierarchy, p_ScreenSystemDisabling);

            /// Close modals
            m_ParserParams.EmitEvent("CloseAllModals");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show message modal
        /// </summary>
        private void ShowMessageModal(string p_Message)
        {
            HideMessageModal();

            m_MessageModalText.text = p_Message;

            m_ParserParams.EmitEvent("ShowMessageModal");
        }
        /// <summary>
        /// Hide the message modal
        /// </summary>
        private void HideMessageModal()
        {
            m_ParserParams.EmitEvent("CloseMessageModal");
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
            Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.Utils.Game.ActiveScene, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Refresh settings
        /// </summary>
        private void RefreshSettings()
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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        [UIAction("click-btn-reset")]
        private void OnResetButton()
        {
            /// Reset settings
            Config.Chat.Reset();

            /// Update floating view
            Chat.Instance.SettingsRightView.OnResetButton();
            Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.Utils.Game.ActiveScene, true);

            /// Refresh values
            RefreshSettings();

            /// Close modal
            m_ParserParams.EmitEvent("CloseAllModals");
        }
        /// <summary>
        /// Reset position settings
        /// </summary>
        [UIAction("click-btn-reset-position")]
        private void OnResetPositionButton()
        {
            /// Reset position settings
            Config.Chat.ResetPosition();

            /// Update floating view
            Chat.Instance.SettingsRightView.OnResetButton();
            Chat.Instance.UpdateFloatingWindow(BeatSaberPlus.Utils.Game.ActiveScene, true);

            /// Refresh values
            RefreshSettings();

            /// Close modal
            m_ParserParams.EmitEvent("CloseAllModals");
        }
        /// <summary>
        /// Open web configuration tool
        /// </summary>
        [UIAction("click-open-web-configuration-tool-btn-pressed")]
        private void OnOpenWebConfigurationToolButton()
        {
            ShowMessageModal("We did open the configuration tool in your desktop browser!");
            BeatSaberPlus.Utils.ChatService.OpenWebConfigurator();
        }
    }
}
