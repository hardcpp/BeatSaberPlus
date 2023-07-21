using CP_SDK.XUI;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Conditions
{
    public class Bits_Amount
        : Interfaces.ICondition<Bits_Amount, Models.Conditions.Bits_Amount>
    {
        private XUIDropdown m_Comparison    = null;
        private XUISlider   m_Count         = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add conditions on chat request queue size!";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform p_Parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Comparison",
                    XUIDropdown.Make()
                        .SetOptions(Enums.Comparison.S).SetValue(Enums.Comparison.ToStr(Model.Comparison))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_Comparison)
                ),

                Templates.SettingsHGroup("Count",
                    XUISlider.Make()
                        .SetMinValue(1.0f).SetMaxValue(10000.0f).SetIncrements(1.0f).SetInteger(true)
                        .SetValue(Model.Count).OnValueChanged((_) => OnSettingChanged())
                        .Bind(ref m_Count)
                )
            };

            BuildUIAuto(p_Parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.Comparison    = Enums.Comparison.ToEnum(m_Comparison.Element.GetValue());
            Model.Count         = (uint)m_Count.Element.GetValue();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override bool Eval(Models.EventContext p_Context)
        {
            if (!p_Context.BitsEvent.HasValue)
                return false;

            return Enums.Comparison.Evaluate(Model.Comparison, (uint)p_Context.BitsEvent.Value, Model.Count);
        }
    }
}
