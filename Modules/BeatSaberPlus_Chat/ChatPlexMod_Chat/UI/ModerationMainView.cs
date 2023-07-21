using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatPlexMod_Chat.UI
{
    /// <summary>
    /// Moderation main screen
    /// </summary>
    internal sealed class ModerationMainView : CP_SDK.UI.ViewController<ModerationMainView>
    {
        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            Templates.FullRectLayoutMainView(
                Templates.TitleBar("Send message")
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override void OnViewActivation()
        {
            ShowKeyboard();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show keyboard
        /// </summary>
        private void ShowKeyboard()
        {
            var l_CustomKeys = new List<(string, Action, string)>()
            {
                (" USERNAME ", () => {
                    if (ModerationRightView.Instance?.SelectedItem == null)
                        ShowMessageModal("Please select an user on the right panel!");
                    else
                        InsertTextWithSpace(ModerationRightView.Instance.SelectedItem.User.DisplayName);
                }, "#7eaffc")
            };

            /// Add custom keys
            var l_ConfigCustomKeys = CConfig.Instance.ModerationKeys;
            if (l_ConfigCustomKeys != null && l_ConfigCustomKeys.Count != 0)
            {
                foreach (var l_Var in l_ConfigCustomKeys)
                    l_CustomKeys.Add((" " + l_Var + " ", () => ChangePrefix(l_Var), null));
            }

            ShowKeyboardModal("", SendPressed, ShowKeyboard, l_CustomKeys);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change message prefix
        /// </summary>
        /// <param name="p_Prefix">New prefix</param>
        private void ChangePrefix(string p_Prefix)
        {
            var l_KeyboardValue = KeyboardModal_GetValue();
            if (p_Prefix.StartsWith("!") || p_Prefix.StartsWith("/"))
            {
                if (!l_KeyboardValue.StartsWith("/") && !l_KeyboardValue.StartsWith("!"))
                    KeyboardModal_SetValue(p_Prefix + " " + l_KeyboardValue);
                else
                {
                    if (l_KeyboardValue.Contains(' '))
                    {
                        var l_Parts = l_KeyboardValue.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        KeyboardModal_SetValue(p_Prefix + " " + string.Join(" ", l_Parts.Skip(1).ToArray()));
                    }
                    else
                        KeyboardModal_Append(p_Prefix + " ");
                }
            }
            else
                InsertTextWithSpace(p_Prefix);
        }
        /// <summary>
        /// Insert text with space
        /// </summary>
        /// <param name="p_Text">Text to insert</param>
        private void InsertTextWithSpace(string p_Text)
        {
            var l_KeyboardValue = KeyboardModal_GetValue();

            if (l_KeyboardValue.Length == 0)                                KeyboardModal_Append(p_Text);
            else if (l_KeyboardValue[l_KeyboardValue.Length - 1] != ' ')    KeyboardModal_Append(" " + p_Text);
            else                                                            KeyboardModal_Append(p_Text);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        internal void SendPressed(string p_Text)
        {
            ShowKeyboard();

            if (CP_SDK.Chat.Service.Multiplexer.Channels.Count == 0)
                return;

            foreach (var l_Channel in CP_SDK.Chat.Service.Multiplexer.Channels)
                l_Channel.Item1.SendTextMessage(l_Channel.Item2, p_Text);
        }
    }
}
