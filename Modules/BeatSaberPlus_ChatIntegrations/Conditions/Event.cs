namespace BeatSaberPlus_ChatIntegrations.Conditions
{
    public class Event_AlwaysFail : Interfaces.ICondition<Event_AlwaysFail, Models.Condition>
    {
        public override string Description => "Always fail the event";

        public Event_AlwaysFail() => UIPlaceHolder = "<b><i>Make the event to always fail</i></b>";

        public override bool Eval(Models.EventContext p_Context)
        {
            return false;
        }
    }

}
