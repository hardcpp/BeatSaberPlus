using ChatPlexMod_ChatIntegrations.Models;
using CP_SDK.XUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class MiscRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("Misc_Delay",             () => new Misc_Delay());
            ChatIntegrations.RegisterActionType("Misc_PlaySound",         () => new Misc_PlaySound());
            ChatIntegrations.RegisterActionType("Misc_WaitMenuScene",     () => new Misc_WaitMenuScene());
            ChatIntegrations.RegisterActionType("Misc_WaitPlayingScene",  () => new Misc_WaitPlayingScene());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_Delay
        : Interfaces.IAction<Misc_Delay, Models.Actions.Misc_Delay>
    {
        private XUISlider m_Delay                       = null;
        private XUISlider m_DelayMs                     = null;
        private XUIToggle m_PreventNextActionsFailure   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Delay next actions";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsVGroup("Delay",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1200.0f).SetIncrements(1.0f).SetFormatter(CP_SDK.UI.ValueFormatters.TimeShortBaseSeconds)
                        .SetValue(Model.Delay)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Delay),
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1000.0f).SetIncrements(1.0f).SetFormatter(CP_SDK.UI.ValueFormatters.MillisecondsShort)
                        .SetValue(Model.DelayMs)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_DelayMs)
                ),

                Templates.SettingsHGroup("Prevent next actions failure",
                    XUIToggle.Make()
                        .SetValue(Model.PreventNextActionFailure)
                        .OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_PreventNextActionsFailure)
                ),

                XUIVLayout.Make(
                    XUIText.Make("This actions will delay next actions execution"),
                    XUIText.Make("If prevent next actions failure is enabled,"),
                    XUIText.Make("any failed action won't refund the user")
                )
                .SetBackground(true),
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Delay                     = (uint)m_Delay.Element.GetValue();
            Model.DelayMs                   = (uint)m_DelayMs.Element.GetValue();
            Model.PreventNextActionFailure  = m_PreventNextActionsFailure.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (Model.PreventNextActionFailure)
                p_Context.PreventNextActionFailure = true;

            yield return new WaitForSecondsRealtime((float)Model.Delay + (((float)Model.DelayMs) / 1000f));
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_PlaySound
        : Interfaces.IAction<Misc_PlaySound, Models.Actions.Misc_PlaySound>
    {
        private XUIDropdown m_Dropdown          = null;
        private XUISlider   m_Volume            = null;
        private XUISlider   m_PitchMin          = null;
        private XUISlider   m_PitchMax          = null;
        private XUIToggle   m_KillOnSceneChange = null;

        private string      m_PathCache     = null;
        private AudioClip   m_AudioClip     = null;
        private AudioSource m_AudioSource   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Play a sound clip";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override void BuildUI(Transform p_Parent)
        {
            var l_Files     = Directory.GetFiles(ChatIntegrations.s_SOUND_CLIPS_ASSETS_PATH, "*.ogg").ToArray();
            var l_Choices   = new List<string>() { "<i>None</i>" };
            var l_Selected  = "<i>None</i>";

            foreach (var l_CurrentFile in l_Files)
            {
                var l_Filtered = Path.GetFileName(l_CurrentFile);
                l_Choices.Add(l_Filtered);

                if (l_Filtered == Model.BaseValue)
                    l_Selected = l_Filtered;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Sound clip",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Dropdown)
                ),

                Templates.SettingsHGroup("Volume",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(1.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.Volume).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref  m_Volume)
                ),

                Templates.SettingsHGroup("Pitch min/max",
                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(2.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.PitchMin).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_PitchMin),

                    XUISlider.Make()
                        .SetMinValue(0.0f).SetMaxValue(2.0f).SetIncrements(0.01f).SetFormatter(CP_SDK.UI.ValueFormatters.Percentage)
                        .SetValue(Model.PitchMax).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_PitchMax)
                ),

                Templates.SettingsHGroup("Kill on scene switch?",
                    XUIToggle.Make()
                        .SetValue(Model.KillOnSceneSwitch).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_KillOnSceneChange)
                ),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            if (Model.BaseValue != m_Dropdown.Element.GetValue())
            {
                m_PathCache = null;
                m_AudioClip = null;
            }

            Model.BaseValue         = m_Dropdown.Element.GetValue();
            Model.Volume            = m_Volume.Element.GetValue();
            Model.PitchMin          = m_PitchMin.Element.GetValue();
            Model.PitchMax          = m_PitchMax.Element.GetValue();
            Model.KillOnSceneSwitch = m_KillOnSceneChange.Element.GetValue();

            if (Model.BaseValue == "<i>None</i>")
                Model.BaseValue = "";
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            CP_SDK.Unity.MTCoroutineStarter.Start(Eval(null));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

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
                        Logger.Instance.Debug("[ChatPlexMod_ChatIntegrations.Actions][Misc_PlaySound.PlayAudioClip] No audio found!");
                        yield break;
                    }

                }
                catch (Exception p_Exception)
                {
                    Logger.Instance.Error("[ChatPlexMod_ChatIntegrations.Actions][Misc_PlaySound.PlayAudioClip] Can't load audio! Exception: ");
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
                    m_AudioSource                       = new GameObject("BSP_CI_Misc_PlaySound").AddComponent<AudioSource>();
                    m_AudioSource.loop                  = false;
                    m_AudioSource.spatialize            = false;
                    m_AudioSource.playOnAwake           = false;
                    m_AudioSource.ignoreListenerPause   = true;

                    if (!Model.KillOnSceneSwitch)
                        GameObject.DontDestroyOnLoad(m_AudioSource);
                }

                m_AudioSource.clip          = m_AudioClip;
                m_AudioSource.volume        = Model.Volume;
                m_AudioSource.pitch         = UnityEngine.Random.Range(Model.PitchMin, Model.PitchMax);
                m_AudioSource.Play();
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_WaitMenuScene
        : Interfaces.IAction<Misc_WaitMenuScene, Models.Action>
    {
        public override string Description      => "Wait for menu scene";
        public override string UIPlaceHolder    => "Wait for menu scene";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(EventContext p_Context)
        {
            yield return new WaitUntil(() => CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Menu);
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Misc_WaitPlayingScene
        : Interfaces.IAction<Misc_WaitPlayingScene, Models.Action>
    {
        public override string Description      => "Wait for playing scene";
        public override string UIPlaceHolder    => "Wait for playing scene";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(EventContext p_Context)
        {
            yield return new WaitUntil(() => CP_SDK.ChatPlexSDK.ActiveGenericScene == CP_SDK.ChatPlexSDK.EGenericScene.Playing);
        }
    }
}
