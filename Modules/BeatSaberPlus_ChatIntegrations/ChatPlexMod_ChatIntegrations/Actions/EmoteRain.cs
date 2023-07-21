using ChatPlexMod_ChatIntegrations.Models;
using CP_SDK.XUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Actions
{
    internal class EmoteRainRegistration
    {
        internal static void Register()
        {
            ChatIntegrations.RegisterActionType("EmoteRain_CustomRain",       () => new EmoteRain_CustomRain());
            ChatIntegrations.RegisterActionType("EmoteRain_EmoteBombRain",    () => new EmoteRain_EmoteBombRain());
            ChatIntegrations.RegisterActionType("EmoteRain_SubRain",          () => new EmoteRain_SubRain());
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class EmoteRain_CustomRain
        : Interfaces.IAction<EmoteRain_CustomRain, Models.Actions.EmoteRain_CustomRain>
    {
        private XUIDropdown m_Dropdown  = null;
        private XUISlider   m_Count     = null;

        private CP_SDK.Unity.EnhancedImage  m_LoadedImage       = null;
        private string                      m_LoadedImageID     = "";
        private string                      m_LoadedImageName   = "";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Make rain custom emotes";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            var l_Files = Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.png")
                .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.gif"))
                .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.apng")).ToArray();

            var l_Selected = "<i>None</i>";
            var l_Choices  = new List<string>() { "<i>None</i>" };
            foreach (var l_CurrentFile in l_Files)
            {
                var l_Filtered = Path.GetFileName(l_CurrentFile);
                l_Choices.Add(l_Filtered);

                if (l_Filtered == Model.BaseValue)
                    l_Selected = l_Filtered;
            }

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Custom emote",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(l_Selected).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Dropdown)
                ),

                Templates.SettingsHGroup("Count per emote",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(100.0f).SetIncrements(1.0f).SetInteger(true).SetValue(Model.Count).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Count)
                ),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.ChatEmoteRain)
                View.ShowMessageModal("ChatEmoteRain module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.BaseValue = m_Dropdown.Element.GetValue();
            Model.Count     = (uint)m_Count.Element.GetValue();

            if (m_Dropdown.Element.GetValue() == "<i>None</i>")
                Model.BaseValue = "";
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance == null || !ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                View.ShowMessageModal("ChatEmoteRain is not enabled!");
                return;
            }

            MakeItRain();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                MakeItRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void MakeItRain()
        {
            EnsureLoaded((p_Loaded) =>
            {
                if (m_LoadedImage == null)
                    return;

                CP_SDK.Unity.MTMainThreadInvoker.Enqueue(() => ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.EmitEnhancedImage(m_LoadedImage, Model.Count));
            });
        }
        private void EnsureLoaded(Action<bool> p_Callback)
        {
            CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
            {
                if (Model.BaseValue == "None")
                {
                    p_Callback?.Invoke(false);
                    return;
                }

                if (m_LoadedImageName != Model.BaseValue)
                {
                    m_LoadedImageName = Model.BaseValue;

                    string l_Path = Path.Combine(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, Model.BaseValue);
                    if (File.Exists(l_Path))
                    {
                        m_LoadedImageID = "$CPM$CI$_" + Model.BaseValue;
                        CP_SDK.Unity.EnhancedImage.FromFile(l_Path, m_LoadedImageID, (p_Result) =>
                        {
                            m_LoadedImage = p_Result;
                            p_Callback?.Invoke(m_LoadedImage != null);
                        });
                    }
                    else
                        p_Callback?.Invoke(false);
                }

                p_Callback?.Invoke(true);
            });
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class EmoteRain_EmoteBombRain
        : Interfaces.IAction<EmoteRain_EmoteBombRain, Models.Actions.EmoteRain_EmoteBombRain>
    {
        private XUISlider m_EmoteKind       = null;
        private XUISlider m_CountPerEmote   = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Trigger a massive emote bomb rain";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Emote kind",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(100.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.EmoteKindCount).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_EmoteKind)
                ),

                Templates.SettingsHGroup("Count per emote",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(100.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.CountPerEmote).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_CountPerEmote)
                ),

                XUIPrimaryButton.Make("Test", OnTestButton)
            };

            BuildUIAuto(p_Parent);

            if (!ModulePresence.ChatEmoteRain)
                View.ShowMessageModal("ChatEmoteRain module is missing!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.EmoteKindCount    = (uint)m_EmoteKind.Element.GetValue();
            Model.CountPerEmote     = (uint)m_CountPerEmote.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnTestButton()
        {
            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance == null || !ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                View.ShowMessageModal("ChatEmoteRain is not enabled!");
                return;
            }

            CP_SDK.Unity.MTCoroutineStarter.Start(Eval(null));
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                CP_SDK.Unity.MTThreadInvoker.EnqueueOnThread(() =>
                {
                    var l_Emotes =
                        CP_SDK.Chat.ChatImageProvider.CachedEmoteInfo.Values.OrderBy(_ => UnityEngine.Random.Range(0, 1000)).Take((int)Model.EmoteKindCount);

                    foreach (var l_Emote in l_Emotes)
                        ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.EmitEnhancedImage(l_Emote, Model.CountPerEmote);
                });
            }
            else if (p_Context != null)
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class EmoteRain_SubRain : Interfaces.IAction<EmoteRain_SubRain, Models.Action>
    {
        public override string Description   => "Trigger a subscription rain";
        public override string UIPlaceHolder => "Will trigger a subscription emote rain";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance != null && ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                ChatPlexMod_ChatEmoteRain.ChatEmoteRain.Instance.StartSubRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }
}
