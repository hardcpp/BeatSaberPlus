using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Actions
{
    internal class SongChartVisualizerBuilder
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
                new SongChartVisualizer_ToggleVisibility()
            };
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class SongChartVisualizer_ToggleVisibility : Interfaces.IAction<SongChartVisualizer_ToggleVisibility, Models.Actions.SongChartVisualizer_ToggleVisibility>
    {
        public override string Description => "Show or hide the SongChartVisualizer ingame";

        #pragma warning disable CS0414
        [UIComponent("TypeList")]
        private ListSetting m_TypeList = null;
        [UIValue("TypeList_Choices")]
        private List<object> m_TypeListList_Choices = new List<object>() { "Toggle", "On", "Off" };
        [UIValue("TypeList_Value")]
        private string m_TypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_TypeList_Value = (string)m_TypeListList_Choices.ElementAt(Model.ToggleType % m_TypeListList_Choices.Count);

            string l_BSML = CP_SDK.Misc.Resources.FromPathStr(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            BeatSaberPlus.SDK.UI.ListSetting.Setup(m_TypeList, l_Event, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.ToggleType = m_TypeListList_Choices.Select(x => (string)x).ToList().IndexOf(m_TypeList.Value);
        }

        public override IEnumerator Eval(Models.EventContext p_Context)
        {
            if (!ModulePresence.SongChartVisualizer)
            {
                p_Context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("SongChartVisualizer: Action failed, SongChartVisualizer module is missing!");
                yield break;
            }

            switch (Model.ToggleType)
            {
                case 0:
                    BeatSaberPlus_SongChartVisualizer.SongChartVisualizer.Instance?.ToggleVisibility();
                    break;
                case 1:
                    BeatSaberPlus_SongChartVisualizer.SongChartVisualizer.Instance?.SetVisible(true);
                    break;
                case 2:
                    BeatSaberPlus_SongChartVisualizer.SongChartVisualizer.Instance?.SetVisible(false);
                    break;
            }

            yield return null;
        }
    }

}
