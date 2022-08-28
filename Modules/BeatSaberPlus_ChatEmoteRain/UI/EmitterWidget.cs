using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using System.Reflection;
using UnityEngine;

using EmitterConfig = CP_SDK.Unity.Components.EnhancedImageParticleEmitter.EmitterConfig;

namespace ChatPlexMod_ChatEmoteRain.UI
{
    class EmitterWidget
    {
#pragma warning disable CS0414
        [UIComponent("NameText")]
        public TMPro.TextMeshProUGUI m_NameText = null;
        [UIComponent("SpeedSlider")]
        public SliderSetting m_SpeedSlider = null;
        [UIComponent("SizeSlider")]
        public SliderSetting m_SizeSlider = null;

        [UIComponent("PosX")]
        public SliderSetting m_PosX = null;
        [UIComponent("PosY")]
        public SliderSetting m_PosY = null;
        [UIComponent("PosZ")]
        public SliderSetting m_PosZ = null;

        [UIComponent("RotX")]
        public SliderSetting m_RotX = null;
        [UIComponent("RotY")]
        public SliderSetting m_RotY = null;
        [UIComponent("RotZ")]
        public SliderSetting m_RotZ = null;

        [UIComponent("SizeX")]
        public SliderSetting m_SizeX = null;
        [UIComponent("SizeY")]
        public SliderSetting m_SizeY = null;
        [UIComponent("SizeZ")]
        public SliderSetting m_SizeZ = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        [UIComponent("InputKeyboard")]
        private ModalKeyboard m_InputKeyboard = null;
        [UIValue("InputKeyboardValue")]
        private string m_InputKeyboardValue = "";
#pragma warning restore CS0414

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current focused emitter
        /// </summary>
        private EmitterConfig m_CurrentEmitter = null;
        /// <summary>
        /// Input keyboard callback
        /// </summary>
        private Action<string> m_InputKeyboardCallback = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public void BuildUI(Transform p_Parent, EmitterConfig l_Emitter)
        {
            m_CurrentEmitter = l_Emitter;

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, GetType().Name));
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            m_NameText.text = l_Emitter.Name;

            var l_AnchorMin = new Vector2(0.15f, -0.05f);
            var l_AnchorMax = new Vector2(0.88f, 1.05f);

            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SpeedSlider, l_Event, null, l_Emitter.Speed, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SizeSlider, l_Event, null, l_Emitter.Size, true, true, l_AnchorMin, l_AnchorMax);

            l_AnchorMin = new Vector2(0.20f, -0.05f);
            l_AnchorMax = new Vector2(0.80f, 1.05f);

            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_PosX, l_Event, null, l_Emitter.PosX, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_PosY, l_Event, null, l_Emitter.PosY, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_PosZ, l_Event, null, l_Emitter.PosZ, true, true, l_AnchorMin, l_AnchorMax);

            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_RotX, l_Event, null, l_Emitter.RotX, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_RotY, l_Event, null, l_Emitter.RotY, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_RotZ, l_Event, null, l_Emitter.RotZ, true, true, l_AnchorMin, l_AnchorMax);

            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SizeX, l_Event, null, l_Emitter.SizeX, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SizeY, l_Event, null, l_Emitter.SizeY, true, true, l_AnchorMin, l_AnchorMax);
            BeatSaberPlus.SDK.UI.SliderSetting.Setup(m_SizeZ, l_Event, null, l_Emitter.SizeZ, true, true, l_AnchorMin, l_AnchorMax);
        }
        /// <summary>
        /// When any value change
        /// </summary>
        /// <param name="p_Value">Event sender</param>
        private void OnSettingChanged(object p_Value)
        {
            m_CurrentEmitter.Speed  = m_SpeedSlider.Value;
            m_CurrentEmitter.Size   = m_SizeSlider.Value;

            m_CurrentEmitter.PosX = m_PosX.Value;
            m_CurrentEmitter.PosY = m_PosY.Value;
            m_CurrentEmitter.PosZ = m_PosZ.Value;
            m_CurrentEmitter.RotX = m_RotX.Value;
            m_CurrentEmitter.RotY = m_RotY.Value;
            m_CurrentEmitter.RotZ = m_RotZ.Value;
            m_CurrentEmitter.SizeX = m_SizeX.Value;
            m_CurrentEmitter.SizeY = m_SizeY.Value;
            m_CurrentEmitter.SizeZ = m_SizeZ.Value;

            ChatEmoteRain.Instance.OnSettingsChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name button pressed
        /// </summary>
        [UIAction("click-name-btn-pressed")]
        private void OnTitleButton()
        {
            UIShowInputKeyboard(m_CurrentEmitter.Name, (x) =>
            {
                m_CurrentEmitter.Name = x.Length > 45 ? x.Substring(0, 45) : x;
                m_NameText.text = m_CurrentEmitter.Name;
                OnSettingChanged(null);
                Settings.Instance.RebuildEmitterList(m_CurrentEmitter);
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show input keyboard
        /// </summary>
        /// <param name="p_Value">Start value</param>
        /// <param name="p_Callback">On enter callback</param>
        /// <param name="p_CustomKeys">Custom keys</param>
        public void UIShowInputKeyboard(string p_Value, Action<string> p_Callback)
        {
            m_InputKeyboardValue = p_Value;

            /// Show keyboard
            m_InputKeyboardCallback = p_Callback;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardText.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardText.enableAutoSizing = true;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMax = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.fontSizeMin = 3;
            m_InputKeyboard.keyboard.KeyboardCursor.enableAutoSizing = true;
            m_InputKeyboard.modalView.Show(true);
        }
        /// <summary>
        /// On input keyboard enter pressed
        /// </summary>
        /// <param name="p_Text"></param>
        [UIAction("InputKeyboardEnterPressed")]
        private void InputKeyboardEnterPressed(string p_Text)
        {
            m_InputKeyboardCallback?.Invoke(p_Text);
        }
    }
}
