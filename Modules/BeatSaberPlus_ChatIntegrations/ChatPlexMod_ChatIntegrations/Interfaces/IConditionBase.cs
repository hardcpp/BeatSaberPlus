using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ChatPlexMod_ChatIntegrations.Interfaces
{
    /// <summary>
    /// ICondition generic class
    /// </summary>
    public abstract class IConditionBase : IUIConfigurable
    {
        public          IEventBase  Event       { get; set; }
        public abstract string      Description { get; }
        public abstract bool        IsEnabled   { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public abstract string GetTypeName();
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public abstract JObject Serialize();
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        public abstract bool Unserialize(JObject p_Serialized);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public abstract void BuildUI(Transform p_Parent);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public abstract bool Eval(Models.EventContext p_Context);
    }

}
