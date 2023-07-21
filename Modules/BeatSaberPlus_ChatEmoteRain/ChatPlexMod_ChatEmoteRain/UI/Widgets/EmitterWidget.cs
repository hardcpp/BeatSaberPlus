using CP_SDK.Unity.Extensions;
using CP_SDK.XUI;
using System;
using TMPro;
using UnityEngine;

namespace ChatPlexMod_ChatEmoteRain.UI.Widgets
{
    /// <summary>
    /// Emitter widget
    /// </summary>
    internal sealed class EmitterWidget : MonoBehaviour
    {
        private XUIVLayout m_NoneFrame = null;
        private XUIVLayout m_EditFrame = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private XUITextInput    m_Name  = null;
        private XUISlider       m_Speed = null;
        private XUISlider       m_Size  = null;

        private XUISlider       m_PosX = null;
        private XUISlider       m_PosY = null;
        private XUISlider       m_PosZ = null;

        private XUISlider       m_RotX = null;
        private XUISlider       m_RotY = null;
        private XUISlider       m_RotZ = null;

        private XUISlider       m_ScaX = null;
        private XUISlider       m_ScaY = null;
        private XUISlider       m_ScaZ = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private SettingsMainView.EmitterConfigListItem m_Current = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On component creation
        /// </summary>
        private void Awake()
        {
            Action<CP_SDK.UI.Components.CText> l_ControlsTextStyle = (x) => x.SetStyle(FontStyles.Bold).SetColor(Color.yellow);

            XUIVLayout.Make(
                XUIText.Make("Please select an emitter in the list!").OnReady(l_ControlsTextStyle)
            )
            .SetSpacing(1f).SetPadding(0, 2, 0, 2)
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            .Bind(ref m_NoneFrame)
            .BuildUI(transform);

            m_NoneFrame.SetActive(true);

            XUIVLayout.Make(
                XUIHLayout.Make(
                    XUIVLayout.Make(
                        XUIText.Make("Name:").OnReady(l_ControlsTextStyle),
                        XUIText.Make("Speed:").OnReady(l_ControlsTextStyle),
                        XUIText.Make("Size:").OnReady(l_ControlsTextStyle)
                    )
                    .SetMinWidth(15f).SetWidth(15f)
                    .OnReady(x => x.VLayoutGroup.childAlignment = TextAnchor.MiddleLeft),

                    XUIVLayout.Make(
                        XUITextInput.Make("Name...").OnValueChanged((_) => OnSettingChanged()).Bind(ref m_Name),
                        XUISlider.Make().SetMinValue(0.1f).SetMaxValue(5.0f).SetIncrements(0.01f).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_Speed),
                        XUISlider.Make().SetMinValue(0.1f).SetMaxValue(5.0f).SetIncrements(0.01f).OnValueChanged((_) => OnSettingChanged()).Bind(ref m_Size)
                    )
                    .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                )
                .SetSpacing(0).SetPadding(0)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                .ForEachDirect<XUIVLayout>(x => x.SetSpacing(0)),

                XUIVLayout.Make(
                    XUIText.Make("Position X Y Z").SetAlign(TextAlignmentOptions.CaplineLeft).OnReady(l_ControlsTextStyle),

                    XUIHLayout.Make(
                        XUISlider.Make().SetMinValue(-20.0f).SetMaxValue(20.0f).SetIncrements(0.01f).Bind(ref m_PosX),
                        XUISlider.Make().SetMinValue(-20.0f).SetMaxValue(20.0f).SetIncrements(0.01f).Bind(ref m_PosY),
                        XUISlider.Make().SetMinValue(-20.0f).SetMaxValue(20.0f).SetIncrements(0.01f).Bind(ref m_PosZ)
                    )
                    .SetPadding(0)
                    .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                    .ForEachDirect<XUISlider>(x => x.SetColor(ColorU.ToUnityColor("#c869ff")).OnValueChanged((_) => OnSettingChanged()))
                )
                .SetSpacing(0).SetPadding(1)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained),

                XUIVLayout.Make(
                    XUIText.Make("Rotation X Y Z").SetAlign(TextAlignmentOptions.CaplineLeft).OnReady(l_ControlsTextStyle),

                    XUIHLayout.Make(
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(360.0f).SetIncrements(1.0f).SetInteger(true).Bind(ref m_RotX),
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(360.0f).SetIncrements(1.0f).SetInteger(true).Bind(ref m_RotY),
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(360.0f).SetIncrements(1.0f).SetInteger(true).Bind(ref m_RotZ)
                    )
                    .SetPadding(0)
                    .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                    .ForEachDirect<XUISlider>(x => x.SetColor(ColorU.ToUnityColor("#3d9eff")).OnValueChanged((_) => OnSettingChanged()))
                )
                .SetSpacing(0).SetPadding(1)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained),

                XUIVLayout.Make(
                    XUIText.Make("Scale X Y Z").SetAlign(TextAlignmentOptions.CaplineLeft).OnReady(l_ControlsTextStyle),

                    XUIHLayout.Make(
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(30.0f).SetIncrements(0.01f).Bind(ref m_ScaX),
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(30.0f).SetIncrements(0.01f).Bind(ref m_ScaY),
                        XUISlider.Make().SetMinValue(0.0f).SetMaxValue(30.0f).SetIncrements(0.01f).Bind(ref m_ScaZ)
                    )
                    .SetPadding(0)
                    .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
                    .OnReady(x => x.LElement.flexibleWidth = 1000.0f)
                    .ForEachDirect<XUISlider>(x => x.SetColor(ColorU.ToUnityColor("#FF6C11")).OnValueChanged((_) => OnSettingChanged()))
                )
                .SetSpacing(0).SetPadding(1)
                .SetBackground(true)
                .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            )
            .SetSpacing(1f).SetPadding(0, 2, 0, 2)
            .OnReady(x => x.CSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            .Bind(ref m_EditFrame)
            .BuildUI(transform);

            m_EditFrame.SetActive(false);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set current
        /// </summary>
        /// <param name="p_Current"></param>
        internal void SetCurrent(SettingsMainView.EmitterConfigListItem p_Current)
        {
            m_Current = null;

            if (p_Current != null)
            {
                m_Name.SetValue(p_Current.EConfig.Name);
                m_Speed.SetValue(p_Current.EConfig.Speed);
                m_Size.SetValue(p_Current.EConfig.Size);

                m_PosX.SetValue(p_Current.EConfig.PosX);
                m_PosY.SetValue(p_Current.EConfig.PosY);
                m_PosZ.SetValue(p_Current.EConfig.PosZ);
                m_RotX.SetValue(p_Current.EConfig.RotX);
                m_RotY.SetValue(p_Current.EConfig.RotY);
                m_RotZ.SetValue(p_Current.EConfig.RotZ);
                m_ScaX.SetValue(p_Current.EConfig.SizeX);
                m_ScaY.SetValue(p_Current.EConfig.SizeY);
                m_ScaZ.SetValue(p_Current.EConfig.SizeZ);

                m_NoneFrame.SetActive(false);
                m_EditFrame.SetActive(true);
            }
            else
            {
                m_EditFrame.SetActive(false);
                m_NoneFrame.SetActive(true);
            }

            m_Current = p_Current;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When any value change
        /// </summary>
        private void OnSettingChanged()
        {
            if (m_Current == null)
                return;

            m_Current.EConfig.Name   = m_Name.Element.GetValue();
            m_Current.EConfig.Speed  = m_Speed.Element.GetValue();
            m_Current.EConfig.Size   = m_Size.Element.GetValue();

            m_Current.EConfig.PosX   = m_PosX.Element.GetValue();
            m_Current.EConfig.PosY   = m_PosY.Element.GetValue();
            m_Current.EConfig.PosZ   = m_PosZ.Element.GetValue();
            m_Current.EConfig.RotX   = m_RotX.Element.GetValue();
            m_Current.EConfig.RotY   = m_RotY.Element.GetValue();
            m_Current.EConfig.RotZ   = m_RotZ.Element.GetValue();
            m_Current.EConfig.SizeX  = m_ScaX.Element.GetValue();
            m_Current.EConfig.SizeY  = m_ScaY.Element.GetValue();
            m_Current.EConfig.SizeZ  = m_ScaZ.Element.GetValue();

            m_Current.RefreshVisual();
            ChatEmoteRain.Instance.OnSettingsChanged();
        }
    }
}
