﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberPlus.Modules.ChatIntegrations.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberPlus.Modules.ChatIntegrations.Actions
{
    internal class MiscBuilder
    {
        internal static List<Interfaces.IActionBase> BuildFor(Interfaces.IEventBase p_Event)
        {
            switch (p_Event)
            {
                default:
                    break;
            }

            return new List<Interfaces.IActionBase>()
            {
                new Misc_Delay(),
                new Misc_PlaySound()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_Delay : Interfaces.IAction<Misc_Delay, Models.Actions.Misc_Delay>
    {
        public override string Description => "Delay next actions";

#pragma warning disable CS0414
        [UIComponent("DelaySlider")]
        private SliderSetting m_DelaySlider = null;
        [UIComponent("PreventFailureToggle")]
        private ToggleSetting m_PreventFailureToggle = null;

        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.SliderSetting.Setup(m_DelaySlider,           l_Event, SDK.UI.BSMLSettingFormartter.Time, Model.Delay,                    true, true, new Vector2(0.08f, 0.10f), new Vector2(0.93f, 0.90f));
            SDK.UI.ToggleSetting.Setup(m_PreventFailureToggle,  l_Event,                                    Model.PreventNextActionFailure, false);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Delay                     = (uint)m_DelaySlider.slider.value;
            Model.PreventNextActionFailure  = m_PreventFailureToggle.Value;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (Model.PreventNextActionFailure)
                p_Context.PreventNextActionFailure = true;

            yield return new WaitForSecondsRealtime((float)Model.Delay);
            yield return null;
        }
    }

    public class Misc_PlaySound : Interfaces.IAction<Misc_PlaySound, Models.Actions.Misc_PlaySound>
    {
        public override string Description => "Play a sound clip";

        private string m_PathCache = null;
        private AudioClip m_AudioClip = null;
        private AudioSource m_AudioSource = null;

#pragma warning disable CS0414
        [UIComponent("File_DropDown")]
        protected DropDownListSetting m_File_DropDown = null;
        [UIValue("File_DropDownOptions")]
        private List<object> m_File_DropDownOptions = new List<object>() { "Loading...", };
        [UIComponent("VolumeIncrement")]
        protected IncrementSetting m_VolumeIncrement = null;
        [UIComponent("PitchIncrement")]
        protected IncrementSetting m_PitchIncrement = null;

        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;
#pragma warning restore CS0414

        public override void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.DropDownListSetting.Setup(m_File_DropDown, l_Event, true);
            SDK.UI.IncrementSetting.Setup(m_VolumeIncrement, l_Event, SDK.UI.BSMLSettingFormartter.Percentage, Model.Volume, false);
            SDK.UI.IncrementSetting.Setup(m_PitchIncrement, l_Event, SDK.UI.BSMLSettingFormartter.Percentage, Model.Pitch, false);

            var l_Files = Directory.GetFiles(ChatIntegrations.s_SOUND_CLIPS_ASSETS_PATH, "*.ogg").ToArray();

            bool l_ChoiceExist = false;
            var l_Choices = new List<object>();
            foreach (var l_CurrentFile in l_Files)
            {
                var l_Filtered = Path.GetFileName(l_CurrentFile);
                l_Choices.Add(l_Filtered);

                if (l_Filtered == Model.BaseValue)
                    l_ChoiceExist = true;
            }

            if (l_Choices.Count == 0)
                l_Choices.Add("None");

            m_File_DropDownOptions = l_Choices;
            m_File_DropDown.values = l_Choices;
            m_File_DropDown.Value = l_ChoiceExist ? Model.BaseValue : l_Choices[0];
            m_File_DropDown.UpdateChoices();
        }
        private void OnSettingChanged(object p_Value)
        {
            if (Model.BaseValue != (string)m_File_DropDown.Value)
            {
                m_PathCache = null;
                m_AudioClip = null;
            }

            Model.BaseValue = (string)m_File_DropDown.Value;
            Model.Volume = m_VolumeIncrement.Value;
            Model.Pitch = m_PitchIncrement.Value;
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            SharedCoroutineStarter.instance.StartCoroutine(Eval(null));
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (Model.BaseValue != null)
            {
                if (m_PathCache == null)
                    m_PathCache = Path.Combine(Environment.CurrentDirectory, ChatIntegrations.s_SOUND_CLIPS_ASSETS_PATH, Model.BaseValue);

                yield return PlayAudioClip(m_PathCache);
            }
            else if (p_Context != null)
                p_Context.HasActionFailed = true;

            yield return null;
        }

        private IEnumerator PlayAudioClip(string p_File)
        {
            if (m_AudioClip == null && File.Exists(p_File))
            {
                UnityWebRequest l_Song = UnityWebRequestMultimedia.GetAudioClip(p_File, AudioType.OGGVORBIS);
                yield return l_Song.SendWebRequest();

                AudioClip l_Clip = null;
                try
                {
                    ((DownloadHandlerAudioClip)l_Song.downloadHandler).streamAudio = true;
                    l_Clip = DownloadHandlerAudioClip.GetContent(l_Song);

                    if (l_Clip == null)
                    {
                        Logger.Instance.Debug("[Modules.ChatIntegrations.Actions][Misc_PlaySound.PlayAudioClip] No audio found!");
                        yield break;
                    }

                }
                catch (Exception p_Exception)
                {
                    Logger.Instance.Error("[Modules.ChatIntegrations.Actions][Misc_PlaySound.PlayAudioClip] Can't load audio! Exception: ");
                    Logger.Instance.Error(p_Exception);

                    yield break;
                }

                yield return new WaitUntil(() => l_Clip);

                m_AudioClip = l_Clip;
            }

            if (m_AudioClip != null)
            {
                if (m_AudioSource == null || !m_AudioSource)
                {
                    m_AudioSource               = new GameObject("BSP_CI_Misc_PlaySound").AddComponent<AudioSource>();
                    m_AudioSource.loop          = false;
                    m_AudioSource.spatialize    = false;
                    m_AudioSource.playOnAwake   = false;

                    GameObject.DontDestroyOnLoad(m_AudioSource);
                }

                m_AudioSource.clip          = m_AudioClip;
                m_AudioSource.volume        = Model.Volume;
                m_AudioSource.pitch         = Model.Pitch;
                m_AudioSource.Play();
            }
        }
    }
}
