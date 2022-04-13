using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine;

namespace BeatSaberPlus_ChatIntegrations.Interfaces
{
    /// <summary>
    /// IAction generic class
    /// </summary>
    public interface IActionBase
    {
        /// <summary>
        /// Event instance
        /// </summary>
        IEventBase Event { get; set; }
        /// <summary>
        /// Action description
        /// </summary>
        string Description { get; }
        /// <summary>
        /// UI PlaceHolder description
        /// </summary>
        string UIPlaceHolder { get; }
        /// <summary>
        /// Is enabled
        /// </summary>
        bool IsEnabled { get; set; }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        string GetTypeName();
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        string GetTypeNameShort();
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        JObject Serialize();
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        bool Unserialize(JObject p_Serialized);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        void BuildUI(Transform p_Parent);

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        IEnumerator Eval(Models.EventContext p_Context);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// IAction generic class
    /// </summary>
    public abstract class IAction<T, M> : IActionBase
        where T : IAction<T, M>, new()
        where M : Models.Action, new()
    {
        /// <summary>
        /// Event instance
        /// </summary>
        public IEventBase Event { get; set; }
        /// <summary>
        /// Action description
        /// </summary>
        public abstract string Description { get; }
        /// <summary>
        /// UI PlaceHolder description
        /// </summary>
        [UIValue("BSPCIUIPlaceHolder")]
        public string UIPlaceHolder { get; protected set; } = "<alpha=#AA><i>No available settings...</i>";
        /// <summary>
        /// Should display a test button?
        /// </summary>
        public bool UIPlaceHolderTestButton { get; protected set; } = false;
        /// <summary>
        /// Is enabled
        /// </summary>
        public bool IsEnabled { get => Model.Enabled; set { Model.Enabled = value; } }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Model
        /// </summary>
        public M Model { get; protected set; } = new M();

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public string GetTypeName()
        {
            return string.Join(".", typeof(T).Namespace, typeof(T).Name);
        }
        /// <summary>
        /// Get type name
        /// </summary>
        /// <returns></returns>
        public string GetTypeNameShort()
        {
            return typeof(T).Name;
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <returns></returns>
        public JObject Serialize()
        {
            Model.Type = GetTypeName();
            return JObject.FromObject(Model);
        }
        /// <summary>
        /// Unserialize
        /// </summary>
        /// <param name="p_Serialized"></param>
        public bool Unserialize(JObject p_Serialized)
        {
            if (!p_Serialized.ContainsKey(nameof(Models.Condition.Type)) || p_Serialized[nameof(Models.Condition.Type)].Value<string>() != GetTypeName())
                return false;

            Model = p_Serialized.ToObject<M>();

            return true;
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Build editing UI
        /// </summary>
        /// <param name="p_Parent">Parent transform</param>
        public virtual void BuildUI(Transform p_Parent)
        {
            BSMLParser.instance.Parse(
                "<vertical>"
                +
                "<text text=\"~BSPCIUIPlaceHolder\" align=\"Center\" />"
                +
                (UIPlaceHolderTestButton ?
                    "<primary-button on-click='UIPlaceholderTestButton' text='Test' min-width='20'></primary-button>"
                    :
                    ""
                )
                +
                "</vertical>",
            p_Parent.gameObject, this);
        }
        /// <summary>
        /// On UI placeholder test button pressed
        /// </summary>
        [UIAction("UIPlaceholderTestButton")]
        protected virtual void OnUIPlaceholderTestButton() { }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handle
        /// </summary>
        /// <param name="p_Context">Event context</param>
        public abstract IEnumerator Eval(Models.EventContext p_Context);
    }
}
