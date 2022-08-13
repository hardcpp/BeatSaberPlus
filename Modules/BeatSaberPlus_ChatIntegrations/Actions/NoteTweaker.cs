using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberPlus_ChatIntegrations.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class NoteTweakerBuilder
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
                new NoteTweaker_SwitchProfile()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class NoteTweaker_SwitchProfile : Interfaces.IAction<NoteTweaker_SwitchProfile, Models.Action>
    {
        public override string Description => "Change active NoteTweaker profile";

#pragma warning disable CS0414
        [UIComponent("Profile_DropDown")]         protected DropDownListSetting m_Profile_DropDown = null;
        [UIValue("Profile_DropDownOptions")]      private List<object> m_Profile_DropDownOptions = new List<object>() { "Loading...", };
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.DropDownListSetting.Setup(m_Profile_DropDown,  l_Event, true);

            int l_ChoiceIndex = 0;
            var l_Choices = new List<object>();
            l_Choices.Add("<i>None</i>");

            if (ModulePresence.NoteTweaker)
            {
                l_Choices = BeatSaberPlus_NoteTweaker.NoteTweaker.Instance.GetAvailableProfiles().ToList<object>();

                if (l_Choices.Count == 0)
                    l_Choices.Add("<i>None</i>");
            }
            else if (!ModulePresence.NoteTweaker)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");

            for (int l_I = 0; l_I < l_Choices.Count; ++l_I)
            {
                if (l_Choices[l_I] as string != Model.BaseValue)
                    continue;

                l_ChoiceIndex = l_I;
                break;
            }

            m_Profile_DropDownOptions = l_Choices;
            m_Profile_DropDown.values = l_Choices;
            m_Profile_DropDown.Value = l_Choices[l_ChoiceIndex];
            m_Profile_DropDown.UpdateChoices();

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.BaseValue = m_Profile_DropDown.Value as string;
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.NoteTweaker)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, NoteTweaker module is missing!");
                yield break;
            }

            if (BeatSaberPlus_NoteTweaker.NoteTweaker.Instance.GetAvailableProfiles().Contains(Model.BaseValue))
                BeatSaberPlus_NoteTweaker.NoteTweaker.Instance.SwitchToProfile(BeatSaberPlus_NoteTweaker.NoteTweaker.Instance.GetAvailableProfiles().IndexOf(Model.BaseValue));
            else
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Event:{Event.GenericModel.Name} Action:NoteTweaker_SwitchProfile Profile:{Model.BaseValue} not found!");
            }

            yield return null;
        }
    }

}
