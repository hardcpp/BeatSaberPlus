using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Conditions
{
    public class Bits_Amount : Interfaces.ICondition<Bits_Amount, Models.Conditions.Bits_Amount>
    {
        public override string Description => "Add conditions on chat request queue size!";

#pragma warning disable CS0414
        [UIComponent("CheckTypeList")]
        private ListSetting m_CheckTypeList = null;
        [UIValue("CheckTypeList_Choices")]
        private List<object> m_CheckTypeList_Choices = new List<object>() { "Greater than", "Less than" };
        [UIValue("CheckTypeList_Value")]
        private string m_CheckTypeList_Value;
        [UIComponent("CountSlider")]
        private SliderSetting m_CountSlider = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_CheckTypeList_Value = (string)m_CheckTypeList_Choices.ElementAt(Model.IsGreaterThan ? 0 : 1);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ListSetting.Setup(m_CheckTypeList,               l_Event,                                    false);
            SDK.UI.SliderSetting.Setup(m_CountSlider,               l_Event, null, Model.Count,                 true, true, new Vector2(0.51f, 0.10f), new Vector2(0.93f, 0.90f));

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.IsGreaterThan         = m_CheckTypeList_Choices.Select(x => (string)x).ToList().IndexOf(m_CheckTypeList.Value) == 0;
            Model.Count                 = (uint)m_CountSlider.slider.value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (!p_Context.BitsEvent.HasValue)
                return false;

            if (Model.IsGreaterThan)
            {
                if (p_Context.BitsEvent.Value > Model.Count)
                    return true;
            }
            else
            {
                if (p_Context.BitsEvent.Value < Model.Count)
                    return true;
            }

            return false;
        }
    }
}
