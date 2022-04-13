using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class EmoteRainBuilder
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
                new EmoteRain_CustomRain(),
                new EmoteRain_EmoteBombRain(),
                new EmoteRain_SubRain()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class EmoteRain_CustomRain : Interfaces.IAction<EmoteRain_CustomRain, Models.Actions.EmoteRain_CustomRain>
    {
        public override string Description => "Make rain custom emotes";

        private BSMLParserParams m_ParserParams;
        private BeatSaberPlus.SDK.Unity.EnhancedImage m_LoadedImage = null;
        private string m_LoadedImageID = "";
        private string m_LoadedImageName = "";

#pragma warning disable CS0414
        [UIComponent("File_DropDown")]
        protected DropDownListSetting m_File_DropDown = null;
        [UIValue("File_DropDownOptions")]
        private List<object> m_File_DropDownOptions = new List<object>() { "Loading...", };
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;
        [UIObject("InfoPanel_Background")]
        private GameObject m_InfoPanel_Background = null;

        [UIObject("EmoteRainNotEnabledModal")]
        private GameObject m_EmoteRainNotEnabledModal = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.Backgroundable.SetOpacity(m_InfoPanel_Background, 0.75f);
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_EmoteRainNotEnabledModal, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.Count, false);
            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_File_DropDown, l_Event, true);

            var l_Files = Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.png")
                   .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.gif"))
                   .Union(Directory.GetFiles(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, "*.apng")).ToArray();

            bool l_ChoiceExist = false;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");
            foreach (var l_CurrentFile in l_Files)
            {
                var l_Filtered = Path.GetFileName(l_CurrentFile);
                l_Choices.Add(l_Filtered);

                if (l_Filtered == Model.BaseValue)
                    l_ChoiceExist = true;
            }

            m_File_DropDownOptions  = l_Choices;
            m_File_DropDown.values  = l_Choices;
            m_File_DropDown.Value   = l_ChoiceExist ? Model.BaseValue : l_Choices[0];
            m_File_DropDown.UpdateChoices();

            if (!ModulePresence.ChatEmoteRain)
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatEmoteRain module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.BaseValue = (string)m_File_DropDown.Value;
            Model.Count     = (uint)m_CountIncrement.Value;

            if ((string)p_Value == "<i>None</i>")
                Model.BaseValue = "";
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance == null || !BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                m_ParserParams.EmitEvent("ShowEmoteRainNotEnabledModal");
                return;
            }

            MakeItRain();
        }

        public override IEnumerator Eval(EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance != null && BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                MakeItRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }

        private void MakeItRain()
        {
            if (EnsureLoaded(true))
            {
                if (m_LoadedImage == null)
                    return;

                BeatSaberPlus.SDK.Unity.MainThreadInvoker.Enqueue(() =>
                {
                    SharedCoroutineStarter.instance.StartCoroutine(
                        BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.StartParticleSystem(m_LoadedImageID, m_LoadedImage, Model.Count)
                    );
                });
            }
        }
        private bool EnsureLoaded(bool p_MakeItRainAfterLoad)
        {
            if (Model.BaseValue == "None")
                return false;

            if (m_LoadedImageName != Model.BaseValue)
            {
                m_LoadedImageName = Model.BaseValue;

                string l_Path = Path.Combine(ChatIntegrations.s_EMOTE_RAIN_ASSETS_PATH, Model.BaseValue);
                if (File.Exists(l_Path))
                {
                    m_LoadedImageID = "$BSP$CI$_" + Model.BaseValue;
                    BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.LoadExternalEmote(l_Path, m_LoadedImageID, (p_Result) => {
                        m_LoadedImage = p_Result;

                        if (m_LoadedImage != null && p_MakeItRainAfterLoad)
                            MakeItRain();
                    });
                }

                return false;
            }

            return true;
        }
    }

    public class EmoteRain_EmoteBombRain : Interfaces.IAction<EmoteRain_EmoteBombRain, Models.Actions.EmoteRain_EmoteBombRain>
    {
        public override string Description => "Trigger a massive emote bomb rain";

        private BSMLParserParams m_ParserParams;

#pragma warning disable CS0414
        [UIComponent("KindIncrement")]
        protected IncrementSetting m_KindIncrement = null;
        [UIComponent("CountIncrement")]
        protected IncrementSetting m_CountIncrement = null;

        [UIObject("EmoteRainNotEnabledModal")]
        private GameObject m_EmoteRainNotEnabledModal = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            m_ParserParams = BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            /// Change opacity
            BeatSaberPlus.SDK.UI.ModalView.SetOpacity(m_EmoteRainNotEnabledModal, 0.75f);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_KindIncrement,  l_Event, null, Model.EmoteKindCount, false);
            BeatSaberPlus.SDK.UI.IncrementSetting.Setup(m_CountIncrement, l_Event, null, Model.CountPerEmote,  false);

            if (!ModulePresence.ChatEmoteRain)
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: ChatEmoteRain module is missing!");
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.EmoteKindCount    = (uint)m_KindIncrement.Value;
            Model.CountPerEmote     = (uint)m_CountIncrement.Value;
        }

        [UIAction("click-test-btn-pressed")]
        private void OnTestButton()
        {
            if (BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance == null || !BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                m_ParserParams.EmitEvent("ShowEmoteRainNotEnabledModal");
                return;
            }

            SharedCoroutineStarter.instance.StartCoroutine(Eval(null));
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance != null && BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
            {
                var l_Emotes =
                    BeatSaberPlus.SDK.Chat.ImageProvider.CachedEmoteInfo.Values.OrderBy(_ => Random.Range(0, 1000)).Take((int)Model.EmoteKindCount);

                foreach (var l_Emote in l_Emotes)
                    yield return BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.StartParticleSystem(l_Emote.ImageID, l_Emote, Model.CountPerEmote);
            }
            else if (p_Context != null)
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }

    public class EmoteRain_SubRain : Interfaces.IAction<EmoteRain_SubRain, Models.Action>
    {
        public override string Description => "Trigger a subscription rain";

        public EmoteRain_SubRain() => UIPlaceHolder = "Will trigger a subscription emote rain";

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.ChatEmoteRain)
            {
                p_Context.HasActionFailed = true;
                BeatSaberPlus.SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatEmoteRain module is missing!");
                yield break;
            }

            if (BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance != null && BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.IsEnabled)
                BeatSaberPlus_ChatEmoteRain.ChatEmoteRain.Instance.StartSubRain();
            else
                p_Context.HasActionFailed = true;

            yield return null;
        }
    }
}
