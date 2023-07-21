using CP_SDK.XUI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Keyboard modal
    /// </summary>
    public sealed class Keyboard : IModal
    {
        private XUIText                         m_Text              = null;
        private XUIFLayout                      m_CustomKeyLayout   = null;
        private List<Components.CPrimaryButton> m_CustomKeys        = new List<Components.CPrimaryButton>();
        private List<XUIPrimaryButton>          m_SecondaryButtons  = new List<XUIPrimaryButton>();
        private string                          m_Value             = "";
        private Action<string>                  m_Callback          = null;
        private Action                          m_CancelCallback    = null;
        private bool                            m_IsCaps            = false;
        private bool                            m_IsTempCaps        = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_SecondaryButtons.Count != 0)
                return;

            Templates.ModalRectLayout(
                XUIHLayout.Make(
                    XUIPrimaryButton.Make("Copy",  OnCopyButton),
                    XUIPrimaryButton.Make("Paste", OnPasteButton)
                )
                .SetPadding(0)
                .OnReady((x) => {
                    x.HOrVLayoutGroup.childControlWidth = false;
                    x.HOrVLayoutGroup.childControlHeight = false;
                    x.HOrVLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                    x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }),

                XUIHLayout.Make(
                    XUIText.Make("")
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_Text)
                )
                .SetPadding(2)
                .SetBackground(true, UISystem.KeyboardTextBGColor)
                .OnReady((x) => {
                    x.HOrVLayoutGroup.childForceExpandWidth = true;
                    x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }),

                XUIFLayout.Make()
                    .Bind(ref m_CustomKeyLayout),

                XUIHLayout.Make(
                    XUIVLayout.Make(
                        BuildKeyRow(true,  "/", ".", ",", "!", "?", "@", "#", ":", "[", "]", "$", "=", "_"),
                        BuildKeyRow(false, "TAB", "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "🔙"),
                        BuildKeyRow(false, "CAPS", "a", "s", "d", "f", "g", "h", "j", "k", "l", "<color=green>ENTER"),
                        BuildKeyRow(false, "🔼", "z", "x", "c", "v", "b", "n", "m", "🔼"),
                        BuildKeyRow(false, "<color=red>Clear", " ", "Cancel")
                    )
                    .SetPadding(0).SetSpacing(1.5f)
                    .SetWidth(90.0f),

                    XUIVLayout.Make(
                        BuildKeyRow(true,  "*", "-", "+"),
                        BuildKeyRow(false, "7", "8", "9"),
                        BuildKeyRow(false, "4", "5", "6"),
                        BuildKeyRow(false, "1", "2", "3"),
                        BuildKeyRow(false, "0")
                    )
                    .SetPadding(0).SetSpacing(1.5f)
                    .SetWidth(20.0f)
                )
                .SetPadding(0).SetSpacing(2)
            )
            .SetWidth(115.0f)
            .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
            .BuildUI(transform);
        }
        /// <summary>
        /// On modal close
        /// </summary>
        public override void OnClose()
        {

        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init
        /// </summary>
        /// <param name="p_Value">Value</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public void Init(string p_Value, Action<string> p_Callback, Action p_CancelCallback = null, List<(string, Action, string)> p_CustomKeys = null)
        {
            m_Value             = p_Value;
            m_Callback          = p_Callback;
            m_CancelCallback    = p_CancelCallback;

            m_Text.SetText(p_Value);

            for (var l_I = 0; l_I < m_CustomKeys.Count; ++l_I)
                GameObject.Destroy(m_CustomKeys[l_I].gameObject);

            m_CustomKeys.Clear();

            if (p_CustomKeys != null)
            {
                for (var l_I = 0; l_I < p_CustomKeys.Count; ++l_I)
                {
                    var l_Button = UISystem.PrimaryButtonFactory.Create("CustomKey", m_CustomKeyLayout.RTransform);
                    l_Button.SetText(string.IsNullOrEmpty(p_CustomKeys[l_I].Item3) ? p_CustomKeys[l_I].Item1 : $"<color={p_CustomKeys[l_I].Item3}>{p_CustomKeys[l_I].Item1}");
                    l_Button.OnClick(p_CustomKeys[l_I].Item2);

                    m_CustomKeys.Add(l_Button);
                }
            }

            m_CustomKeyLayout.SetActive(m_CustomKeys.Count != 0);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get current value
        /// </summary>
        /// <returns></returns>
        public string GetValue() => m_Value;
        /// <summary>
        /// Set value
        /// </summary>
        /// <param name="p_Value">New value</param>
        public void SetValue(string p_Value)
        {
            m_Value = p_Value;
            m_Text.SetText(m_Value);
        }
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="p_ToAppend">Value to append</param>
        public void Append(string p_ToAppend)
        {
            m_Value += p_ToAppend;
            m_Text.SetText(m_Value);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On copy button
        /// </summary>
        private void OnCopyButton()
        {
            GUIUtility.systemCopyBuffer = m_Value;
        }
        /// <summary>
        /// On paste button
        /// </summary>
        private void OnPasteButton()
        {
            var l_Buffer = GUIUtility.systemCopyBuffer;
            if (!string.IsNullOrEmpty(l_Buffer))
            {
                m_Value += l_Buffer;
                m_Text.SetText(m_Value);
            }
        }
        /// <summary>
        /// On key press
        /// </summary>
        /// <param name="p_Text">Text</param>
        private void OnKeyPress(string p_Text)
        {
            switch (p_Text)
            {
                case "🔙":
                    if (m_Value.Length > 0) m_Value = m_Value.Substring(0, m_Value.Length - 1);
                    m_Text.SetText(m_Value);
                    break;

                case "TAB":
                    m_Value += "\t";
                    m_Text.SetText(m_Value);
                    break;

                case "CAPS":
                    m_IsTempCaps = false;
                    SwitchCaps();
                    break;

                case "🔼":
                    if (m_IsCaps && !m_IsTempCaps)
                    {
                        SwitchCaps();
                        break;
                    }

                    m_IsTempCaps = !m_IsTempCaps;
                    SwitchCaps();
                    break;

                case "<color=green>ENTER":
                    VController.CloseModal(this);

                    try { m_Callback?.Invoke(m_Value); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Keyboard.OnKeyPress] Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }
                    break;

                case "<color=red>Clear":
                    m_Value = string.Empty;
                    m_Text.SetText(m_Value);
                    break;

                case "Cancel":
                    VController.CloseModal(this);

                    try { m_CancelCallback?.Invoke(); }
                    catch (System.Exception l_Exception)
                    {
                        ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][Keyboard.OnKeyPress] Error:");
                        ChatPlexSDK.Logger.Error(l_Exception);
                    }
                    break;

                default:
                    m_Value += p_Text;
                    m_Text.SetText(m_Value);

                    if (m_IsTempCaps)
                    {
                        m_IsTempCaps = false;
                        SwitchCaps();
                    }
                    break;

            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build a key row
        /// </summary>
        /// <param name="p_ForcePrimary">Force as primary</param>
        /// <param name="p_Keys">Keys to build</param>
        /// <returns></returns>
        private XUIHLayout BuildKeyRow(bool p_ForcePrimary, params string[] p_Keys)
        {
            var l_Keys = new List<IXUIElement>();

            for (var l_I = 0; l_I < p_Keys.Length; ++l_I)
            {
                var l_Text = p_Keys[l_I];
                if (p_ForcePrimary || char.IsHighSurrogate(l_Text[0]) || l_Text.Length > 1)
                {
                    var l_Key = XUISecondaryButton.Make(l_Text);
                    l_Key.SetWidth(6.0f);
                    l_Key.OnClick(() => OnKeyPress(l_Key.Element.GetText()));
                    l_Key.OnReady((x) => {
                        x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        x.LElement.flexibleWidth = 1000.0f;
                    });
                    l_Keys.Add(l_Key);
                }
                else
                {
                    var l_Key = XUIPrimaryButton.Make(l_Text);
                    l_Key.SetWidth(6.0f);
                    l_Key.OnClick(() => OnKeyPress(l_Key.Element.GetText()));

                    if (l_Text == " " || l_Text == "0")
                    {
                        l_Key.OnReady((x) =>
                        {
                            x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                            x.LElement.flexibleWidth = 4000.0f;
                        });
                    }

                    l_Keys.Add(l_Key);
                    m_SecondaryButtons.Add(l_Key);
                }
            }

            return XUIHLayout.Make(l_Keys.ToArray())
                .SetPadding(0).SetSpacing(1.0f)
                .OnReady(x => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
        }
        /// <summary>
        /// Switch caps
        /// </summary>
        private void SwitchCaps()
        {
            m_IsCaps = !m_IsCaps;

            for (var l_I = 0; l_I < m_SecondaryButtons.Count; ++l_I)
            {
                var l_Button = m_SecondaryButtons[l_I];
                var l_Text   = l_Button.Element.GetText();

                l_Button.SetText(m_IsCaps ? l_Text.ToUpper() : l_Text.ToLower());
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On GUI event
        /// </summary>
        private void OnGUI()
        {
            var l_Event = Event.current;
            if (l_Event.isKey && l_Event.type == EventType.KeyDown)
            {
                var l_KeyCode = l_Event.keyCode;

                /// Convert top row keyboard numbers to numpad numbers
                if (l_KeyCode >= KeyCode.Alpha0 && l_KeyCode <= KeyCode.Alpha9)
                    l_KeyCode += 208;

                switch (l_KeyCode)
                {
                    case KeyCode.Backspace:
                        if (m_Value.Length > 0) m_Value = m_Value.Substring(0, m_Value.Length - 1);
                        m_Text.SetText(m_Value);
                        break;

                    default:
                        if (l_Event.character != '\0' && !char.IsControl(l_Event.character))
                        {
                            m_Value += l_Event.character.ToString();
                            m_Text.SetText(m_Value);
                        }
                        break;
                }
            }
        }
    }
}
