using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using System.Linq;
using UnityEngine;

namespace BeatSaberPlus.Modules.Chat.UI
{
    /// <summary>
    /// Moderation main screen
    /// </summary>
    internal class ModerationMain : SDK.UI.ResourceViewController<ModerationMain>
    {
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

            var l_Color         = new Color(0.92f, 0.64f, 0);
            var l_ButtonY       = 11f;
            var l_ButtonHeight  = 10;

            KEYBOARD.KEY l_Host         = new KEYBOARD.KEY(m_MessageKeyboard.keyboard, new Vector2(-35.0f, l_ButtonY), "/host",      15, l_ButtonHeight, l_Color);
            KEYBOARD.KEY l_Unban        = new KEYBOARD.KEY(m_MessageKeyboard.keyboard, new Vector2(-27.0f, l_ButtonY), "/unban",     20, l_ButtonHeight, l_Color);
            KEYBOARD.KEY l_UnTimeout    = new KEYBOARD.KEY(m_MessageKeyboard.keyboard, new Vector2(-16.5f, l_ButtonY), "/untimeout", 25, l_ButtonHeight, l_Color);

            l_Host.keyaction        += (_) => ChangePrefix("/host");
            l_Unban.keyaction       += (_) => ChangePrefix("/unban");
            l_UnTimeout.keyaction   += (_) => ChangePrefix("/untimeout");

            m_MessageKeyboard.keyboard.keys.Add(l_Host);
            m_MessageKeyboard.keyboard.keys.Add(l_Unban);
            m_MessageKeyboard.keyboard.keys.Add(l_UnTimeout);
        }
        /// <summary>
        /// On view activation
        /// </summary>
        protected override sealed void OnViewActivation()
        {
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
            if (!m_MessageKeyboard.keyboard.KeyboardText.text.StartsWith("/"))
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
