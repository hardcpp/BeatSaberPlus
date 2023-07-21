using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using UnityEngine;

namespace CP_SDK.UI.Modals
{
    /// <summary>
    /// Color picker modal
    /// </summary>
    public sealed class ColorPicker : IModal
    {
        private XUISlider       m_H;
        private XUISlider       m_S;
        private XUISlider       m_V;
        private XUISlider       m_O;
        private XUIText         m_HLabel;
        private XUIText         m_SLabel;
        private XUIText         m_VLabel;
        private XUIText         m_OLabel;
        private XUIImage        m_Image;
        private Action<Color>   m_Callback;
        private Action          m_CancelCallback;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On modal show
        /// </summary>
        public override void OnShow()
        {
            if (m_H != null)
                return;

            Templates.ModalRectLayout(
                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("H"),
                        XUIText.Make("S"),
                        XUIText.Make("V"),
                        XUIText.Make("O")
                    )
                    .SetPadding(0)
                    .SetSpacing(1)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.Center)),

                    XUIVLayout.Make(
                        XUISlider.Make().SwitchToColorMode(true,  false, false, false).Bind(ref m_H),
                        XUISlider.Make().SwitchToColorMode(false , true, false, false).Bind(ref m_S),
                        XUISlider.Make().SwitchToColorMode(false, false,  true, false).Bind(ref m_V),
                        XUISlider.Make().SwitchToColorMode(false, false, false,  true).Bind(ref m_O)
                    )
                    .SetPadding(0)
                    .SetSpacing(1)
                    .SetWidth(40f)
                    .ForEachDirect<XUISlider>(x => x.SetFormatter(ValueFormatters.Percentage))
                    .ForEachDirect<XUISlider>(x => x.OnValueChanged(_ => OnColorChanged())),

                    XUIVLayout.Make(
                        XUIText.Make(string.Empty).Bind(ref m_HLabel),
                        XUIText.Make(string.Empty).Bind(ref m_SLabel),
                        XUIText.Make(string.Empty).Bind(ref m_VLabel),
                        XUIText.Make(string.Empty).Bind(ref m_OLabel)
                    )
                    .SetPadding(0)
                    .SetSpacing(1)
                    .SetWidth(10f)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.CaplineRight))
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true),

                    XUIVLayout.Make(
                        XUIImage.Make()
                            .SetWidth(20 + 3).SetHeight(20 + 3)
                            .SetType(UnityEngine.UI.Image.Type.Sliced)
                            .SetSprite(UISystem.GetUIRoundBGSprite())
                            .Bind(ref m_Image)
                    )
                    .SetPadding(0)
                    .SetSpacing(1)
                ),

                XUIHLayout.Make(
                    XUISecondaryButton.Make("Cancel", OnCancelButton).SetWidth(30f),
                    XUIPrimaryButton.Make("Apply" , OnApplyButton)   .SetWidth(30f)
                )
                .SetPadding(0)
            )
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
        /// <param name="p_Value">Initial value</param>
        /// <param name="p_Opacity">If opacity supported?</param>
        /// <param name="p_Callback">Callback</param>
        /// <param name="p_CancelCallback">On cancel callback</param>
        public void Init(Color p_Value, bool p_Opacity, Action<Color> p_Callback, Action p_CancelCallback)
        {
            m_Callback          = null;
            m_CancelCallback    = null;

            Color.RGBToHSV(p_Value, out var l_H, out var l_S, out var l_V);

            m_H.SetValue(l_H, false);
            m_S.SetValue(l_S, false);
            m_V.SetValue(l_V, false);
            m_O.SetValue(p_Opacity ? p_Value.a : 1.0f, false);
            m_O.SetInteractable(p_Opacity);

            OnColorChanged();

            m_Callback          = p_Callback;
            m_CancelCallback    = p_CancelCallback;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On color changed
        /// </summary>
        private void OnColorChanged()
        {
            var l_Color = ColorU.WithAlpha(Color.HSVToRGB(m_H.Element.GetValue(), m_S.Element.GetValue(), m_V.Element.GetValue()), m_O.Element.GetValue());

            m_Image.SetColor(l_Color);
            m_S.ColorModeSetHue(m_H.Element.GetValue());
            m_V.ColorModeSetHue(m_H.Element.GetValue());

            m_HLabel.SetText(ValueFormatters.Percentage(m_H.Element.GetValue()));
            m_SLabel.SetText(ValueFormatters.Percentage(m_S.Element.GetValue()));
            m_VLabel.SetText(ValueFormatters.Percentage(m_V.Element.GetValue()));
            m_OLabel.SetText(ValueFormatters.Percentage(m_O.Element.GetValue()));

            m_Callback?.Invoke(l_Color);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On cancel button
        /// </summary>
        private void OnCancelButton()
        {
            VController.CloseModal(this);

            try { m_CancelCallback?.Invoke(); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][ColorPicker.OnCancelButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
        /// <summary>
        /// On apply button
        /// </summary>
        private void OnApplyButton()
        {
            VController.CloseModal(this);

            try { m_Callback?.Invoke(m_Image.Element.ImageC.color); }
            catch (System.Exception l_Exception)
            {
                ChatPlexSDK.Logger.Error($"[CP_SDK.UI.Modals][ColorPicker.OnApplyButton] Error:");
                ChatPlexSDK.Logger.Error(l_Exception);
            }
        }
    }
}
