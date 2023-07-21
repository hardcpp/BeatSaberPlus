using ChatPlexMod_ChatIntegrations.Interfaces;
using CP_SDK.Chat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatPlexMod_ChatIntegrations.Models
{
    /// <summary>
    /// Event context
    /// </summary>
    public class EventContext : ICloneable
    {
        /// <summary>
        /// Provided values
        /// </summary>
        private Dictionary<(EValueType, string), object> m_Values = new Dictionary<(EValueType, string), object>();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Trigger type
        /// </summary>
        public ETriggerType Type = ETriggerType.None;
        /// <summary>
        /// Service instance
        /// </summary>
        public IChatService ChatService = null;
        /// <summary>
        /// Channel
        /// </summary>
        public IChatChannel Channel = null;
        /// <summary>
        /// User
        /// </summary>
        public IChatUser User = null;
        /// <summary>
        /// Message
        /// </summary>
        public IChatMessage Message = null;
        /// <summary>
        /// Bits
        /// </summary>
        public int? BitsEvent = null;
        /// <summary>
        /// Raider count
        /// </summary>
        public int? RaidEvent = null;
        /// <summary>
        /// Points
        /// </summary>
        public IChatChannelPointEvent PointsEvent = null;
        /// <summary>
        /// Subscription
        /// </summary>
        public IChatSubscriptionEvent SubscriptionEvent = null;
        /// <summary>
        /// VoiceAttack command GUID
        /// </summary>
        public string VoiceAttackCommandGUID = null;
        /// <summary>
        /// VoiceAttack command name
        /// </summary>
        public string VoiceAttackCommandName = null;
        /// <summary>
        /// Custom event data
        /// </summary>
        public object CustomData = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Variable count
        /// </summary>
        public int VariableCount => m_Values.Count;
        /// <summary>
        /// Has an action failed
        /// </summary>
        public bool HasActionFailed = false;
        /// <summary>
        /// Prevent next actions failure
        /// </summary>
        public bool PreventNextActionFailure = false;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var l_Result = this.MemberwiseClone();
            (l_Result as EventContext).m_Values = new Dictionary<(EValueType, string), object>();

            return l_Result;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add value
        /// </summary>
        /// <param name="p_Type">IValueType Type</param>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Value">Value data</param>
        public void AddValue(EValueType p_Type, string p_Name, object p_Value)
        {
            var l_Key = (p_Type, p_Name);

            if (m_Values.ContainsKey(l_Key))
                throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Duplicate for {p_Type}.{p_Name}!");

            switch (p_Type)
            {
                case EValueType.Boolean:
                    if (p_Value != null && !(p_Value is bool?))
                        throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Wrong value type for {p_Type}.{p_Name}, bool? excepted, got {p_Value.GetType()}!");
                    break;

                case EValueType.Integer:
                    if (p_Value != null && !(p_Value is Int64?))
                        throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Wrong value type for {p_Type}.{p_Name}, Int64? excepted, got {p_Value.GetType()}!");
                    break;

                case EValueType.Floating:
                    if (p_Value != null && !(p_Value is float?))
                        throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Wrong value type for {p_Type}.{p_Name}, float? excepted, got {p_Value.GetType()}!");
                    break;

                case EValueType.String:
                    if (p_Value != null && !(p_Value is string))
                        throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Wrong value type for {p_Type}.{p_Name}, string excepted, got {p_Value.GetType()}!");
                    break;

                case EValueType.Emotes:
                    if (p_Value != null && !(p_Value is IChatEmote[]))
                        throw new System.Exception($"[ChatPlexMod_ChatIntegrations.Models][EventContext.AddValue] Wrong value type for {p_Type}.{p_Name}, List<IChatEmote> excepted, got {p_Value.GetType()}!");
                    break;

            }

            m_Values.Add(l_Key, p_Value);
        }
        /// <summary>
        /// Get values by types
        /// </summary>
        /// <param name="p_Types">Types</param>
        /// <returns></returns>
        public List<(EValueType, string)> GetValues(params EValueType[] p_Types)
        {
            return m_Values.Select(x => x.Key).Where(x => p_Types.Contains(x.Item1)).ToList();
        }
        /// <summary>
        /// Get first value of type
        /// </summary>
        /// <param name="p_Type">Variable</param>
        /// <returns></returns>
        public (EValueType, string) GetFirstValueOfType(EValueType p_Type)
        {
            return m_Values.Select(x => x.Key).Where(x => x.Item1 == p_Type).FirstOrDefault();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get boolean value
        /// </summary>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Out">Out result</param>
        /// <returns>If the value exist</returns>
        public bool GetBooleanValue(string p_Name, out bool? p_Out)
        {
            p_Out = null;

            if (m_Values != null && m_Values.TryGetValue((EValueType.Boolean, p_Name), out var l_Result))
            {
                p_Out = (bool?)l_Result;
                return p_Out != null && p_Out.HasValue;
            }

            return false;
        }
        /// <summary>
        /// Get integer value
        /// </summary>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Out">Out result</param>
        /// <returns>If the value exist</returns>
        public bool GetIntegerValue(string p_Name, out Int64? p_Out)
        {
            p_Out = null;

            if (m_Values != null && m_Values.TryGetValue((EValueType.Integer, p_Name), out var l_Result))
            {
                p_Out = (Int64?)l_Result;
                return p_Out != null && p_Out.HasValue;
            }

            return false;
        }
        /// <summary>
        /// Get floating value
        /// </summary>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Out">Out result</param>
        /// <returns>If the value exist</returns>
        public bool GetFloatingValue(string p_Name, out float? p_Out)
        {
            p_Out = null;

            if (m_Values != null && m_Values.TryGetValue((EValueType.Floating, p_Name), out var l_Result))
            {
                p_Out = (float?)l_Result;
                return p_Out != null && p_Out.HasValue;
            }

            return false;
        }
        /// <summary>
        /// Get string value
        /// </summary>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Out">Out result</param>
        /// <returns>If the value exist</returns>
        public bool GetStringValue(string p_Name, out string p_Out)
        {
            p_Out = null;

            if (m_Values != null && m_Values.TryGetValue((EValueType.String, p_Name), out var l_Result))
            {
                p_Out = (string)l_Result;
                return p_Out != null;
            }

            return false;
        }
        /// <summary>
        /// Get emotes value
        /// </summary>
        /// <param name="p_Name">Value name</param>
        /// <param name="p_Out">Out result</param>
        /// <returns>If the value exist</returns>
        public bool GetEmotesValue(string p_Name, out IChatEmote[] p_Out)
        {
            p_Out = null;

            if (m_Values != null && m_Values.TryGetValue((EValueType.Emotes, p_Name), out var l_Result))
            {
                p_Out = (IChatEmote[])l_Result;
                return p_Out != null;
            }

            return false;
        }
    }
}
