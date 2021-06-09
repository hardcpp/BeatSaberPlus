using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatSaberPlus.Modules.ChatIntegrations.Conditions
{
    public class Subscription_IsGift : Interfaces.ICondition<Subscription_IsGift, Models.Condition>
    {
        public override string Description => "Is a gift subscription event?";

        public Subscription_IsGift() => UIPlaceHolder = "<b><i>Ensure that this is a subscription gift</i></b>";

        public override bool Eval(Models.EventContext p_Context)
        {
            return p_Context.SubscriptionEvent.IsGift;
        }
    }

    public class Subscription_PlanType : Interfaces.ICondition<Subscription_PlanType, Models.Conditions.Subscription_PlanType>
    {
        public override string Description => "Put condition on the kind of subscription";

#pragma warning disable CS0414
        [UIComponent("PlanTypeList")]
        private ListSetting m_PlanTypeList = null;
        [UIValue("PlanTypeList_Choices")]
        private List<object> m_PlanTypeList_Choices = new List<object>() { "Prime", "Tier1", "Tier2", "Tier3" };
        [UIValue("PlanTypeList_Value")]
        private string m_PlanTypeList_Value;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            m_PlanTypeList_Value = (string)m_PlanTypeList_Choices.ElementAt(Model.PlanType % m_PlanTypeList_Choices.Count);

            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.ListSetting.Setup(m_PlanTypeList, l_Event, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.PlanType = m_PlanTypeList_Choices.Select(x => (string)x).ToList().IndexOf(m_PlanTypeList.Value);
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            if (p_Context.SubscriptionEvent != null
                && p_Context.SubscriptionEvent.SubPlan.ToLower() == ((string)m_PlanTypeList_Choices.ElementAt(Model.PlanType % m_PlanTypeList_Choices.Count)).ToLower())
                return true;

            return false;
        }
    }

    public class Subscription_PurchasedMonthCount : Interfaces.ICondition<Subscription_PurchasedMonthCount, Models.Conditions.Subscription_PurchasedMonthCount>
    {
        public override string Description => "Check for purchased month count";

#pragma warning disable CS0414
        [UIComponent("CountSlider")]
        private SliderSetting m_CountSlider = null;
#pragma warning restore CS0414

        public override sealed void BuildUI(Transform p_Parent)
        {
            string l_BSML = Utilities.GetResourceContent(Assembly.GetAssembly(GetType()), string.Join(".", GetType().Namespace, "Views", GetType().Name) + ".bsml");
            BSMLParser.instance.Parse(l_BSML, p_Parent.gameObject, this);

            var l_Event = new BeatSaberMarkupLanguage.Parser.BSMLAction(this, this.GetType().GetMethod(nameof(OnSettingChanged), BindingFlags.Instance | BindingFlags.NonPublic));

            SDK.UI.SliderSetting.Setup(m_CountSlider, l_Event, null, Model.Count, false);

            OnSettingChanged(null);
        }
        private void OnSettingChanged(object p_Value)
        {
            Model.Count = (uint)m_CountSlider.slider.value;
        }

        public override bool Eval(Models.EventContext p_Context)
        {
            return p_Context.SubscriptionEvent != null && p_Context.SubscriptionEvent.PurchasedMonthCount == Model.Count;
        }
    }
}
