using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Moderation main screen
    /// </summary>
    internal class ModerationMain : SDK.UI.ResourceViewController<ModerationMain>
    {
        /// <summary>
        /// Keyboard original key count
        /// </summary>
        private int m_InputKeyboardInitialKeyCount = -1;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

#pragma warning disable CS0414
        [UIComponent("MessageKeyboard")]
        private ModalKeyboard m_MessageKeyboard = null;
        [UIValue("MessageContent")]
        private string m_MessageContent = "";
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override void OnViewCreation()
        {
            /// Update opacity
            SDK.UI.ModalView.SetOpacity(m_MessageKeyboard.modalView, 0.75f);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
            ShowKeyboard();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void ShowKeyboard()
        {
            /// Clear old keys
            if (m_InputKeyboardInitialKeyCount == -1)
                m_InputKeyboardInitialKeyCount = m_MessageKeyboard.keyboard.keys.Count;

            while (m_MessageKeyboard.keyboard.keys.Count > m_InputKeyboardInitialKeyCount)
            {
                var l_Key = m_MessageKeyboard.keyboard.keys.ElementAt(m_MessageKeyboard.keyboard.keys.Count - 1);
                m_MessageKeyboard.keyboard.Clear(l_Key);
                m_MessageKeyboard.keyboard.keys.RemoveAt(m_MessageKeyboard.keyboard.keys.Count - 1);

                GameObject.Destroy(l_Key.mybutton.gameObject);
            }

            /// Add custom keys
            var l_CustomKeys = Config.Chat.ModerationKeys.Split(new string[] { Config.Chat.s_ModerationKeyDefault_Split }, System.StringSplitOptions.RemoveEmptyEntries);
            if (l_CustomKeys != null && l_CustomKeys.Length != 0)
            {
                var l_FirstButton   = m_MessageKeyboard.keyboard.BaseButton.GetComponentInChildren<TextMeshProUGUI>();
                var l_Color         = new Color(0.92f, 0.64f, 0);
                var l_ButtonY       = 11f;
                var l_Margin        = 1f;
                var l_TotalLeft     = -35.0f;

                var l_I = 0;
                foreach (var l_Var in l_CustomKeys)
                {
                    var l_Position  = new Vector2(l_TotalLeft, l_ButtonY);
                    var l_Width     = l_FirstButton.GetPreferredValues(" " + l_Var + " ").x * 2.0f;
                    var l_Key       = new KEYBOARD.KEY(m_MessageKeyboard.keyboard, l_Position, " " + l_Var + " ", l_Width, 10f, l_Color);

                    l_TotalLeft     += ((l_Width / 2.0f) + l_Margin);
                    l_Key.keyaction += (_) => ChangePrefix(l_Var);

                    m_MessageKeyboard.keyboard.keys.Add(l_Key);
                    ++l_I;
                }
            }

            ShowModal("OpenMessageModal");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Change message prefix
        /// </summary>
        /// <param name="p_Prefix">New prefix</param>
        private void ChangePrefix(string p_Prefix)
        {
            if (p_Prefix.StartsWith("!") || p_Prefix.StartsWith("/"))
            {
                if (!m_MessageKeyboard.keyboard.KeyboardText.text.StartsWith("/") && !m_MessageKeyboard.keyboard.KeyboardText.text.StartsWith("!"))
                    m_MessageKeyboard.keyboard.KeyboardText.text = p_Prefix + " " + m_MessageKeyboard.keyboard.KeyboardText.text;
                else
                {
                    if (m_MessageKeyboard.keyboard.KeyboardText.text.Contains(' '))
                    {
                        var l_Parts = m_MessageKeyboard.keyboard.KeyboardText.text.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                        m_MessageKeyboard.keyboard.KeyboardText.text = p_Prefix + " " + string.Join(" ", l_Parts.Skip(1).ToArray());
                    }
                    else
                        m_MessageKeyboard.keyboard.KeyboardText.text = p_Prefix + " ";
                }
            }
            else
                m_MessageKeyboard.keyboard.KeyboardText.text = m_MessageKeyboard.keyboard.KeyboardText.text + " " + p_Prefix;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("SendPressed")]
        internal void SendPressed(string p_Text)
        {
            if (SDK.Chat.Service.Multiplexer.Channels.Count == 0)
                return;

            var l_Channel = SDK.Chat.Service.Multiplexer.Channels.First();
            l_Channel.Item1.SendTextMessage(l_Channel.Item2, p_Text);

            m_MessageContent = "";
            ShowModal("OpenMessageModal");
        }
    }
}
