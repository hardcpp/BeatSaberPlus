using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    /// <summary>
    /// Settings right view
    /// </summary>
    internal sealed class SettingsRightView : CP_SDK.UI.ViewController<SettingsRightView>
    {
        private XUIToggle m_Enabled;
        private XUISlider m_EmoteCount;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On view creation
        /// </summary>
        protected override sealed void OnViewCreation()
        {
            Templates.FullRectLayout(
                Templates.TitleBar("Subrain"),

                XUIVLayout.Make(
                    XUIText.Make("Enabled")
                        .SetColor(Color.yellow)
                        .SetAlign(TMPro.TextAlignmentOptions.Midline),
                    XUIToggle.Make()
                        .SetValue(CERConfig.Instance.SubRain)
                        .OnValueChanged((x) => CERConfig.Instance.SubRain = x)
                        .Bind(ref m_Enabled),
                    XUIText.Make("Emote count")
                        .SetColor(Color.yellow)
                        .SetAlign(TMPro.TextAlignmentOptions.Midline),
                    XUISlider.Make()
                        .SetMinValue(1).SetMaxValue(100).SetIncrements(1).SetInteger(true)
                        .SetValue(CERConfig.Instance.SubRainEmoteCount)
                        .OnValueChanged((x) => CERConfig.Instance.SubRainEmoteCount = (int)x)
                        .Bind(ref m_EmoteCount)
                )
                .SetWidth(80f)
                .SetSpacing(0)
                .SetBackground(true),

                XUIVLayout.Make(
                    XUIText.Make(
                        "SubRain folder is located at Beat Saber/CustomSubRain" +
                        "\nPaste in your favorite PNGs to set as SubRain!" +
                        "\n1:1 ratio recommended"
                    )
                    .SetAlign(TMPro.TextAlignmentOptions.Center)
                )
                .SetSpacing(0)
                .SetBackground(true),

                Templates.ExpandedButtonsLine(
                    XUIPrimaryButton.Make("Reload SubRain textures", OnReloadSubRainButton)
                ),
                Templates.ExpandedButtonsLine(
                    XUISecondaryButton.Make("Test it", OnTestSubRainButton)
                )
            )
            .SetBackground(true, null, true)
            .BuildUI(transform);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reset settings
        /// </summary>
        internal void RefreshSettings()
        {
            m_Enabled.SetValue(CERConfig.Instance.SubRain, false);
            m_EmoteCount.SetValue(CERConfig.Instance.SubRainEmoteCount, false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On reload button pressed
        /// </summary>
        private void OnReloadSubRainButton()
        {
            /// Reload sub rain
            ChatEmoteRain.Instance.LoadSubRainFiles();

            /// Show message
            ShowMessageModal("SubRain textures were reloaded!");
        }
        /// <summary>
        /// On test button pressed
        /// </summary>
        private void OnTestSubRainButton()
        {
            ChatEmoteRain.Instance.StartSubRain();
        }
    }
}
